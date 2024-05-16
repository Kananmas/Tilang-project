﻿
using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Syntax.Analyzer
{
    public class ExprAnalyzer
    {

        public TilangVariable? ReadExpression(List<string> tokens, Processor? stack = null)
        {
            if (tokens.Count == 0) return null;
            if (tokens.Count == 1)
            {
                if (TypeSystem.IsTypeCreation(tokens[0])) return TypeSystem.ParseType(tokens[0], stack);
                var res = ResolveExpression(tokens[0], stack);
                return res;
            }

            var rightSide = stack.Stack.GetFromStack(tokens[0]);
            var op = tokens[1];

            TilangVariable leftSide;

            if (TypeSystem.IsTypeCreation(tokens[2]))
            {
                leftSide = TypeSystem.ParseType(tokens[2], stack);
            }

            else
            {
                leftSide = ResolveExpression(tokens[2], stack);
            }


            rightSide.Assign(leftSide, op);


            return rightSide;
        }

        public TilangVariable? ReadExpression(string token, Processor? stack = null)
        {
            var list = new List<string>();
            list.Add(token);

            return ReadExpression(list, stack);
        }


        private bool IsTernaryOperation(List<string> token)
        {
            return token.Contains("?") && token.Contains(":") && token.IndexOf(":") > token.IndexOf("?");
        }


        private TilangVariable? HandleTernaryOperator(string token, Processor? stack = null)
        {
            var indexOfQuestionMark = token.IndexOf("?");

            var conditionSide = token.Substring(0, indexOfQuestionMark).Trim();
            var operationsSide = token.Substring(indexOfQuestionMark + 1).Trim();

            var lefSide = operationsSide.Substring(0, operationsSide.IndexOf(':')).Trim();
            var rightSide = operationsSide.Substring(operationsSide.IndexOf(':') + 1).Trim();

            var conditionSideResult = ReadExpression(conditionSide, stack);

            if (conditionSideResult.Value == true)
            {
                return ReadExpression(lefSide);
            }

            return ReadExpression(rightSide);

        }
        int g = 12 + 2-1;

        private TilangVariable ResolveExpression(string expression, Processor? stack = null)
        {
            var parsedExpression = ParseMathExpression(expression);

            if (stack != null)
            {
                stack.ReplaceItemsFromStack(parsedExpression);
            }

            return ExpressionGen(parsedExpression, stack);
        }

        private List<string> ParseMathExpression(string str)
        {
            var ops = "> < >= <= + - / * *= /= += -= == != || && = & ! ? : %".Split(" ").ToList();
            IgnoringRanges ranges = new IgnoringRanges();
            ranges.AddIndexes(str, new List<char>() { '\"', '\'' });
            var result = new List<string>();
            var val = "";
            var op = "";
            bool isDoubleChared = false;
            bool inPranthesis = false;
            int prCount = 0;
            for (int i = 0; i < str.Length; i++)
            {
                string current = "" + str[i];

                if (isDoubleChared)
                {
                    isDoubleChared = false;
                    continue;
                }

                if (current == "(" && !ranges.IsIgnoringIndex(i))
                {
                    if (!inPranthesis) inPranthesis = true;
                    prCount++;
                }
                if (ops.IndexOf(current) != -1)
                {
                    if (i < str.Length - 1)
                    {
                        var nextChar = str[i + 1];
                        if (ops.IndexOf(nextChar.ToString()) != -1)
                        {
                            current += nextChar;
                            isDoubleChared = true;
                        }
                    }

                    if (!inPranthesis)
                    {
                        if (val.Trim().Length > 0)
                        {
                            result.Add(val.Trim());
                        }
                        op += current;
                        result.Add(op);

                        val = "";
                        op = "";
                    }
                    else
                    {
                        val += current;
                    }
                }
                else
                {
                    val += current;
                }
                if (current == ")" && !ranges.IsIgnoringIndex(i))
                {
                    prCount--;
                    if (prCount == 0)
                    {
                        if (val.Trim().Length > 0) result.Add(val.Trim());
                        val = "";
                        inPranthesis = false;
                    }
                }

            }
            if (val.Trim().Length > 0)
            {
                result.Add(val.Trim());
            }


            result.ForEach(val =>
            {
                if (TypeSystem.IsTypeCreation(val)) throw new Exception("cannot use operator with custom type");
            });


            return result;

        }


        private TilangVariable? ReplaceTernaryOperations(List<string> code, Processor stack)
        {

            var op = "";

            code.ForEach(val =>
            {
                op += val;
            });

            if (IsTernaryOperation(code))
            {
                return HandleTernaryOperator(op, stack);
            }


            return null;
        }

        private TilangVariable ExpressionGen(List<string> code, Processor stack)
        {
            string lastOp = "";
            var ops = "> < >= <= + - / * *= /= += -= == != || && %".Split(" ").ToList();

            dynamic res = null;
            dynamic next;
            if (code.Count == 1 && !code[0].StartsWith("Sys"))
            {
                return TypeSystem.ParsePrimitiveType(code[0]);
            }

            if(code.Count == 2)
            {
                return TypeSystem.ParseType(code[0] + code[1] + "");
            }

            for (int i = 0; i < code.Count; i++)
            {
                var _char = code[i];
                if (ops.Contains(_char))
                {
                    lastOp = _char;

                    if (IsTernaryOperation(code.Skip(i - 1).ToList()))
                    {
                        return ReplaceTernaryOperations(code.Skip(i - 1).ToList(), stack);
                    }


                    res = (res == null) ? TypeSystem.ParsePrimitiveType(code[i - 1]) : res;
                    next = TypeSystem.ParsePrimitiveType(code[i + 1]);

                    if (lastOp != string.Empty)
                    {
                        if (next.GetType() == typeof(string))
                        {
                            if (lastOp != "+")
                            {
                                throw new Exception("cannot do " + lastOp + " to type string");
                            }
                        }

                    }

                    if (lastOp == "&&" || lastOp == "||")
                    {
                        var rest = code.Skip(i + 1).ToList();
                        next = ExpressionGen(rest, stack);

                        res = ResolveValueBaseOnAction(res, next, lastOp);

                        return res;
                    }


                    res = ResolveValueBaseOnAction(res, next, lastOp);
                }
            }

            return res;
        }


        private TilangVariable ResolveValueBaseOnAction(TilangVariable val1, TilangVariable val2, string op)
        {
            var result = new TilangVariable();
            switch (op)
            {
                case "+=":
                case "+":
                    val1.Assign(val2, "+");
                    break;
                case "-=":
                case "-":
                    val1.Assign(val2, "-");
                    break;
                case "*=":
                case "*":
                    val1.Assign(val2, "*");
                    break;
                case "/=":
                case "/":
                    val1.Assign(val2, "/");
                    break;
                case "!=":
                    result.TypeName = "bool";
                    result.Value = val1.Value != val2.Value;
                    return result;
                case "==":
                    result.TypeName = "bool";
                    result.Value = val1.Value == val2.Value;
                    return result;
                case "||":
                    result.TypeName = "bool";
                    result.Value = val1.Value || val2.Value;
                    return result;
                case ">=":
                    result.TypeName = "bool";
                    result.Value = val1.Value >= val2.Value;
                    return result;
                case ">":
                    result.TypeName = "bool";
                    result.Value = val1.Value > val2.Value;
                    return result;
                case "<=":
                    result.TypeName = "bool";
                    result.Value = val1.Value <= val2.Value;
                    return result;
                case "<":
                    result.TypeName = "bool";
                    result.Value = val1.Value < val2.Value;
                    return result;
                case "&&":
                    result.TypeName = "bool";
                    result.Value = val1.Value && val2.Value;
                    return result;
                case "%":
                    result.TypeName = "int";
                    result.Value = val1.Value % val2.Value;
                    return result;
            }

            return val1;

        }
    }
}
