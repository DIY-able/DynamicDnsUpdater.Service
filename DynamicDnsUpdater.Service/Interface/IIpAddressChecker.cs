using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDnsUpdater.Service.Interface
{
    public interface IIpAddressChecker
    {       
          string GetCurrentIpAddress(string IpProviderURL, IClient client);     
    }
}
