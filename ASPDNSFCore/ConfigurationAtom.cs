// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace AspDotNetStorefrontCore
{
	public interface IConfigurationAtom
	{
		IEnumerable<AppConfigAtomInfo> AtomAppConfigs { get; }
		string HTMLHeader { get; }
		string Title { get; }
		IEnumerable<string> ConfigurationErrors(int storeId);
		bool IsConfigured(int storeId);
	}

	public class ConfigurationAtom : IConfigurationAtom
	{
		private const string XmlDirectory = "ConfigurationAtoms/";
		public IEnumerable<AppConfigAtomInfo> AtomAppConfigs { get; protected set; }
		public string HTMLHeader { get; protected set; }
		public string Title { get; protected set; }

		public ConfigurationAtom(string xmlFileName)
		{
			string xmlFilePath = CommonLogic.SafeMapPath(String.Format("{0}/{1}{2}", AppLogic.GetAdminDir(), XmlDirectory, xmlFileName));
			using(XmlTextReader reader = new XmlTextReader(xmlFilePath))
			{
				reader.WhitespaceHandling = WhitespaceHandling.None;
				var doc = new XmlDocument();
				doc.Load(reader);
				reader.Close();
				InitFromXML(doc);
			}
		}

		public ConfigurationAtom(XmlDocument xmlDoc)
		{
			InitFromXML(xmlDoc);
		}

		private void InitFromXML(XmlDocument xmlDoc)
		{
			var headerNode = xmlDoc.SelectSingleNode("/ConfigurationAtom/HtmlHeader");
			var header = string.Empty;
			if(headerNode != null && !string.IsNullOrEmpty(headerNode.InnerText))
				header = headerNode.InnerText;

			var titleNode = xmlDoc.SelectSingleNode("/ConfigurationAtom/Title");
			var title = string.Empty;
			if(titleNode != null && !string.IsNullOrEmpty(titleNode.InnerText))
				title = titleNode.InnerText;

			var configs = new List<AppConfigAtomInfo>();
			foreach(XmlNode acNode in xmlDoc.SelectNodes("/ConfigurationAtom/AppConfig"))
			{
				string name = null;

				var nameNode = acNode.SelectSingleNode("Name");
				if(nameNode == null || string.IsNullOrEmpty(nameNode.InnerText))
					throw new ArgumentException("Name node invalid.");

				name = nameNode.InnerText;

				var appConfig = AppConfigManager.GetAppConfig(nameNode.InnerText);
				bool hasCreateInfo = false;
				if(appConfig == null)
				{
					var createNode = acNode.SelectSingleNode("CreateValues");
					if(createNode != null)
					{
						var description = string.Empty;
						var defaultValue = string.Empty;
						var groupName = "CUSTOM";
						bool superOnly = false;
						var valueType = "string";
						var allowableValues = new List<string>();

						var descriptionNode = createNode.SelectSingleNode("Description");
						if(descriptionNode != null && !string.IsNullOrEmpty(descriptionNode.InnerText))
							description = descriptionNode.InnerText;

						var defaultVauleNode = createNode.SelectSingleNode("DefaultValue");
						if(defaultVauleNode != null && !string.IsNullOrEmpty(defaultVauleNode.InnerText))
							defaultValue = defaultVauleNode.InnerText;

						var groupNameNode = createNode.SelectSingleNode("GroupName");
						if(groupNameNode != null && !string.IsNullOrEmpty(groupNameNode.InnerText))
							groupName = groupNameNode.InnerText.ToUpper();

						var superOnlyNode = createNode.SelectSingleNode("SuperOnly");
						if(superOnlyNode != null && !string.IsNullOrEmpty(superOnlyNode.InnerText))
							superOnly = superOnlyNode.InnerText.ToBool();

						var valueTypeNode = createNode.SelectSingleNode("ValueType");
						if(valueTypeNode != null && !string.IsNullOrEmpty(valueTypeNode.InnerText))
							valueType = valueTypeNode.InnerText;

						var allowableValuesNode = createNode.SelectSingleNode("AllowableValues");
						if(allowableValuesNode != null && !string.IsNullOrEmpty(allowableValuesNode.InnerText))
							allowableValues = allowableValuesNode.InnerText.Split(',').ToList();

						appConfig = new AppConfig(
							appConfigId: 0,
							appConfigGuid: Guid.NewGuid(),
							storeId: 0,
							name: name,
							description: description,
							configValue: defaultValue,
							groupName: groupName,
							superOnly: superOnly,
							createdOn: DateTime.Now,
							updatedOn: DateTime.Now,
							valueType: valueType,
							allowableValues: allowableValues,
							hidden: false);
						hasCreateInfo = true;
					}
				}

				var appConfigAtomInfo = new AppConfigAtomInfo(appConfig);
				appConfigAtomInfo.HasCreateInfo = hasCreateInfo;

				if(acNode.Attributes["Required"] != null)
					appConfigAtomInfo.IsRequired = acNode.Attributes["Required"].InnerText.ToBool();

				if(acNode.Attributes["Advanced"] != null)
					appConfigAtomInfo.IsAdvanced = acNode.Attributes["Advanced"].InnerText.ToBool();

				if(acNode.Attributes["FriendlyName"] != null)
					appConfigAtomInfo.FriendlyName = acNode.Attributes["FriendlyName"].InnerText;

				var contextualDescriptionNode = acNode.SelectSingleNode("ContextualDescription");
				if(contextualDescriptionNode != null && !string.IsNullOrEmpty(contextualDescriptionNode.InnerText))
					appConfigAtomInfo.ContextualDescription = contextualDescriptionNode.InnerText;

				configs.Add(appConfigAtomInfo);
			}

			this.Init(configs, header, title);
		}

		private void Init(IEnumerable<AppConfigAtomInfo> atomAppConfigs, string htmlHeader, string title)
		{
			this.AtomAppConfigs = atomAppConfigs;
			this.HTMLHeader = htmlHeader;
			this.Title = title;
			EnsureAppConfigsExist();
		}

		private void EnsureAppConfigsExist()
		{
			foreach(AppConfigAtomInfo acai in this.AtomAppConfigs)
			{
				if(!acai.HasCreateInfo || AppConfigManager.AppConfigExists(acai.Config.Name))
					break;

				AppConfigManager.AddAppConfig(
					name: acai.Config.Name,
					description: acai.Config.Description,
					groupName: acai.Config.GroupName,
					configValue: acai.Config.ConfigValue,
					valueType: acai.Config.ValueType,
					allowableValues: acai.Config.AllowableValues.ToDelimitedString(),
					storeId: acai.Config.StoreId,
					superOnly: acai.Config.SuperOnly);
			}
		}

		public bool IsConfigured(int storeId)
		{
			return ConfigurationErrors(storeId).Count() == 0;
		}

		public IEnumerable<string> ConfigurationErrors(int storeId)
		{
			var errors = new List<string>();
			foreach(AppConfigAtomInfo ac in AtomAppConfigs)
			{
				try
				{
					if(ac == null || ac.Config == null)
						continue;

					if(ac.IsRequired && string.IsNullOrEmpty(AppLogic.AppConfig(ac.Config.Name, storeId, true)))
						errors.Add("Please configure the app config " + ac.Config.Name + ".");
				}
				catch(Exception e)
				{
					SysLog.LogException(e, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
				}
			}
			return errors;
		}
	}

	public class SearchConfigurationAtom : IConfigurationAtom
	{
		#region IConfigurationAtom Members
		public IEnumerable<AppConfigAtomInfo> AtomAppConfigs { get; protected set; }
		public string HTMLHeader { get; protected set; }
		public string Title { get; protected set; }
		#endregion

		public SearchConfigurationAtom(string searchTerm, string htmlHeader, string title)
		{
			var acinfo = new List<AppConfigAtomInfo>();
			IEnumerable<AppConfig> acs = AppConfigManager.GetAppConfigs(0).Where(appConfig => appConfig.Name.ContainsIgnoreCase(searchTerm));

			foreach(AppConfig ac in acs)
				acinfo.Add(new AppConfigAtomInfo(ac));

			this.AtomAppConfigs = acinfo;
			this.HTMLHeader = htmlHeader;
			this.Title = title;
		}

		public IEnumerable<string> ConfigurationErrors(int StoreId)
		{
			return new List<string>();
		}

		public bool IsConfigured(int storeId)
		{
			return true;
		}
	}

	public class AppConfigAtomInfo
	{
		public AppConfig Config { get; protected set; }
		public bool IsRequired { get; set; }
		public bool IsAdvanced { get; set; }
		public string ContextualDescription { get; set; }
		public string FriendlyName { get; set; }
		public bool HasCreateInfo { get; set; }

		public AppConfigAtomInfo(AppConfig config) : this(config, false, false) { }
		public AppConfigAtomInfo(AppConfig config, bool isRequired, bool isAdvanced) : this(config, isAdvanced, isRequired, null) { }
		public AppConfigAtomInfo(AppConfig config, bool isRequired, bool isAdvanced, string contextualDescription) : this(config, isRequired, isAdvanced, contextualDescription, null) { }
		public AppConfigAtomInfo(AppConfig config, bool isRequired, bool isAdvanced, string contextualDescription, string friendlyName)
		{
			this.Config = config;
			this.IsRequired = isRequired;
			this.IsAdvanced = IsAdvanced;
			this.ContextualDescription = contextualDescription;
			this.FriendlyName = friendlyName;
		}
	}

}
