# DynamicDnsUpdater.Service
The project was published under "RiniBoo" nickname in 2015. DynamicDnsUpdater.Service runs as Windows Service, automatically updates your dynamic IP address, supports multiple DNS Providers and IpCheckers. Project was started to update my home IP address using Amazon AWS Route 53. For the details on how to use the code, refers to:

- http://www.codeproject.com/Articles/882487/Amazon-AWS-Route-Dynamic-IP-Updater-Windows-Servic

CodeProject won't be have the latest code, it's just more for reference.  You can implement other DNS providers if you like. 

Ideally, this project should be upgraded to .NET Core with AWS .NET Core SDK library and rewrite XML config to JSON config. However, after 10+ years of the initial code was published, I doubt anyone still needs to publish the home dynamic IP with a VM, everyone should have moved everything to the cloud. If you are still running this windows service in your basement, it's such as good match for this project to stay with traditional .NET Framework. 


Latest update: Visual Studio 2022 + .NET 4.8.1 are required. 

2024-02-08 (ver 1.0.0.7)
- Compiled with VS2022 (.NET 4.8.1)
- Updated AWSSDK.Core 3.7.302.7
- Updated AWSSDK.Route53 3.7.302.14
- Updated Topshelf 4.3.0
- Updated Unity 5.11.10

2020-01-22 (ver 1.0.0.6)
- Compiled with VS2019 (.NET 4.7.2)
- Updated AWSSDK.Core 3.3.104.19
- Updated AWSSDK.Route53 3.3.102.72

2020-01-12 (ver 1.0.0.5):
- Compiled with VS2017 (.NET 4.6.2), \v4.6.2\System.Web.Extensions.dll has JavaScriptSerializer
- Updated AWSSDK.Core v3.3.104.15
- Updated AWSSDK.Route53 v3.3.102.68
- Updated Topshelf V4.2.1
- Updated Unity v5.11.3
- Updated CommonServiceLocator v2.0.5
- It automatically added System.Runtime.CompilerServices.Unsafe.4.7.0

2016-11-07 (ver 1.0.0.4):
- Compiled with VS2015 (.NET 4.6.1)
- NuGet Update on Topshelf to 4.0.3
- Uninstalled Amazon AWS 2.3.53 and Installed AWSSDK.Route53 (3.3.1.1)
- Newtonsoft.Json.dll is needed from JavaScriptSerializer (Json.NET)
- Replaced AWSClientFactory.CreateAmazonRoute53Client (AWS obsolete API) with new AmazonRoute53Client

2016-05-18 (ver 1.0.0.3):
- dougkwilson on GitHub updated .NET v4.6.1, Topshelf v4.0.1 and AWSSDK v3

2015-12-19 (ver 1.0.0.2):
- Compiled with VS2015 (.NET 4.5.0)  
- Newtonsoft.Json.dll is missing compare to the old bin folder, remove it no longer been used
- NuGet Update Amazon AWS 2.3.19 to 2.3.53
- NuGet Update Topshelf 3.1.4 to 3.3.31
- NuGet Update Unity 3.5.1404 to 4.0.1

2015-12-03 (ver 1.0.0.1): 
- Added: HistoricalIPAddress and LastUpdatedReason in domain level for log and notification (forced/changed)
- Fixed: Not able to send email after timeout, Smtpclient is not disposed properly




