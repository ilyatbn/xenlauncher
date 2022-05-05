using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Collections.Specialized;
using System.Diagnostics;

namespace XenLauncher
{
    public partial class Generator : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string[] Farms = null;
            try
            {
                Farms = ConfigurationManager.AppSettings.AllKeys
                .Where(key => key.StartsWith("Farm:"))
                .Select(key => ConfigurationManager.AppSettings[key])
                .ToArray();
            }
            catch (Exception exp) { Debug.WriteLine("Error getting farm list from Web.Config: " + exp.Message);
            }

            if (Farms==null)
            {
                Response.Write("Could not find farm information. Make sure your configuration is set correctly.");
                Response.End();
            }

            //Get all URL Query items.
            NameValueCollection URLQueryParams = new NameValueCollection();
            try
            {
                URLQueryParams = HelpersWeb.ParseURLQuery();
                if (URLQueryParams == null)
                {
                    Response.Write("<B>XenLauncher V0.2.1</B><BR/><BR/>");
                    Response.Write("No URL Query found or error in parsing.<BR/><BR/>");
                    Response.Write("Too see a list of commands add <B>/?help</B> in the URL.");
                    Response.End();
                }
            }
            catch (Exception exp) { Debug.WriteLine("Error getting URLQuery. " + exp.Message); }


            if (Request.RawUrl.ToLower().Contains("?logoff"))
            {
                Response.Clear();
                ICAGenerator.Logoff(Session, Response, Farms);
            }

            if (Request.RawUrl.ToLower().Contains("?list"))
            {
                Response.Clear();
                Debug.WriteLine("Getting list of available apps.");
                ICAGenerator.GetAppList(Session, Response, Farms);
            }

            if (Request.RawUrl.ToLower().Contains("?help"))
            {
                Response.Clear();
                Response.Write("<B>XenLauncher V0.2.1</B><BR/><BR/>");
                Response.Write("To list all available applications for the current user, add <B>/?list</B><BR/>");
                Response.Write("To launch an application for the current user type <B>/?AppName=NAME</B><BR/>");
                Response.Write("To add an application parameter, add <B>&Param=parameter</B> to the query.<BR/>");
                Response.Write("To impersonate a launch request, add <B>&ImpUser=DOMAIN\\Username</B> to the query. User must still provide a password manually unless password is specified in the ICA file programmatically.<BR/>");
                Response.Write("To logoff all sessions for the current user, add <B>/?logoff</B><BR/>");
                Response.Write("To view the generated ICA file in the browser, add <B>&txtview=true</B><BR/>");
                Response.Write(@"<BR/>Example:<BR/> http://MyCompany.io/XenLauncher/?AppName=InternetExplorer&Param=www.google.com/ncr&ImpUser=MyDomain\MyUser&txtview=true<BR/>");

            }
            string AppName = null;
            try
            {
                AppName = Request.QueryString["AppName"].ToLower();
            }catch{}

            if (AppName!=null)
            {
                Response.Clear();
                Debug.WriteLine("Launching ICA Generator.");
                ICAGenerator.Generate(Session, Response, Farms, URLQueryParams);
            }
            AppName = null;
        }
    }
}