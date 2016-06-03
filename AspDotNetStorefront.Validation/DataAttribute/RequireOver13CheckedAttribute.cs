// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Validation.DataAttribute
{
	public class RequireOver13CheckedAttribute : RequiredAttribute, IDynamicRequiredAttribute, IClientValidatable
	{

		public override bool IsValid(object value)
		{
			if(!(value is bool))
				return true;

			return !IsRequired() || (bool)value;
		}

		public bool IsRequired(object value = null)
		{
			return AppLogic.AppConfigBool("RequireOver13Checked") && !Customer.Current.IsOver13;
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			// The default implementation doesn't return a custom error message
			if(metadata.IsRequired)
				return new[] 
				{
					new ModelClientValidationRequiredRule(ErrorMessage)
					{
						ValidationType = "requirechecked"
					}
				};

			return Enumerable.Empty<ModelClientValidationRequiredRule>();
		}
	}

}
