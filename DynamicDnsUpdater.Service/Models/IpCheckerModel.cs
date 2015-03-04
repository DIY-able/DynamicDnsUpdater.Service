using DynamicDnsUpdater.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDnsUpdater.Service.Models
{

    /// <summary>
    /// Model mapping of IpChecker for XmlConfig
    /// </summary>
    public  class IpCheckerModel
    {
        public Meta.Enum.IpCheckerType IpCheckerType { get; set; }
        public Meta.Enum.ClientType ClientType { get; set; }
        public string IpCheckerUrl { get; set; }
        public IClient Client { get; set; }
        public IIpAddressChecker IpAddressChecker { get; set; }
    }



}
