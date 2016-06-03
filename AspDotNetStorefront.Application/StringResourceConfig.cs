// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefront.StringResource;
using AspDotNetStorefront.Validation;

namespace AspDotNetStorefront.Application
{
	public static class StringResourceConfig
	{
		public static void Configure()
		{
			var existingProviders = ModelValidatorProviders
				.Providers
				.OfType<DataAnnotationsModelValidatorProvider>()
				.ToArray();

			foreach(var provider in existingProviders)
				ModelValidatorProviders.Providers.Remove(provider);

			ModelValidatorProviders.Providers.Add(new StringResourceWrappedModelValidatorProvider(new StringResourceProviderFactory()));
		}
	}
}
