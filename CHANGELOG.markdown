# Overview

_:exclamation: Changes are no longer maintained in this file. See (https://github.com/dotliquid/dotliquid/commits/master) for a full list of changes._

## 2.1.x - 2021-03-08

### New Features

* \#420 Introduced a new Compatibility flag to opt-in to breaking changes. These changes are described in the [Wiki](https://github.com/dotliquid/dotliquid/wiki/DotLiquid-Syntax-Compatibility#strict-liquid-syntax)

## 2.0.x - 2016-07-25

## 1.8.0 - 2014-05-13

### New Features

* \#82 Allow interfaces to be registered as safe types (Matt Brailsford)

* \#79 Implemented `DateTimeOffset` as a supported primitive type (Rodrigo Dumont)

* \#77 Added `EmbeddedFileSystem` to support loading includes from embedded resources (Rodrigo Dumont)

* \#72 Added `Template.RegisterValueTypeTransformer(Type type, Func<object, object> func)`. It can be used to
  override primitive-type rendering - i.e. to render custom strings for boolean values. (grexican)

* \#72 Added `Template.RegisterSafeType(Type type, string[] allowedMembers, Func<object, object> func)`. It acts as
  a combination of the existing `RegisterSafeType` methods. It allows a single type to be registered with both
  allowed members, and a transform function that is used as a post-filter after a variable of the specified type
  is rendered. (grexican)

* \#68 Implemented `Guid` as a supported primitive type

* \#64 Implemented ERB-like trimming for leading whitespace

* \#63 Added `startswith` and `endswith` conditions for arrays and strings (Dave Glick)

### Resolved Issues

* \#98 Assign tag now (once again) supports ILiquidizable objects

* \#80 Template path cannot contain round brackets

* \#66 Tablerow tag throws exception when specifying limit or offset attributes

* Decreased maximum scope count from 100 to 80, because .NET 3.5 threw a StackOverflowException
  before the maximum scope count was reached.

* \#61 Nested template inheritance not working for blocks defined above parent (Dave Glick)

* \#60 Nested template inheritance not correctly placing blocks (Dave Glick)

## 1.7.0 - 2012-08-03
* Fixed bug: Handle negative result from `GetHashCode` (Sam Listopad)

* `Drop` does not output itself (Alessandro Petrelli)

* Added `raw` tag

* Added `modulo` filter

* Allow filters in assign

* Added `%e` date format code (Paul Jenkins)

* Added `split` filter

* Fixed bug in ERB-like trimming (Alessandro Petrelli)

* Added `[LiquidType]` attribute. Use it to decorate a POCO type, and specify the list of allowed members
  (property or method names). If DotLiquid sees this attribute, it will treat the object as though it was a Drop
  with the specified members. (Benn Hoffman and Greg MacLellan)

* Added `Template.RegisterSafeType(Type type, string[] allowedMembers)`. Similar to `[LiquidType]`, it can be
  used if you can't / don't want to decorate a POCO with `[LiquidType]`. (Benn Hoffman / Greg MacLellan)

* Added `Template.RegisterSafeType(Type type, Func<object, object> func)`. Similar to the other `RegisterSafeType`
  overload, it can be used to transform an object into a "Liquidizable" type - i.e. an anonymous type or
  one that implements `ILiquidizable`, such as `Drop`. (Benn Hoffman / Greg MacLellan)

## 1.6.1 - 2011-09-30

* Updated `TypeUtility.IsAnonymousType` to detect anonymous types when running on Mono

* Removed `System.Web` reference for .NET 4.0 version because `System.Web.HttpUtility` can be replaced by `System.Net.WebUtility`

* Changed the overload of `Template.Render` that takes in a stream to use `TextWriter` instead of `StreamWriter`. This change
  will not require code changes in client code, but will require a recompilation.

* DotLiquid is now compatible with .NET 4.0 Client Profile

## 1.6.0 - 2011-06-25

* Added SymbolSource support to NuGet package generation script

* Planning to use semantic versioning for future releases (http://semver.org/)

## 1.5.6 - 2011-06-21

* ERB-like trimming now works correctly for `\r\n` newlines

## 1.5.4 - 2011-04-30

* Because it's a common (and easy) mistake to use C# property names while `Template.NamingConvention` is set to
  `RubyNamingConvention`, and this situation is both surprising and difficult to debug, I have added
  a "special case" error message when the code detects that the property name doesn't match, but would
  match if you used a Ruby-style name.

## 1.5.3 - 2011-04-05

* Minor fixes to error handling in Include tag; now returns (or throws, if `RenderParameters.RethrowErrors` is true)
  correct error message.

* Minor fixes to null template handling in `LocalFileSystem`

## 1.5.2 - 2011-03-19

* "Assign" tag now works correctly with European (i.e. comma) decimal separators.

* If "Assign" tag is used with a decimal (or float or double) value, and the decimal
  separator for the current culture is ",", then the code will first try parsing
  using "," and then fallback to the invariant culture's decimal separator (".").

## 1.5.1 - 2011-03-19

* Filters can now access the current context. To do this, modify your filter method(s) to take
  in a `Context` object as the first argument. DotLiquid will automatically pass the current
  context to your filter.

* Minor updates to website

## 1.5.0 - 2011-03-01

* Fix for `RubyNamingConvention` incorrectly handling full uppercase members.
  Let's do it the same way Ruby does!

* Dispose `MemoryStreamWriter` correctly

* BREAKING CHANGE: Refactored `Template.Render(*)` methods, because they were
  getting a little out of hand. I'm no longer using C# 4 optional parameters.
  Instead, I've simplified the primary (i.e. the one that returns a string) 
  Render method to three overloads:
  * `Render()`
  * `Render(Hash)`
  * `Render(RenderParameters)`
  
  That last one is where you can specify filters, registers, and whether to
  rethrow errors. If any of the `RenderParameters` values are null, then default
  values will be used, as before.
  
  For the `Render()` method that takes in a stream, I have not provided any
  overloads apart from the basic `Render(StreamWriter streamWriter,
  RenderParameters parameters)`, because I want to keep it as simple as possible
  and I don't think this version will be commonly used.

## 1.2.1 - 2011-02-12

* Signed DotLiquid assembly with strong name. DotLiquid.dll can now be placed in GAC.

* Created changelog.txt :)