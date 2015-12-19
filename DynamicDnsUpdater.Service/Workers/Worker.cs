using DynamicDnsUpdater.Service.Clients;
using DynamicDnsUpdater.Service.Configuration;
using DynamicDnsUpdater.Service.Encryption;
using DynamicDnsUpdater.Service.Interface;
using DynamicDnsUpdater.Service.Models;
using DynamicDnsUpdater.Service.Notification;
using DynamicDnsUpdater.Service.Providers;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DynamicDnsUpdater.Service.Workers
{
    /// <summary>
    /// Timer class to periodically check for new treatment data
    /// </summary>
    public class Worker
    {
        // Timer for interval update and monitor status
        private Timer _updateTimer;
        private Timer _montiorTimer;

        // IoC for flexbile configuration
        private IUnityContainer _container = new UnityContainer();

        // XmlConfig (XML serialization objects) maps to Model Collection
        private List<IpCheckerModel> _ipCheckerModelList;
        private List<DomainModel> _domainModelList;


        /// <summary>
        /// Constructor
        /// </summary>
        public Worker()
        {
            try
            {

                // Init the enterprise log
                Logger.SetLogWriter(new LogWriterFactory().Create());
                Logger.Write("Updater worker Initialized.", Meta.Enum.LogCategoryType.WIN_SERVICE.ToString());

                // Mappings for Unity container
                _container.RegisterType<IDnsProvider, AmazonRoute53DnsProvider>(Meta.Enum.DnsProviderType.AMAZON_ROUTE_53.ToString());
                _container.RegisterType<IClient, WebHttpClient>(Meta.Enum.ClientType.WEB_HTTP.ToString());
                _container.RegisterType<IIpAddressChecker, CustomIpAddressChecker>(Meta.Enum.IpCheckerType.CUSTOM.ToString());
                _container.RegisterType<IIpAddressChecker, DynDnsIpAddressChecker>(Meta.Enum.IpCheckerType.DYN_DNS.ToString());
                _container.RegisterType<IIpAddressChecker, JsonIpAddressChecker>(Meta.Enum.IpCheckerType.JSON_IP.ToString());
                _container.RegisterType<INotification, EmailNotification>(Meta.Enum.NotificationType.EMAIL.ToString());

                // Read the XML config file for all the Domains/Providers/IpCheckers and Map them to Model
                MappingToModel(ConfigHelper.LoadConfig());


                // Configure the Timers and handlers
                _updateTimer = new Timer(Convert.ToDouble(ConfigHelper.UpdateIntervalInMinutes) * 1000 * 60);
                _updateTimer.Elapsed += new ElapsedEventHandler(UpdateTimerElapsed);

                _montiorTimer = new Timer(Convert.ToDouble(ConfigHelper.MonitorStatusInMinutes) * 1000 * 60);
                _montiorTimer.Elapsed += new ElapsedEventHandler(MonitorTimerElapsed);

            }
            catch (Exception ex)
            {
                Logger.Write(String.Format("FATAL ERROR, Exception={0}", ex.ToString()), Meta.Enum.LogCategoryType.WIN_SERVICE.ToString());
            }

        }

        /// <summary>
        /// Event hook for the update job
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.RunUpdate();
        }


        /// <summary>
        /// Monitor the update status after submit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonitorTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.RunMonitor();
        }


        /// <summary>
        /// Start the timer
        /// </summary>
        public void Start()
        {
            _updateTimer.Start();
            _montiorTimer.Start();
            Logger.Write("Service has started.", Meta.Enum.LogCategoryType.WIN_SERVICE.ToString());

            // First time running it no waiting 
            this.RunUpdate();
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        public void Stop()
        {
            _updateTimer.Stop();
            _montiorTimer.Stop();
            this.CleanUp();
            Logger.Write("Service has stopped.", Meta.Enum.LogCategoryType.WIN_SERVICE.ToString());
        }

        /// <summary>
        /// Clean up
        /// </summary>
        private void CleanUp()
        {
            _container.Dispose();
        }



        /// <summary>
        /// Loop through the config files and update the IP
        /// </summary>
        public void RunUpdate()
        {

            string currentIpAddress = null;
            string updatedId = null;

            // Resolve the instance based on the xml config
            if (_domainModelList.Count() > 0)
            {
                // Loop through and update all domains
                foreach (DomainModel domain in _domainModelList)
                {
                    try
                    {
                        updatedId = null; // reset it for each domain                       

                        // Check if it meets the minimal update settings (some DNS update system blocks your update to prevent flooding if you update too frequtently)
                        bool isUpdatable = ((domain.LastUpdatedDateTime != null) &&
                            (((DateTime)domain.LastUpdatedDateTime).AddMinutes(Convert.ToInt32(domain.MinimalUpdateIntervalInMinutes)) < DateTime.UtcNow)) ? true : false;

                        // Check if we should force update with the preset days in config
                        bool isForceUpdate = ((domain.LastUpdatedDateTime != null) &&
                            (((DateTime)domain.LastUpdatedDateTime) < DateTime.UtcNow.AddDays(-Convert.ToInt32(ConfigHelper.ForceUpdateInDays)))) ? true : false;

                        // Get current IP (prevent from calling IPChecker for multiple times between domains)
                        if (currentIpAddress == null)
                            currentIpAddress = GetCurrentIpAddress();

                        // Check if able to get IP from IpChecker
                        if (currentIpAddress != null)
                        {
                            // Compare to last IP with current Ip
                            bool hasIpAddressChanged = ((String.IsNullOrEmpty(domain.LastIpAddress)) || (currentIpAddress.Trim() != domain.LastIpAddress.Trim())) ? true : false;

                            // Update the IP
                            if ((isUpdatable && isForceUpdate) || (isUpdatable && hasIpAddressChanged))
                            {
                                // Call the Provider to update the IP, then add it to the monitoring list
                                updatedId = domain.DnsProvider.UpdateDns(domain.AccessID, domain.SecretKey, domain.ProviderUrl, domain.DomainName, domain.HostedZoneId, currentIpAddress);

                                // Successful with ID return, update information in the xml
                                if (updatedId != null)
                                {
                                    if (isForceUpdate)
                                        Logger.Write(String.Format("Domain={0} - FORCE UPDATED provider successfully from IP={1} to IP={2} with ID={3}, passed {4} days", domain.DomainName, domain.LastIpAddress, currentIpAddress, updatedId, ConfigHelper.ForceUpdateInDays.ToString()), Meta.Enum.LogCategoryType.DNS_UPDATE.ToString());
                                    else
                                        Logger.Write(String.Format("Domain={0} - UPDATED provider successfully from IP={1} to IP={2} with ID={3}", domain.DomainName, domain.LastIpAddress, currentIpAddress, updatedId), Meta.Enum.LogCategoryType.DNS_UPDATE.ToString());

                                    // Update the model                                     
                                    domain.HistoricalIpAddress = domain.LastIpAddress; // save this for last history (has to be first)
                                    domain.LastIpAddress = currentIpAddress;
                                    domain.LastUpdatedDateTime = DateTime.UtcNow;
                                    domain.ChangeStatusID = updatedId;   // For monitoring inteval timer to use
                                    domain.LastUpdatedReason = (isForceUpdate ? Meta.Enum.UpdateReasonType.FORCED : Meta.Enum.UpdateReasonType.CHANGED);

                                    // Update Xml Config
                                    ConfigHelper.UpdateDomainInformation(domain);
                                    ConfigHelper.UpdateChangeStatusInformation(domain.DomainName, updatedId);

                                    Logger.Write(String.Format("Domain={0} - UPDATED XML configuration from {1} to {2}, LastUpdatedDateTime={3}, Reason={4}", domain.DomainName, domain.HistoricalIpAddress, domain.LastIpAddress, domain.LastUpdatedDateTime, Enum.GetName(typeof(Meta.Enum.UpdateReasonType), domain.LastUpdatedReason)), Meta.Enum.LogCategoryType.DNS_UPDATE.ToString());

                                }
                                else
                                {
                                    Logger.Write(String.Format("Domain={0} - NOT UPDATED because of unknown error with Provider={1}", domain.DomainName, domain.DnsProviderType.ToString()), Meta.Enum.LogCategoryType.DNS_UPDATE.ToString());
                                }
                            }
                            else
                            {
                                if ((!isForceUpdate) && (!hasIpAddressChanged))
                                    Logger.Write(String.Format("Domain={0} - NOT UPDATED because IP={1} has not been changed", domain.DomainName, currentIpAddress), Meta.Enum.LogCategoryType.DNS_UPDATE.ToString());

                                if (!isUpdatable)
                                    Logger.Write(String.Format("Domain={0} - NOT UPDATED because update is too often, mininal interval={1} minute(s)", domain.DomainName, domain.MinimalUpdateIntervalInMinutes), Meta.Enum.LogCategoryType.DNS_UPDATE.ToString());
                            }
                        } // else


                    } // try
                    catch (Exception ex)
                    {
                        Logger.Write(String.Format("Domain={0} - FATAL ERROR, Exception={1}", domain.DomainName, ex.ToString()), Meta.Enum.LogCategoryType.DNS_UPDATE.ToString());
                    }

                }
            }

        } // method



        /// <summary>
        /// Monitor the DNS update status after submitted to the provider
        /// </summary>
        public void RunMonitor()
        {
            List<DomainModel> notificationDomainList = new List<DomainModel>();

            // Loop through and update all domains
            foreach (DomainModel domain in _domainModelList.Where(x => x.ChangeStatusID != null))
            {
                try
                {
                    if (domain.DnsProvider.CheckUpdateStatus(domain.AccessID, domain.SecretKey, domain.ProviderUrl, domain.ChangeStatusID) == Meta.Enum.ChangeStatusType.INSYNC)
                    {
                        // Update config file and reset model
                        ConfigHelper.UpdateChangeStatusInformation(domain.DomainName, String.Empty); // ChangeStatusID = Empty = Insync
                        domain.ChangeStatusID = null;
                        notificationDomainList.Add(domain); // For notification

                        Logger.Write(String.Format("Domain={0} - XML configuration ChangeStatusType Updated to {1}", domain.DomainName, Meta.Enum.ChangeStatusType.INSYNC.ToString()), Meta.Enum.LogCategoryType.STATUS_MONITOR.ToString());
                    }
                    else
                        Logger.Write(String.Format("Domain={0} - ChangeStatus={1}", domain.DomainName, Meta.Enum.ChangeStatusType.PENDING.ToString()), Meta.Enum.LogCategoryType.STATUS_MONITOR.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Write(String.Format("Domain={0} -  FATAL ERROR during check change status, Exception={1}", domain.DomainName, ex.ToString()), Meta.Enum.LogCategoryType.STATUS_MONITOR.ToString());
                }
            }

            // Group send only 1 email for all completed domains
            if (notificationDomainList.Count > 0)
                SendNotification(notificationDomainList);

        }

        /// <summary>
        /// Get Ip Address from IpCheckers (If one fails, automatically try next one)
        /// </summary>
        /// <returns></returns>
        private string GetCurrentIpAddress()
        {
            string currentIpAddress = null;

            // Loop through the whole list, if one fails go to next one
            foreach (IpCheckerModel model in _ipCheckerModelList)
            {
                try
                {
                    currentIpAddress = model.IpAddressChecker.GetCurrentIpAddress(model.IpCheckerUrl, model.Client);
                    Logger.Write(String.Format("IpChecker={0}, IP={1}", model.IpCheckerUrl, currentIpAddress), Meta.Enum.LogCategoryType.IP_CHECKER.ToString());
                }
                catch (Exception ex)
                {
                    // Suppress the error, log it and continue to next one
                    Logger.Write(String.Format("Fail to get the IP from IpChecker={0}, Exception={1}", model.IpCheckerUrl, ex.ToString()), Meta.Enum.LogCategoryType.IP_CHECKER.ToString());
                }

                // Exit the loop and return the IP 
                if (currentIpAddress != null)
                    break;

            }

            // All failed
            if (currentIpAddress == null)
            {
                Logger.Write("FATAL ERROR, fail to get the IP from All IpCheckers.", Meta.Enum.LogCategoryType.IP_CHECKER.ToString());
                throw new InvalidOperationException();
            }

            return currentIpAddress;
        }


        /// <summary>
        /// Mapping XmlConfig to Model, AutoMapper can be used indeed
        /// </summary>
        /// <param name="xmlConfig"></param>
        private void MappingToModel(XmlConfig xmlConfig)
        {
            // Load Domains from Config, Resolve the DnsProvider instance based on configuration
            _domainModelList = new List<DomainModel>();
            foreach (Domain domain in xmlConfig.Domains)
            {
                DomainModel domainModel = new DomainModel();
                domainModel.DomainName = domain.DomainName;
                domainModel.DnsProviderType = domain.ProviderType;
                domainModel.DnsProvider = _container.Resolve<IDnsProvider>(domain.ProviderType.ToString());  // Resolve DnsProvider
                domainModel.ProviderUrl = ((Provider)xmlConfig.Providers.FirstOrDefault(x => x.ProviderType == domain.ProviderType)).ProviderUrl;  // Find the matching Provider (XmlConfig object) and get the URL, flatten it for this Model                        
                domainModel.HostedZoneId = domain.HostedZoneId;
                domainModel.AccessID = domain.AccessID;

                // Decrypt the string if it the data is encrypted
                if (ConfigHelper.EnablePasswordEncryption)
                    domainModel.SecretKey = Des3.Decrypt(domain.SecretKey, ConfigHelper.EncryptionKey);
                else
                    domainModel.SecretKey = domain.SecretKey;
               
                domainModel.MinimalUpdateIntervalInMinutes = domain.MinimalUpdateIntervalInMinutes;
                domainModel.LastIpAddress = domain.LastIpAddress;
                domainModel.LastUpdatedDateTime = domain.LastUpdatedDateTime;

                if (domainModel.DnsProvider == null)
                {
                    Logger.Write("Cannot resolve Provider, misconfiguration in XML config file in Domain section.", Meta.Enum.LogCategoryType.CONFIGURATION.ToString());
                    throw new ArgumentNullException();
                }

                if (String.IsNullOrWhiteSpace(domain.ChangeStatusID))
                    domainModel.ChangeStatusID = null;
                else
                    domainModel.ChangeStatusID = domain.ChangeStatusID;

                _domainModelList.Add(domainModel);
            }

            // Load IpCheckers from Config, Resolve the instance
            _ipCheckerModelList = new List<IpCheckerModel>();
            foreach (IpChecker ipChecker in xmlConfig.IpCheckers)
            {
                // Mapping from Config to Model
                IpCheckerModel model = new IpCheckerModel();
                model.IpCheckerType = ipChecker.IpCheckerType;
                model.IpCheckerUrl = ipChecker.IpCheckerUrl;
                model.ClientType = ipChecker.ClientType;
                model.Client = _container.Resolve<IClient>(ipChecker.ClientType.ToString());      // Resolve Client
                model.IpAddressChecker = _container.Resolve<IIpAddressChecker>(ipChecker.IpCheckerType.ToString());  // Resolve IpAddressChecker

                if ((model.Client == null) || (model.IpAddressChecker == null))
                {
                    Logger.Write("Cannot resolve Checker or Client, misconfiguration in XML config file in IpChecker section.", Meta.Enum.LogCategoryType.CONFIGURATION.ToString());
                    throw new ArgumentNullException();
                }

                _ipCheckerModelList.Add(model);
            }
        } // method


        /// <summary>
        /// Send notification
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="ip"></param>
        private void SendNotification(List<DomainModel> domainModelList)
        {
            try
            {
                string body = "The following domains have been updated:<br/><br/>";
                foreach (DomainModel item in domainModelList)
                {
                    body += String.Format("Domain={0}, IP from {1} to IP={2}, Local Time={3}, Reason={4}", item.DomainName, item.HistoricalIpAddress, item.LastIpAddress, item.LastUpdatedDateTime.ToLocalTime(), Enum.GetName(typeof(Meta.Enum.UpdateReasonType), item.LastUpdatedReason)) + "<br/>";
                }

                // Send email (only email is supported for now, can add Facebook or others in future)
                INotification notification = _container.Resolve<INotification>(Meta.Enum.NotificationType.EMAIL.ToString());
                notification.Send(body);

                Logger.Write(String.Format("Notification has been sent successfully"), Meta.Enum.LogCategoryType.NOTIFICATION.ToString());
            }
            catch (Exception ex)
            {
                Logger.Write(String.Format("Fail to send notification, Exception={0}.",ex.ToString()) , Meta.Enum.LogCategoryType.NOTIFICATION.ToString());
                
            }

        }

    } // class


} // namespace
