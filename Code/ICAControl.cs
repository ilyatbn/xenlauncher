using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace XenLauncher
{
    class ICAControl
    {
        public static string GenerateICAFile(string CTXAppName, 
                                             string ServerAddress, 
                                             string CGPAddress,
                                             string SessionSharingKey,
                                             string LaunchRefValue,
                                             string LongCommandLine,
                                             string LogonTicket,
                                             string Username,
                                             bool ImpUser
                                            )
        {
            string DefaultICAFile = Properties.Resources.DefaultICA;

            if (LogonTicket != null)
            {
                LogonTicket = "LogonTicket=" + LogonTicket;
            }
            string ModifiedICAFile = DefaultICAFile.Replace("DefaultAppName", CTXAppName)
                                                   .Replace("DefaultIPAddress", ServerAddress)
                                                   .Replace("DefaultCGPPort", CGPAddress)
                                                   .Replace("DefaultSessionSharingKey", SessionSharingKey)
                                                   .Replace("DefaultLongCommandLine", LongCommandLine)
                                                   .Replace("DefaultLaunchReference", LaunchRefValue)
                                                   .Replace("LogonTicket=DefaultLogonTicket", LogonTicket)
                                                   .Replace("DefaultUnixTime", (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Ticks).ToString()
                                                   );

            if (ImpUser == true)
            {
                ModifiedICAFile = InsertProperty(ModifiedICAFile, "Username=" + Username.Split('\\')[1]);
                ModifiedICAFile = InsertProperty(ModifiedICAFile, "Domain=" + Username.Split('\\')[0]);
            }
            else
            {
                ModifiedICAFile = InsertProperty(ModifiedICAFile, "UseLocalUserAndPassword=On");
            }
            return ModifiedICAFile;
        }

        internal static string InsertProperty(string ICAFile, string Property, bool aboveBelow = true)
        {
            List<string> txtLines = ICAFile.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
            int index = aboveBelow ? txtLines.IndexOf("[Compress]") - 2 : txtLines.IndexOf("[Compress]");
            if (index > 0)
            {
                txtLines.Insert(index, Property);
                return String.Join(Environment.NewLine, txtLines);
            }
            return ICAFile;
        }

        public static bool ErrorChecker(string PostData,string farm)
        {
            //6.x error message that states the attribute it received was not found.
            if (HelpersXML.FindValueByName("MPSError", PostData) == "0x80060001")
            {
                Debug.WriteLine("Error IMA_RESULT_ATTR_NOT_FOUND received from " + farm);
                return true;
            }
            //6.x unspecified error message
            if (HelpersXML.FindValueByName("MPSError", PostData) == "0x800D0006")
            {
                Debug.WriteLine("Unspecified error received from " + farm);
                return true;
            }
            //7.x "no-available-workstation" message
            if (HelpersXML.FindValueByName("ErrorId", PostData) == "no-available-workstation")
            {
                Debug.WriteLine("Error no-available-workstation received from " + farm);
                return true;
            }
            //7.x "retry-required" message
            if (HelpersXML.FindValueByName("ErrorId", PostData) == "retry-required")
            {
                Debug.WriteLine("Error retry-required received from " + farm);
                return true;
            }

            return false;
        }
    }
}
