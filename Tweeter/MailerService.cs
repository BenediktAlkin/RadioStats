using Serilog;
using System;
using System.Net;
using System.Net.Mail;

namespace Tweeter
{
    public record MailerServiceConfig(string SmtpHost, int Port, string SenderEmail, string SenderPassword);

    public class MailerService
    {
        private MailerServiceConfig Config { get; set; }
        public MailerService(MailerServiceConfig config)
        {
            Config = config;
        }

        public void SendErrorMail(string targetEmail, string subject, string message)
        {
            using var smtpClient = new SmtpClient
            {
                Host = Config.SmtpHost,
                Port = Config.Port,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Config.SenderEmail, Config.SenderPassword),
            };
            using var mail = new MailMessage(Config.SenderEmail, targetEmail)
            {
                Subject = subject,
                Body = message,
                IsBodyHtml = false,
            };
            try
            {
                smtpClient.Send(mail);
            }
            catch (Exception e)
            {
                Log.Error($"failed to send message from {Config.SenderEmail} to {targetEmail}: {e.Message}");
            }
        }
    }
}
