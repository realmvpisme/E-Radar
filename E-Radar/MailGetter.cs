using System;
using System.Collections.Generic;
using MailKit.Net.Imap;
using System.Threading.Tasks;
using MailKit;
using MimeKit;
using E_Radar.Data;
using Microsoft.EntityFrameworkCore;
using E_Radar.Data.Models;

namespace E_Radar
{
    public class MailGetter
    {
        private readonly string _hostName;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;
        private readonly bool _ssl;
        private readonly string[] _subjectSearchTerms;
        private readonly string[] _bodySearchTerms;

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
            using (var client = new ImapClient())
            {
               


                client.Connect(_hostName, _port, _ssl);
                await client.AuthenticateAsync(_userName, _password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                Console.WriteLine($"Total Messages: {inbox.Count}");
                Console.WriteLine($"Recent Messages: {inbox.Recent}");

                await searchMessages(inbox);
              

                await client.DisconnectAsync(true);
            }
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
                            Console.WriteLine($"Found matching message {message.From}, {message.Subject}.");

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
                            Console.WriteLine($"Found matching message {message.From}, {message.Subject}.");
                        }
                    }
                }
            }

            await sendToDatabase(matchingMessages);
        }

        private async Task sendToDatabase(List<MimeMessage> messages)
        {
            using(var context = new E_RadarDbContext())
            {
                await context.Database.MigrateAsync();

                foreach (var message in messages)
                {
                    if(!await context.Messages.AnyAsync(x => x.UniqueId == message.MessageId))
                    {
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
            
        }
    }
}