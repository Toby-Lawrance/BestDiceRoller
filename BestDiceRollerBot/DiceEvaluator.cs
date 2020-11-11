using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Discord.Rest;

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
            var dice = context.diceRoll();
            var keepExpression = context.keepExpression();
            var expression = context.expression();

            if (lit is not null) return Visit(lit);
            if (keepExpression is not null) return Visit(keepExpression);
            if (dice is not null) return Visit(dice);

            var (total, textV) = Visit(expression);
            return (total, $"({textV})");
        }

        public override (double, string) VisitKeepExpression(DiceParser.KeepExpressionContext context)
        {
            var options = context.keepOptions();
            var diceRoll = options.diceRoll();
            var expressions = options.expression();
            bool keepHighest = context.KeepHighest() is not null;
            var numToKeep = int.Parse(context.naturalNum().GetText());
            IEnumerable<(double, string)> rolls;
            if (diceRoll is not null)
            {
                var numDice = diceRoll.naturalNum() is not null
                    ? int.Parse(diceRoll.naturalNum().GetText())
                    : (numToKeep + 1);
                rolls = Enumerable.Range(0, numDice).Select(_ => Visit(diceRoll.dieRoll()));
            }
            else
            {
                rolls = expressions.Select(Visit);
            }

            var count = 0;
            var indexed = rolls.Select(r => (count++,(r.Item1,r.Item2)));

            var orderedRolls = keepHighest
                ? indexed.OrderByDescending(x => x.Item2.Item1).ToList()
                : indexed.OrderBy(x => x.Item2.Item1).ToList();

            var numerical = orderedRolls.Take(numToKeep).Sum(x => x.Item2.Item1);
            var keptBolded = orderedRolls
                .Take(numToKeep)
                .Select(x => (x.Item1, (x.Item2.Item1, $"**{x.Item2.Item2}**")));
            var discardCrossed = orderedRolls
                .Skip(numToKeep)
                .Select(x => (x.Item1, (x.Item2.Item1, $"~~{x.Item2.Item2}~~")));
            var complete = keptBolded
                .Concat(discardCrossed)
                .OrderBy(x=> x.Item1)
                .Select(x => x.Item2)
                .ToArray();



            var textual = $"[{string.Join(", ", complete.Select(x => x.Item2))}]";

            return (numerical, textual);
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
            var textual = "";
            if (extraRolls.Count == 0)
            {
                textual = $"{string.Join(',', extraRolls.Prepend(baseRoll))}";
            }
            else
            {
                textual = $"[{string.Join(',', extraRolls.Prepend(baseRoll))}]";
            }

            return (numerical, textual);
        }

        public override (double, string) VisitDiceRoll(DiceParser.DiceRollContext context)
        {
            var numDice = context.naturalNum() is not null ? int.Parse(context.naturalNum().GetText()) : 1;
            var rolls = Enumerable.Range(0, numDice).Select(_ => Visit(context.dieRoll())).ToArray();
            var numerical = rolls.Sum(x => x.Item1);
            var textual = "";
            if (numDice == 1)
            {
                textual = $"{string.Join(',', rolls.Select(x => x.Item2))}";
            }
            else
            {
                textual = $"[{string.Join(',', rolls.Select(x => x.Item2))}]";
            }

            return (numerical, textual);
        }

        public override (double, string) VisitDieRoll(DiceParser.DieRollContext context)
        {
            var exploding = context.ExplodeMark() is not null;
            var diceSize = int.Parse(context.naturalNum().GetText());

            if (!exploding)
            {
                var roll = Program.Generator.Get(1, diceSize + 1);
                var textual = $"{roll}";
                return (roll, textual);
            }
            else
            {
                var rolls = RollExplodingDice(diceSize);
                var numerical = rolls.Item1;
                var textual = $"{rolls.Item2}";
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