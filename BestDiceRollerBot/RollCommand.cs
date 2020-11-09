using System.Threading.Tasks;
using Discord.Commands;
using Antlr4.Runtime;

namespace BestDiceRollerBot
{
    public class RollCommand : ModuleBase<SocketCommandContext>
    {
        [Command("r")]
        [Summary("Rolls the dice")]
        public Task RollDice(string argument = "")
        {
            AntlrInputStream inputStream = new AntlrInputStream(argument);
            DiceLexer dl = new DiceLexer(inputStream);
            CommonTokenStream cts = new CommonTokenStream(dl);
            DiceParser dp = new DiceParser(cts);
            DiceParser.ExpressionContext context = dp.expression();
            DiceEvaluator de = new DiceEvaluator();
            double Result = de.Visit(context);
            return ReplyAsync($"Evaluates to: {Result}");
        }
    }
}
