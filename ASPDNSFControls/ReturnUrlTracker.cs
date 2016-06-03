// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AspDotNetStorefrontControls
{
	public class ReturnUrlTracker : Control, IPostBackEventHandler
	{
		public string DefaultReturnUrl
		{ get; set; }

		[TypeConverter(typeof(StringArrayConverter))]
		public string[] Blacklist
		{ get; set; }

		readonly HiddenField ReturnUrlField;
		Uri ControlStateReturnUrl;

		public ReturnUrlTracker()
		{
			Blacklist = new string[0];

			ReturnUrlField = new HiddenField
				{
					ID = "ReturnUrl",
					ClientIDMode = ClientIDMode.Static,
				};
		}

		protected override void OnInit(EventArgs e)
		{
			Page.RegisterRequiresControlState(this);

			base.OnInit(e);

			Controls.Add(ReturnUrlField);
		}

		protected override void OnPreRender(EventArgs e)
		{
			Page.ClientScript.RegisterOnSubmitStatement(
				GetType(),
				"SetReturnUrl",
				@"$('#ReturnUrl').val($.sessionStorage.get('returnUrl'))");

			var nonDefaultReturnUrl = GetNonDefaultReturnUrl();
			if(nonDefaultReturnUrl == null)
				return;

			Page.ClientScript.RegisterStartupScript(
				GetType(),
				"SetSessionReturnUrl",
				String.Format(
					@"(function(returnUrl) {{
						$.sessionStorage.set('returnUrl', '{0}');
					}})({0});",
					JValue.FromObject(nonDefaultReturnUrl.ToString()).ToString(Formatting.None)),
				true);

			base.OnPreRender(e);
		}

		protected override object SaveControlState()
		{
			return new object[]
				{
					base.SaveControlState(),
					DefaultReturnUrl,
					GetNonDefaultReturnUrl(),
				};
		}

		protected override void LoadControlState(object savedState)
		{
			var state = (object[])savedState;
			if(state == null)
				return;

			base.LoadControlState(state[0]);
			DefaultReturnUrl = (string)state[1];
			ControlStateReturnUrl = (Uri)state[2];
		}

		Uri GetNonDefaultReturnUrl()
		{
			var referrerUrl = Page.Request.UrlReferrer;
			var currentUrl = Page.Request.Url;

			var returnUrl = GetReferrerReturnUrl(referrerUrl, currentUrl)
				?? ControlStateReturnUrl
				?? (String.IsNullOrWhiteSpace(ReturnUrlField.Value)
					? null
					: new Uri(ReturnUrlField.Value));

			if(returnUrl == null)
				return null;

			// Don't link back to the same page
			if(StringComparer.OrdinalIgnoreCase.Equals(returnUrl.LocalPath, currentUrl.LocalPath))
				return null;

			return IsBlacklisted(returnUrl)
				? null
				: returnUrl;
		}

		public string GetReturnUrl()
		{
			var returnUrl = GetNonDefaultReturnUrl();

			if(returnUrl != null)
				return returnUrl.ToString();
			else
				return DefaultReturnUrl;
		}

		public string GetHyperlinkReturnUrl()
		{
			return Page.ClientScript.GetPostBackClientHyperlink(this, "RedirectToReturnUrl");
		}

		Uri GetReferrerReturnUrl(Uri referrerUrl, Uri currentUrl)
		{
			if(Page.IsPostBack)
				return null;

			if(referrerUrl == null)
				return null;

			// The idea is that these URL's should be used to return to listing from an editor. To that end,
			// we perform some "validation" on the referrer.

			// Don't use a referrer if it's the same page
			if(StringComparer.OrdinalIgnoreCase.Equals(referrerUrl.LocalPath, currentUrl.LocalPath))
				return null;
	
			// Ensure we're linking back to the same host
			if(referrerUrl.Host != currentUrl.Host)
				return null;

			// All path segments must match except the last, i.e. don't link out of admin
			if(Math.Abs(referrerUrl.Segments.Length - currentUrl.Segments.Length) > 1)
				return null;

			for(var i = 0; i < referrerUrl.Segments.Length && i < currentUrl.Segments.Length; i++)
				if(!StringComparer.OrdinalIgnoreCase.Equals(referrerUrl.Segments[i], currentUrl.Segments[i]))
					if(referrerUrl.Segments.Length - i > 1 || currentUrl.Segments.Length - i > 1)
						return null;

			return referrerUrl;
		}

		bool IsBlacklisted(Uri targetUrl)
		{
			if(targetUrl == null)
				return false;

			var requestPath = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;

			var result = Blacklist
				.Select(url => VirtualPathUtility.Combine(requestPath, url))
				.Select(url => VirtualPathUtility.ToAbsolute(url))
				.Where(url => StringComparer.OrdinalIgnoreCase.Equals(url, targetUrl.AbsolutePath))
				.Any();

			return result;
		}

		public void RaisePostBackEvent(string eventArgument)
		{
			if(eventArgument != "RedirectToReturnUrl")
				return;

			Page.Response.Redirect(GetReturnUrl());
		}
	}
}
