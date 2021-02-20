using System;
using System.Drawing.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace E_Radar
{
    class Program
    {
        static async Task Main(string[] args)
        {
            

            while (true)
            {
                var builder = new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.json", true, true)
                    .AddEnvironmentVariables();
                var config = builder.Build();
                var configSection = config.GetSection(nameof(ConfigurationProfile));
                var configProfile = configSection.Get<ConfigurationProfile>();

                Console.WriteLine($"Starting mail fetch process at {DateTime.Now}.");
                var mailGetter = new MailGetter(configProfile.EmailClient);
                var mailSender = new MailSender(configProfile.EmailSender);

                await mailGetter.GetNewMessages();
                await mailSender.SendNotifications();
                
                await Task.Delay(TimeSpan.FromMinutes(configProfile.EmailClient.ServerSettings.TimeDelay));
            }

        }
    }
}
