using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace BestDiceRollerBot
{
    public class CombatCommands : ModuleBase<SocketCommandContext>
    {
        private static readonly string QuickEdge = "Quick-Edge";
        private static readonly string LevelHeadedEdge = "Level-Headed-Edge";
        private static readonly string ImprovedLevelHeadedEdge = "Improved-Level-Headed-Edge";
        private static readonly string[] InCombatRoles = {"In-Combat", "test-in-combat", "Player"};
        private static readonly string[] InitiativeModifierRoles = { QuickEdge, LevelHeadedEdge, ImprovedLevelHeadedEdge };
        
        [Command("initiative")]
        [Summary("Roll initiative for everyone inCombat")]
        public Task RollInitiative()
        {
            var combatUsers = this.Context.Guild.Users
                .Where(u => u.Roles
                    .Any(r => InCombatRoles
                        .Any(icr => icr == r.Name)))
                .Select(user => (user,user.Roles.Where(modRole => InitiativeModifierRoles.Any(s => s == modRole.Name)).Select(x => x.Name)))
                .Select(userAndRoles => string.IsNullOrWhiteSpace(userAndRoles.user.Nickname) ? (userAndRoles.user.Username,userAndRoles.Item2) : (userAndRoles.user.Nickname,userAndRoles.Item2));

            var standardInitiativeDiceRoll = "d54";
            var levelHeadedDiceRoll = "2d54kh1";
            var improvedLevelHeadedDiceRoll = "3d54kh1";
            

            var initiative = combatUsers
                    .Select(nickAndEdges =>
                    {
                        var quick = nickAndEdges.Item2!.Contains(QuickEdge);
                        var levelHeaded = nickAndEdges.Item2.Contains(LevelHeadedEdge);
                        var improvedLevelHeaded = nickAndEdges.Item2.Contains(ImprovedLevelHeadedEdge);
                        if (quick)
                        {
                            var cards = new List<(double, string)> {RollQuickInit(standardInitiativeDiceRoll)};
                            
                            if (!levelHeaded && !improvedLevelHeaded) return (nickAndEdges.Item1, cards.First());
                            
                            cards.Add(RollQuickInit(standardInitiativeDiceRoll));
                            if (improvedLevelHeaded)
                            {
                                cards.Add(RollQuickInit(standardInitiativeDiceRoll));
                            }

                            var ordered = cards.OrderByDescending(x => x.Item1).ToArray();
                            var first = ordered.First();
                            var discarded = cards.Where(x => x != first).ToArray();
                            return (nickAndEdges.Item1, (first.Item1, $"{string.Join(',',discarded.Select(x => x.Item2))}{first.Item2}"));

                        }
                        if (levelHeaded)
                            return (nickAndEdges.Item1, RollCommand.EvaluateExpression(levelHeadedDiceRoll).First());
                            
                        if (improvedLevelHeaded)
                            return (nickAndEdges.Item1,
                                RollCommand.EvaluateExpression(improvedLevelHeadedDiceRoll).First());
                        
                        return (nickAndEdges.Item1, RollCommand.EvaluateExpression(standardInitiativeDiceRoll).First());
                    })
                    .Select( t =>
                    {
                        if (t.Item2.Item1 != 1 && t.Item2.Item1 != 54) return t;
                        
                        t.Item2.Item1 = 55; //Sentry value above deck size to put at top
                        t.Item2.Item2 += " JOKER";
                        return t;
                    })
                    .OrderByDescending(tuple => tuple.Item2.Item1)
                    .Select(tuple => $"{tuple.Item1} ({tuple.Item2.Item2}) -> {tuple.Item2.Item1}");
            
            
            var output = $"Initiative order:\n{string.Join('\n',initiative)}";
            return ReplyAsync(output);
        }

        private static (double finalVal, string textual) RollQuickInit(string standardInitiativeDiceRoll)
        {
            Func<(double,string),bool> predicate = (x => x.Item1 >= 2 && x.Item1 <= 17); 
            (double, string) roll;
            double finalVal = 0;
            var rolls = new List<(double, string)>();
            do
            {
                roll = RollCommand.EvaluateExpression(standardInitiativeDiceRoll).First();
                if (predicate(roll))
                {
                    roll.Item2 = $"~~{roll.Item2}~~";
                }
                rolls.Add(roll);
                finalVal = roll.Item1;
            } while (predicate(roll)); //2 -> 5 of each suit

            var textual = string.Join(',', rolls.Select(x => x.Item2));
            textual = $"[{textual}]";
            return (finalVal, textual);
        }
    }
}