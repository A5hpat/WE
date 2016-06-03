// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using Microsoft.Win32;

namespace AspDotNetStorefrontCommon
{
	/// <summary>
	/// An useful class to read/write/delete/count registry keys
	/// </summary>
	public class WindowsRegistry
	{
		private string m_SubKey = String.Empty;
		private RegistryKey m_BaseRegistryKey = Registry.LocalMachine;

		public WindowsRegistry(String KeyLocation)
		{
			m_SubKey = KeyLocation;
		}

		/// <summary>
		/// To read a registry key.
		/// input: KeyName (string)
		/// output: value (string) 
		/// </summary>
		public string Read(string KeyName)
		{
			// Opening the registry key
			RegistryKey rk = m_BaseRegistryKey ;
			// Open a subKey as read-only
			RegistryKey sk1 = rk.OpenSubKey(m_SubKey);
			// If the RegistrySubKey doesn't exist -> (null)
			if ( sk1 == null )
			{
				return null;
			}
			else
			{
				try 
				{
					// If the RegistryKey exists I get its value
					// or null is returned.
					return (string)sk1.GetValue(KeyName.ToUpperInvariant());
				}
				catch (Exception e)
				{
					return null;
				}
			}
		}
	}
}
