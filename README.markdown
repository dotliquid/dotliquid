# DotLiquid

[![DotLiquid tag on Stack Overflow](https://img.shields.io/badge/stackoverflow-dotliquid-orange.svg)](https://stackoverflow.com/questions/tagged/dotliquid)
[![AppVeyor build](https://ci.appveyor.com/api/projects/status/github/dotliquid/dotliquid?branch=master&svg=true)](https://ci.appveyor.com/project/tgjones/dotliquid)
[![TravisCI build](https://travis-ci.org/dotliquid/dotliquid.svg?branch=master)](https://travis-ci.org/dotliquid/dotliquid)
[![codecov](https://codecov.io/gh/dotliquid/dotliquid/branch/master/graph/badge.svg)](https://codecov.io/gh/dotliquid/dotliquid)
[![NuGet](https://img.shields.io/nuget/v/dotliquid.svg)](https://www.nuget.org/packages/dotliquid)
[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/dotliquid/dotliquid?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Maintainers wanted

Have you sent a PR to this repository? In that case, would you consider getting
in touch with me so I can give you commit access to this repository? Please ping
me at [gitter/dotliquid](https://gitter.im/dotliquid/dotliquid) or here on
github.

### What is this?

DotLiquid is a .Net port of the popular [Ruby Liquid templating
language](https://shopify.github.io/liquid/). It is a separate project that aims to
retain the same template syntax as the original, while using .NET coding
conventions where possible.

For more information about the original Liquid project, see
<https://shopify.github.io/liquid/>.

### Quick start

1. Download the latest release from the [downloads
   page](https://github.com/dotliquid/dotliquid/downloads) or [NuGet](https://www.nuget.org/packages/dotliquid).  The zip file
   contains DotLiquid.dll, which is the only one you need.
2. Read the [docs](//github.com/dotliquid/dotliquid/wiki) for information
   on writing and using DotLiquid templates.

### Why should I use DotLiquid?

* You want to leave business logic in your compiled controllers and out of your templates.
* You're looking for a logic-less template language that also exists for other platforms (ie: node, python).
* You want to allow your users to edit their own page templates, but want to
  ensure they don't run insecure code.
* You want to render templates directly from the database.
* You want a template engine for emails.

### What does it look like?

``` liquid
<ul id="products">
  {% for product in products %}
    <li>
      <h2>{{product.name}}</h2>
      Only {{product.price | price }}

      {{product.description | prettyprint | paragraph }}
    </li>
  {% endfor %}
</ul>
```

### How to use DotLiquid

DotLiquid supports a very simple API based around the DotLiquid.Template class.
Generally, you can read the contents of a file into a template, and then render
the template by passing it parameters in the form of a `Hash` object. There are
several ways you can construct a `Hash` object, including from a Dictionary, or
using the `Hash.FromAnonymousObject` method.

```c#
Template template = Template.Parse("hi {{name}}"); // Parses and compiles the template
template.Render(Hash.FromAnonymousObject(new { name = "tobi" })); // => "hi tobi"
```

### Projects using DotLiquid

Are you using DotLiquid in an open source project? Tell us with a PR!

 - [Suave.DotLiquid](https://github.com/SuaveIO/suave#introduction)
 - [Pretzel](https://github.com/Code52/Pretzel)
 - [Docfx](https://github.com/dotnet/docfx)
 - [DotLiquid.Mailer](https://github.com/miseeger/DotLiquid.Mailer)
 - [DotLiquid Template Engine for Suave.IO](https://www.nuget.org/packages/Suave.DotLiquid/)
 - [DotLiquid View Engine for ASP.NET MVC](https://www.nuget.org/packages/DotLiquid.ViewEngine)
 - [DotLiquid View Engine for Nancy](https://www.nuget.org/packages/Nancy.Viewengines.DotLiquid)
