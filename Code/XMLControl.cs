using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using System.Web;
using System.Security.Principal;
using System.Diagnostics;

namespace XenLauncher
{
    class XMLControl
    {
        public static string BuildPostLaunchRequest(string URL, string AppName, string Username, bool ImpUser)
        {
            string RemoteIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            string result;
            Encoding utf8noBOM = new UTF8Encoding();  
            XmlWriterSettings settings = new XmlWriterSettings();  
            settings.Indent = true;  
            settings.Encoding = utf8noBOM;
            using (MemoryStream output = new MemoryStream())
            {
                using (XmlWriter MyPostXML = XmlWriter.Create(output, settings))
                {
                    MyPostXML.WriteStartDocument(true);
                    MyPostXML.WriteDocType("NFuseProtocol", null, "NFuse.dtd", null);
                    MyPostXML.WriteStartElement("NFuseProtocol");
                    MyPostXML.WriteStartAttribute("version");
                    MyPostXML.WriteString("5.8");
                    MyPostXML.WriteEndAttribute();
                    //
                    MyPostXML.WriteStartElement("RequestAddress");
                    //
                    MyPostXML.WriteStartElement("Name");
                    //
                    Debug.WriteLine("Debug: AppName to launch = " + AppName);
                    MyPostXML.WriteStartElement("AppName");
                    MyPostXML.WriteString(AppName);
                    MyPostXML.WriteEndElement();
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("ClientName");
                    MyPostXML.WriteString("WR_" + Username.GetHashCode().ToString());
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("ClientAddress");
                    MyPostXML.WriteStartAttribute("addresstype");
                    MyPostXML.WriteString("dot");
                    MyPostXML.WriteEndAttribute();
                    MyPostXML.WriteString(RemoteIPAddress);
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("ServerAddress");
                    MyPostXML.WriteStartAttribute("addresstype");
                    MyPostXML.WriteString("dns-port");
                    MyPostXML.WriteEndAttribute();
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("Credentials");
                    //
                        MyPostXML.WriteStartElement("ID");
                        MyPostXML.WriteStartAttribute("type");
                        MyPostXML.WriteString("SAM");
                        MyPostXML.WriteEndAttribute();
                        MyPostXML.WriteString(Username);
                        MyPostXML.WriteEndElement();
                        //
                        List<string> ADsidList = new List<string>();
                        if (ImpUser == true)
                        {
                            ADsidList = HelpersSecurity.GetGroupSID(Username.Split('\\')[1]);
                        }
                        else
                        {
                            ADsidList.Add(HttpContext.Current.Request.LogonUserIdentity.User.Value.ToString());
                            try
                            {
                                IdentityReferenceCollection ADGroupCollection = new IdentityReferenceCollection();
                                ADGroupCollection = HttpContext.Current.Request.LogonUserIdentity.Groups;
                                try
                                {
                                    foreach (IdentityReference grp in ADGroupCollection)
                                    {
                                        ADsidList.Add(grp.Value.ToString());
                                    }
                                }
                                catch (Exception e) { Debug.WriteLine("Error getting a group from the group list\r\n" + e.Message); }
                            }
                            catch (Exception e) { Debug.WriteLine("Error getting the grouplist of the user.\r\n" + e.Message); }
                        }
  
                        foreach (string group in ADsidList)
                        {
                            MyPostXML.WriteStartElement("ID");
                            MyPostXML.WriteStartAttribute("type");
                            MyPostXML.WriteString("SID");
                            MyPostXML.WriteEndAttribute();
                            MyPostXML.WriteString(group);
                            MyPostXML.WriteEndElement();
                        }
                        ADsidList.Clear();
                        ADsidList = null;
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("ClientType");
                    MyPostXML.WriteString("ica30");
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("DeviceId");
                    MyPostXML.WriteString("WR_"+Username.GetHashCode().ToString());
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("SessionSettings");
                    //
                    MyPostXML.WriteStartElement("ColorDepth");
                    MyPostXML.WriteString("24bpp"); //Change VIA web.config
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("EncryptionLevel");
                    MyPostXML.WriteString("basic");
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("DomainName");
                    MyPostXML.WriteString(Username.Split('\\')[0]);
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("UserName");
                    MyPostXML.WriteString(Username.Split('\\')[1]);
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("FarmName");
                    MyPostXML.WriteString(" ");
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("SpecialFolderRedirection");
                    MyPostXML.WriteString("Off"); //Change VIA web.config
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("VirtualCOMPortEmulation");
                    MyPostXML.WriteString("On"); //Change VIA web.config
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("DisplaySize");
                    MyPostXML.WriteString("seamless");
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("TWIDisableSessionSharing");
                    MyPostXML.WriteString("Off");
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("EnableSessionSharing");
                    MyPostXML.WriteString("On");
                    MyPostXML.WriteEndElement();
                    MyPostXML.WriteEndElement();

                    MyPostXML.WriteEndElement();
                    MyPostXML.WriteEndElement();


                    MyPostXML.WriteEndDocument();
                    MyPostXML.Close();
                }
                result = Encoding.UTF8.GetString(output.ToArray());

            }
            utf8noBOM = null;
            settings = null;
            return result;
        }

