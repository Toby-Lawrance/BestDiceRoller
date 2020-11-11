using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace BestDiceRollerBot
{
    public class CombatCommands : ModuleBase<SocketCommandContext>
    {
        private static string[] inCombatRoles = {"In-Combat", "test-in-combat", "Player"};
        private static string[] initiativeModifierRoles = { "Quick-Edge", "Level-Headed-Edge", "Improved-Level-Headed-Edge" };
        
        [Command("initiative")]
        [Summary("Roll initiative for everyone inCombat")]
        public Task RollInitiative()
        {
            var combatUsers = this.Context.Guild.Users
                .Where(u => u.Roles
                    .Any(r => inCombatRoles
                        .Any(icr => icr == r.Name)))
                .Select(user => (user,user.Roles.Where(modRole => initiativeModifierRoles.Any(s => s == modRole.Name)).Select(x => x.Name)))
                .Select(userAndRoles => string.IsNullOrWhiteSpace(userAndRoles.user.Nickname) ? (userAndRoles.user.Username,userAndRoles.Item2) : (userAndRoles.user.Nickname,userAndRoles.Item2));

            var standardInitiativeDiceRoll = "d52";
            var levelHeadedDiceRoll = "2d52kh1";
            var improvedLevelHeadedDiceRoll = "3d52kh1";
            

            var initiative = combatUsers
                    .Select(nick => (nick, RollCommand.EvaluateExpression(standardInitiativeDiceRoll)))
                    .OrderByDescending(tuple => tuple.Item2.Item1)
                    .Select(tuple => $"{tuple.nick} ({standardInitiativeDiceRoll}) -> {tuple.Item2.Item1}");
            
            
            var output = $"Initiative order:\n{string.Join('\n',initiative)}";
            return ReplyAsync(output);
        }
    }
}