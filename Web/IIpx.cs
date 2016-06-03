// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ServiceModel;

namespace AspDotNetStorefront
{
	[ServiceContract(
		Namespace = "http://www.aspdotnetstorefront.com/",
		SessionMode = SessionMode.Allowed)]
	public interface IIpx
	{
		[OperationContract(
			Action = "http://www.aspdotnetstorefront.com/DoItUsernamePwd",
			ReplyAction = "http://www.aspdotnetstorefront.com/DoItUsernamePwd")]
		string DoItUsernamePwd(string AuthenticationEMail, string AuthenticationPassword, string XmlInputRequestString);

		[OperationContract(
			Action = "http://www.aspdotnetstorefront.com/DoIt",
			ReplyAction = "http://www.aspdotnetstorefront.com/DoIt")]
		string DoIt(string AuthenticationEMail, string AuthenticationPassword, string XmlInputRequestString);
	}
}
