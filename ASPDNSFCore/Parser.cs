// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AspDotNetStorefrontCore.Tokens;

namespace AspDotNetStorefrontCore
{
	public class Parser
	{
		readonly TokenExecutor TokenExecutor;

		public Parser()
		{
			TokenExecutor = DependencyResolver.Current.GetService<TokenExecutor>();
		}

		public string GetTokenValue(string tokenKey, Dictionary<string, string> parameters = null)
		{
			var customer = HttpContext.Current.GetCustomer();
			var value = TokenExecutor.GetTokenValue(customer, tokenKey, parameters);

			return value == null
				? string.Empty
				: value;
		}

		public string ReplaceTokens(string unparsedInput)
		{
			var customer = HttpContext.Current.GetCustomer();
			using(var writer = new StringWriter())
			{
				var mode = ParserMode.Literal;
				var context = new ParserContext();
				ResetParserContext(context);

				for(var index = 0; index < unparsedInput.Length; index++)
				{
					mode = ParseElement(unparsedInput[index], index, mode, customer, writer, context);

					// If parsing the token failed, we need to copy everything 
					// from the start of the token to the current point as a literal.
					if(mode == ParserMode.Invalid)
					{
						for(var replayIndex = context.TokenStartIndex; replayIndex <= index; replayIndex++)
							writer.Write(unparsedInput[replayIndex]);

						mode = ParserMode.Literal;
						ResetParserContext(context);
					}
				}

				// Consider an unclosed token a failure
				if(context.TokenStartIndex != 0)
					for(var replayIndex = context.TokenStartIndex; replayIndex < unparsedInput.Length; replayIndex++)
						writer.Write(unparsedInput[replayIndex]);

				writer.Flush();

				return writer.ToString();
			}
		}

