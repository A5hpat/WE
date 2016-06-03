// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;

namespace AspDotNetStorefrontCore.Tokens
{
	[Serializable]
	public class TokenException : Exception
	{
		public TokenException() { }

		public TokenException(string message) : base(message) { }

		public TokenException(string message, Exception inner) : base(message, inner) { }

		protected TokenException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
	}

	[Serializable]
	public class TokenParameterMissingException : TokenException
	{
		public string ParameterName
		{ get; protected set; }

		public TokenParameterMissingException(string parameterName)
		{
			ParameterName = parameterName;
		}

		public TokenParameterMissingException(string parameterName, string message) : 
			base(message)
		{
			ParameterName = parameterName;
		}

		public TokenParameterMissingException(string parameterName, string message, Exception inner) 
			: base(message, inner)
		{
			ParameterName = parameterName;
		}

		protected TokenParameterMissingException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
	}
}
