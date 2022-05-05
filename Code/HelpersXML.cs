using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace XenLauncher
{
    class HelpersXML
    {
        public static string FindValueByName(string Name, string XMLData)
        {
            byte[] encodedString = Encoding.UTF8.GetBytes(XMLData);
            MemoryStream ms = new MemoryStream(encodedString);
            ms.Flush();
            ms.Position = 0;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.XmlResolver = null;
            xmlDoc.Load(ms);

            XmlNodeList elemList = xmlDoc.GetElementsByTagName(Name);

            if (elemList.Count == 1)
            {
                return elemList[0].InnerXml;
            }
            else if (elemList.Count > 1)
            {
                StringBuilder FoundElements = new StringBuilder();
                for (int i = 0; i < elemList.Count; i++)
                {
                    FoundElements.Append(elemList[i].InnerText + ",");
                }
                return FoundElements.ToString();
            }
            return null;
        }

        public static string ParseAppNames(string XMLData)
        {
            string result = null;
            result = FindValueByName("InName", XMLData);
            Debug.WriteLine("AppName Values:" + result + "\r\n");
            return result;
        }
    }
}
