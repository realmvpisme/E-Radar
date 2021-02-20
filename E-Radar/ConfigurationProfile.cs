using System.Collections.Generic;

namespace E_Radar
{
    public class ConfigurationProfile
    {
        public EmailClient EmailClient { get; set; }
        public EmailSender EmailSender { get; set; }
    }

    public class EmailClient
    {
        public ServerSettings ServerSettings { get; set; }
        public Credentials Credentials { get; set; }
        public SearchTerms SearchTerms { get; set; }
    }
    public class ServerSettings
    {
        public string HostName { get; set; }
        public int IMAPPort { get; set; }
        public bool UseSSL { get; set; }
        public int TimeDelay { get; set; }
    }

    public class Credentials
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class SearchTerms
    {
        public string[] Subject { get; set; }
        public string[] Body { get; set; }
    }

    public class EmailSender
    {
        public SenderSettings SenderSettings { get; set; }
        public Credentials Credentials { get; set; }
    }

    public class SenderSettings
    {
        public string SenderName { get; set; }
        public string RecipientName { get; set; }
        public string HostName { get; set; }
        public int SMTPPort { get; set; }
        public bool UseSSL { get; set; }
        public string SMSEmailAddress { get; set; }
        public string EmailSubject { get; set; }
    }
}