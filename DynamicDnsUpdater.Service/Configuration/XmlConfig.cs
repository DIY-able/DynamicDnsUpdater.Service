using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DynamicDnsUpdater.Service.Configuration
{
    /// <summary>
    /// Xml Serialization on mapping to the XmlConfig.xml
    /// </summary>

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class XmlConfig
    {

        [System.Xml.Serialization.XmlArrayItemAttribute("Domain", IsNullable = false)]
        public Domain[] Domains { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("Provider", IsNullable = false)]
        public Provider[] Providers { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("IpChecker", IsNullable = false)]
        public IpChecker[] IpCheckers { get; set; }

    }

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class Domain
    {
        public string DomainName { get; set; }
        public DynamicDnsUpdater.Service.Meta.Enum.DnsProviderType ProviderType { get; set; }
        public string HostedZoneId { get; set; }
        public string AccessID { get; set; }
        public string SecretKey { get; set; }
        public string MinimalUpdateIntervalInMinutes { get; set; }
        public string LastIpAddress { get; set; }

        [XmlElement(DataType = "dateTime", IsNullable = false)]
        public DateTime LastUpdatedDateTime { get; set; }

        // DnsProvider such as Amazon Route53 takes several minutes from PENDING to INSYNC, when ChangeStatusID is empty/null = INSYNC, otherwise it is PENDING
        public string ChangeStatusID { get; set; }

        public string HistoricalIPAddress { get; set; }
        public DynamicDnsUpdater.Service.Meta.Enum.UpdateReasonType LastUpdatedReason { get; set; }

    }

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class Provider
    {
        public DynamicDnsUpdater.Service.Meta.Enum.DnsProviderType ProviderType { get; set; }
        public string ProviderUrl { get; set; }
    }

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class IpChecker
    {
        public DynamicDnsUpdater.Service.Meta.Enum.IpCheckerType IpCheckerType { get; set; }
        public DynamicDnsUpdater.Service.Meta.Enum.ClientType ClientType { get; set; }
        public string IpCheckerUrl { get; set; }
    }
}

