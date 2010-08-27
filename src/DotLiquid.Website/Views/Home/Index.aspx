<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div id="content">
		<h1>What is DotLiquid?</h1>
		<p>DotLiquid is a templating system ported to the .net framework from 
		Ruby’s <a href="http://www.liquidmarkup.org">Liquid Markup</a>. <blockquote>It’s <strong>easy</strong> to learn, <strong>fast</strong> and <strong>safe</strong>.</blockquote> You can have 
		your users build their own templates without affecting your server security in any way.</p>
		<h2>Requirements</h2>
		<p>.NET Framework 4.0</p>
	</div>
	<div id="side">
		<h1>What does it look like?</h1>
		<div class="sample-code">
			<p>...</p>
			<p>&lt;p&gt;<span class="code">{{ user.name }}</span> has to do:&lt;/p&gt;</p>
			<p>&lt;ul&gt;</p>
			<p>&nbsp;&nbsp;&nbsp;&nbsp;<span class="code">{% for item in user.tasks %}</span></p>
			<p>&nbsp;&nbsp;&nbsp;&nbsp;&lt;li&gt;<span class="code">{{ item.name }}</span>&lt;/li&gt;</p>
			<p>&nbsp;&nbsp;&nbsp;&nbsp;<span class="code">{% endfor %}</span></p>
			<p>&lt;/ul&gt;</p>
			<p>...</p>
		</div>
		<h2>Get started</h2>
		<p>Grab the current version from <a href="http://github.com/formosatek/dotliquid/downloads">here</a>
		or get the latest bits from <a href="http://github.com/formosatek/dotliquid">github</a> and compile it.
		<br/>
		<br/>
		Read our <a href="http://github.com/formosatek/dotliquid/wiki">documentation</a>.
		</p>
	</div>
</asp:Content>