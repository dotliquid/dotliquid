namespace DotLiquid.PerformanceTest
{
    public static class TestTemplates
    {
        public const string BasicTemplate = @"
<div>
<p><b>
{% if user.name == 'Steve Lillis' -%}
  Welcome back
{% else -%}
  I don't know you!
{% endif -%}
</b></p>
{% unless user.name == 'Steve Thompson' -%}
  <i>Unless example</i>
{% endunless -%}
{% comment %}A comment for comments sake{% endcomment %}
<ul>
<li>This entry and something about baked goods</li>
<li>
{% assign handle = 'cake' -%}
{% case handle -%}
  {% when 'cake' -%}
     This is a cake
  {% when 'cookie' -%}
     This is a cookie
  {% else -%}
     This is not a cake nor a cookie
{% endcase -%}
</li>
</ul>
</div>
<p>{{ user.name | upcase }} has the following items:</p>
<table>
{% for item in user.items -%}
  <tr>
     <td>
        {% cycle 'one', 'two', 'three' %}
     </td>
     <td>
        {{ item.description }} 
        {% assign handle = 'cake' -%}
        {% case handle -%}
          {% when 'cake' -%}
             This is a cake
          {% when 'cookie' -%}
             This is a cookie
          {% else -%}
             This is not a cake nor a cookie
        {% endcase -%}
     </td>
     <td>
        {{ item.cost }}
     </td>
  </tr>
{% endfor -%}
{% for item in user.items reversed -%}
  <tr>
     <td>e
        {% cycle 'one', 'two', 'three' %}
     </td>
     <td>
        {% if item.description == 'First Item' -%}
            {{ item.description | upcase }}
        {% else %}
            {{ item.description }}
        {% endif %}
     </td>
     <td>
        {{ item.cost }}
     </td>
  </tr>
{% endfor -%}
</table>";

        public const string AdvancedTemplate = @"
{% for x in (1..5) %}
<h1>Tests all except filters</h1>
Also doesn't use INCLUDE or EXTENDS, to be tested later
<div>
<h2>Variable Tags</h3>
<h3>Assign</h3>
{% assign handle = 'cake' -%}
{{ handle }}
<h3>Capture</h3>
{% capture my_variable %}I am being captured.{% endcapture -%}
{{ my_variable }}
</div>
<div>
<h2>Control Flow Tags</h2>
<h3>Case (non-else)</h3>
{% case handle -%}
  {% when 'cake' -%}
     This is a cake
  {% when 'cookie' -%}
     This is a cookie
  {% else -%}
     This is not a cake nor a cookie
{% endcase -%}
<h3>Case (else)</h3>
{% case handle -%}
  {% when 'a' -%}
     This is a cake
  {% when 'b' -%}
     This is a cookie
  {% else -%}
     The else statement was reached
{% endcase -%}
<h3>If equals (non-else)</h3>
{% if user.name == 'Steve Jackson' -%}
  Equals failed on match
{% elsif user.name == 'Steve Lillis' -%}
  Equals was a success
{% else -%}
  Equals failed to else
{% endif -%}
<h3>If not equals (non-else)</h3>
{% if user.name != 'Steve Jackson' -%}
  Not equals was a success
{% else -%}
  Not equals failed
{% endif -%}
<h3>If (else)</h3>
{% if user.name == 'Steve Jackson' -%}
  Unexpected user
{% else -%}
  Else body reached
{% endif -%}
<h3>Unless</h3>
{% unless user.name == 'Steve Jackson' -%}
  Unless worked
{% else -%}
  Unless failed
{% endunless -%}
</div>
<div>
<h2>Iteration Tags</h2>
<h3>For (with cycle)</h3>
{% for item in user.items -%}
	{% cycle 'one', 'two', 'three' %}: {{ item.description }} 
{% endfor -%}
<h3>For (reversed)</h3>
{% for item in user.items reversed -%}
	{% cycle 'one', 'two', 'three' -%}: {% if item.description == 'First Item' -%} 
		{{ item.description | upcase -}} 
	{% else -%} 
		{{ item.description -}} 
	{% endif -%}
{% endfor -%}
<h3>For (Limit: 2)</h3>
{% for item in user.items limit:2 -%}
	{% cycle 'one', 'two', 'three' %}: {{ item.description }} 
{% endfor -%}
<h3>For (Offset: 2)</h3>
{% for item in user.items offset:2 -%}
	{% cycle 'one', 'two', 'three' %}: {{ item.description }} 
{% endfor -%}
<h3>For Range</h3>
{% for i in (1..4) -%}{{ i -}},
{% endfor -%}
<h3>For Range (Continue on 2)</h3>
{% for i in (1..4) -%} {% if i == 2 %} {% continue %} {% endif %} {{ i -}},
{% endfor -%}
<h3>For Range (Break on 2)</h3>
{% for i in (1..4) -%} {% if i == 2 %} {% break %} {% endif %} {{ i -}},
{% endfor -%}
<h3>Table Row (Cols:2, Limit:4)</h3>
<table>
{% tablerow item in user.items cols:2 limit:4 %}
  {{ item.Description }}
  {{ item.Cost }}
{% endtablerow %}
</table>
</div>
{% endfor %}
";
    }
}