		ParserMode ParseElement(char element, int index, ParserMode mode, Customer customer, StringWriter writer, ParserContext context)
		{
			// This is a _really_ simple parser for handling our token format. See the ParserMode for the various
			// parser states.

			// An "element" would be called a "token" in any other parser, but that term's already in use here.

			// Within this method:
			//	"continue" means switch modes but keep the current element.
			//	"return" means consume the current element and move to the next one.
			while(true)
			{
				switch(mode)
				{
					// Non-token text that is to be copied as-is.
					case ParserMode.Literal:
						if(element == '(')
						{
							mode = ParserMode.Open_1;
							continue;
						}
						else
						{
							writer.Write(element);
							return mode;
						}

					// The start of a token, first character
					case ParserMode.Open_1:
						if(element == '(')
						{
							context.TokenStartIndex = index;
							return ParserMode.Open_2;
						}
						else
						{
							return ParserMode.Invalid;
						}

					// The start of a token, second character
					case ParserMode.Open_2:
						if(element == '!')
						{
							return ParserMode.Name;
						}
						else
						{
							return ParserMode.Invalid;
						}


					// The name of the token
					case ParserMode.Name:
						if(char.IsWhiteSpace(element))
						{
							return ParserMode.AttributeKeyScan;
						}
						else if(element == '!')
						{
							mode = ParserMode.Close_1;
							continue;
						}
						else if(char.IsLetterOrDigit(element) || element == '_')
						{
							context.Name.Append(element);
							return mode;
						}
						else
						{
							return ParserMode.Invalid;
						}

					// Whitespace until an attribute key
					case ParserMode.AttributeKeyScan:
						if(char.IsWhiteSpace(element))
						{
							return mode;
						}
						else if(element == '!')
						{
							mode = ParserMode.Close_1;
							continue;
						}
						else if(char.IsLetterOrDigit(element) || element == '_')
						{
							mode = ParserMode.AttributeKey;
							continue;
						}
						else
						{
							return ParserMode.Invalid;
						}

					// An attribute key
					case ParserMode.AttributeKey:
						if(char.IsWhiteSpace(element) || element == '=')
						{
							mode = ParserMode.AttributeKeyValueSeparator;
							continue;
						}
						else if(element == '!')
						{
							mode = ParserMode.Close_1;
							continue;
						}
						else if(char.IsLetterOrDigit(element) || element == '_')
						{
							context.AttributeKey.Append(element);
							return mode;
						}
						else
						{
							return ParserMode.Invalid;
						}

					// The characters between an attribute key and value
					case ParserMode.AttributeKeyValueSeparator:
						if(char.IsWhiteSpace(element))
						{
							return mode;
						}
						else if(element == '=')
						{
							return ParserMode.AttributeValueWrap;
						}
						else
						{
							// Treat as a key with no value
							context.Attributes[context.AttributeKey.ToString()] = string.Empty;
							context.AttributeKey = new StringBuilder();
							mode = ParserMode.AttributeKeyScan;
							continue;
						}

					// An attribute value wrapped in quotes or apostrophes
					case ParserMode.AttributeValueWrap:
						if(char.IsWhiteSpace(element))
						{
							return mode;
						}
						else if(element == '"')
						{
							return ParserMode.AttributeValueQuoteWrapped;
						}
						else if(element == '\'')
						{
							return ParserMode.AttributeValueApostropheWrapped;
						}
						else
						{
							// Treat as a key with no value
							context.Attributes[context.AttributeKey.ToString()] = string.Empty;
							context.AttributeKey = new StringBuilder();
							mode = ParserMode.AttributeKeyScan;
							continue;
						}

					// An attribute value wrapped in quotes
					case ParserMode.AttributeValueQuoteWrapped:
						if(element == '"')
						{
							// Add the attribute key and value
							context.Attributes[context.AttributeKey.ToString()] = context.AttributeValue.ToString();
							context.AttributeKey = new StringBuilder();
							context.AttributeValue = new StringBuilder();
							return ParserMode.AttributeKeyScan;
						}
						else
						{
							context.AttributeValue.Append(element);
							return mode;
						}

					// An attribute value wrapped in apostrophes
					case ParserMode.AttributeValueApostropheWrapped:
						if(element == '\'')
						{
							// Add the attribute key and value
							context.Attributes[context.AttributeKey.ToString()] = context.AttributeValue.ToString();
							context.AttributeKey = new StringBuilder();
							context.AttributeValue = new StringBuilder();
							return ParserMode.AttributeKeyScan;
						}
						else
						{
							context.AttributeValue.Append(element);
							return mode;
						}

					// The end of a token, first character
					case ParserMode.Close_1:
						if(element == '!')
						{
							return ParserMode.Close_2;
						}
						else
						{
							return ParserMode.AttributeKeyScan;
						}

					// The end of a token, second character
					case ParserMode.Close_2:
						if(element == ')')
						{
							var tokenName = context.Name.ToString();
							if(string.IsNullOrWhiteSpace(tokenName))
								return ParserMode.Invalid;

							// Execute the token and write out the value
							var attributeValue = TokenExecutor.GetTokenValue(customer, tokenName, context.Attributes);
							writer.Write(attributeValue);

							ResetParserContext(context);

							return ParserMode.Literal;
						}
						else
						{
							return ParserMode.Invalid;
						}

					default:
						return ParserMode.Invalid;
				}
			}
		}

		void ResetParserContext(ParserContext context)
		{
			context.TokenStartIndex = 0;
			context.Name = new StringBuilder();
			context.Attributes = new Dictionary<string, string>();
			context.AttributeKey = new StringBuilder();
			context.AttributeValue = new StringBuilder();
		}

		enum ParserMode
		{
			Literal,
			Open_1,
			Open_2,
			Close_1,
			Close_2,
			Name,
			AttributeKeyScan,
			AttributeKey,
			AttributeKeyValueSeparator,
			AttributeValueWrap,
			AttributeValueQuoteWrapped,
			AttributeValueApostropheWrapped,
			Invalid,
		}

		class ParserContext
		{
			public int TokenStartIndex;
			public StringBuilder Name;
			public Dictionary<string, string> Attributes;
			public StringBuilder AttributeKey;
			public StringBuilder AttributeValue;
		}
	}
}