        public static string BuildPostLaunchRef(string IMAHostId, string SessionSharingKey, string Username, bool ImpUser)
        {
            string result;
            Encoding utf8noBOM = new UTF8Encoding(false);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = utf8noBOM;
            using (MemoryStream output = new MemoryStream())
            {
                using (XmlWriter MyPostXML = XmlWriter.Create(output, settings))
                {
                    MyPostXML.WriteStartDocument(true);
                    MyPostXML.WriteDocType("NFuseProtocol", null, "NFuse.dtd", null);
                    MyPostXML.WriteStartElement("NFuseProtocol");
                    MyPostXML.WriteStartAttribute("version");
                    MyPostXML.WriteString("5.8");
                    MyPostXML.WriteEndAttribute();
                    //
                    MyPostXML.WriteStartElement("RequestLaunchRef");
                    //
                    MyPostXML.WriteStartElement("LaunchRefType");
                    MyPostXML.WriteString("ICA");
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("TicketTag");
                    MyPostXML.WriteString(IMAHostId);
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("DeviceId");
                    MyPostXML.WriteString("WR_"+Username.GetHashCode().ToString());
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("SessionSharingKey");
                    MyPostXML.WriteString(SessionSharingKey);
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("Credentials");
                    //
                    MyPostXML.WriteStartElement("ID");
                    MyPostXML.WriteStartAttribute("type");
                    MyPostXML.WriteString("SAM");
                    MyPostXML.WriteEndAttribute();
                    MyPostXML.WriteString(Username);
                    MyPostXML.WriteEndElement();
                    //
                    List<string> ADsidList = new List<string>();
                    if (ImpUser == true)
                    {
                        ADsidList = HelpersSecurity.GetGroupSID(Username.Split('\\')[1]);
                    }
                    else
                    {
                        ADsidList.Add(HttpContext.Current.Request.LogonUserIdentity.User.Value.ToString());
                        try
                        {
                            IdentityReferenceCollection ADGroupCollection = new IdentityReferenceCollection();
                            ADGroupCollection = HttpContext.Current.Request.LogonUserIdentity.Groups;
                            try
                            {
                                foreach (IdentityReference grp in ADGroupCollection)
                                {
                                    ADsidList.Add(grp.Value.ToString());
                                }
                            }
                            catch (Exception e) { Debug.WriteLine("Error getting a group from the group list\r\n" + e.Message); }
                        }
                        catch (Exception e) { Debug.WriteLine("Error getting the grouplist of the user.\r\n" + e.Message); }
                    }

                    try
                    {
                        IdentityReferenceCollection ADGroupCollection = new IdentityReferenceCollection();
                        ADGroupCollection = HttpContext.Current.Request.LogonUserIdentity.Groups;
                        try
                        {
                            foreach (IdentityReference grp in ADGroupCollection)
                            {
                                ADsidList.Add(grp.Value.ToString());
                            }
                        }
                        catch (Exception e) { Debug.WriteLine("Error getting a group from the group list\r\n" + e.Message); }
                    }
                    catch (Exception e) { Debug.WriteLine("Error getting the grouplist of the user.\r\n" + e.Message); }

                    foreach (string group in ADsidList)
                    {
                        MyPostXML.WriteStartElement("ID");
                        MyPostXML.WriteStartAttribute("type");
                        MyPostXML.WriteString("SID");
                        MyPostXML.WriteEndAttribute();
                        MyPostXML.WriteString(group);
                        MyPostXML.WriteEndElement();
                    }
                    ADsidList.Clear();
                    ADsidList = null;
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("TimeToLive");
                    MyPostXML.WriteString("200");
                    MyPostXML.WriteEndElement();

                    MyPostXML.WriteEndElement();
                    MyPostXML.WriteEndElement();


                    MyPostXML.WriteEndDocument();
                    MyPostXML.Close();
                }
                result = Encoding.Default.GetString(output.ToArray());
            }
            utf8noBOM = null;
            settings = null;
            return result;
        }

