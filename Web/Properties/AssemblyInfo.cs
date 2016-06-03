// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Owin;

[assembly: AssemblyTitle("Web")]
[assembly: AssemblyDescription("AspDotNetStorefront Web Site")]
[assembly: AssemblyCompany("AspDotNetStorefront.com")]
[assembly: AssemblyProduct("AspDotNetStorefront Multistore")]
[assembly: AssemblyCopyright("Copyright © AspDotNetStorefront")]
[assembly: AssemblyVersion("10.0.0.0")]
[assembly: AssemblyFileVersion("10.0.0.0")]
[assembly: ComVisibleAttribute(false)]
[assembly: System.CLSCompliant(true)]

[assembly: OwinStartup(typeof(AspDotNetStorefront.Application.MvcApplication), "Owin_Start")]
