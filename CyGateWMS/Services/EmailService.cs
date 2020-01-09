using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using SmtpClient = System.Net.Mail.SmtpClient;

namespace CyGateWMS.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration Configuration;
        public EmailService(IConfiguration config)
        {
            this.Configuration = config;
        }
        public async Task SendEmailAsync(string email, string subject, string message)
        {          
            try
            {
                MailMessage mailMessage = new MailMessage();

                MailAddress fromAddress = new MailAddress(Configuration.GetSection("EmailConfiguration:fromAddress").Value);

                mailMessage.From = fromAddress;

                mailMessage.To.Add(email);

                mailMessage.Body = message;

                mailMessage.IsBodyHtml = true;

                mailMessage.Subject = subject;

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Port = 25;
                smtpClient.Host = "localhost";
                smtpClient.Send(mailMessage);
            }
            catch(System.Exception ex)
            {
                string fileName= Configuration.GetSection("Logging:Location").Value;
                if (!File.Exists(fileName))
                    File.Create(fileName);                

                File.WriteAllText(fileName, ex.InnerException.Message + "-" + ex.InnerException.InnerException);
            }
        }


        public async Task SendEmailAsync(string email, string subject, string message, string content)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();

                MailAddress fromAddress = new MailAddress(Configuration.GetSection("EmailConfiguration:fromAddress").Value);

                mailMessage.From = fromAddress;

                mailMessage.To.Add(email);

                mailMessage.Body = message;

                mailMessage.IsBodyHtml = true;

                mailMessage.Subject = subject;

                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(content);
                writer.Flush();
                stream.Position = 0;
                Attachment attachment = new Attachment(stream, "Roster.xls");
                attachment.ContentType = new ContentType("application/vnd.ms-excel");
                attachment.TransferEncoding = TransferEncoding.Base64;
                attachment.NameEncoding = Encoding.UTF8;
                string encodedAttachmentName = Convert.ToBase64String(Encoding.UTF8.GetBytes("Roster.xls"));
                mailMessage.Attachments.Add(attachment);

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Port = 25;
                smtpClient.Host = "localhost";
                smtpClient.Send(mailMessage);
            }
            catch (System.Exception ex)
            {
                string fileName = Configuration.GetSection("Logging:Location").Value;
                if (!File.Exists(fileName))
                    File.Create(fileName);

                File.WriteAllText(fileName, ex.InnerException.Message + "-" + ex.InnerException.InnerException);
            }
        }

    }
}
