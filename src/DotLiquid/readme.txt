------------------------------------------------
---------- DotLiquid Breaking Changes ----------
------------------------------------------------

6/16/2017 - Capital letters are allowed in conditional operators, but they must have the correct capitalization to work in the template.
eg: {% if "bob" startswith "b" %} vs {% if "bob" StartsWith "b" %} or {% if "bob" starts_with "b" %}