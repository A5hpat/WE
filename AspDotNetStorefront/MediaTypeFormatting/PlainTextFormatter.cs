// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AspDotNetStorefront.MediaTypeFormatting
{
	public class PlainTextFormatter : MediaTypeFormatter
	{
		readonly Encoding Encoding;

		public PlainTextFormatter(Encoding encoding = null)
		{
			Encoding = encoding ?? Encoding.Default;
			SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
		}

		public override bool CanReadType(Type type)
		{
			return type == typeof(string);
		}

		public override bool CanWriteType(Type type)
		{
			return type == typeof(string);
		}

		public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContent content, IFormatterLogger formatterLogger)
		{
			string result;
			using(var reader = new StreamReader(stream, Encoding))
				result = reader.ReadToEnd();

			return Task.FromResult<object>(result);
		}

		public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContent content, TransportContext transportContext)
		{
			using(var writer = new StreamWriter(stream, Encoding))
				writer.Write((string)value);

			return Task.FromResult<object>(null);
		}
	}
}
