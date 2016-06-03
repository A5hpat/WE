// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AspDotNetStorefrontCore
{
	public interface ISkinProvider
	{
		Skin GetSkinById(int id);
		string GetSkinNameById(int id);
		Skin GetSkinByName(string name);
		int GetSkinIdByName(string name);
		IEnumerable<Skin> GetSkins();
	}

	public class SkinProvider : ISkinProvider
	{
		public const string DefaultSkinName = "Default";
		static Dictionary<string, int> SkinIdCache;
		readonly MD5 Md5;

		public SkinProvider() 
		{
			Md5 = MD5CryptoServiceProvider.Create();
		}

		public void Initialize()
		{
			SkinIdCache = new Dictionary<string, int>();
			var skinDirectories = Directory
				.GetDirectories(HostingEnvironment.MapPath("~/Skins/"), "*");

			// Get the right id for the skin
			foreach(var skinDirectory in skinDirectories)
			{
				var skinName = Path.GetFileName(skinDirectory);
				// Validate the skin name
				if(skinName.Contains('&') || skinName.Contains('?') || skinName.Contains('+'))
					throw new Exception("Your skin name cannot contain the &, ?, or + character");

				int skinId;

				// If the xml file has an id specified use it
				var dataFilePath = HostingEnvironment.MapPath(String.Format("~/Skins/{0}/SkinInfo/skininfo.xml", skinName));
				if(File.Exists(dataFilePath))
				{
					var xdocument = XDocument.Load(dataFilePath);

					var id = (int?)xdocument.XPathSelectElement("/Skin/Id");
					if(id != null)
					{
						SkinIdCache.Add(skinName, id.Value);
						continue;
					}
				}
				// If the id is specified in the name of the skins (backwards compatibility).
				if(skinName.StartsWith("Skin_"))
				{
					var id = skinName.Replace("Skin_", string.Empty);
					if(int.TryParse(id, out skinId))
					{
						SkinIdCache.Add(skinName, skinId);
						continue;
					}
							
				}
				// Otherwise create a hash of the name to use it as the id
				skinId = HashNameToId(skinName);
				SkinIdCache.Add(skinName, skinId);
			}

			//Make sure there is at least one skin
			if(!SkinIdCache.Any())
				throw new Exception("You do not have any skins so the site cannot run.");

			// Make sure that the ids are all unique
			var duplicateIds = SkinIdCache
				.GroupBy(o => o.Value)
				.Where(o => o.Count() > 1)
				.Select(y => y.Key);

			if(duplicateIds.Any())
				throw new Exception(String.Format("You have two or more skins with the same id of {0}. To fix this edit the <Id> node of the ~/Skins/{{SkinName}}/SkinInfo/skininfo.xml file", duplicateIds.FirstOrDefault()));

		}

		public Skin GetSkinById(int id)
		{
			return GetSkinByName(GetSkinNameById(id));
		}

		public string GetSkinNameById(int id)
		{
			var name = SkinIdCache
				.Where(skinInfo => skinInfo.Value == id)
				.Select(skinInfo => skinInfo.Key)
				.FirstOrDefault();

			if(name != null)
				return name;

			// If we don't have a skin for the given id give the default skin
			if(SkinIdCache.ContainsKey(DefaultSkinName))
				return DefaultSkinName;

			// If we don't have a skin for the given id and we don't have a default skin get the skin with the id of 1.
			var skin1Name = SkinIdCache
				.Where(skinInfo => skinInfo.Value == 1)
				.Select(skinInfo => skinInfo.Key)
				.FirstOrDefault();
			if(skin1Name != null)
				return skin1Name;

			// If we don't have the skinid they are looking for or a default get the first skin
			return SkinIdCache
				.Select(skinInfo => skinInfo.Key)
				.FirstOrDefault();
		}

		public int GetSkinIdByName(string name)
		{
			var id = SkinIdCache
				.Where(skinInfo => skinInfo.Key.EqualsIgnoreCase(name))
				.Select(skinInfo => skinInfo.Value)
				.FirstOrDefault();

			if(id != 0)
				return id;

			// If we don't have a skin for the given name give the default skin id
			if(SkinIdCache.ContainsKey(DefaultSkinName))
				return GetSkinByName(DefaultSkinName).Id;

			// If we don't have a skin for the given name and we don't have a default skin get the skin with the id of 1.
			var skin1Name = SkinIdCache
				.Where(skinInfo => skinInfo.Value == 1)
				.Select(skinInfo => skinInfo.Key)
				.FirstOrDefault();
			if(skin1Name != null)
				return GetSkinByName(skin1Name).Id;

			// If we don't have the skinid they are looking for or a default get the first skin
			return SkinIdCache
				.Select(skinInfo => skinInfo.Value)
				.FirstOrDefault();
		}

		public Skin GetSkinByName(string name)
		{
			var id = SkinIdCache[name];

			//get skin data from the data file if we have one.
			var dataFilePath = HostingEnvironment.MapPath(String.Format("~/Skins/{0}/SkinInfo/skininfo.xml", name));
			string displayName = null;
			string description = null;
			var isMobile = false;
			if(File.Exists(dataFilePath))
			{
				var skinDataFile = File.ReadAllText(dataFilePath);
				var xElement = XElement.Parse(skinDataFile);
				displayName = xElement.XPathSelectElement("/DisplayName") == null ? String.Empty : xElement.XPathSelectElement("/DisplayName").Value;
				description = xElement.XPathSelectElement("/Description") == null ? String.Empty : xElement.XPathSelectElement("/Description").Value;
				isMobile = xElement.XPathSelectElement("/MobileOnly") == null ? false : (xElement.XPathSelectElement("/MobileOnly").Value.ToLower() == "true");
			}

			//get the preview image
			string previewUrl = null;
			if(File.Exists(HostingEnvironment.MapPath(String.Format("~/Skins/{0}/SkinInfo/preview.jpg", name))))
				previewUrl = String.Format("~/Skins/{0}/SkinInfo/preview.jpg", name);
			else if(File.Exists(HostingEnvironment.MapPath(String.Format("~/Skins/{0}/SkinInfo/preview.png", name))))
				previewUrl = String.Format("~/Skins/{0}/SkinInfo/preview.png", name);
			else if(File.Exists(HostingEnvironment.MapPath(String.Format("~/Skins/{0}/SkinInfo/preview.gif", name))))
				previewUrl = String.Format("~/Skins/{0}/SkinInfo/preview.gif", name);

			return new Skin(id, name, displayName, description, previewUrl, isMobile);
		}

		public IEnumerable<Skin> GetSkins()
		{
			return SkinIdCache
				.Select(skinInfo => GetSkinByName(skinInfo.Key));
		}

		int HashNameToId(string name)
		{
			var encodedName = Encoding.UTF8.GetBytes(name);
			var hash = Md5.ComputeHash(encodedName);
			var number = BitConverter.ToInt32(hash, 0);
			return Math.Abs(number);
		}

		internal bool SkinIdIsValid(int skinId)
		{
			return SkinIdCache.ContainsValue(skinId);
		}
	}
}
