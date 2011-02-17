# DotLiquid

### What is this?

DotLiquid is a .NET 3.5 port of the popular [Ruby Liquid templating language](http://www.liquidmarkup.org). It is a separate project that aims to retain the same template syntax as the original, while using .NET coding conventions where possible.

For more information about the original Liquid project, see <http://www.liquidmarkup.org>.

### Quick start

1. Download the latest release from the [downloads page](http://github.com/formosatek/dotliquid/downloads).
   The zip file contains DotLiquid.dll, which is the only one you need.
2. Read the [wiki](http://github.com/formosatek/dotliquid/wiki) for information on writing and using
   DotLiquid templates.

### Why should I use DotLiquid?

* You want to allow your users to edit their own page templates, but want to ensure they don't run insecure code.
* You want to render templates directly from the database
* You want a template engine for emails

### What does it look like?

	<ul id="products">
		{% for product in products %}
			<li>
				<h2>{{product.name}}</h2>
				Only {{product.price | price }}

				{{product.description | prettyprint | paragraph }}
			</li>
		{% endfor %}
	</ul>

### How to use DotLiquid

DotLiquid supports a very simple API based around the DotLiquid.Template class. Generally, you can read the contents of a file into a template, and then render the template by passing it parameters in the form of a `Hash` object. There are several ways you can construct a `Hash` object, including from a Dictionary, or using the `Hash.FromAnonymousObject` method.

	Template template = Template.Parse("hi {{name}}"); // Parses and compiles the template
	template.Render(Hash.FromAnonymousObject(new { name = "tobi" })); // => "hi tobi" 