using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Antlr4.Runtime;
using Discord;

namespace BestDiceRollerBot
{
    public class RollCommand : ModuleBase<SocketCommandContext>
    {
        public static IEnumerable<(double, string)> EvaluateDiceRequest(string arg)
        {
            var request = Task.Run(() => EvaluateExpression(arg.Trim()));
            request.Wait();
            return request.Result;
        }

        public static IEnumerable<(double, string)> EvaluateExpression(string exp)
        {
            try
            {
                Console.WriteLine($"Evaluating: {exp}");
                var inputStream = new AntlrInputStream(exp);
                var lexer = new DiceLexer(inputStream);
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new DiceParser(tokenStream);
                var context = parser.request();
                var evaluator = new RequestEvaluator();
                return evaluator.Visit(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new[] {(0.0, "Failed to parse and evaluate")};
            }
        }

        [Command("r")]
        [Alias("roll", "Roll", "R")]
        [Summary("Rolls the dice")]
        public Task RollDice([Remainder] string argument = "")
        {
            var indexOfCommentSig = argument.IndexOf('#');
            var hasComment = indexOfCommentSig > -1;
            var comment = hasComment ? argument.Substring(indexOfCommentSig) : "";
            var diceRoll = hasComment ? argument.Substring(0, indexOfCommentSig) : argument;
            var results = EvaluateDiceRequest(diceRoll).ToArray();
            var text = string.Join(", ", results.Select(t => t.Item2));
            var totals = string.Join(", ", results.Select(t => $"`{t.Item1}`"));
            var output = $"{this.Context.User.Mention} => {text} = **{totals}** {comment}";
            if (output.Length > DiscordConfig.MaxMessageSize)
            {
                output = $"{this.Context.User.Mention} => {argument} (shortened) = **{totals}** {comment}";
            }

            return ReplyAsync(output);
        }
    }
}