namespace DotLiquid
{
    public static class ResourceManager
    {
        public const string AssignTagSyntaxException = "Syntax Error in 'assign' tag - Valid syntax: assign [var] = [source]";
        public const string BlankFileSystemDoesNotAllowIncludesException = "Error - This liquid context does not allow includes";
        public const string BlockTagAlreadyDefinedException = "Liquid Error - Block '{0}' already defined";
        public const string BlockTagNoElseException = "{0} tag does not expect else tag";
        public const string BlockTagNoEndException = "'end' is not a valid delimiter for {0} tags. Use {1}";
        public const string BlockTagNotClosedException = "{0} tag was never closed";
        public const string BlockTagNotTerminatedException = "Tag '{0}' was not properly terminated with regexp: {1}";
        public const string BlockTagSyntaxException = "Syntax Error in 'block' tag - Valid syntax: block [name]";
        public const string BlockUnknownTagException = "Unknown tag '{0}'";
        public const string BlockVariableNotTerminatedException = "Variable '{0}' was not properly terminated with regexp: {1}";
        public const string CaptureTagSyntaxException = "Syntax Error in 'capture' tag - Valid syntax: capture [var]";
        public const string CaseTagElseSyntaxException = "Syntax Error in 'case' tag - Valid else condition: {{% else %}} (no parameters)";
        public const string CaseTagSyntaxException = "Syntax Error in 'case' tag - Valid syntax: case [condition]";
        public const string CaseTagWhenSyntaxException = "Syntax Error in 'case' tag - Valid when condition: {{% when [condition] [or condition2...] %}}";
        public const string ConditionUnknownOperatorException = "Unknown operator {0}";
        public const string ContextLiquidError = "Liquid error: {0}";
        public const string ContextLiquidSyntaxError = "Liquid syntax error: {0}";
        public const string ContextObjectInvalidException = "Object '{0}' is invalid because it is neither a built-in type nor implements ILiquidizable";
        public const string ContextStackException = "Nesting too deep";
        public const string CycleTagSyntaxException = "Syntax Error in 'cycle' tag - Valid syntax: cycle [name :] var [, var2, var3 ...]";
        public const string DropWrongNamingConventionMessage = "Missing property. Did you mean '{0}'?";
        public const string ExtendsTagCanBeUsedOneException = "Liquid Error - 'extends' tag can be used only once";
        public const string ExtendsTagMustBeFirstTagException = "Liquid Error - 'extends' must be the first tag in an extending template";
        public const string ExtendsTagSyntaxException = "Syntax Error in 'extends' tag - Valid syntax: extends [template]";
        public const string ExtendsTagUnallowedTagsException = "Liquid Error - Only 'comment' and 'block' tags are allowed in an extending template";
        public const string ForTagSyntaxException = "Syntax Error in 'for' tag - Valid syntax: for [item] in [collection]";
        public const string IfTagSyntaxException = "Syntax Error in 'if' tag - Valid syntax: if [expression]";
        public const string IncludeTagSyntaxException = "Syntax Error in 'include' tag - Valid syntax: include [template]";
        public const string LocalFileSystemIllegalTemplateNameException = "Error - Illegal template name '{0}'";
        public const string LocalFileSystemIllegalTemplatePathException = "Error - Illegal template path '{0}'";
        public const string LocalFileSystemTemplateNotFoundException = "Error - No such template '{0}'";
        public const string StrainerFilterHasNoValueException = "Error - Filter '{0}' does not have a default value for '{1}' and no value was supplied";
        public const string TableRowTagSyntaxException = "Syntax Error in 'tablerow' tag - Valid syntax: tablerow [item] in [collection] cols=[number]";
        public const string VariableFilterNotFoundException = "Error - Filter '{0}' in '{1}' could not be found.";
        public const string WeakTableKeyNotFoundException = "key could not be found";
    }
}
