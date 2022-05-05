using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web.SessionState;
using System.Text;

namespace XenLauncher
{
    public class ICAGenerator : System.Web.UI.Page
    {
        public static void Generate(HttpSessionState State, HttpResponse Response, string[] Farms, NameValueCollection URLQueryParams)
        {
            bool ResultFound = false;
            try
            {
                foreach (string farm in Farms)
                {
                    string FarmType = farm.Split(',')[0];
                    string FarmURL = farm.Split(',')[1] + "/scripts/wpnbr.dll";
                    Debug.WriteLine("FarmURL:" + FarmURL + " Farm type:" + FarmType);

                    string BuiltPost = null;
                    string ResponseFromPost = null;
                    string AppName = null;
                    string Username = URLQueryParams["ImpUser"];
                    bool ImpUser = false;
                    //Build first request to XML service.
                    try
                    {
                        AppName = URLQueryParams["AppName"].Replace("+", " ");
                        if (Username == null)
                        {
                            Username = HttpContext.Current.User.Identity.Name;
                        }
                        else
                        {
                            if (HelpersSecurity.GrantImpersonationForUser(HttpContext.Current.User.Identity.Name))
                            {
                                ImpUser = true;
                            }
                            else
                            {
                                Response.Write(HttpContext.Current.User.Identity.Name + " does not have permission to impersonate " + Username);
                            }
                            
                        }

                        BuiltPost = XMLControl.BuildPostLaunchRequest(FarmURL, AppName, Username, ImpUser);
                        Debug.WriteLine("Post to XML-Request:\n" + BuiltPost);
                        ResponseFromPost = XMLControl.postXMLData(FarmURL, BuiltPost);
                        BuiltPost = null;
                        Debug.WriteLine("Post to XML-Response:\n" + ResponseFromPost);
                    }
                    catch (Exception exp) { Debug.WriteLine("Error in first request." + exp.Message); BuiltPost = null; ResponseFromPost = null; }

                    //Check if there were some errors in the response from the XML
                    if(ICAControl.ErrorChecker(ResponseFromPost, farm.Split(',')[1]))
                    {
                        continue;
                    }
                    
                    //Aquire data needed to build the ICA file.
                    string CGPAddress = null;
                    string ServerAddress = null;
                    string IMAHostID = null;
                    string SessionSharingKey = null;
                    string LaunchRefValue = null;
                    string LogonTicket = null;
                    try
                    {
                        CGPAddress = HelpersXML.FindValueByName("CGPAddress", ResponseFromPost);
                        ServerAddress = HelpersXML.FindValueByName("ServerAddress", ResponseFromPost);
                        IMAHostID = HelpersXML.FindValueByName("TicketTag", ResponseFromPost);
                        SessionSharingKey = HelpersXML.FindValueByName("SessionSharingKey", ResponseFromPost);
                        LaunchRefValue = HelpersXML.FindValueByName("LaunchRef", ResponseFromPost);
                        LogonTicket = HelpersXML.FindValueByName("Ticket", ResponseFromPost);
                        Debug.WriteLine("Got info from XML." +
                                        "\r\nCGPAddress: " + CGPAddress +
                                        "\r\nServer Address: " + ServerAddress +
                                        "\r\nIMAHostID: " + IMAHostID +
                                        "\r\nSession Sharing Key: " + SessionSharingKey +
                                        "\r\nLaunch Reference: " + LaunchRefValue +
                                        "\r\nLogon Ticket: " + LogonTicket + "\r\n"
                                        );

                    }
                    catch (Exception exp) { Debug.WriteLine("Error finding values from XML." + exp.Message); }

                    //Result was found so the Foreach will finish after this farm has been processed.
                    try
                    {
                        if (SessionSharingKey.Length > 0)
                        {
                            ResultFound = true;
                        }
                        ResponseFromPost = null;
                    }
                    catch (Exception exp) { Debug.WriteLine("Error checking if result was found. Continue processing. " + exp.Message); }


                    //For Xenapp 6.x we need to make another request for a Launch Reference.
                    try
                    {
                        if (FarmType == "6.x")
                        {
                            BuiltPost = XMLControl.BuildPostLaunchRef(IMAHostID, SessionSharingKey, Username, ImpUser);
                            Debug.WriteLine("Post to XML Launch reference-Request:\r\n" + BuiltPost);
                            ResponseFromPost = XMLControl.postXMLData(FarmURL, BuiltPost);
                            Debug.WriteLine("Post to XML Launch reference-Response:\r\n" + BuiltPost);
                            BuiltPost = null;
                            LaunchRefValue = HelpersXML.FindValueByName("LaunchRef", ResponseFromPost);
                            ResponseFromPost = null;
                        }
                    }
                    catch (Exception exp) { Debug.WriteLine("Error building launch reference POST." + exp.Message); BuiltPost = null; ResponseFromPost = null; break; }

                    //Generate ICA file for the user.
                    string ICAFile = null;
                    try
                    {
                        string LongCommandLine = URLQueryParams["Param"];
                        ICAFile = ICAControl.GenerateICAFile(AppName, ServerAddress, CGPAddress, SessionSharingKey, LaunchRefValue, LongCommandLine, LogonTicket, Username, ImpUser);
                        string TextView = URLQueryParams["txtview"];
                        if (TextView == "true")
                        {
                            Response.Write(ICAFile.Replace(Environment.NewLine, "<br/>"));
                        }
                        else
                        {
                            Response.ContentType = "application/x-ica";
                            Response.Write(ICAFile);
                        }


                    }
                    catch (Exception exp) { Debug.WriteLine("Error generating ICA file." + exp.Message); break; }

                    ICAFile = null;

                    //End current Foreach because the Application has been found.
                    if (ResultFound)
                    { break; }
                }
            }
            catch (Exception exp) { Debug.WriteLine("Error in main module: " + exp.Message); }

            if (!ResultFound)
            {
                Debug.WriteLine("Could not find or launch application. Check your settings.");
                Response.Clear();
                Response.Write("Error launching application. Check server logs.");
                Response.End();
            }
        }

