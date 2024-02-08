
// DynamicDnsUpdaterService
//
// Description: Automatically update your dynamic IP address, supports multiple DNS Providers and IpCheckers. Project was started to update Amazon AWS Route 53. 
//
// Architecture and Installation: See CodeProject and GitHub
//
// Article: http://www.codeproject.com/Articles/882487/Amazon-AWS-Route-Dynamic-IP-Updater-Windows-Service
// It was published under "Rini Boo" nickname
//
// Source : https://github.com/riniboo/DynamicDnsUpdaterService
//
// Written and Created By: Rini Boo (DIYable)
// Created on: 2015-03-04
//
// 2015-12-03 (ver 1.0.0.1): 
// - Added HistoricalIPAddress and LastUpdatedReason in domain level for log and notification (forced/changed)
// - Fixed not able to send email after timeout, Smtpclient is not disposed properly

// 2015-12-19 (ver 1.0.0.2):
// - Compiled with VS2015 (.NET 4.5.0)  
// - Newtonsoft.Json.dll is missing compare to the old bin folder, remove it no longer been used
// - NuGet Update Amazon AWS 2.3.19 to 2.3.53
// - NuGet Update Topshelf 3.1.4 to 3.3.31
// - NuGet Update Unity 3.5.1404 to 4.0.1

// 2016-05-18 (ver 1.0.0.3):
// dougkwilson on GitHub updated .NET v4.6.1, Topshelf v4.0.1 and AWSSDK v3

// 2016-11-07 (ver 1.0.0.4):
// - Compiled with VS2015 (.NET 4.6.1)
// - NuGet Update on Topshelf to 4.0.3
// - Uninstall Amazon AWS 2.3.53 and Installed AWSSDK.Route53 (3.3.1.1)
// - Newtonsoft.Json.dll is needed from JavaScriptSerializer (Json.NET)
// - Replace AWSClientFactory.CreateAmazonRoute53Client (AWS obsolete API) with new AmazonRoute53Client

// 2020-01-12 (ver 1.0.0.5):
// - Compiled with VS2017 (.NET 4.6.2), \v4.6.2\System.Web.Extensions.dll has JavaScriptSerializer
// - Updated AWSSDK.Core v3.3.104.15
// - Updated AWSSDK.Route53 v3.3.102.68
// - Updated Topshelf V4.2.1
// - Updated Unity v5.11.3
// - Updated CommonServiceLocator v2.0.5
// - It automatically added System.Runtime.CompilerServices.Unsafe.4.7.0

// 2020-01-22 (ver 1.0.0.6)
// - Compiled with VS2019 (.NET 4.7.2)
// - Updated AWSSDK.Core 3.3.104.19
// - Updated AWSSDK.Route53 3.3.102.72

// 2024-02-08 (ver 1.0.0.7)
// - Compiled with VS2022 (.NET 4.8.1)
// - Updated AWSSDK.Core 3.7.302.7
// - Updated AWSSDK.Route53 3.7.302.14
// - Updated Topshelf 4.3.0
// - Updated Unity 5.11.10


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;
using System.Threading;
using DynamicDnsUpdater.Service.Workers;


namespace DynamicDnsUpdater.Service
{

    public class Program
    {
        public static void Main()
        {
            // Using Open Source project "Topshelf" to handle the run as windows service
            // Ref: http://topshelf-project.com/

            HostFactory.Run(x =>
            {
                x.Service<Worker>(s =>
                {
                    s.ConstructUsing(name => new Worker());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Update current IP address supports multiple DNS providers");
                x.SetDisplayName("DynamicDnsUpdater.Service");
                x.SetServiceName("DynamicDnsUpdater");
            });               
            
        } // main


    } //  program


} // ns