
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