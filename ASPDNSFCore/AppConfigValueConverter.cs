// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Linq;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Converts strong types to and from AppConfig string values.
	/// </summary>
	public class AppConfigValueConverter
	{
		readonly TypeConverter StringConverter;

		public AppConfigValueConverter()
		{
			StringConverter = TypeDescriptor.GetConverter(typeof(string));
		}

		public T ConvertAppConfigValueToTypedValue<T>(string value)
		{
			var typedValue = GetTypedValue<T>(value);
			return typedValue is T
				? (T)typedValue
				: default(T);
		}

		public Tuple<AppConfigType, string> ConvertTypedValueToAppConfigValue<T>(T value)
		{
			var valueType = value.GetType();

			if(typeof(bool).IsAssignableFrom(valueType))
				return Tuple.Create(
					AppConfigType.boolean,
					value.ToString());

			if(typeof(int).IsAssignableFrom(valueType))
				return Tuple.Create(
					AppConfigType.integer,
					value.ToString());

			if(typeof(decimal).IsAssignableFrom(valueType))
				return Tuple.Create(
					AppConfigType.@decimal,
					value.ToString());

			if(typeof(double).IsAssignableFrom(valueType))
				return Tuple.Create(
					AppConfigType.@double,
					value.ToString());

			return Tuple.Create(
				AppConfigType.@string,
				value == null
					? string.Empty
					: value.ToString());
		}

		object GetTypedValue<T>(string value)
		{
			var type = typeof(T);

			if(typeof(Enum).IsAssignableFrom(type))
				return GetEnumTypedValue<T>(value);

			if(typeof(bool).IsAssignableFrom(type))
				return GetBooleanTypedValue(value);

			if(typeof(int).IsAssignableFrom(type))
				return GetGenericTypedValue<int>(value);

			if(typeof(decimal).IsAssignableFrom(type))
				return GetGenericTypedValue<decimal>(value);

			if(typeof(double).IsAssignableFrom(type))
				return GetGenericTypedValue<double>(value);

			return value;
		}

		object GetEnumTypedValue<T>(string value)
		{
			var type = typeof(T);

			var valueExists = Enum
				.GetNames(type)
				.Contains(value, StringComparer.OrdinalIgnoreCase);

			if(!valueExists)
				return null;

			return Enum.Parse(type, value, ignoreCase: true);
		}

		bool GetBooleanTypedValue(string value)
		{
			return new[]
				{ "true", "yes", "1" }
				.Contains(
					value,
					StringComparer.OrdinalIgnoreCase);
		}

		T GetGenericTypedValue<T>(string value)
		{
			if(!StringConverter.CanConvertTo(typeof(T)))
				return default(T);

			if(value == null)
				return default(T);

			return (T)StringConverter.ConvertTo(value, typeof(T));
		}
	}
}
