using E_Radar.Data;
using E_Radar.Data.Models;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace E_Radar
{
    public class MailGetter : IDisposable
    {
        private readonly string _hostName;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;
        private readonly bool _ssl;
        private readonly string[] _subjectSearchTerms;
        private readonly string[] _bodySearchTerms;
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

        public MailGetter(EmailClient clientOptions)
        {
            _hostName = clientOptions.ServerSettings.HostName;
            _port = clientOptions.ServerSettings.IMAPPort;
            _userName = clientOptions.Credentials.UserName;
            _password = clientOptions.Credentials.Password;
            _ssl = clientOptions.ServerSettings.UseSSL;
            _subjectSearchTerms = clientOptions.SearchTerms.Subject;
            _bodySearchTerms = clientOptions.SearchTerms.Body;

        }
        public async Task GetNewMessages()
        {
            using var client = new ImapClient();

            client.Connect(_hostName, _port, _ssl);
            await client.AuthenticateAsync(_userName, _password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            Console.WriteLine($"Total Messages: {inbox.Count}");
            Console.WriteLine($"Recent Messages: {inbox.Recent}");

            await searchMessages(inbox);

            await client.DisconnectAsync(true);
        }

        private async Task searchMessages(IMailFolder inbox)
        {
            //Create a list to store messages that match the search criteria.
            List<MimeMessage> matchingMessages = new List<MimeMessage>();

            foreach (var summary in await inbox.FetchAsync(0, -1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId))
            {
                foreach (var term in _subjectSearchTerms)
                {
                    if (summary.Envelope.Subject != null)
                    {
                        if (summary.Envelope.Subject.ToLower().Contains(term.ToLower()))
                        {
                            var message = await inbox.GetMessageAsync(summary.UniqueId);
                            matchingMessages.Add(message);
                        }
                    }
                }

                foreach (var term in _bodySearchTerms)
                {
                    if (summary.TextBody != null)
                    {
                        if (summary.TextBody.ToString().ToLower().Contains(term.ToLower()))
                        {
                            var message = await inbox.GetMessageAsync(summary.UniqueId);
                            matchingMessages.Add(message);
                        }
                    }
                }
            }

            await sendToDatabase(matchingMessages);
        }

        private async Task sendToDatabase(List<MimeMessage> messages)
        {
            await using var context = new E_RadarDbContext();
            await context.Database.MigrateAsync();

            foreach (var message in messages)
            {
                if (!await context.Messages.AnyAsync(x => x.UniqueId == message.MessageId))
                {
                    Console.WriteLine($"Found new matching message {message.From}, {message.Subject}.");

                    var newMessage = new MessageModel()
                    {
                        CreatedTime = DateTime.UtcNow,
                        SentTime = DateTime.Parse(message.Date.ToString()),
                        Notified = false,
                        UniqueId = message.MessageId,
                        From = message.From != null ? message.From?.ToString() : message.ReplyTo?.ToString(),
                        To = message.To.ToString(),
                        Cc = message.Cc?.ToString(),
                        Bcc = message.Bcc?.ToString(),
                        Subject = message.Subject?.ToString(),
                        Body = message.TextBody

                    };
                    await context.Messages.AddAsync(newMessage);
                    await context.SaveChangesAsync();
                }
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

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources, but leave the other methods
        // exactly as they are.
        ~MailGetter()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
    }
}