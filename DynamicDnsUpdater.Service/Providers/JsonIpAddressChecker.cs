using DynamicDnsUpdater.Service.Helpers;
using DynamicDnsUpdater.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;


namespace DynamicDnsUpdater.Service.Providers
{
    public class JsonIpAddressChecker : IIpAddressChecker
    {
        /// <summary>
        /// Get Current IP address 
        /// </summary>
        /// <param name="ipProviderURL"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public string GetCurrentIpAddress(string ipProviderURL, IClient client)
        {
            // Pass the parser as function to the client
            DelegateParser handler = Parse;
            return client.GetContent(ipProviderURL, handler);

        }

        /// <summary>
        /// Parse the JsonIP in JSON format to IP
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string Parse(string jsonString)
        {
            string ipString = null;

            // format: {"ip":"x.x.x.x","about":"/about","Pro!":"http://getjsonip.com"}

            var jsonSerializer = new JavaScriptSerializer();
            Dictionary<string, string> data = jsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
            ipString = data["ip"];

            // Validate if this is a valid IPV4 address
            if (IpHelper.IpAddressV4Validator(ipString))
                return ipString;
            else
                return null;
        }

    }
}
