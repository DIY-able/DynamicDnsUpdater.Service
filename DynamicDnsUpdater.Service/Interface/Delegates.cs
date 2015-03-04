using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDnsUpdater.Service.Interface
{
    // Define the delegate for the parser to pass it as parameter from checker to client
    public delegate string DelegateParser(string html);

}