        public static void GetAppList(HttpSessionState State, HttpResponse Response, string[] Farms)
        {
            try
            {
                Response.ContentType = "text/html";
                StringBuilder ResponseBuild = new StringBuilder("<HTML><Body>");
                foreach (string farm in Farms)
                {
                    string FarmType = farm.Split(',')[0];
                    string FarmURL = farm.Split(',')[1] + "/scripts/wpnbr.dll";
                    Debug.WriteLine("FarmURL:" + FarmURL + " Farm type:" + FarmType);
                    ResponseBuild.Append("<B>List of available applications from " + farm.Split(',')[1] + ":</B><br/><br/>");

                    string BuiltPost = null;
                    string ResponseFromPost = null;
                    
                    //Build request to XML service.
                    try
                    {
                        BuiltPost = XMLControl.BuildPostListAvailableApps();
                        Debug.WriteLine("Post to XML-Request:\n" + BuiltPost);
                        ResponseFromPost = XMLControl.postXMLData(FarmURL, BuiltPost);
                        BuiltPost = null;
                        Debug.WriteLine("Post to XML-Response:\n" + ResponseFromPost);
                        ResponseBuild.Append(HelpersXML.ParseAppNames(ResponseFromPost).Replace(",","<br/>")+"<br/><br/>");
                    }
                    catch (Exception exp) { Debug.WriteLine("Error in first request." + exp.Message); BuiltPost = null; ResponseFromPost = null; }
                }
                ResponseBuild.Append("Active Directory group count: <B>" + HttpContext.Current.Request.LogonUserIdentity.Groups.Count+ "</B></BODY></HTML>");
                Response.Write(ResponseBuild);
            }
            catch (Exception exp) { Debug.WriteLine("Error in main module." + exp.Message); }
        }

        public static void Logoff(HttpSessionState State, HttpResponse Response, string[] Farms)
        {
            try
            {
                foreach (string farm in Farms)
                {
                    string FarmType = farm.Split(',')[0];
                    string FarmURL = farm.Split(',')[1] + "/scripts/wpnbr.dll";
                    Debug.WriteLine("FarmURL:" + FarmURL + " Farm type:" + FarmType);

                    string BuiltPost = null;
                    string ResponseFromPost = null;

                    //Build request to XML service.
                    try
                    {
                        BuiltPost = XMLControl.BuildPostLogoff();
                        Debug.WriteLine("Post to XML-Request:\n" + BuiltPost);
                        ResponseFromPost = XMLControl.postXMLData(FarmURL, BuiltPost);
                        BuiltPost = null;
                        Debug.WriteLine("Post to XML-Response:\n" + ResponseFromPost);
                    }
                    catch (Exception exp) { Debug.WriteLine("Error in first request." + exp.Message); BuiltPost = null; ResponseFromPost = null; }
                }
            }
            catch (Exception exp) { Debug.WriteLine("Error in main module." + exp.Message); }
        }
    }
}