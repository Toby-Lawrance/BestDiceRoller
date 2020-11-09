using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Antlr4.Runtime;
using Discord;

namespace BestDiceRollerBot
{
    public class RollCommand : ModuleBase<SocketCommandContext>
    {
        private (double,string) EvaluateExpression(string exp)
        {
            Console.WriteLine($"Evaluating: {exp}");
            var inputStream = new AntlrInputStream(exp);
            var lexer = new DiceLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new DiceParser(tokenStream);
            var context = parser.expression();
            var evaluator = new DiceEvaluator();
            return evaluator.Visit(context);
        }
        
        [Command("r")]
        [Alias("roll","Roll","R")]
        [Summary("Rolls the dice")]
        public Task RollDice([Remainder]string argument = "")
        {
            var requests = argument.Split(',').Select(arg => arg.Trim()).Select(e => Task.Run(() => EvaluateExpression(e))).ToArray();
            Task.WaitAll(requests);
            var results = requests.Select(t => t.Result).ToArray();
            var text = string.Join(", ", results.Select(t => t.Item2));
            var totals = string.Join(", ", results.Select(t => t.Item1));
            var output = $"{this.Context.User.Mention} => {text} = {totals}";
            if (output.Length > DiscordConfig.MaxMessageSize)
            {
                output = $"{this.Context.User.Mention} => {argument} (shortened) = {totals}";
            }

            return ReplyAsync(output);
        }
    }
}
