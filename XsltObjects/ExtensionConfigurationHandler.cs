// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using System.Xml;

namespace XsltObjects
{
	/// <summary>
	/// Loads extension configuration from web.config.
	/// </summary>
	public class ExtensionConfigurationHandler : IConfigurationSectionHandler
	{
		public static ExtensionConfiguration GetExtensionConfiguration(string extension)
		{
			return (ExtensionConfiguration)WebConfigurationManager.GetSection("system.web/" + extension);
		}

		public object Create(object parent, object configContext, XmlNode sectionNode)
		{
			return new ExtensionConfiguration(
				extensions: GetExtensions(sectionNode),
				defaultExtension: GetDefaultExtension(sectionNode));
		}

		Dictionary<string, Extension> GetExtensions(XmlNode sectionNode)
		{
			// Flatten out all of the action nodes. They must be evaluated in the order they're declared in.
			var actionNodes = sectionNode
				.SelectNodes("extensions")
				.OfType<XmlNode>()
				.SelectMany(extensionsNode => extensionsNode
					.ChildNodes
					.OfType<XmlNode>());

			// Create a dictionary of extensions and apply the mutations.
			var extensions = new Dictionary<string, Extension>();
			foreach(var actionNode in actionNodes)
				if(StringComparer.InvariantCultureIgnoreCase.Equals("add", actionNode.Name))
					extensions.Add(actionNode.Attributes["name"].Value, CreateExtension(actionNode));
				else if(StringComparer.InvariantCultureIgnoreCase.Equals("remove", actionNode.Name))
					extensions.Remove(actionNode.Attributes["name"].Value);
				else if(StringComparer.InvariantCultureIgnoreCase.Equals("clear", actionNode.Name))
					extensions.Clear();

			return extensions;
		}

		Extension CreateExtension(XmlNode actionNode)
		{
			return new Extension(
				name: actionNode.Attributes["name"].Value,
				type: actionNode.Attributes["type"].Value,
				attributes: actionNode.Attributes
				.OfType<XmlAttribute>()
				.Where(attribute => attribute.Name != "name" && attribute.Name != "type")
				.ToDictionary(
					attribute => attribute.Name,
					attribute => attribute.Value));
		}

		string GetDefaultExtension(XmlNode sectionNode)
		{
			return sectionNode.Attributes["defaultExtension"] != null
				? sectionNode.Attributes["defaultExtension"].Value
				: string.Empty;
		}
	}
}
