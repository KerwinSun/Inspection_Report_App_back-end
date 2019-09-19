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
        Task SendAsync(MimeMessage message);
        Task SendAsync(EmailMessage emailMessage);
    }

    public class EmailService : IEmailService
    {
        private readonly IEmailConfiguration _emailConfiguration;

        public EmailService(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }


        public async Task SendAsync(MimeMessage message)
        {
            using (var emailClient = new SmtpClient())
            {
                if (!emailClient.IsConnected)
                {
                    // Last parameter is SSL
                    await emailClient.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, true);
                    await emailClient.AuthenticateAsync(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);
                }
                await emailClient.SendAsync(message);
                await emailClient.DisconnectAsync(true);
            }
        }

        public async Task SendAsync(EmailMessage emailMessage)
        {
            MimeMessage message = new MimeMessage();
            message.To.Add(new MailboxAddress(emailMessage.to.Name, emailMessage.to.Address));
            message.From.Add(new MailboxAddress(_emailConfiguration.SenderName,_emailConfiguration.SmtpUsername));
            message.Subject = emailMessage.subject;

            var builder = new BodyBuilder { HtmlBody = emailMessage.body };

            if (emailMessage.file != null)
            {
                builder.Attachments.Add(emailMessage.fname, emailMessage.file);
            }
            message.Body = builder.ToMessageBody();

            await SendAsync(message);
        }
    }
}
