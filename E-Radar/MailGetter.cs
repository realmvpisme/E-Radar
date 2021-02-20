using System.Threading.Tasks;
using MailKit;
using MailKit.Search;
using MailKit.Net;
using MailKit.Net.Imap;

namespace E_Radar
{
    public class MailGetter
    {
        public async Task GetNewMessages()
        {
            using (var client = new ImapClient())
            {
                client.Connect();
            }
        }
    }
}