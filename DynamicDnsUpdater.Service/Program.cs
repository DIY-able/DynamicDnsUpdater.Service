
// DynamicDnsUpdaterService
//
// Description: Automatically update your dynamic IP address, supports multiple DNS Providers and IpCheckers. Project was started to update Amazon AWS Route 53. 
//
// Architecture and Installation: See CodeProject and GitHub
//
// Pre-req: Visual Studio 2013 with .NET Framework 4.5 (full, not client profile)
// Compile: Enable NuGet to restore package
//
// Article: http://www.codeproject.com/Articles/882487/Amazon-AWS-Route-Dynamic-IP-Updater-Windows-Service
// Source : https://github.com/riniboo/DynamicDnsUpdaterService
//
// License: The GNU General Public License v3.0 
//
// Written and Created By: Rini Boo 
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
                x.SetDisplayName("Dynamic DNS Updater Service");
                x.SetServiceName("DynamicDnsUpdater");
            });               
            
        } // main


    } //  program


} // ns