// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace AspDotNetStorefrontCore
{
	public interface ISearchEngineNameProvider
	{
		string EnsureDatabaseObjectSeName(string type, int id);

		string GenerateSeName(string name);
	}

	public abstract class SearchEngineNameProviderBase : ISearchEngineNameProvider
	{
		public readonly HttpContextBase HttpContext;

		public SearchEngineNameProviderBase(HttpContextBase httpContext)
		{
			HttpContext = httpContext;
		}

		public virtual string EnsureDatabaseObjectSeName(string type, int id)
		{
			if(id == 0)
				return string.Empty;

			using(var connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				string name, seName;
				using(var command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText = string.Format("select Name, SEName from [{0}] with(nolock) where {0}ID = @id", type);
					command.Parameters.AddWithValue("id", id);

					using(var reader = command.ExecuteReader())
					{
						if(!reader.Read())
							return string.Empty;

						name = reader.FieldByLocale("Name", Localization.GetUSLocale()); // SENames are ALWAYS from U.S locale
						seName = reader.Field("SEName");
					}
				}

				if(!string.IsNullOrEmpty(seName))
					return CommonLogic.Left(GenerateSeName(seName), 90);

				// Update the SEName field since it's empty
				var newSeName = CommonLogic.Left(GenerateSeName(name), 90);
				using(var command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText = string.Format("update [{0}] set SEName = @seName where {0}ID = @id", type);
					command.Parameters.AddWithValue("seName", newSeName);
					command.Parameters.AddWithValue("id", id);
					command.ExecuteNonQuery();
				}

				return newSeName;
			}
		}

		public abstract string GenerateSeName(string entityName);

	}

	public class LegacySearchEngineNameProvider : SearchEngineNameProviderBase
	{
		const string AllowedSeNameCharacters = "abcdefghijklmnopqrstuvwxyz1234567890 _-";

		public LegacySearchEngineNameProvider(HttpContextBase httpContext)
			: base(httpContext)
		{ }

		public override string GenerateSeName(string entityName)
		{
			if(string.IsNullOrEmpty(entityName))
				return entityName ?? string.Empty;

			var cleanName = entityName
				.Trim()
				.ToLower();

			var seNameBuilder = new StringBuilder(cleanName.Length);
			for(var i = 0; i < cleanName.Length; i++)
				if(AllowedSeNameCharacters.IndexOf(cleanName[i]) != -1)
					seNameBuilder.Append(cleanName[i]);

			var seName = seNameBuilder
				.ToString()
				.TrimStart('-')		// Trim leading dashes
				.Replace(" ", "-");

			// Reduce repetitions
			while(seName.Contains("--"))
				seName = seName.Replace("--", "-");

			while(seName.Contains("__"))
				seName = seName.Replace("__", "_");

			// Trim trailing dashes
			seName = seName.TrimEnd('-');

			return HttpContext.Server.UrlEncode(seName);
		}
	}

	public class Utf8SearchEngineNameProvider : SearchEngineNameProviderBase
	{
		const char Separator = '-';
		readonly Regex MultipleSeparatorPattern;

		public Utf8SearchEngineNameProvider(HttpContextBase httpContext)
			: base(httpContext)
		{
			MultipleSeparatorPattern = new Regex(string.Format(@"{0}{{2,}}", Separator));
		}

		public override string GenerateSeName(string entityName)
		{
			return string.IsNullOrWhiteSpace(entityName)
				? string.Empty
				: MultipleSeparatorPattern
					.Replace(
						input: string.Concat(entityName
							.Trim()
							.ToLower()
							.ToCharArray()
							.Select(character => char.IsLetterOrDigit(character)
								? character
								: Separator)),
						replacement: Separator.ToString())
					.Trim(Separator);
		}
	}
}
