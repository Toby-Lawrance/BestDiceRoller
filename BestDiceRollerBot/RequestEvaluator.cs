using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BestDiceRollerBot
{
    public class RequestEvaluator : DiceBaseVisitor<IEnumerable<(double, string)>>
    {
        public override IEnumerable<(double, string)> VisitRequest(DiceParser.RequestContext context)
        {
            var expressionResults =
                context.expression().Select(e => Task.Run(() =>
                {
                    var evaluator = new DiceEvaluator();
                    return evaluator.VisitExpression(e);
                })).ToArray();
            Task.WaitAll(expressionResults);
            return expressionResults.Select(t => t.Result).ToArray();
        }
    }
}