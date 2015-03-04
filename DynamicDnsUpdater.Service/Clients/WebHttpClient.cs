using DynamicDnsUpdater.Service.Configuration;
using DynamicDnsUpdater.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDnsUpdater.Service.Clients
{

    /// <summary>
    /// Web Http Client with Delegate Parser
    /// </summary>
    public class WebHttpClient : IClient
    {
        /// <summary>
        /// Get the content of the Html as string
        /// </summary>
        /// <param name="IpProviderUrl"></param>
        /// <returns></returns>
        public string GetContent(string IpProviderUrl, DelegateParser parser)
        {
            string content = null;
            int timeoutInMilliSeconds = Convert.ToInt32(ConfigHelper.ClientTimeoutInMinutes) * 60 *1000;

            // Use IDisposable webclient to get the page of content of existing IP
            using (TimeoutWebClient client = new TimeoutWebClient((timeoutInMilliSeconds)))
            {
                content = client.DownloadString(IpProviderUrl);
            }

            if (content != null)
                return parser(content);
            else
                return null;
        }

        /// <summary>
        /// To support timeout value, alternatively you can use HttpWebRequest and Stream
        /// </summary>
        public class TimeoutWebClient : WebClient
        {
            public int Timeout { get; set; }

            public TimeoutWebClient(int timeout)
            {
                Timeout = timeout;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);
                request.Timeout = Timeout;
                return request;
            }
        }


    }
}
