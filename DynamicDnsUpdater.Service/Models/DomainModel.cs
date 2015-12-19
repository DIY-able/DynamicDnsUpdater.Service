using DynamicDnsUpdater.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDnsUpdater.Service.Models
{


    /// <summary>
    /// Model mapping of Domain for XmlConfig
    /// </summary>
    public class DomainModel
    {
        public string DomainName { get; set; }
        public Meta.Enum.DnsProviderType DnsProviderType { get; set; }
        public IDnsProvider DnsProvider { get; set; }
        public string ProviderUrl { get; set; }     // Linked in XmlConfig and now it is flatten as one of the properties
        public string HostedZoneId { get; set; }
        public string AccessID { get; set; }
        public string SecretKey { get; set; }
        public string MinimalUpdateIntervalInMinutes { get; set; }
        public string LastIpAddress { get; set; }    // CURRENT ip address
        public DateTime LastUpdatedDateTime { get; set; }
        public string ChangeStatusID { get; set; }     // Id for tracking update status 
        public string HistoricalIpAddress { get; set; }  // PREVIOUS ip address
        public Meta.Enum.UpdateReasonType LastUpdatedReason { get; set; }
    }

}
