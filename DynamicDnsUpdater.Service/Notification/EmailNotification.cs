using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using DynamicDnsUpdater.Service.Configuration;
using DynamicDnsUpdater.Service.Interface;

namespace DynamicDnsUpdater.Service.Notification
{
    public class EmailNotification : INotification
    {
        /// <summary>
        /// Simple Send email 
        /// </summary>
        /// <param name="body"></param>
        public void Send(string body)
        {            
            var fromAddress = new MailAddress(ConfigHelper.FromEmail);
            var toAddress = new MailAddress(ConfigHelper.ToEmail);
            string password = ConfigHelper.Password;
             string subject = ConfigHelper.Subject;

            var smtp = new SmtpClient
            {
                Host = ConfigHelper.Host,
                Port = Convert.ToInt32(ConfigHelper.Port),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, password),
                Timeout = 20000
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Subject = subject,
                Body = body
                
            })
            {
                smtp.Send(message);
            }
        } 
    }
}