        public static string BuildPostListAvailableApps()
        {
            string RemoteIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            string Username = null;
            Username = HttpContext.Current.User.Identity.Name;

            string result = null;
            Encoding utf8noBOM = new UTF8Encoding(false);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = utf8noBOM;

            using (MemoryStream output = new MemoryStream())
            {
                using (XmlWriter MyPostXML = XmlWriter.Create(output, settings))
                {
                    MyPostXML.WriteStartDocument(true);
                    MyPostXML.WriteDocType("NFuseProtocol", null, "NFuse.dtd", null);
                    MyPostXML.WriteStartElement("NFuseProtocol");
                    MyPostXML.WriteStartAttribute("version");
                    MyPostXML.WriteString("5.8");
                    MyPostXML.WriteEndAttribute();
                        //
                        MyPostXML.WriteStartElement("RequestAppData");
                            //
                            MyPostXML.WriteStartElement("Scope");
                            MyPostXML.WriteStartAttribute("traverse");
                            MyPostXML.WriteString("subtree");
                            MyPostXML.WriteEndAttribute();
                            MyPostXML.WriteEndElement();
                            //
                            MyPostXML.WriteStartElement("DesiredDetails");
                            MyPostXML.WriteString("rade-offline-mode");
                            MyPostXML.WriteEndElement();
                            //
                            MyPostXML.WriteStartElement("DesiredDetails");
                            MyPostXML.WriteString("permissions");
                            MyPostXML.WriteEndElement();
                            //
                            MyPostXML.WriteStartElement("ServerType");
                            MyPostXML.WriteString("all");
                            MyPostXML.WriteEndElement();
                            //
                            MyPostXML.WriteStartElement("ClientType");
                            MyPostXML.WriteString("ica30");
                            MyPostXML.WriteEndElement();
                            //
                            MyPostXML.WriteStartElement("ClientType");
                            MyPostXML.WriteString("rade");
                            MyPostXML.WriteEndElement();
                            //
                            MyPostXML.WriteStartElement("ClientType");
                            MyPostXML.WriteString("content");
                            MyPostXML.WriteEndElement();
                            //
                            MyPostXML.WriteStartElement("Credentials");
                            //
                            MyPostXML.WriteStartElement("ID");
                            MyPostXML.WriteStartAttribute("type");
                            MyPostXML.WriteString("SAM");
                            MyPostXML.WriteEndAttribute();
                            MyPostXML.WriteString(Username);
                            MyPostXML.WriteEndElement();
                            //
                            List<string> ADsidList = new List<string>();
                            ADsidList.Add(HttpContext.Current.Request.LogonUserIdentity.User.Value.ToString());
                            try
                            {
                                IdentityReferenceCollection ADGroupCollection = new IdentityReferenceCollection();
                                ADGroupCollection = HttpContext.Current.Request.LogonUserIdentity.Groups;
                                try
                                {
                                    foreach (IdentityReference grp in ADGroupCollection)
                                    {
                                        ADsidList.Add(grp.Value.ToString());
                                    }
                                }
                                catch (Exception e) { Debug.WriteLine("Error getting a group from the group list\r\n" + e.Message); }
                            }
                            catch (Exception e) { Debug.WriteLine("Error getting the grouplist of the user.\r\n" + e.Message); }

                            foreach (string group in ADsidList)
                            {
                                MyPostXML.WriteStartElement("ID");
                                MyPostXML.WriteStartAttribute("type");
                                MyPostXML.WriteString("SID");
                                MyPostXML.WriteEndAttribute();
                                MyPostXML.WriteString(group);
                                MyPostXML.WriteEndElement();
                            }
                            ADsidList.Clear();
                            ADsidList = null;
                            MyPostXML.WriteEndElement();
                            //
                            MyPostXML.WriteStartElement("ClientName");
                            MyPostXML.WriteString("WR_"+Username.GetHashCode().ToString());
                            MyPostXML.WriteEndElement();
                            //
                            MyPostXML.WriteStartElement("ClientAddress");
                            MyPostXML.WriteStartAttribute("addresstype");
                            MyPostXML.WriteString("dot");
                            MyPostXML.WriteEndAttribute();
                            MyPostXML.WriteString(RemoteIPAddress);
                            MyPostXML.WriteEndElement();
                        //
                        MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteEndElement();
                    MyPostXML.WriteEndDocument();
                }
                result = Encoding.Default.GetString(output.ToArray());
            }
            utf8noBOM = null;
            settings = null;
            return result;
        }

