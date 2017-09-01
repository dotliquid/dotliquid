------------------------------------------------
---------- DotLiquid Breaking Changes ----------
------------------------------------------------

2017-06-17

1. In order to fix compatibility issues from last version some modification have been made on historic operator keys in `Condition.Operators`: 
  - startswith -> startsWith
  - endswith -> endsWith

They can be used as before but if you manipulate them through their key, be sure to update your code to avoid any error.

2. For the same fix `INamingConvention` have now a new member: `bool OperatorEquals(string testedOperator, string referenceOperator)`
It is used in order to allow all operator to be compared through the naming convention specific:
  - ruby: snake_case e.g. starts_with
  - c#: pascalCase or PascalCase e.g; startsWith or StartsWith
Operators can still be used in lower case.

2017-06-16

Capital letters are allowed in conditional operators, but they must have the correct capitalization to work in the template.
eg: {% if "bob" startswith "b" %} vs {% if "bob" StartsWith "b" %} or {% if "bob" starts_with "b" %}