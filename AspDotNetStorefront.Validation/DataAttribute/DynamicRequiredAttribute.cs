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
	public interface IDynamicRequiredAttribute
	{
		bool IsRequired(object value = null);
	}

	public class RequiredIfAppConfigTrueAttribute : RequiredAttribute, IDynamicRequiredAttribute, IClientValidatable
	{
		readonly string AppConfigName;

		public RequiredIfAppConfigTrueAttribute(string appConfigName)
		{
			AppConfigName = appConfigName;
		}

		public override bool IsValid(object value)
		{
			return !IsRequired() || value != null;
		}

		public bool IsRequired(object value = null)
		{
			return AppLogic.AppConfigBool(AppConfigName);
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			// The default implementation doesn't return a custom error message
			if(metadata.IsRequired)
				return new[] { new ModelClientValidationRequiredRule(ErrorMessage) };

			return Enumerable.Empty<ModelClientValidationRequiredRule>();
		}
	}

	public class RequiredIfAppConfigFalseAttribute : RequiredAttribute, IDynamicRequiredAttribute, IClientValidatable
	{
		readonly string AppConfigName;

		public RequiredIfAppConfigFalseAttribute(string appConfigName)
		{
			AppConfigName = appConfigName;
		}

		public override bool IsValid(object value)
		{
			return !IsRequired() || value != null;
		}

		public bool IsRequired(object value = null)
		{
			return !AppLogic.AppConfigBool(AppConfigName);
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			// The default implementation doesn't return a custom error message
			if(metadata.IsRequired)
				return new[] { new ModelClientValidationRequiredRule(ErrorMessage) };

			return Enumerable.Empty<ModelClientValidationRequiredRule>();
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class RequiredIfContextValidAttribute : RequiredAttribute, IDynamicRequiredAttribute, IClientValidatable
	{
		readonly IContextValidationPlugin ContextValidationPlugin;

		public RequiredIfContextValidAttribute(Type contextValidationPluginType)
		{
			ContextValidationPlugin = Activator.CreateInstance(contextValidationPluginType) as IContextValidationPlugin;
			if(ContextValidationPlugin == null)
				throw new InvalidOperationException("contextValidationPluginType must be of type IContextValidationPlugin");
		}

		public override bool IsValid(object value)
		{
			return !IsRequired(value);
		}

		public bool IsRequired(object value = null)
		{
			return ContextValidationPlugin.IsRequired(value);
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			// The default implementation doesn't return a custom error message
			if(metadata.IsRequired)
				return new[] { new ModelClientValidationRequiredRule(ErrorMessage) };

			return Enumerable.Empty<ModelClientValidationRequiredRule>();
		}
	}

	public class RequiredTrueIfAppConfigTrueAttribute : RequiredAttribute, IDynamicRequiredAttribute, IClientValidatable
	{
		readonly string AppConfigName;

		public RequiredTrueIfAppConfigTrueAttribute(string appConfigName)
		{
			AppConfigName = appConfigName;
		}

		public override bool IsValid(object value)
		{
			if(!(value is bool))
				return true;

			return !IsRequired() || (bool)value;
		}

		public bool IsRequired(object value = null)
		{
			return AppLogic.AppConfigBool(AppConfigName);
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

	public interface IContextValidationPlugin
	{
		bool IsRequired(object value);
	}
}
