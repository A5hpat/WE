// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspDotNetStorefrontCore.Tokens.Handlers
{
	public class ImageUrl : IParameterizedTokenHandler
	{
		const string IdParam = "id";
		const string TypeParam = "type";
		const string SizeParam = "size";

		readonly string[] Tokens = { "imageurl" };
		readonly string[] ParameterNames = { IdParam, TypeParam, SizeParam };

		readonly TokenParameterConverter TokenParameterConverter;

		public ImageUrl(TokenParameterConverter tokenParameterConverter)
		{
			TokenParameterConverter = tokenParameterConverter;
		}

		public string RenderToken(TokenHandlerContext context)
		{
			if(!Tokens.Contains(context.Token, StringComparer.OrdinalIgnoreCase))
				return null;

			if(!context.Parameters.ContainsKey(IdParam) || string.IsNullOrWhiteSpace(context.Parameters[IdParam]))
				throw new TokenParameterMissingException(IdParam);

			if(!context.Parameters.ContainsKey(TypeParam) || string.IsNullOrWhiteSpace(context.Parameters[TypeParam]))
				throw new TokenParameterMissingException(TypeParam);

			if(!context.Parameters.ContainsKey(SizeParam) || string.IsNullOrWhiteSpace(context.Parameters[SizeParam]))
				throw new TokenParameterMissingException(SizeParam);

			int objectId;
			if(!int.TryParse(context.Parameters[IdParam], out objectId))
				throw new TokenException("The \"id\" parameter must be an integer.");

			return AppLogic.LookupImage(
				context.Parameters[TypeParam],
				objectId,
				context.Parameters[SizeParam],
				context.Customer.SkinID,
				context.Customer.LocaleSetting);
		}

		public IDictionary<string, string> ConvertPositionalParametersToNamedParameters(IEnumerable<string> positionalParameters)
		{
			return TokenParameterConverter.ConvertPositionalToNamedParameters(positionalParameters, ParameterNames);
		}
	}
}
