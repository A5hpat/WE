// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AspDotNetStorefrontCore
{
    class Paging
    {
        #region Public Static Methods
        public static String GetAllPagesOldFormat(String BaseURL, int PageNum, int NumPages, Customer ThisCustomer)
        {
            if (NumPages < 2)
                return String.Empty;

            BaseURL = EnforceBaseURL(BaseURL);
            PageNum = EnforcePageNum(PageNum);
            String Separator = GetSeperator(BaseURL);

            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("Page: ");

            if (PageNum > 1)
                tmpS.Append(GetPreviousPageLink(BaseURL, PageNum, Separator));

            for (int CurrentPage = 1; CurrentPage <= NumPages; CurrentPage++)
                tmpS.Append(GetPageLink(BaseURL, Separator, CurrentPage));

            if (PageNum < NumPages)
                tmpS.Append(GetNextPageLink(BaseURL, PageNum, Separator));

            return tmpS.ToString();
        }
        public static String GetPagedPages(String BaseURL, int PageNum, int NumPages, Customer ThisCustomer)
        {
            if (NumPages < 2) return String.Empty;

            StringBuilder tmpS = new StringBuilder(4096);
            int pagesForward = AppLogic.AppConfigNativeInt("Paging.PagesForward");
            int pagesBackward = AppLogic.AppConfigNativeInt("Paging.PagesBackward");
            int firstShownPage = CommonLogic.IIF(PageNum - pagesBackward > 0, PageNum - pagesBackward, 1);
            int lastShownPage = CommonLogic.IIF(PageNum + pagesForward < NumPages, PageNum + pagesForward, NumPages);
            BaseURL = EnforceBaseURL(BaseURL);
            PageNum = EnforcePageNum(PageNum);
            String Separator = GetSeperator(BaseURL);
            tmpS.Append("<ul class=\"pagination\">");

            if (PageNum > 1)
            {
                tmpS.Append("<li class=\"pager-back\">");
                tmpS.Append(GetPreviousPageLink(BaseURL, PageNum, Separator));
                tmpS.Append("</li>");
            }
            else
            {
                tmpS.Append("<li class=\"page-between\"><span class=\"disabled\">&laquo;</span></li>");
            }

            tmpS.Append("<li>");
            if (firstShownPage > 1)
                tmpS.Append(GetPageLink(BaseURL, Separator, 1));
            tmpS.Append("</li>");

            if (firstShownPage > 2)
                tmpS.Append("<li class=\"paging-ellipses\"><span>&hellip;</a></span>");

            for (int i = firstShownPage; i <= lastShownPage; i++)
            {
                tmpS.Append((i == PageNum) ? "<li class=\"page-link active\">" : "<li class=\"page-link\">");
                tmpS.Append(GetPageLink(BaseURL, Separator, i));
                tmpS.Append("</li>");
            }

            if (lastShownPage < NumPages - 1)
                tmpS.Append("<li class=\"paging-ellipses\"><span>&hellip;</span></li>");

            tmpS.Append("<li>");
            if (lastShownPage < NumPages)
                tmpS.Append(GetPageLink(BaseURL, Separator, NumPages));
            tmpS.Append("</li>");

            if (PageNum < NumPages)
            {
                tmpS.Append("<li>");
                tmpS.Append(GetNextPageLink(BaseURL, PageNum, Separator));
                tmpS.Append("</li>");
            }
            else
            {
                tmpS.Append("<li class=\"pager-forward\"><span class=\"disabled\">&raquo;</span></li>");
            }

            tmpS.Append("</ul>");
            return tmpS.ToString();
        }
        #endregion
        #region Private Static Methods
        private static String GetNextPageLink(String BaseURL, int PageNum, String Separator)
        {
            StringBuilder tmpS = new StringBuilder();
            if (BaseURL.IndexOf("pagenum=", StringComparison.InvariantCultureIgnoreCase) == -1)
                tmpS.Append("<a href=\"" + BaseURL + Separator + "pagenum=" + Convert.ToString(PageNum + 1) + "\">");
            else
                tmpS.Append("<a href=\"" + Regex.Replace(BaseURL, @"pagenum=\w*", "pagenum=" + Convert.ToString(PageNum + 1), RegexOptions.Compiled) + "\">");
            tmpS.Append("&raquo;");
            tmpS.Append("</a>");
            return tmpS.ToString();
        }
        private static String GetPageLink(String BaseURL, String Separator, int PageToDisplay)
        {
            StringBuilder tmpS = new StringBuilder();
            tmpS.Append("<a class=\"page-number\" href=\"");
            if (BaseURL.IndexOf("pagenum=", StringComparison.InvariantCultureIgnoreCase) == -1)
                tmpS.Append(BaseURL + Separator + "pagenum=" + PageToDisplay.ToString());
            else
                tmpS.Append(Regex.Replace(BaseURL, @"pagenum=\w*", "pagenum=" + PageToDisplay.ToString(), RegexOptions.Compiled));
            tmpS.Append("\">" + PageToDisplay.ToString() + "</a>");
            return tmpS.ToString();
        }
        private static String GetPreviousPageLink(String BaseURL, int PageNum, String Separator)
        {
            StringBuilder tmpS = new StringBuilder();
            if (BaseURL.IndexOf("pagenum=", StringComparison.InvariantCultureIgnoreCase) == -1)
                tmpS.Append("<a href=\"" + BaseURL + Separator + "pagenum=" + Convert.ToString(PageNum - 1) + "\">");
            else
                tmpS.Append("<a href=\"" + Regex.Replace(BaseURL, @"pagenum=\w*", "pagenum=" + Convert.ToString(PageNum - 1), RegexOptions.Compiled) + "\">");
            tmpS.Append("&laquo;");
            tmpS.Append("</a>");
            return tmpS.ToString();
        }
        private static String GetSeperator(String BaseURL)
        {
            return CommonLogic.IIF(BaseURL.IndexOf("?") != -1, "&", "?");
        }
        private static int EnforcePageNum(int PageNum)
        {
            if (PageNum == 0)
                PageNum = CommonLogic.QueryStringUSInt("PageNum");
            if (PageNum == 0)
                PageNum = 1;

            return PageNum;
        }
        private static String EnforceBaseURL(String baseURL)
        {
            if (string.IsNullOrEmpty(baseURL))
				baseURL = CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING");

            return baseURL;
        }
        #endregion
    }
}
