using System.Threading.Tasks;
using Discord.Commands;

namespace BestDiceRoller
{
    public class RollCommand : ModuleBase<SocketCommandContext>
    {
        [Command("r")]
        [Summary("Rolls the dice")]
        public Task RollDice(string argument = "")
        {
            return ReplyAsync("4: as chosen by a fair and Balanced Roll");
        }
    }
}
