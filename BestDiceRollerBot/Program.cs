using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace BestDiceRollerBot
{
    class Program
    {
        private DiscordSocketClient _client;
        public static readonly RandomProducer Generator = new RandomProducer();

        static void Main(string[] args)
        {
            var p = new Program();
            try
            {
                p.MainAsync().GetAwaiter().GetResult();
            }
            finally
            {
                p._client?.LogoutAsync().GetAwaiter().GetResult();
            }
            
        }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;

            var commandService = new CommandService();
            var handler = new CommandHandler(_client,commandService);
            await handler.InstallCommandsAsync();

            Console.CancelKeyPress +=  delegate(object sender, ConsoleCancelEventArgs e)
            {
               _client.LogoutAsync().GetAwaiter().GetResult();
            };

            var secretsContents = await File.ReadAllTextAsync("secrets.json");
            var secrets = JObject.Parse(secretsContents);

            // Remember to keep token private or to read it from an 
            // external source! In this case, we are reading the token 
            // from an environment variable. If you do not know how to set-up
            // environment variables, you may find more information on the 
            // Internet or by using other methods such as reading from 
            // a configuration.
            await _client.LoginAsync(TokenType.Bot,
                secrets["Token"].Value<string>());
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}