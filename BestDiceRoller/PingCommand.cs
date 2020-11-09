using System.Threading.Tasks;
using Discord.Commands;

namespace BestDiceRoller
{
    public class PingCommand : ModuleBase<SocketCommandContext>
    {
        [Command("Ping")]
        [Summary("PingPong")]
        public Task PingAsync() => ReplyAsync("Pong!");
    }
}
