using E_Radar.Data;
using E_Radar.Data.Models;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace E_Radar
{
    public class MailSender : IDisposable
    {
        private readonly string _senderName;
        private readonly string _hostName;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;
        private readonly bool _ssl;
        private readonly string _smsEmailAddress;
        private readonly string _recipientName;
        private readonly string _emailSubject;
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);


        public MailSender(EmailSender senderOptions)
        {
            _senderName = senderOptions.SenderSettings.SenderName;
            _hostName = senderOptions.SenderSettings.HostName;
            _port = senderOptions.SenderSettings.SMTPPort;
            _userName = senderOptions.Credentials.UserName;
            _password = senderOptions.Credentials.Password;
            _ssl = senderOptions.SenderSettings.UseSSL;
            _smsEmailAddress = senderOptions.SenderSettings.SMSEmailAddress;
            _recipientName = senderOptions.SenderSettings.RecipientName;
            _emailSubject = senderOptions.SenderSettings.EmailSubject;

        }
        public async Task SendNotifications()
        {
            using (var context = new E_RadarDbContext())
            {
                var unsentNotifications = await context.Messages.Where(x => !x.Notified).ToListAsync();

                if (unsentNotifications.Any())
                {
                    foreach (var unsentNotification in unsentNotifications)
                    {
                        await sendSingleNotification(unsentNotification);
                        await updateNotifiedMessage(unsentNotification, context);

                    }
                }

            }
        }

        private async Task sendSingleNotification(MessageModel emailToNotify)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_senderName, _userName));
            message.To.Add(new MailboxAddress(_recipientName, _smsEmailAddress));
            message.Subject = _emailSubject;

            message.Body = new TextPart("plain")
            {
                Text = $"You've received a new email from {message.From} regarding {message.Subject}."
            };

            using (var client = new SmtpClient())
            {
                client.Connect(_hostName, _port, _ssl);
                await client.AuthenticateAsync(_userName, _password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                await logSentNotification(message);
            }
        }

        private async Task updateNotifiedMessage(MessageModel message, E_RadarDbContext context)
        {
            message.Notified = true;
            context.Update(message);
            await context.SaveChangesAsync();
        }

        private async Task logSentNotification(MimeMessage message)
        {
            using (var context = new E_RadarDbContext())
            {
                var newSentMessage = new SentMessageModel()
                {
                    CreatedTime = DateTime.UtcNow,
                    SentTime = DateTime.Parse(message.Date.ToString()),
                    UniqueId = message.MessageId,
                    From = message.From.ToString(),
                    To = message.To.ToString(),
                    Subject = message.Subject,
                    Body = message.TextBody
                };

                await context.SentMessages.AddAsync(newSentMessage);
                await context.SaveChangesAsync();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                // free managed resources
            }

            if (nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }

            isDisposed = true;
        }

        ~MailSender()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

    }
}