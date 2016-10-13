using EAGetMail;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace TMS.Utils
{
    public class EmailUtil
    {
        public static async void SendToUserWhenCreate(string username, string password, string fullname, string email)
        {
            ILog log = LogManager.GetLogger(typeof(EmailUtil));
            string emailSubject = "[TMS] Account Info";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/CreateRequesterEmailTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", fullname);
            emailMessage = emailMessage.Replace("$username", username);
            emailMessage = emailMessage.Replace("$password", password);
            await SendEmail("huytcdse61256@fpt.edu.vn", emailSubject, emailMessage);
        }

        public static async Task<bool> ResendToUserWhenCreate(string username, string password, string fullname, string email)
        {
            string emailSubject = "[TMS] Account Info";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/CreateRequesterEmailTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", fullname);
            emailMessage = emailMessage.Replace("$username", username);
            emailMessage = emailMessage.Replace("$password", password);
            return await SendEmail("huytcdse61256@fpt.edu.vn", emailSubject, emailMessage);
        }

        private static async Task<bool> SendEmail(string toEmailAddress, string emailSubject, string emailMessage)
        {
            ILog log = LogManager.GetLogger(typeof(EmailUtil));
            try
            {
                var message = new MailMessage();
                message.To.Add(toEmailAddress);
                message.From = new System.Net.Mail.MailAddress(ConstantUtil.ContactEmailInfo.MailAddress, "TMS-Support-Service");
                message.Subject = emailSubject;
                message.Body = emailMessage;
                message.IsBodyHtml = true;
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential
                    {
                        UserName = ConstantUtil.ContactEmailInfo.MailAddress,
                        Password = ConstantUtil.ContactEmailInfo.Password
                    };
                    smtpClient.Host = "smtp.gmail.com";
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;
                    await smtpClient.SendMailAsync(message);
                }
                return true;
            }
            catch (Exception e)
            {
                log.Debug(e);
                return false;
            }
        }

        public static List<Mail> GetUnreadMailsUsingEAGetMail()
        {
            ILog log = LogManager.GetLogger(typeof(EmailUtil));
            MailServer oServer = new MailServer("imap.gmail.com", ConstantUtil.ContactEmailInfo.MailAddress,
                ConstantUtil.ContactEmailInfo.Password, ServerProtocol.Imap4);
            MailClient oClient = new MailClient("TryIt");

            // Set SSL connection,
            oServer.SSLConnection = true;
            oClient.GetMailInfosParam.Reset();
            oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.NewOnly;
            // Set 993 IMAP4 port
            oServer.Port = 993;

            List<Mail> mailList = new List<Mail>();
            try
            {
                oClient.Connect(oServer);
                MailInfo[] infos = oClient.GetMailInfos();
                for (int i = 0; i < infos.Length; i++)
                {
                    MailInfo info = infos[i];

                    //Debug.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
                    //info.Index, info.Size, info.UIDL);

                    // Download email from GMail IMAP4 server
                    Mail oMail = oClient.GetMail(info);

                    //Debug.WriteLine("From: " + oMail.From.ToString());
                    //Debug.WriteLine("Subject: " + oMail.Subject + "\r\n");
                    //Debug.WriteLine("Content: " + oMail.TextBody + "\r\n");
                    //Debug.WriteLine("Number of attachment: " + oMail.Attachments.Length + "\r\n");

                    mailList.Add(oMail);

                    // mark read email as unread 
                    oClient.MarkAsRead(info, true);
                }

                // Quit and pure emails marked as deleted from Gmail IMAP4 server.
                oClient.Quit();
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
            return mailList;
        }

    }
}