using EAGetMail;
using log4net;
using OpenPop.Mime;
using OpenPop.Pop3;
using S22.Imap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Hosting;
using TMS.Models;
using System.Linq;

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
                await SendEmail(email, emailSubject, emailMessage);
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
                await SendEmail(email, emailSubject, emailMessage);
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
                await SendEmail(technician.Email, emailSubject, emailMessage);
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
                await SendEmail(technician.Email, emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send assign ticket notification email to {0} unsuccessfully!", technician.Fullname));
            }
        }

        public static async void SendToTechnicianWhenTicketIsOverdue(Ticket ticket, AspNetUser technician)
        {
            string emailSubject = "[TMS] Overdue Ticket Notification";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/OverdueTicketTechnicianTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", technician.Fullname);
            emailMessage = emailMessage.Replace("$code", ticket.Code);
            emailMessage = emailMessage.Replace("$delayDay", ((int)(DateTime.Now.Date - ticket.DueByDate.Date).TotalDays).ToString());
            try
            {
                await SendEmail(technician.Email, emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send overdue ticket notification email to {0} unsuccessfully!", technician.Fullname));
            }
        }

        public static async void SendToHelpdesksWhenTicketIsOverdue(Ticket ticket, IEnumerable<AspNetUser> helpdesks)
        {
            string emailSubject = "[TMS] Overdue Ticket Notification";
            foreach (AspNetUser helpdesk in helpdesks)
            {
                string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/OverdueTicketTechnicianTemplate.txt"));
                emailMessage = emailMessage.Replace("$fullname", helpdesk.Fullname);
                emailMessage = emailMessage.Replace("$code", ticket.Code);
                emailMessage = emailMessage.Replace("$delayDay", ((int)(DateTime.Now.Date - ticket.DueByDate.Date).TotalDays).ToString());
                try
                {
                    await SendEmail(helpdesk.Email, emailSubject, emailMessage);
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
                await SendEmail(requester.Email, emailSubject, emailMessage);
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
                await SendEmail(requester.Email, emailSubject, emailMessage);
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
                await SendEmail(requester.Email, emailSubject, emailMessage);
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
                await SendEmail(requester.Email, emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send cancelled ticket notification email to {0} unsuccessfully!", requester.Fullname));
            }
        }

        public static async void SendToHelpDeskWhenBusinessRuleIsApplied(Ticket ticket, AspNetUser helpdesk)
        {
            string emailSubject = "[TMS] Business Rule Is Applied";
            string emailMessage = File.ReadAllText(HostingEnvironment.MapPath(@"~/EmailTemplates/BusinessRuleAppliedTemplate.txt"));
            emailMessage = emailMessage.Replace("$fullname", helpdesk.Fullname);
            emailMessage = emailMessage.Replace("$code", ticket.Code);
            emailMessage = emailMessage.Replace("$subject", ticket.Subject);
            emailMessage = emailMessage.Replace("$description", (ticket.Description == null) ? "-" : ticket.Description.Replace(Environment.NewLine, "<br />"));

            try
            {
                await SendEmail(helpdesk.Email, emailSubject, emailMessage);
            }
            catch
            {
                log.Warn(string.Format("Send cancel ticket notification email to {0} unsuccessfully!", helpdesk.Fullname));
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

        public static List<Message> GetUnreadMailsUsingOpenPop(List<string> seenUids)
        {
            ILog log = LogManager.GetLogger(typeof(EmailUtil));

            // The client disconnects from the server when being disposed
            using (Pop3Client client = new Pop3Client())
            {
                try
                {
                    // Connect to the server
                    client.Connect("pop.gmail.com", 995, true);

                    // Authenticate ourselves towards the server
                    //client.Authenticate(ConstantUtil.ContactEmailInfo.MailAddress, ConstantUtil.ContactEmailInfo.Password);
                    client.Authenticate("tms.g4.hotro@gmail.com", "mdx4kISu");

                    // Fetch all the current uids seen
                    List<string> uids = client.GetMessageUids();

                    // Create a list we can return with all new messages
                    List<Message> newMessages = new List<Message>();

                    // All the new messages not seen by the POP3 client
                    for (int i = 0; i < uids.Count; i++)
                    {
                        string currentUidOnServer = uids[i];
                        if (!seenUids.Contains(currentUidOnServer))
                        {
                            // We have not seen this message before.
                            // Download it and add this new uid to seen uids

                            // the uids list is in messageNumber order - meaning that the first
                            // uid in the list has messageNumber of 1, and the second has 
                            // messageNumber 2. Therefore we can fetch the message using
                            // i + 1 since messageNumber should be in range [1, messageCount]
                            Message unseenMessage = client.GetMessage(i + 1);

                            // Add the message to the new messages
                            newMessages.Add(unseenMessage);

                            // Add the uid to the seen uids, as it has now been seen
                            seenUids.Add(currentUidOnServer);
                        }
                    }

                    // Return our new found messages
                    return newMessages;
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                    return new List<Message>();
                }
            }
        }

        public static List<MailMessage> GetUnreadMailsUsingS22()
        {
            ILog log = LogManager.GetLogger(typeof(EmailUtil));
            try
            {
                //Uri myUri = new Uri("imap.gmail.com", UriKind.Absolute);
                // The client disconnects from the server when being disposed
                using (ImapClient client = new ImapClient("imap.gmail.com", 993, ConstantUtil.ContactEmailInfo.MailAddress, ConstantUtil.ContactEmailInfo.Password, AuthMethod.Login, true))
                {
                    // Returns a collection of identifiers of all mails matching the specified search criteria.
                    IEnumerable<uint> uids = client.Search(SearchCondition.Unseen());
                    // Download mail messages from the default mailbox.
                    IEnumerable<MailMessage> messages = client.GetMessages(uids);
                    return messages.ToList();
                }
            }
            catch (Exception e)
            {
                log.Error("Cannot get unread emails", e);
                return new List<MailMessage>();
            }
        }
    }
}