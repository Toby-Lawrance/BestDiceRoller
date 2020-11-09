using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace BestDiceRollerBot
{
    public class DiceEvaluator : DiceBaseVisitor<double>
    {
        private readonly Dictionary<string, Func<double, double, double>> _funcMap =
            new Dictionary<string, Func<double, double, double>>
            {
                {"+", (a, b) => a + b},
                {"-", (a, b) => a - b},
                {"*", (a, b) => a* b},
                {"/", (a, b) => a / b}
            };

        public override double VisitExpression([NotNull] DiceParser.ExpressionContext context)
        {
            var operandCtxs = context.multiplyTerm();
            var operatorCtxs = context.TermOperator();
            List<double> operands = operandCtxs.Select(Visit).ToList();
            Queue<string> operators = new Queue<string>(operatorCtxs.Select(o => o.GetText()));

            if (operators.Count > 0)
            {
                return operands.Aggregate((a, c) => _funcMap[operators.Dequeue()](a, c));
            }
            else
            {
                return operands.First();
            }
        }

        public override double VisitMultiplyTerm([NotNull] DiceParser.MultiplyTermContext context)
        {
            var operandCtxs = context.powerTerm();
            var operatorCtxs = context.MultiplyOperator();
            List<double> operands = operandCtxs.Select(Visit).ToList();
            Queue<string> operators = new Queue<string>(operatorCtxs.Select(o => o.GetText()));

            if (operators.Count > 0)
            {
                return operands.Aggregate((a, c) => _funcMap[operators.Dequeue()](a, c));
            }
            else
            {
                return operands.First();
            }

        }

        public override double VisitPowerTerm([NotNull] DiceParser.PowerTermContext context)
        {
            var coreValue = Visit(context.coreTerm(0));
            var powerSymbol = context.PowerOperator();
            if (powerSymbol != null)
            {
                double power = Visit(context.coreTerm(1));
                return Math.Pow(coreValue, power);
            }
            return coreValue;
        }


        public override double VisitCoreTerm([NotNull] DiceParser.CoreTermContext context)
        {
            var lit = context.Literal();
            var expression = context.expression();

            return lit != null ? double.Parse(lit.GetText()) : Visit(expression);
        }
    }
}