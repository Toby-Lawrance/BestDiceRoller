using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace BestDiceRollerBot
{
    public class DiceEvaluator : DiceBaseVisitor<(double,string)>
    {
        private readonly Dictionary<string, Func<double, double, double>> _funcMap =
            new Dictionary<string, Func<double, double, double>>
            {
                {"+", (a, b) => a + b},
                {"-", (a, b) => a - b},
                {"*", (a, b) => a* b},
                {"/", (a, b) => a / b}
            };

        public override (double, string) VisitExpression([NotNull] DiceParser.ExpressionContext context)
        {
            var operandCtxs = context.multiplyTerm();
            var operatorCtxs = context.TermOperator();
            var operands = operandCtxs.Select(Visit);
            Queue<string> operators = new Queue<string>(operatorCtxs.Select(o => o.GetText()));

            if (operators.Count > 0)
            {
                
                var total = operands.Aggregate((a, c) =>
                {
                    var numerical = _funcMap[operators.Dequeue()](a.Item1, c.Item1);
                    var text = $"{a.Item2} + {c.Item2}";
                    return (numerical,text);
                });
                return (total.Item1,total.Item2);
            }
            else
            {
                return operands.First();
            }
        }

        public override (double, string) VisitMultiplyTerm([NotNull] DiceParser.MultiplyTermContext context)
        {
            var operandCtxs = context.powerTerm();
            var operatorCtxs = context.MultiplyOperator();
            var operands = operandCtxs.Select(Visit);
            Queue<string> operators = new Queue<string>(operatorCtxs.Select(o => o.GetText()));

            if (operators.Count > 0)
            {
                
                var total = operands.Aggregate((a, c) =>
                {
                    var numerical = _funcMap[operators.Dequeue()](a.Item1, c.Item1);
                    var text = $"{a.Item2} + {c.Item2}";
                    return (numerical,text);
                });
                return (total.Item1,total.Item2);
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
            if (powerSymbol != null)
            {
                var power = Visit(context.coreTerm(1));
                var numerical = Math.Pow(coreValue.Item1, power.Item1);
                var textual = $"{coreValue.Item2}^{power.Item2}";
                return (numerical,textual);
            }
            return coreValue;
        }


        public override (double, string) VisitCoreTerm([NotNull] DiceParser.CoreTermContext context)
        {
            var lit = context.Literal();
            var die = context.dieRoll();
            var expression = context.expression();

            if (lit is null) return die is not null ? Visit(die) : Visit(expression);
            
            var numerical = double.Parse(lit.GetText());
            var textual = lit.GetText();
            return (numerical, textual);
        }

        private (double, string) RollExplodingDice(int diceSize)
        {
            if (diceSize == 1)
            {
                return (1, "[Rolling a d1! is a bad idea: 1]");
            }
            
            var baseRoll = Program.Generator.Get(1,diceSize + 1);
            var extraRolls = LINQExtension.Unfold<int,int>(baseRoll, i =>
            {
                if (i != diceSize)
                {
                    return null;
                }
                var newNum = Program.Generator.Get(1, diceSize + 1);
                return new Tuple<int, int>(newNum,newNum);
            }).ToList();
            var numerical = baseRoll + extraRolls.Sum();
            var textual = $"[{string.Join(',',extraRolls.Prepend(baseRoll))}]";
            return (numerical, textual);
        }
        
        public override (double, string) VisitDieRoll(DiceParser.DieRollContext context)
        {
            var exploding = context.ExplodeMark() is not null;
            var numDice = context.Natural().Length > 1 ? int.Parse(context.Natural(0).GetText()) : 1;
            var diceSize = int.Parse(context.Natural().Last().GetText());

            if (!exploding)
            {
                var rolls = Enumerable.Range(0,numDice).Select(_ => Program.Generator.Get(1, diceSize + 1)).ToList();
                var numerical = rolls.Sum();
                var textual = $"[{string.Join(',',rolls)}]";
                return (numerical, textual);
            }
            else
            {
                var rolls = Enumerable.Range(0,numDice).Select(_ => RollExplodingDice(diceSize)).ToList();
                var numerical = rolls.Select(t => t.Item1).Sum();
                var textual = $"[{string.Join(',',rolls.Select(t => t.Item2))}]";
                return (numerical, textual);
            }
        }
    }

    public class ThrowingErrorListener : BaseErrorListener
    {
        public static readonly ThrowingErrorListener Instance = new ThrowingErrorListener();

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
            string msg, RecognitionException e)
        {
            throw new ParseCanceledException($"Char:{offendingSymbol.Text} at: {charPositionInLine}");
        }
    }
}