using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Security.Principal;


namespace XenLauncher
{
    public class HelpersWeb
    {
        public static NameValueCollection ParseURLQuery()
        {
            Debug.WriteLine("Debug: Parsing Query.");
            string FullURL = HttpContext.Current.Request.RawUrl;
            string QueryString = null;
            NameValueCollection UrlQueryParams = new NameValueCollection();
            int CheckQuery = FullURL.IndexOf('?');
            if (CheckQuery >= 0)
            {
                QueryString = (CheckQuery < FullURL.Length - 1) ? FullURL.Substring(CheckQuery + 1) : String.Empty;
                UrlQueryParams = HttpUtility.ParseQueryString(QueryString, Encoding.UTF8);
                FullURL = null;
                QueryString = null;

                return UrlQueryParams;
            }
            FullURL = null;
            QueryString = null;
            UrlQueryParams.Clear();
            UrlQueryParams = null;

            return null;
        }

    }
}