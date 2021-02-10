using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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

            _client.MessageReceived += HandleTextAsync;

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

        private async Task HandleTextAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.IsBot) return; //Else will respond to self responses

            const string textRollSyntax = @"(?:(?<label>(?:\w+?)|(?:\""[\w\s]+\""))\s*?\:\s*?)?\[\[(?<expression>[dDkhl\s\d\.\,\(\)\,\!\+\-\*\/\^]+)\]\]";
            
            var matches = Regex.Matches(message.Content, textRollSyntax);
            if(matches.Count == 0) return; //None found
            
            List<(string,string)> diceRollRequestsLongShort = new List<(string,string)>();
            
            foreach (Match match in matches)
            {
                var request = match.Groups["expression"].Value;
                var label = match.Groups["label"].Value.Replace("\"","");
                var results = RollCommand.EvaluateDiceRequest(request).ToArray();
                var text = string.Join(", ", results.Select(t => t.Item2));
                var totals = string.Join(", ", results.Select(t => $"`{t.Item1}`"));
                var output = $"({request} => {text} = {totals})";
                var shortOutput = $"({request} => { totals })";
                if (!String.IsNullOrWhiteSpace(label))
                {
                    output = $"**{label}**: ({request} => {text} = **{totals}**)";
                    shortOutput = $"**{label}**: ({request} => **{totals}**)";
                }
                diceRollRequestsLongShort.Add((output,shortOutput));
            }

            var allLongRequests = string.Join(",\n", diceRollRequestsLongShort.Select(t => t.Item1));
            var finalOutput = diceRollRequestsLongShort.Count > 1 ? $"{message.Author.Mention} ->\n{allLongRequests}" : $"{message.Author.Mention} -> {allLongRequests}";
            if (finalOutput.Length > DiscordConfig.MaxMessageSize)
            {
                var allShortRequests = string.Join(",\n", diceRollRequestsLongShort.Select(t => t.Item2));
                finalOutput = $"{message.Author.Mention} (shortened) -> {allShortRequests}";
            }
            var context = new SocketCommandContext(_client, message);

            await context.Channel.SendMessageAsync(finalOutput);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}