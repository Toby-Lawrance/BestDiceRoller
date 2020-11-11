using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace BestDiceRollerBot
{
    public class DiceEvaluator : DiceBaseVisitor<(double, string)>
    {
        private readonly Dictionary<string, Func<double, double, double>> _funcMap =
            new Dictionary<string, Func<double, double, double>>
            {
                {"+", (a, b) => a + b},
                {"-", (a, b) => a - b},
                {"*", (a, b) => a * b},
                {"/", (a, b) => a / b}
            };

        private string Spacing(int depth, char c = '\t')
        {
            return string.Concat(Enumerable.Repeat(c, depth));
        }

        public override (double, string) VisitExpression([NotNull] DiceParser.ExpressionContext context)
        {
            var operandCtxs = context.multiplyTerm();
            var operatorCtxs = context.termOperation();
            var operands = operandCtxs.Select(Visit);
            Queue<string> operators = new Queue<string>(operatorCtxs.Select(o => o.GetText()));

            if (operators.Count > 0)
            {
                var (total, textVersion) = operands.Aggregate((a, c) =>
                {
                    var op = operators.Dequeue();
                    var numerical = _funcMap[op](a.Item1, c.Item1);
                    var text = $"{a.Item2} {op} {c.Item2}";
                    return (numerical, text);
                });
                return (total, textVersion);
            }
            else
            {
                return operands.First();
            }
        }

        public override (double, string) VisitMultiplyTerm([NotNull] DiceParser.MultiplyTermContext context)
        {
            var operandCtxs = context.powerTerm();
            var operatorCtxs = context.multOperation();
            var operands = operandCtxs.Select(Visit);
            Queue<string> operators = new Queue<string>(operatorCtxs.Select(o => o.GetText()));

            if (operators.Count > 0)
            {
                var (total, textVersion) = operands.Aggregate((a, c) =>
                {
                    var op = operators.Dequeue();
                    var numerical = _funcMap[op](a.Item1, c.Item1);
                    var text = $"{a.Item2} {op} {c.Item2}";
                    return (numerical, text);
                });
                return (total, textVersion);
            }
            else
            {
                return operands.First();
            }
        }

        public override (double, string) VisitPowerTerm([NotNull] DiceParser.PowerTermContext context)
        {
            var coreValue = Visit(context.coreTerm(0));
            var powerSymbol = context.PowerOperator();
            if (powerSymbol == null) return coreValue;
            
            var power = Visit(context.coreTerm(1));
            var numerical = Math.Pow(coreValue.Item1, power.Item1);
            var textual = $"{coreValue.Item2}^{power.Item2}";
            return (numerical, textual);

        }


        public override (double, string) VisitCoreTerm([NotNull] DiceParser.CoreTermContext context)
        {
            var lit = context.decimalNum();
            var die = context.dieRoll();
            var expression = context.expression();

            if (lit is not null) return Visit(lit);
            if (die is not null)
            {
                return Visit(die);
            }

            var (total, textV) = Visit(expression);
            return (total, $"({textV})");
        }

        public override (double, string) VisitDecimalNum(DiceParser.DecimalNumContext context)
        {
            return (double.Parse(context.GetText()), context.GetText());
        }

        public override (double, string) VisitNaturalNum(DiceParser.NaturalNumContext context)
        {
            return (double.Parse(context.GetText()), context.GetText());
        }

        private (double, string) RollExplodingDice(int diceSize)
        {
            if (diceSize == 1)
            {
                return (1, "[Rolling a d1! is a bad idea: 1]");
            }

            var baseRoll = Program.Generator.Get(1, diceSize + 1);
            var extraRolls = baseRoll.Unfold(i =>
            {
                if (i != diceSize)
                {
                    return null;
                }

                var newNum = Program.Generator.Get(1, diceSize + 1);
                return new Tuple<int, int>(newNum, newNum);
            }).ToList();
            var numerical = baseRoll + extraRolls.Sum();
            var textual = $"[{string.Join(',', extraRolls.Prepend(baseRoll))}]";
            return (numerical, textual);
        }

        public override (double, string) VisitDieRoll(DiceParser.DieRollContext context)
        {
            var exploding = context.ExplodeMark() is not null;
            var numDice = context.naturalNum().Length > 1 ? int.Parse(context.naturalNum(0).GetText()) : 1;
            var diceSize = int.Parse(context.naturalNum().Last().GetText());

            if (!exploding)
            {
                var rolls = Enumerable.Range(0, numDice).Select(_ => Program.Generator.Get(1, diceSize + 1)).ToList();
                var numerical = rolls.Sum();
                var textual = $"[{string.Join(',', rolls)}]";
                return (numerical, textual);
            }
            else
            {
                var rolls = Enumerable.Range(0, numDice).Select(_ => RollExplodingDice(diceSize)).ToList();
                var numerical = rolls.Select(t => t.Item1).Sum();
                var textual = $"[{string.Join(',', rolls.Select(t => t.Item2))}]";
                return (numerical, textual);
            }
        }
    }

    public class ThrowingErrorListener : BaseErrorListener
    {
        public static readonly ThrowingErrorListener Instance = new ThrowingErrorListener();

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
            int charPositionInLine,
            string msg, RecognitionException e)
        {
            throw new ParseCanceledException($"Char:{offendingSymbol.Text} at: {charPositionInLine}");
        }
    }
}