
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
                if (SyntaxAnalyzer.IsIndexer(tokens[0]) && !tokens[0].StartsWith("("))
                    return TilangArray.UseIndexer(tokens[0], stack);
                var res = ResolveExpression(tokens[0], stack);
                return res;
            }

            if (SyntaxAnalyzer.IsFunctionCall(tokens[0]))
            {
                throw new Exception("cannot assign to a function call");
            }

            var rightSide = stack.Stack.GetFromStack(tokens[0], stack);
            var op = tokens[1];
            if (rightSide.Tag == "Constant") 
                throw new Exception("Cannot assign Value To Constant");


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

        private List<string> ParseMathExpression(string text)
        {
            var result = new List<string>();
            string[] ops = ["+",
                "-",
                "*",
                "%",
                "/",
                "<",
                ">",
                "+=",
                "-=",
                "/=",
                "*=",
                "==",
                "!=",
                "<=",
                ">=",
                "||",
                "&&"];

            var reformText = () =>
            {
                List<string> spaceOps = ["+=",
                    "-=",
                    "/=",
                    "*=",
                    "==",
                    "!=",
                    "<=",
                    ">=",
                    "||",
                    "&&"];
                foreach (var op in spaceOps)
                {
                   text = text.Replace(op, $" {op} ");
                }
            };

           
            reformText();

            var stringCache = "";

            var pranCount = 0;
            var brackCount = 0;
            var prevIndex = 0;
            var curvyBCount = 0;

            var addToResult = (string op, int index) =>
            {
                var len = index - prevIndex;
                var str = text.Substring(prevIndex, len).Trim();
                if(len > 0 && str.Length > 0)
                {
                    result.Add(str);
                    stringCache = "";
                }
                result.Add(op);

            };

            var checkOpenPran = (char ch) => {
                if (ch == '(') pranCount++;
            };
            var checkClosePran = (char ch) => {
                if (ch == ')') pranCount--;
            };
            var checkOpenBrack = (char ch) =>
            {
                if (ch == '[') brackCount++;
            };
            var checkCloseBrack = (char ch) =>
            {
                if (ch == ']') brackCount--;
            };
            var checkOpenCB = (char ch) =>
            {
                if (ch == '{') curvyBCount++;
            };
            var checkCloseCB = (char ch) =>
            {
                if (ch == '}') curvyBCount--;
            };

            var shouldAdd = () =>
            {
                return pranCount == 0 && brackCount == 0 && curvyBCount == 0;
            };


            for (int i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];
                string doubleChar = i > 0 && i < text.Length - 1 ? "" + text[i] + text[i + 1] : "";

                checkOpenPran(currentChar);
                checkClosePran(currentChar);
                checkOpenBrack(currentChar);
                checkCloseBrack(currentChar);
                checkOpenCB(currentChar);
                checkCloseCB(currentChar);

                if (doubleChar.Trim().Length > 1 && ops.Contains(doubleChar.Trim() ) && shouldAdd())
                {
                    addToResult(doubleChar, i);
                    prevIndex = i + 2;
                    i++;
                    continue;
                }
                if (ops.Contains(currentChar.ToString()) && shouldAdd())
                {
                    addToResult(currentChar.ToString(), i);
                    prevIndex = i + 1;
                    continue;
                }

                stringCache += currentChar;
            }

            if (stringCache.Trim().Length > 0) result.Add(stringCache.Trim());
            var finalResult = new List<string>(result);


            if (result.Count % 2 == 0)
            {
                int current = 0;
                foreach(var item in  result)
                {
                    if (current > 0 && current < result.Count - 1)
                    {
                        var firstItem = result[current];
                        var secondItem = result[current + 1];

                        var combine = firstItem + secondItem;

                        if (TypeSystem.IsRawValue(combine))
                        {
                            finalResult.RemoveAt(current + 1);
                            finalResult[current] = combine;
                        }
                    }

                    current++;
                }
            }


            return finalResult;

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
                case "-=":
                case "-":
                case "*=":
                case "*":
                case "/=":
                case "/":
                    val1.Assign(val2, op);
                    break;
                case "!=":
                case "==":
                case "||":
                case ">=":
                case ">":
                case "<=":
                case "<":
                case "&&":
                   result.TypeName = "bool";
                    result.Value = UnBoxer.UnboxCompare(val1.Value, val2.Value, op);
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
