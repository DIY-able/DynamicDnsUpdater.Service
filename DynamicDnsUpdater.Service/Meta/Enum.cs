using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDnsUpdater.Service.Meta
{
    public class Enum
    {
        /// <summary>
        /// Category are define to match the one defined in "categorySources" names in app.config can be routed differently, now default everything goes to "General" bucket
        /// </summary>
        public enum LogCategoryType { WIN_SERVICE, DNS_UPDATE, STATUS_MONITOR, IP_CHECKER, CONFIGURATION, NOTIFICATION }


        /// <summary>
        /// Define multiple providers
        /// </summary>
        public enum DnsProviderType { AMAZON_ROUTE_53 }


        /// <summary>
        /// Define multiple type of clients
        /// </summary>
        public enum ClientType { WEB_HTTP }

        /// <summary>
        /// Define multiple checker/parser for different services
        /// </summary>
        public enum IpCheckerType { DYN_DNS, JSON_IP, CUSTOM }

        /// <summary>
        /// Define the Change Status (DnsProvider such as Amazon Route53 takes several minutes from PENDING to INSYNC)
        /// </summary>
        public enum ChangeStatusType { PENDING, INSYNC }


        /// <summary>
        /// Define multiple types of notification 
        /// </summary>
        public enum NotificationType { EMAIL }

    }
}
