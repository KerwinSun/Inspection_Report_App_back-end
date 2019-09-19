using InspectionReport.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Services
{
    public interface IEmailService
    {
        void Send(EmailMessage emailMessage);
    }

    public class EmailService : IEmailService
    {
        private readonly IEmailConfiguration _emailConfiguration;

        public EmailService(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public void Send(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.Add(new MailboxAddress(emailMessage.to.Name, emailMessage.to.Address));
            message.From.Add(new MailboxAddress(emailMessage.from.Name, emailMessage.from.Address));

            message.Subject = emailMessage.subject;
            //We will say we are sending HTML. But there are options for plaintext etc.
            //message.Body = new TextPart("TextFormat.Html")
            message.Body = new TextPart("html")
            {
                Text = emailMessage.content
            };
            /*
            var body = new TextPart("TextFormat.Html")
            {
                Text = emailMessage.content
            };

            
            var attachment = new MimePart("application", "pdf")
            {
                Content = new MimeContent(file),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(file.path);
            }

            var multipart = new Multipart ("mixed");
            multipart.Add (body);
            multipart.Add (attachment);

            // now set the multipart/mixed as the message body
            message.Body = multipart;
            */

            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            using (var emailClient = new SmtpClient())
            {
                //The last parameter here is to use SSL (Which you should!)
                emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, true);

                //Remove any OAuth functionality as we won't be using it. 
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

                emailClient.Send(message);

                emailClient.Disconnect(true);
            }
        }
    }
}