        public static string BuildPostLogoff()
        {
            string RemoteIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            string Username = null;
            Username = HttpContext.Current.User.Identity.Name;

            string result = null;
            Encoding utf8noBOM = new UTF8Encoding(false);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = utf8noBOM;

            using (MemoryStream output = new MemoryStream())
            {
                using (XmlWriter MyPostXML = XmlWriter.Create(output, settings))
                {
                    MyPostXML.WriteStartDocument(true);
                    MyPostXML.WriteDocType("NFuseProtocol", null, "NFuse.dtd", null);
                    MyPostXML.WriteStartElement("NFuseProtocol");
                    MyPostXML.WriteStartAttribute("version");
                    MyPostXML.WriteString("5.8");
                    MyPostXML.WriteEndAttribute();
                    //
                    MyPostXML.WriteStartElement("RequestDisconnectUserSessions");
                    //
                    MyPostXML.WriteStartElement("Credentials");
                    //
                    MyPostXML.WriteStartElement("ID");
                    MyPostXML.WriteStartAttribute("type");
                    MyPostXML.WriteString("SAM");
                    MyPostXML.WriteEndAttribute();
                    MyPostXML.WriteString(Username);
                    MyPostXML.WriteEndElement();
                    //
                    List<string> ADsidList = new List<string>();
                    ADsidList.Add(HttpContext.Current.Request.LogonUserIdentity.User.Value.ToString());
                    try
                    {
                        IdentityReferenceCollection ADGroupCollection = new IdentityReferenceCollection();
                        ADGroupCollection = HttpContext.Current.Request.LogonUserIdentity.Groups;
                        try
                        {
                            foreach (IdentityReference grp in ADGroupCollection)
                            {
                                ADsidList.Add(grp.Value.ToString());
                            }
                        }
                        catch (Exception e) { Debug.WriteLine("Error getting a group from the group list\r\n" + e.Message); }
                    }
                    catch (Exception e) { Debug.WriteLine("Error getting the grouplist of the user.\r\n" + e.Message); }

                    foreach (string group in ADsidList)
                    {
                        MyPostXML.WriteStartElement("ID");
                        MyPostXML.WriteStartAttribute("type");
                        MyPostXML.WriteString("SID");
                        MyPostXML.WriteEndAttribute();
                        MyPostXML.WriteString(group);
                        MyPostXML.WriteEndElement();
                    }
                    ADsidList.Clear();
                    ADsidList = null;
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("ServerType");
                    MyPostXML.WriteString("all");
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteStartElement("ClientType");
                    MyPostXML.WriteString("ica30");
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteEndElement();
                    //
                    MyPostXML.WriteEndElement();
                    MyPostXML.WriteEndDocument();
                }
                result = Encoding.Default.GetString(output.ToArray());
            }
            utf8noBOM = null;
            settings = null;
            return result;
        }

        public static string postXMLData(string destinationUrl, string requestXml)
        {
            Uri myUri = new Uri(destinationUrl);   
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
            byte[] bytes;
            bytes = System.Text.Encoding.UTF8.GetBytes(requestXml);
            request.ContentType = "text/xml";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            request.Host = myUri.Host + ":" + myUri.Port;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            requestStream.Dispose();
            requestStream = null;
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();

            myUri = null;
            bytes = null;
            request = null;


            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();
                responseStream.Close();
                responseStream.Dispose();
                responseStream = null;

                return responseStr;
            }

            response = null;
            return null;
        }
    }
}
