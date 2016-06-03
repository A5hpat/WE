// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace AspDotNetStorefront.Validation.DataAttribute
{
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public class WhatsThisAttribute : Attribute
	{
		public readonly string Content;
		public readonly string Label;
		public readonly string Title;

		public WhatsThisAttribute(string content, string label = null, string title = null)
		{
			Content = content;
			Label = label;
			Title = title;
		}
	}
}
