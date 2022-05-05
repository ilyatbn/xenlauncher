using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;
using System.Security.Principal;
using System.Configuration;

namespace XenLauncher
{
    public class HelpersSecurity
    {
        public static List<string> GetGroupSID(string Username)
        {
            List<string> userNestedMembership = new List<string>();
            try
            {
                var domainConnection = new DirectoryEntry();
                domainConnection.AuthenticationType = System.DirectoryServices.AuthenticationTypes.Secure;

                var samSearcher = new DirectorySearcher();
                samSearcher.SearchRoot = domainConnection;
                samSearcher.Filter = "(samAccountName=" + Username + ")";
                samSearcher.PropertiesToLoad.Add("displayName");

                var samResult = samSearcher.FindOne();

                samSearcher.Dispose(); 
                samSearcher = null;

                if (samResult != null)
                {
                    var theUser = samResult.GetDirectoryEntry();
                    theUser.RefreshCache(new string[] { "tokenGroups" });

                    foreach (byte[] resultBytes in theUser.Properties["objectSid"])
                    {
                        var SID = new SecurityIdentifier(resultBytes, 0);
                        userNestedMembership.Add(SID.Value.ToString());
                        SID = null;
                    }

                    foreach (byte[] resultBytes in theUser.Properties["tokenGroups"])
                    {
                        var SID = new SecurityIdentifier(resultBytes, 0);
                        var sidSearcher = new DirectorySearcher();
                        userNestedMembership.Add(SID.Value.ToString());
                        SID = null;
                        sidSearcher.Dispose();
                        sidSearcher = null;
                    }
                    samResult = null;
                    theUser.Dispose();
                    theUser = null;
                }

                try
                {
                    domainConnection.Close();
                    domainConnection.Dispose();
                    domainConnection = null;
                }
                catch (Exception er)
                {
                }

                return userNestedMembership;
            }
            catch (Exception ex)
            {
                userNestedMembership.Add("Error");
                return userNestedMembership;
            }

        }

        //Quite silly. The ability to impersonate an ICA launch request is granted here based on the user
        //in Web.Config Feel free to change it to a more complex solution if you want.
        public static bool GrantImpersonationForUser(string Username)
        {
            if (Username == ConfigurationManager.AppSettings["ImpUser"])
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
    }
}