using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid
{
    public class Context
    {
        private static readonly Regex SingleQuotedRegex = R.C(R.Q(@"^'(.*)'$"));
        private static readonly Regex DoubleQuotedRegex = R.C(R.Q(@"^""(.*)""$"));
        private static readonly Regex IntegerRegex = R.C(R.Q(@"^([+-]?\d+)$"));
        private static readonly Regex RangeRegex = R.C(R.Q(@"^\((\S+)\.\.(\S+)\)$"));
        private static readonly Regex FloatRegex = R.C(R.Q(@"^([+-]?\d[\d\.|\,]+)$"));
        private static readonly Regex SquareBracketedRegex = R.C(R.Q(@"^\[(.*)\]$"));
        private static readonly Regex VariableParserRegex = R.C(Liquid.VariableParser);

        private readonly bool _rethrowErrors;
        private Strainer _strainer;

        public List<Hash> Environments { get; private set; }

        public List<Hash> Scopes { get; private set; }

        public Hash Registers { get; private set; }

        public List<Exception> Errors { get; private set; }

        public Context(List<Hash> environments, Hash outerScope, Hash registers, bool rethrowErrors)
        {
            Environments = environments;

            Scopes = new List<Hash>();
            if (outerScope != null)
                Scopes.Add(outerScope);

            Registers = registers;

            Errors = new List<Exception>();
            _rethrowErrors = rethrowErrors;
            SquashInstanceAssignsWithEnvironments();
        }

        public Context()
            : this(new List<Hash>(), new Hash(), new Hash(), false)
        {
        }

        public Strainer Strainer
        {
            get { return (_strainer = _strainer ?? Strainer.Create(this)); }
        }

        public void AddFilter<TIn, TOut>(string filterName, Func<TIn, TOut> func)
        {
            Strainer.AddFunction(filterName, func);
        }

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

        public void AddFilters(params Type[] filters)
        {
            if (filters != null)
                AddFilters(filters.AsEnumerable());
        }

        public string HandleError(Exception ex)
        {
            if (ex is InterruptException)
                throw ex;

            Errors.Add(ex);
            if (_rethrowErrors)
                throw ex;

            if (ex is SyntaxException)
                return string.Format(Liquid.ResourceManager.GetString("ContextLiquidSyntaxError"), ex.Message);
            return string.Format(Liquid.ResourceManager.GetString("ContextLiquidError"), ex.Message);
        }

        public object Invoke(string method, List<object> args)
        {
            if (Strainer.RespondTo(method))
                return Strainer.Invoke(method, args);

            return args.First();
            //throw new FilterNotFoundException("Filter not found: '{0}'", method);
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
        /// pushes a new local scope on the stack, pops it at the end of the block
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

        public void Stack(Action callback)
        {
            Stack(new Hash(), callback);
        }

        public void ClearInstanceAssigns()
        {
            Scopes[0].Clear();
        }

        /// <summary>
        /// Only allow String, Numeric, Hash, Array, Proc, Boolean or <tt>Liquid::Drop</tt>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this [string key, bool notifyNotFound = true]
        {
            get { return Resolve(key, notifyNotFound); }
            set { Scopes[0][key] = value; }
        }

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
        /// <param name="notifyNotFound"></param>
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
                    return new Symbol(o => o is IEnumerable && !((IEnumerable)o).Cast<object>().Any());
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
                return Convert.ToInt32(match.Groups[1].Value);

            // Ranges.
            match = RangeRegex.Match(key);
            if (match.Success)
                return Range.Inclusive(Convert.ToInt32(Resolve(match.Groups[1].Value)),
                    Convert.ToInt32(Resolve(match.Groups[2].Value)));

            // Floats.
            match = FloatRegex.Match(key);
            if (match.Success)
            {
                // For cultures with "," as the decimal separator, allow
                // both "," and "." to be used as the separator.
                // First try to parse using current culture.
                float result;
                if (float.TryParse(match.Groups[1].Value, out result))
                    return result;

                // If that fails, try to parse using invariant culture.
                return float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            }

            return Variable(key, notifyNotFound);
        }

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
            if (variable is IContextAware)
                ((IContextAware)variable).Context = this;
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
                else if (!partResolved && (@object is IEnumerable) && ((part as string) == "size" || (part as string) == "first" || (part as string) == "last"))
                {
                    var castCollection = ((IEnumerable)@object).Cast<object>();
                    if ((part as string) == "size")
                        @object = castCollection.Count();
                    else if ((part as string) == "first")
                        @object = castCollection.FirstOrDefault();
                    else if ((part as string) == "last")
                        @object = castCollection.LastOrDefault();
                }
                    // No key was present with the desired value and it wasn't one of the directly supported
                    // keywords either. The only thing we got left is to return nil
                else
                {
                    if (notifyNotFound)
                        Errors.Add(new VariableNotFoundException(string.Format(Liquid.ResourceManager.GetString("VariableNotFoundException"), markup)));
                    return null;
                }

                // If we are dealing with a drop here we have to
                if (@object is IContextAware)
                    ((IContextAware)@object).Context = this;
            }

            return @object;
        }

        private static bool IsHashOrArrayLikeObject(object obj, object part)
        {
            if (obj == null)
                return false;

            if ((obj is IDictionary && ((IDictionary)obj).Contains(part)))
                return true;

            if ((obj is IList) && (part is int))
                return true;

            if (TypeUtility.IsAnonymousType(obj.GetType()) && obj.GetType().GetRuntimeProperty((string)part) != null)
                return true;

            if ((obj is IIndexable) && ((IIndexable)obj).ContainsKey((string)part))
                return true;

            return false;
        }

        private object LookupAndEvaluate(object obj, object key)
        {
            object value;
            if (obj is IDictionary)
                value = ((IDictionary)obj)[key];
            else if (obj is IList)
                value = ((IList)obj)[(int)key];
            else if (TypeUtility.IsAnonymousType(obj.GetType()))
                value = obj.GetType().GetRuntimeProperty((string)key).GetValue(obj, null);
            else if (obj is IIndexable)
                value = ((IIndexable)obj)[key];
            else
                throw new NotSupportedException();

            if (value is Proc)
            {
                object newValue = ((Proc)value).Invoke(this);
                if (obj is IDictionary)
                    ((IDictionary)obj)[key] = newValue;
                else if (obj is IList)
                    ((IList)obj)[(int)key] = newValue;
                else if (TypeUtility.IsAnonymousType(obj.GetType()))
                    obj.GetType().GetRuntimeProperty((string)key).SetValue(obj, newValue, null);
                else
                    throw new NotSupportedException();
                return newValue;
            }

            return value;
        }

        private static object Liquidize(object obj)
        {
            if (obj == null)
                return obj;
            if (obj is ILiquidizable)
                return ((ILiquidizable)obj).ToLiquid();
            if (obj is string)
                return obj;
            if (obj is IEnumerable)
                return obj;
            if (obj.GetType().GetTypeInfo().IsPrimitive)
                return obj;
            if (obj is decimal)
                return obj;
            if (obj is DateTime)
                return obj;
            if (obj is DateTimeOffset)
                return obj;
            if (obj is TimeSpan)
                return obj;
            if (obj is Guid)
                return obj;
            if (TypeUtility.IsAnonymousType(obj.GetType()))
                return obj;
            if (obj is KeyValuePair<string, object>)
                return obj;

            var safeTypeTransformer = Template.GetSafeTypeTransformer(obj.GetType());
            if (safeTypeTransformer != null)
                return safeTypeTransformer(obj);

            if (obj.GetType().GetTypeInfo().GetCustomAttributes(typeof(LiquidTypeAttribute), false).Any())
            {
                var attr = (LiquidTypeAttribute)obj.GetType().GetTypeInfo().GetCustomAttributes(typeof(LiquidTypeAttribute), false).First();
                return new DropProxy(obj, attr.AllowedMembers);
            }

            throw new SyntaxException(Liquid.ResourceManager.GetString("ContextObjectInvalidException"), obj.ToString());
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
    }
}
