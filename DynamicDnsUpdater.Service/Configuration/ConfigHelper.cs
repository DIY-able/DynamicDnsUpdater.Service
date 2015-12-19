using DynamicDnsUpdater.Service.Encryption;
using DynamicDnsUpdater.Service.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Xml;
using System.Xml.Serialization;


namespace DynamicDnsUpdater.Service.Configuration
{
    public class ConfigHelper
    {
        // Basic encryption key 
        public const string EncryptionKey = "bsBg34asdfi28B9N3489saduiAB23sdJNKIJFadSIUsaaFUU1344IDdsfF535fhB";

        // Helper for App.config
        public static string XmlConfigFileName { get { return ConfigurationManager.AppSettings["XmlConfigFileName"]; } }
        public static string UpdateIntervalInMinutes { get { return ConfigurationManager.AppSettings["UpdateIntervalInMinutes"]; } }
        public static string MonitorStatusInMinutes { get { return ConfigurationManager.AppSettings["MonitorStatusInMinutes"]; } }
        public static string ClientTimeoutInMinutes { get { return ConfigurationManager.AppSettings["ClientTimeoutInMinutes"]; } }
        public static string ForceUpdateInDays { get { return ConfigurationManager.AppSettings["ForceUpdateInDays"]; } }
        
        public static string FromEmail { get { return ConfigurationManager.AppSettings["FromEmail"]; } }
        public static string ToEmail { get { return ConfigurationManager.AppSettings["ToEmail"]; } }
        public static string Password
        {
            get
            {
                if (EnablePasswordEncryption)
                    return Des3.Decrypt(ConfigurationManager.AppSettings["Password"], EncryptionKey);
                else
                    return ConfigurationManager.AppSettings["Password"];
            }
        }
        public static string Subject { get { return ConfigurationManager.AppSettings["Subject"]; } }
        public static string Host { get { return ConfigurationManager.AppSettings["Host"]; } }
        public static string Port { get { return ConfigurationManager.AppSettings["Port"]; } }
        public static bool EnablePasswordEncryption { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["EnablePasswordEncryption"]); } }
        

        /// <summary>
        /// Load Config from XML to objects
        /// </summary>
        /// <returns></returns>
        public static XmlConfig LoadConfig()
        {
            XmlConfig config = null;
            XmlSerializer ser = new XmlSerializer(typeof(XmlConfig));
            using (XmlReader reader = XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + XmlConfigFileName))
            {
                config = (XmlConfig)ser.Deserialize(reader);
            }

            return config;
        }


        /// <summary>
        /// Update several domain info in XmlConfig using XPath  (Called by Update Dns)
        /// </summary>
        /// <param name="domainName"></param>
        /// <param name="dateTimeinUTC"></param>
        /// <returns></returns>
        public static bool UpdateDomainInformation(DomainModel domain)
        {
            // Use Xpath to update (debatable - not deserialize the xml?)
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(AppDomain.CurrentDomain.BaseDirectory + XmlConfigFileName);
            XmlNode root = xmlDocument.DocumentElement;


            // Find the matching domain name using Xpath and update the datetime
            XmlNode node = root.SelectSingleNode("//Domains/Domain[DomainName=\"" + domain.DomainName + "\"]");

            if (node != null)
            {
                node["LastIpAddress"].InnerText = domain.LastIpAddress;
                node["LastUpdatedDateTime"].InnerText = domain.LastUpdatedDateTime.ToString("o");    // UTC timestamp in ISO 8601 format
                node["HistoricalIPAddress"].InnerText = domain.HistoricalIpAddress;
                node["LastUpdatedReason"].InnerText = Enum.GetName(typeof(Meta.Enum.UpdateReasonType), domain.LastUpdatedReason);

                // Need to use this to fix carriage return problem if InnerText is an empty string
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                using (XmlWriter writer = XmlWriter.Create(AppDomain.CurrentDomain.BaseDirectory + XmlConfigFileName, settings))
                {
                    xmlDocument.Save(writer);
                }


                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Update ChangeStatus in XmlConfig using XPath (called by Monitor Status Change)
        /// </summary>
        /// <param name="domainName"></param>
        /// <param name="changeStatus"></param>
        /// <returns></returns>
        public static bool UpdateChangeStatusInformation(string domainName, string changeStatusId)
        {
            // Use Xpath to update (debatable - not deserialize the xml?)
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(AppDomain.CurrentDomain.BaseDirectory + XmlConfigFileName);
            XmlNode root = xmlDocument.DocumentElement;

            // Find the matching domain name using Xpath and update the datetime
            XmlNode node = root.SelectSingleNode("//Domains/Domain[DomainName=\"" + domainName + "\"]");

            if (node != null)
            {
                node["ChangeStatusID"].InnerText = changeStatusId;

                // Need to use this to fix carriage return problem if InnerText is an empty string
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                using (XmlWriter writer = XmlWriter.Create(AppDomain.CurrentDomain.BaseDirectory + XmlConfigFileName, settings))
                {
                    xmlDocument.Save(writer);
                }
                
                return true;
            }
            else
                return false;
        }


    }
}
