
using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Services.BoxingOps;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_Keywords;
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
                if (TypeSystem.IsRawValue(tokens[0])) return TypeSystem.ParseType(tokens[0], stack);
                if (SyntaxAnalyzer.IsTernaryOperation(tokens[0])) return HandleTernaryOperator(tokens[0] , stack);
                if (SyntaxAnalyzer.IsIndexer(tokens[0])) return TilangArray.UseIndexer(tokens[0], stack);
                var res = ResolveExpression(tokens[0], stack);
                return res;
            }

            if (SyntaxAnalyzer.IsFunctionCall(tokens[0]))
            {
                throw new Exception("cannot assign to a function call");
            }

            var rightSide = stack.Stack.GetFromStack(tokens[0], stack);
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

        public TilangVariable? ReadExpression(string token, Processor stack)
        {
            var list = new List<string>
            {
                token
            };

            return ReadExpression(list, stack);
        }

        private TilangVariable? HandleTernaryOperator(string token, Processor stack)
        {
            var indexOfQuestionMark = token.IndexOf("?");

            var conditionSide = token.Substring(0, indexOfQuestionMark).Trim();
            var operationsSide = token.Substring(indexOfQuestionMark + 1).Trim();

            var lefSide = operationsSide.Substring(0, operationsSide.IndexOf(':')).Trim();
            var rightSide = operationsSide.Substring(operationsSide.IndexOf(':') + 1).Trim();

            var conditionSideResult = ReadExpression(conditionSide, stack);

            if ((bool)conditionSideResult.Value == true)
            {
                return ReadExpression(lefSide, stack);
            }

            return ReadExpression(rightSide, stack);

        }

        private TilangVariable? HandleTernaryOperator(List<dynamic> token, Processor stack)
        {
            var indexOfQuestionMark = token.IndexOf("?");

            var conditionSide = token.Slice(0, indexOfQuestionMark);
            var operationsSide = token.Skip(indexOfQuestionMark + 1).ToList();

            var lefSide = operationsSide.Slice(0, operationsSide.IndexOf(":"));
            var rightSide = operationsSide.Skip(operationsSide.IndexOf(':') + 1).ToList();

            if ((bool)ExpressionGen(conditionSide , stack).Value == true)
            {
                return ExpressionGen(lefSide, stack);
            }

            return ExpressionGen(rightSide, stack);

        }

        private TilangVariable ResolveExpression(string expression, Processor stack)
        {
            var parsedExpression = ParseMathExpression(expression);

            var exprStack = stack.GetItemsFromStack(parsedExpression);


            return ExpressionGen(exprStack, stack);
        }

        private List<string> ParseMathExpression(string str)
        {
            var ops = Keywords.AllOperators;
            IgnoringRanges ranges = new IgnoringRanges();
            ranges.AddIndexes(str);
            var result = new List<string>();
            var val = "";
            var op = "";
            bool isDoubleChared = false;
            bool inPranthesis = false;
            int prCount = 0;
            int brackeysCount = 0;
            for (int i = 0; i < str.Length; i++)
            {
                string current = "" + str[i];

                if (isDoubleChared)
                {
                    isDoubleChared = false;
                    continue;
                }
                if (current == "[" && !ranges.IsIgnoringIndex(i))
                {
                    brackeysCount++;
                }
                if (current == "(" && !ranges.IsIgnoringIndex(i))
                {
                    if (!inPranthesis) inPranthesis = true;
                    prCount++;
                }
                if (ops.IndexOf(current) != -1 && !ranges.IsIgnoringIndex(i))
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

                    if (!inPranthesis && brackeysCount == 0)
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
                if (current == "]" && !ranges.IsIgnoringIndex(i))
                {
                    brackeysCount--;
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


        private TilangVariable? ReplaceTernaryOperations(List<dynamic> code, Processor stack)
        {

            var op = "";

            code.ForEach(val =>
            {
                op += val.ToString();
            });

            if (SyntaxAnalyzer.IsTernaryOperation(code))
            {
                return HandleTernaryOperator(op, stack);
            }


            return null;
        }

        private TilangVariable ExpressionGen(List<dynamic> code, Processor stack)
        {
            string lastOp = "";
            var ops = Keywords.AllOperators;

            TilangVariable res = null;
            TilangVariable next;

            if (code[0].GetType() == typeof(string) && code[0] == "!")
            {
                var result = ExpressionGen(code.Skip(1).ToList(), stack);
                result.Value = ! (bool)result.Value;
                return result;
            }

            if (code.Count == 1)
            {
                return code[0];
            }

            if (code.Count == 2)
            {
                return TypeSystem.ParseType(code[0] + code[1].Value.ToString() + "", stack);
            }

            for (int i = 0; i < code.Count; i++)
            {
                var _char = code[i];
                if (_char.GetType() == typeof(string) && ops.Contains(_char))
                {
                    lastOp = _char;


                    if (SyntaxAnalyzer.IsTernaryOperation(code.Skip(i - 1).ToList()))
                    {
                        return ReplaceTernaryOperations(code.Skip(i - 1).ToList(), stack);
                    }


                    res = res == null ? code[i - 1].GetCopy() : res;
                    next = code[i + 1];

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

                    if (Keywords.TwoSidedOperators.Contains(lastOp) || Keywords.AssignmentOperators.Contains(lastOp))
                    {
                        var rest = code.Skip(i + 1).ToList();
                        next = ExpressionGen(rest, stack);

                        res = ResolveValueBaseOnAction(res, next, lastOp);

                        return res;
                    }


                    res = ResolveValueBaseOnAction(res, next, lastOp);
                }
            }
            res.VariableName = "";
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
                    result.Value = UnBoxer.UnboxBool(val1) || UnBoxer.UnboxBool(val2);
                    return result;
                case ">=":
                    result.TypeName = "bool";
                    result.Value = UnBoxer.ForceUnboxFloat(val1) >= UnBoxer.ForceUnboxFloat(val2);
                    return result;
                case ">":
                    result.TypeName = "bool";
                    result.Value = UnBoxer.ForceUnboxFloat(val1) > UnBoxer.ForceUnboxFloat(val2);
                    return result;
                case "<=":
                    result.TypeName = "bool";
                    result.Value = UnBoxer.ForceUnboxFloat(val1) <= UnBoxer.ForceUnboxFloat(val2);
                    return result;
                case "<":
                    result.TypeName = "bool";
                    result.Value = UnBoxer.ForceUnboxFloat(val1) < UnBoxer.ForceUnboxFloat(val2);
                    return result;
                case "&&":
                    result.TypeName = "bool";
                    result.Value = UnBoxer.UnboxBool(val1) && UnBoxer.UnboxBool(val2);
                    return result;
                case "%":
                    result.TypeName = "int";
                    result.Value = UnBoxer.ForceUnboxFloat(val1) % UnBoxer.ForceUnboxFloat(val2);
                    return result;
            }

            return val1;

        }
    }
}
