using EAGetMail;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using TMS.Models;

namespace TMS.Utils
{
    public class EmailUtil
    {
        static ILog log = LogManager.GetLogger(typeof(EmailUtil));
        public static async void SendToUserWhenCreate(string username, string password, string fullname, string email)
        {
            ILog log = LogManager.GetLogger(typeof(EmailUtil));
            string emailSubject = "[TMS] Account Info";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/CreateRequesterEmailTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", fullname);
            emailMessage = emailMessage.Replace("$username", username);
            emailMessage = emailMessage.Replace("$password", password);
            try
            {
                await SendEmail("huytcdse61256@fpt.edu.vn", emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send user account email to {0} unsuccessfully!", username));
            }
        }

        public static async Task<bool> ResendToUserWhenCreate(string username, string password, string fullname, string email)
        {
            string emailSubject = "[TMS] Account Info";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/CreateRequesterEmailTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", fullname);
            emailMessage = emailMessage.Replace("$username", username);
            emailMessage = emailMessage.Replace("$password", password);
            try
            {
                await SendEmail("huytcdse61256@fpt.edu.vn", emailSubject, emailMessage);
                return true;
            }
            catch
            {
                log.Warn(string.Format("Resend email to {0} unsuccessfully!", username));
                return false;
            }
        }

        public static async void SendToTechnicianWhenCancelTicket(Ticket ticket, AspNetUser technician)
        {
            string emailSubject = "[TMS] Cancel Ticket Notification";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/CancelTicketTechnicianTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", technician.Fullname);
            emailMessage = emailMessage.Replace("$code", ticket.Code);
            emailMessage = emailMessage.Replace("$subject", ticket.Subject);
            emailMessage = emailMessage.Replace("$description", (ticket.Description == null) ? "-" : ticket.Description.Replace(Environment.NewLine, "<br />"));
            try
            {
                await SendEmail("huytcdse61256@fpt.edu.vn", emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send cancel ticket notification email to {0} unsuccessfully!", technician.Fullname));
            }
        }

        public static async void SendToTechnicianWhenAssignTicket(Ticket ticket, AspNetUser technician)
        {
            string emailSubject = "[TMS] Assign Ticket Notification";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/AssignTicketTechnicianTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", technician.Fullname);
            emailMessage = emailMessage.Replace("$code", ticket.Code);
            emailMessage = emailMessage.Replace("$subject", ticket.Subject);
            emailMessage = emailMessage.Replace("$description", (ticket.Description == null) ? "-" : ticket.Description.Replace(Environment.NewLine, "<br />"));
            emailMessage = emailMessage.Replace("$scheduleStartDate", (ticket.ScheduleStartDate == null) ? "-" : ticket.ScheduleStartDate.ToString());
            emailMessage = emailMessage.Replace("$scheduleEndDate", (ticket.ScheduleEndDate == null) ? "-" : ticket.ScheduleEndDate.ToString());
            try
            {
                await SendEmail("huytcdse61256@fpt.edu.vn", emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send assign ticket notification email to {0} unsuccessfully!", technician.Fullname));
            }
        }

        public static async void SendToHelpdesksWhenTicketIsOverdue(Ticket ticket, IEnumerable<AspNetUser> helpdesks)
        {
            string emailSubject = "[TMS] Overdue Ticket Notification";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/OverdueTicketHelpdeskTemplate.txt"));
            foreach (AspNetUser helpdesk in helpdesks)
            {
                emailMessage = emailMessage.Replace("$fullname", helpdesk.Fullname);
                emailMessage = emailMessage.Replace("$code", ticket.Code);
                emailMessage = emailMessage.Replace("$delayDay", ((int)(DateTime.Now.Date - ticket.ScheduleEndDate.Value.Date).TotalDays).ToString());
                try
                {
                    await SendEmail("huytcdse61256@fpt.edu.vn", emailSubject, emailMessage);
                }
                catch
                {
                    log.Warn(string.Format("Send overdue ticket notification email to {0} unsuccessfully!", helpdesk.Fullname));
                }
            }
        }

        public static async void SendToRequesterWhenCloseTicket(Ticket ticket, AspNetUser requester)
        {
            string emailSubject = "[TMS] Closed Ticket Notification";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/CloseTicketRequesterTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", requester.Fullname);
            emailMessage = emailMessage.Replace("$code", ticket.Code);
            emailMessage = emailMessage.Replace("$subject", ticket.Subject);
            emailMessage = emailMessage.Replace("$description", (ticket.Description == null) ? "-" : ticket.Description.Replace(Environment.NewLine, "<br />"));
            try
            {
                await SendEmail("huytcdse61256@fpt.edu.vn", emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send closed ticket notification email to {0} unsuccessfully!", requester.Fullname));
            }
        }

        public static async void SendToRequesterWhenSolveTicket(Ticket ticket, AspNetUser requester)
        {
            string emailSubject = "[TMS] Solved Ticket Notification";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/SolveTicketRequesterTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", requester.Fullname);
            emailMessage = emailMessage.Replace("$code", ticket.Code);
            emailMessage = emailMessage.Replace("$subject", ticket.Subject);
            emailMessage = emailMessage.Replace("$description", (ticket.Description == null) ? "-" : ticket.Description.Replace(Environment.NewLine, "<br />"));
            try
            {
                await SendEmail("huytcdse61256@fpt.edu.vn", emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send solved ticket notification email to {0} unsuccessfully!", requester.Fullname));
            }
        }

        public static async void SendToRequesterWhenCreateTicket(Ticket ticket, AspNetUser requester)
        {
            string emailSubject = "[TMS] Created Ticket Notification";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/CreateTicketRequesterTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", requester.Fullname);
            emailMessage = emailMessage.Replace("$code", ticket.Code);
            emailMessage = emailMessage.Replace("$subject", ticket.Subject);
            emailMessage = emailMessage.Replace("$description", (ticket.Description == null) ? "-" : ticket.Description.Replace(Environment.NewLine, "<br />"));
            try
            {
                await SendEmail("huytcdse61256@fpt.edu.vn", emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send created ticket notification email to {0} unsuccessfully!", requester.Fullname));
            }
        }

        public static async void SendToRequesterWhenCancelTicket(Ticket ticket, AspNetUser requester)
        {
            string emailSubject = "[TMS] Cancelled Ticket Notification";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/CancelTicketRequesterTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", requester.Fullname);
            emailMessage = emailMessage.Replace("$code", ticket.Code);
            emailMessage = emailMessage.Replace("$subject", ticket.Subject);
            emailMessage = emailMessage.Replace("$description", (ticket.Description == null) ? "-" : ticket.Description.Replace(Environment.NewLine, "<br />"));
            try
            {
                await SendEmail("huytcdse61256@fpt.edu.vn", emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send cancelled ticket notification email to {0} unsuccessfully!", requester.Fullname));
            }
        }

        public static async void SendToTechnicianWhenBusinessRuleIsApplied(Ticket ticket, AspNetUser technician)
        {
            string emailSubject = "[TMS] Business Rule Is Applied";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/BusinessRuleAppliedTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", technician.Fullname);
            emailMessage = emailMessage.Replace("$code", ticket.Code);
            emailMessage = emailMessage.Replace("$subject", ticket.Subject);
            emailMessage = emailMessage.Replace("$description", (ticket.Description == null) ? "-" : ticket.Description.Replace(Environment.NewLine, "<br />"));
            try
            {
                await SendEmail(technician.Email, emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send cancel ticket notification email to {0} unsuccessfully!", technician.Fullname));
            }
        }

        private static async Task SendEmail(string toEmailAddress, string emailSubject, string emailMessage)
        {
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
            }
            catch (Exception e)
            {
                log.Debug(e);
                throw;
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