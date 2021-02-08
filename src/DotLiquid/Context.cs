using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid
{
    /// <summary>
    /// Context keeps the variable stack and resolves variables, as well as keywords
    /// </summary>
    public class Context
    {
        private static readonly Regex SingleQuotedRegex = R.C(R.Q(@"^'(.*)'$"));
        private static readonly Regex DoubleQuotedRegex = R.C(R.Q(@"^""(.*)""$"));
        private static readonly Regex IntegerRegex = R.C(R.Q(@"^([+-]?\d+)$"));
        private static readonly Regex RangeRegex = R.C(R.Q(@"^\((\S+)\.\.(\S+)\)$"));
        private static readonly Regex NumericRegex = R.C(R.Q(@"^([+-]?\d[\d\.|\,]+)$"));
        private static readonly Regex SquareBracketedRegex = R.C(R.Q(@"^\[(.*)\]$"));
        private static readonly Regex VariableParserRegex = R.C(Liquid.VariableParser);

        private readonly ErrorsOutputMode _errorsOutputMode;
        
        private readonly int _maxIterations;

        public int MaxIterations
        {
            get { return _maxIterations; }
        }

        private Strainer _strainer;

        /// <summary>
        /// Environments
        /// </summary>
        public List<Hash> Environments { get; private set; }

        /// <summary>
        /// Scopes
        /// </summary>
        public List<Hash> Scopes { get; private set; }

        /// <summary>
        /// Hash of user-defined, internally-available variables
        /// </summary>
        public Hash Registers { get; private set; }

        /// <summary>
        /// Exceptions that have been raised during rendering
        /// </summary>
        public List<Exception> Errors { get; private set; }

        /// <summary>
        /// Creates a new rendering context
        /// </summary>
        /// <param name="environments"></param>
        /// <param name="outerScope"></param>
        /// <param name="registers"></param>
        /// <param name="errorsOutputMode"></param>
        /// <param name="maxIterations"></param>
        /// <param name="timeout"></param>
        /// <param name="formatProvider"></param>
        [Obsolete("The method with timeout argument is deprecated. Please use the one with CancellationToken.")]
        public Context
            (List<Hash> environments
             , Hash outerScope
             , Hash registers
             , ErrorsOutputMode errorsOutputMode
             , int maxIterations
             , int timeout
             , IFormatProvider formatProvider)
            : this(environments, outerScope, registers, errorsOutputMode, maxIterations, formatProvider, CancellationToken.None)
        {
            _timeout = timeout;
            RestartTimeout();
        }

        /// <summary>
        /// Creates a new rendering context
        /// </summary>
        /// <param name="environments"></param>
        /// <param name="outerScope"></param>
        /// <param name="registers"></param>
        /// <param name="errorsOutputMode"></param>
        /// <param name="maxIterations"></param>
        /// <param name="formatProvider"></param>
        /// <param name="cancellationToken"></param>
        public Context
            (List<Hash> environments
             , Hash outerScope
             , Hash registers
             , ErrorsOutputMode errorsOutputMode
             , int maxIterations
             , IFormatProvider formatProvider
             , CancellationToken cancellationToken)
        {
            Environments = environments;

            Scopes = new List<Hash>();
            if (outerScope != null)
                Scopes.Add(outerScope);

            Registers = registers;

            Errors = new List<Exception>();
            _errorsOutputMode = errorsOutputMode;
            _maxIterations = maxIterations;
            _cancellationToken = cancellationToken;
            FormatProvider = formatProvider;

            SquashInstanceAssignsWithEnvironments();
        }

        /// <summary>
        /// Creates a new rendering context
        /// </summary>
        public Context(IFormatProvider formatProvider)
            : this(new List<Hash>(), new Hash(), new Hash(), ErrorsOutputMode.Display, 0, 0, formatProvider )
        {
        }

        /// <summary>
        /// Strainer for the current context
        /// </summary>
        public Strainer Strainer
        {
            get { return (_strainer = _strainer ?? Strainer.Create(this)); }
        }

        /// <summary>
        /// Adds a filter from a function
        /// </summary>
        /// <typeparam name="TIn">Type of the parameter</typeparam>
        /// <typeparam name="TOut">Type of the returned value</typeparam>
        /// <param name="filterName">Filter name</param>
        /// <param name="func">Filter function</param>
        public void AddFilter<TIn, TOut>(string filterName, Func<TIn, TOut> func)
        {
            Strainer.AddFunction(filterName, func);
        }

        /// <summary>
        /// Adds a filter from a function
        /// </summary>
        /// <typeparam name="TIn">Type of the first parameter</typeparam>
        /// <typeparam name="TIn2">Type of the second paramter</typeparam>
        /// <typeparam name="TOut">Type of the returned value</typeparam>
        /// <param name="filterName">Filter name</param>
        /// <param name="func">Filter function</param>
        public void AddFilter<TIn, TIn2, TOut>(string filterName, Func<TIn, TIn2, TOut> func)
        {
            Strainer.AddFunction(filterName, func);
        }

        /// <summary>
        /// Adds filters to this context.
        /// this does not register the filters with the main Template object. see <tt>Template.register_filter</tt>
        /// for that
        /// </summary>
        /// <param name="filters"></param>
        public void AddFilters(IEnumerable<Type> filters)
        {
            foreach (Type f in filters)
                Strainer.Extend(f);
        }

        /// <summary>
        /// Add filters from a list of types
        /// </summary>
        /// <param name="filters"></param>
        public void AddFilters(params Type[] filters)
        {
            if (filters != null)
                AddFilters(filters.AsEnumerable());
        }

        /// <summary>
        /// Handles error during rendering
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public string HandleError(Exception ex)
        {
            if (ex is InterruptException || ex is TimeoutException || ex is RenderException || ex is OperationCanceledException)
            {
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            Errors.Add(ex);

            if (_errorsOutputMode == ErrorsOutputMode.Suppress)
                return string.Empty;

            if (_errorsOutputMode == ErrorsOutputMode.Rethrow)
            {
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            if (ex is SyntaxException)
            {
                return string.Format(Liquid.ResourceManager.GetString("ContextLiquidSyntaxError"), ex.Message);
            }

            return string.Format(Liquid.ResourceManager.GetString("ContextLiquidError"), ex.Message);
        }

        /// <summary>
        /// Invokes a strainer method
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object Invoke(string method, List<object> args)
        {
            if (Strainer.RespondTo(method))
            {
                return Strainer.Invoke(method, args);
            }

            return args.First();
        }

        /// <summary>
        /// Push new local scope on the stack. use <tt>Context#stack</tt> instead
        /// </summary>
        /// <param name="newScope"></param>
        public void Push(Hash newScope)
        {
            if (Scopes.Count > 80)
                throw new StackLevelException(Liquid.ResourceManager.GetString("ContextStackException"));

            Scopes.Insert(0, newScope);
        }

        /// <summary>
        /// Merge a hash of variables in the current local scope
        /// </summary>
        /// <param name="newScopes"></param>
        public void Merge(Hash newScopes)
        {
            Scopes[0].Merge(newScopes);
        }

        /// <summary>
        /// Pop from the stack. use <tt>Context#stack</tt> instead
        /// </summary>
        public Hash Pop()
        {
            if (Scopes.Count == 1)
                throw new ContextException();
            Hash result = Scopes[0];
            Scopes.RemoveAt(0);
            return result;
        }

        /// <summary>
        /// Pushes a new local scope on the stack, pops it at the end of the block
        ///
        /// Example:
        ///
        /// context.stack do
        /// context['var'] = 'hi'
        /// end
        /// context['var] #=> nil
        /// </summary>
        /// <param name="newScope"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Stack(Hash newScope, Action callback)
        {
            Push(newScope);
            try
            {
                callback();
            }
            finally
            {
                Pop();
            }
        }

        /// <summary>
        /// Pushes a new hash on the stack, pops it at the end of the block
        /// </summary>
        /// <param name="callback"></param>
        public void Stack(Action callback)
        {
            Stack(new Hash(), callback);
        }

        /// <summary>
        /// Clear the current instance assigns
        /// </summary>
        public void ClearInstanceAssigns()
        {
            Scopes[0].Clear();
        }

        /// <summary>
        /// Only allow String, Numeric, Hash, Array, Proc, Boolean or <tt>Liquid::Drop</tt>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="notifyNotFound">True to notify if variable is not found; Default true.</param>
        /// <returns></returns>
        public object this[string key, bool notifyNotFound = true]
        {
            get { return Resolve(key, notifyNotFound); }
            set { Scopes[0][key] = value; }
        }

        /// <summary>
        /// Checks if a variable key exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            return Resolve(key, false) != null;
        }

        /// <summary>
        /// Look up variable, either resolve directly after considering the name. We can directly handle
        /// Strings, digits, floats and booleans (true,false). If no match is made we lookup the variable in the current scope and
        /// later move up to the parent blocks to see if we can resolve the variable somewhere up the tree.
        /// Some special keywords return symbols. Those symbols are to be called on the rhs object in expressions
        ///
        /// Example:
        ///
        /// products == empty #=> products.empty?
        /// </summary>
        /// <param name="key"></param>
        /// <param name="notifyNotFound">True to notify if variable is not found; Default true.</param>
        /// <returns></returns>
        private object Resolve(string key, bool notifyNotFound = true)
        {
            switch (key)
            {
                case null:
                case "nil":
                case "null":
                case "":
                    return null;
                case "true":
                    return true;
                case "false":
                    return false;
                case "blank":
                case "empty":
                    return new Symbol(o => o is IEnumerable && !((IEnumerable)o).Cast<object>().Any());
            }

            // Single quoted strings.
            Match match = SingleQuotedRegex.Match(key);
            if (match.Success)
                return match.Groups[1].Value;

            // Double quoted strings.
            match = DoubleQuotedRegex.Match(key);
            if (match.Success)
                return match.Groups[1].Value;

            // Integer.
            match = IntegerRegex.Match(key);
            if (match.Success)
            {
                try
                {
                    return Convert.ToInt32(match.Groups[1].Value);
                }
                catch (OverflowException)
                {
                    return Convert.ToInt64(match.Groups[1].Value);
                }
            }

            // Ranges.
            match = RangeRegex.Match(key);
            if (match.Success)
                return Range.Inclusive(Convert.ToInt32(Resolve(match.Groups[1].Value)),
                    Convert.ToInt32(Resolve(match.Groups[2].Value)));

            // Floating point numbers.
            match = NumericRegex.Match(key);
            if (match.Success)
            {
                // For cultures with "," as the decimal separator, allow
                // both "," and "." to be used as the separator.
                // First try to parse using current culture.
                // If that fails, try to parse using invariant culture.
                // Also, first try higher precision decimal.
                // If that fails, try to parse as double (precision float).
                // Double is less precise but has a larger range.
                if (decimal.TryParse(match.Groups[1].Value, NumberStyles.Number | NumberStyles.Float, FormatProvider, out decimal parsedDecimalCurrentCulture))
                    return parsedDecimalCurrentCulture;
                if (decimal.TryParse(match.Groups[1].Value, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsedDecimalInvariantCulture))
                    return parsedDecimalInvariantCulture;
                if (double.TryParse(match.Groups[1].Value, NumberStyles.Number | NumberStyles.Float, FormatProvider, out double parsedDouble))
                    return parsedDouble;
                return double.Parse(match.Groups[1].Value, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            return Variable(key, notifyNotFound);
        }

        public IFormatProvider FormatProvider { get; }

        /// <summary>
        /// Fetches an object starting at the local scope and then moving up
        /// the hierarchy
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private object FindVariable(string key)
        {
            Hash scope = Scopes.FirstOrDefault(s => s.ContainsKey(key));
            object variable = null;
            if (scope == null)
            {
                foreach (Hash e in Environments)
                    if ((variable = LookupAndEvaluate(e, key)) != null)
                    {
                        scope = e;
                        break;
                    }
            }
            scope = scope ?? Environments.LastOrDefault() ?? Scopes.Last();
            variable = variable ?? LookupAndEvaluate(scope, key);

            variable = Liquidize(variable);
            if (variable is IContextAware contextAwareVariable)
            {
                contextAwareVariable.Context = this;
            }
            return variable;
        }

        /// <summary>
        /// Resolves namespaced queries gracefully.
        ///
        /// Example
        ///
        /// @context['hash'] = {"name" => 'tobi'}
        /// assert_equal 'tobi', @context['hash.name']
        /// assert_equal 'tobi', @context['hash["name"]']
        /// </summary>
        /// <param name="markup"></param>
        /// <param name="notifyNotFound"></param>
        /// <returns></returns>
        private object Variable(string markup, bool notifyNotFound)
        {
            List<string> parts = R.Scan(markup, VariableParserRegex);

            // first item in list, if any
            string firstPart = parts.TryGetAtIndex(0);

            Match firstPartSquareBracketedMatch = SquareBracketedRegex.Match(firstPart);
            if (firstPartSquareBracketedMatch.Success)
                firstPart = Resolve(firstPartSquareBracketedMatch.Groups[1].Value).ToString();

            object @object;
            if ((@object = FindVariable(firstPart)) == null)
            {
                if (notifyNotFound)
                    Errors.Add(new VariableNotFoundException(string.Format(Liquid.ResourceManager.GetString("VariableNotFoundException"), markup)));
                return null;
            }

            // try to resolve the rest of the parts (starting from the second item in the list)
            for (int i = 1; i < parts.Count; ++i)
            {
                var forEachPart = parts[i];
                Match partSquareBracketedMatch = SquareBracketedRegex.Match(forEachPart);
                bool partResolved = partSquareBracketedMatch.Success;

                object part = forEachPart;
                if (partResolved)
                    part = Resolve(partSquareBracketedMatch.Groups[1].Value);

                // If object is a KeyValuePair, we treat it a bit differently - we might be rendering
                // an included template.
                if (IsKeyValuePair(@object) && (part.SafeTypeInsensitiveEqual(0L) || part.Equals("Key")))
                {
                    object res = @object.GetType().GetRuntimeProperty("Key").GetValue(@object);
                    @object = Liquidize(res);
                }
                // If object is a hash- or array-like object we look for the
                // presence of the key and if its available we return it
                else if (IsKeyValuePair(@object) && (part.SafeTypeInsensitiveEqual(1L) || part.Equals("Value")))
                {
                    // If its a proc we will replace the entry with the proc
                    object res = @object.GetType().GetRuntimeProperty("Value").GetValue(@object);
                    @object = Liquidize(res);
                }
                // No key was present with the desired value and it wasn't one of the directly supported
                // keywords either. The only thing we got left is to return nil
                else
                {
                    // If object is a KeyValuePair, we treat it a bit differently - we might be rendering
                    // an included template.
                    if (@object is KeyValuePair<string, object> && ((KeyValuePair<string, object>)@object).Key == (string)part)
                    {
                        object res = ((KeyValuePair<string, object>)@object).Value;
                        @object = Liquidize(res);
                    }
                    // If object is a hash- or array-like object we look for the
                    // presence of the key and if its available we return it
                    else if (IsHashOrArrayLikeObject(@object, part))
                    {
                        // If its a proc we will replace the entry with the proc
                        object res = LookupAndEvaluate(@object, part);
                        @object = Liquidize(res);
                    }
                    // Some special cases. If the part wasn't in square brackets and
                    // no key with the same name was found we interpret following calls
                    // as commands and call them on the current object
                    else if (!partResolved && (@object is IEnumerable) && (Template.NamingConvention.OperatorEquals(part as string, "size") || Template.NamingConvention.OperatorEquals(part as string, "first") || Template.NamingConvention.OperatorEquals(part as string, "last")))
                    {
                        var castCollection = ((IEnumerable)@object).Cast<object>();
                        if (Template.NamingConvention.OperatorEquals(part as string, "size"))
                            @object = castCollection.Count();
                        else if (Template.NamingConvention.OperatorEquals(part as string, "first"))
                        {
                            object res = castCollection.FirstOrDefault();
                            @object = Liquidize(res);
                        }
                        else if (Template.NamingConvention.OperatorEquals(part as string, "last"))
                        {
                            object res = castCollection.LastOrDefault();
                            @object = Liquidize(res);
                        }
                    }
                    // No key was present with the desired value and it wasn't one of the directly supported
                    // keywords either. The only thing we got left is to return nil
                    else
                    {
                        Errors.Add(new VariableNotFoundException(string.Format(Liquid.ResourceManager.GetString("VariableNotFoundException"), markup)));
                        return null;
                    }
                }

                // If we are dealing with a drop here we have to
                if (@object is IContextAware contextAwareObject)
                {
                    contextAwareObject.Context = this;
                }
            }

            return @object;
        }

        private static bool IsHashOrArrayLikeObject(object obj, object part)
        {
            if (obj == null)
                return false;

            if ((obj is IDictionary && ((IDictionary)obj).Contains(part)))
                return true;

            if ((obj is IList) && (part is int || part is long))
                return true;

            if (TypeUtility.IsAnonymousType(obj.GetType()) && obj.GetType().GetRuntimeProperty((string)part) != null)
                return true;

            if ((obj is IIndexable) && ((IIndexable)obj).ContainsKey(part))
                return true;

            return false;
        }

        private object LookupAndEvaluate(object obj, object key)
        {
            object value;
            if (obj is IDictionary dictionaryObj)
            { 
                value = dictionaryObj[key];
            }
            else if (obj is IList listObj)
            { 
                value = listObj[Convert.ToInt32(key)];
            }
            else if (TypeUtility.IsAnonymousType(obj.GetType()))
            { 
                value = obj.GetType().GetRuntimeProperty((string)key).GetValue(obj, null);
            }
            else if (obj is IIndexable indexableObj)
            { 
                value = indexableObj[key];
            }
            else
            { 
                throw new NotSupportedException();
            }

            if (value is Proc procValue)
            {
                object newValue = procValue.Invoke(this);
                if (obj is IDictionary dicObj)
                {
                    dicObj[key] = newValue;
                }
                else if (obj is IList listObj)
                {
                    listObj[Convert.ToInt32(key)] = newValue;
                }
                else if (TypeUtility.IsAnonymousType(obj.GetType()))
                { 
                    obj.GetType().GetRuntimeProperty((string)key).SetValue(obj, newValue, null);
                }
                else
                { 
                    throw new NotSupportedException();
                }
                return newValue;
            }

            return value;
        }

        private static object Liquidize(object obj)
        {
            if (obj == null)
            {
                return obj;
            }
            if (obj is ILiquidizable liquidizableObj)
            {
                return liquidizableObj.ToLiquid();
            }
            if (obj is string)
            {
                return obj;
            }
            if (obj is IEnumerable)
            {
                return obj;
            }
            if (obj.GetType().GetTypeInfo().IsPrimitive)
            {
                return obj;
            }
            if (obj is decimal)
            {
                return obj;
            }
            if (obj is DateTime)
            {
                return obj;
            }
            if (obj is DateTimeOffset)
            {
                return obj;
            }
            if (obj is TimeSpan)
            {
                return obj;
            }
            if (obj is Guid)
            {
                return obj;
            }
            if (TypeUtility.IsAnonymousType(obj.GetType()))
            {
                return obj;
            }

            var safeTypeTransformer = Template.GetSafeTypeTransformer(obj.GetType());
            if (safeTypeTransformer != null)
            {
                return safeTypeTransformer(obj);
            }

            if (obj.GetType().GetTypeInfo().GetCustomAttributes(typeof(LiquidTypeAttribute), false).Any())
            {
                var attr = (LiquidTypeAttribute) obj.GetType().GetTypeInfo().GetCustomAttributes(typeof(LiquidTypeAttribute), false).First();
                return new DropProxy(obj, attr.AllowedMembers);
            }

            if (IsKeyValuePair(obj))
            {
                return obj;
            }

            throw new SyntaxException(Liquid.ResourceManager.GetString("ContextObjectInvalidException"), obj.ToString());
        }

        private static bool IsKeyValuePair(object obj)
        {
            if (obj != null)
            {
                Type valueType = obj.GetType();
                if (valueType.GetTypeInfo().IsGenericType)
                {
                    Type baseType = valueType.GetGenericTypeDefinition();
                    if (baseType == typeof(KeyValuePair<,>))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void SquashInstanceAssignsWithEnvironments()
        {
            Dictionary<string, object> tempAssigns = new Dictionary<string, object>(Template.NamingConvention.StringComparer);

            Hash lastScope = Scopes.Last();
            foreach (string k in lastScope.Keys)
                foreach (Hash env in Environments)
                    if (env.ContainsKey(k))
                    {
                        tempAssigns[k] = LookupAndEvaluate(env, k);
                        break;
                    }

            foreach (string k in tempAssigns.Keys)
                lastScope[k] = tempAssigns[k];
        }

        private readonly int _timeout;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public void RestartTimeout()
        {
            _stopwatch.Restart();
            _cancellationToken.ThrowIfCancellationRequested();
        }

        public void CheckTimeout()
        {
            if (_timeout > 0 && _stopwatch.ElapsedMilliseconds > _timeout)
            {
                throw new TimeoutException();
            }

            _cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
