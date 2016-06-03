// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;

namespace AspDotNetStorefrontCore.Tokens
{
	/// <summary>
	/// A limited-functionality wrapper around HtmlHelper to allow its usage in tokens handling.
	/// </summary>
	public class TokenHtmlHelper : HtmlHelper
	{
		public TokenHtmlHelper(RequestContext requestContext)
			: base(new ViewContext(
				new ControllerContext(
					requestContext,
					new TokenStubController()),
				new TokenStubView(),
				new ViewDataDictionary(),
				new TempDataDictionary(),
				TextWriter.Null),
			new ViewPage())
		{ }

		/// <summary>
		/// A non-functional stub controller to allow using HtmlHelper in token handling.
		/// </summary>
		class TokenStubController : ControllerBase
		{
			protected override void ExecuteCore()
			{ throw new NotImplementedException("This class just a stub and can not be executed."); }
		}

		/// <summary>
		/// A non-functional stub view to allow using HtmlHelper in token handling.
		/// </summary>
		class TokenStubView : IView
		{
			public void Render(ViewContext viewContext, TextWriter writer)
			{ throw new NotImplementedException("This class is just a stub and can not be rendered."); }
		}
	}
}
