// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.CodeDom;
using System.Linq;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Web.UI;

namespace AspDotNetStorefrontCore.Tokens
{
	[ExpressionPrefix("Tokens")]
	public class TokenExpressionBuilder : ExpressionBuilder
	{
		public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			// Generate a CodeDOM expression to invoke the below static method
			return new CodeMethodInvokeExpression(
				new CodeTypeReferenceExpression(GetType()),     // What type to invoke the method off of (this class)
				"EvaluateTokenString",                          // The name of the method to invoke
				new CodePrimitiveExpression(entry.Expression)); // The string parameter
		}

		// This method must be public and static so the above CodeMethodInvokeExpression can find it
		public static object EvaluateTokenString(string tokenString)
		{
			var parameters = (tokenString ?? string.Empty)
				.ParseAsDelimitedList()
				.ToArray();

			return DependencyResolver
				.Current
				.GetService<TokenExecutor>()
				.GetTokenValue(
					AppLogic.GetCurrentCustomer(),
					parameters.FirstOrDefault(),
					parameters.Skip(1))
				?? string.Empty;
		}
	}
}
