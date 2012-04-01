using System;

namespace DotLiquid
{
	/// <summary>
	/// Specifies the type is safe to be rendered by DotLiquid.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class LiquidTypeAttribute : Attribute
	{
		public string[] AllowedMembers { get; private set; }

		public LiquidTypeAttribute(params string[] allowedMembers)
		{
			AllowedMembers = allowedMembers;
		}
	}
}