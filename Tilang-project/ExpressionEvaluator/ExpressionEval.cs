using System.Text.RegularExpressions;
using Tilang_project.Tilang_TypeSystem;
using Tilang_project.Tailang_Scope;

namespace Tilang_project.ExpressionEvaluator
{
    public class ExpressionEval
    {
        private bool IsFunctionCall(string str)
        {
            string pattern = @"\w+\s*\(";
            return Regex.IsMatch(str, pattern);
        }

        public dynamic ReadExpression(string Expr, Scope executionScope)
        {
            if (Expr == string.Empty) { return ""; }
            Expr = GetObjectProp(Expr, executionScope);
            var tokenList = ExpressionParser(Expr);
            ReplaceVariables(tokenList, executionScope);
            tokenList = EvaluateFunctionCall(tokenList, executionScope);
            return ExpressionGen(tokenList);
        }

        public dynamic ReadTheCode(List<List<string>> tokenLines, Scope executionScope)
        {
            dynamic result = null;
            foreach (var tokenLine in tokenLines)
            {
                switch (tokenLine[0])
                {
                    case "var":
                    case "const":
                    case "function":
                    case "type":
                    case ";":
                        break;
                    default:
                        if (tokenLine[0].StartsWith("return"))
                        {
                            var rest = ExtractRest(tokenLine[0]);
                            return ReadExpression(rest, executionScope);
                        }
                        var assignMentType = "";
                        var isAssignMent = HasAssignment(tokenLine[0], out assignMentType);
                        if (isAssignMent)
                        {
                            var splitByEqual = SeperateTokensByAssingments(tokenLine[0], assignMentType);
                            var rightSideInterpetaion = ReadExpression(splitByEqual[1], executionScope);
                            executionScope.UpdateValue(splitByEqual[0], rightSideInterpetaion, assignMentType);
                            break;
                        }
                        ReadExpression(tokenLine[0], executionScope);

                        break;
                }
            }

            return result;
        }

        private List<string> SeperateTokensByAssingments(string Tokens, string assignMentType)
        {
            if (assignMentType == "") throw new InvalidOperationException();
            var left = Tokens.Substring(0,Tokens.IndexOf(assignMentType)).Trim();
            var right = Tokens.Substring(Tokens.IndexOf(assignMentType)+1).Trim();
            string filler;
            if (HasAssignment(left, out filler))
            {
                throw new InvalidOperationException();
            }

            var tokens = new List<string>();

            tokens.Add(left);
            tokens.Add(right);

            return tokens.Select((item) => item.Trim()).ToList();

        }

        private bool HasAssignment(string Tokens, out string type)
        {

            var assignments = "+= -= = /= *=".Split(" ");
            var individualTokens = Tokens.Split(" ");
            var res = individualTokens.Any(Tokens => assignments.Contains(Tokens));
            type = "";
            if (IsFunctionCall(Tokens) && !res)
            {
                type = "";
                return false;
            }
            for (int i = 0; i < assignments.Length; i++)
            {
                for (int j = 0; j < individualTokens.Length; j++)
                {
                    if (assignments[i] == individualTokens[j])
                    {
                        type = assignments[i];
                        break;
                    }
                }
            }
            return res;
        }

        private string ExtractRest(string expr)
        {
            var indexOfReturn = expr.IndexOf("return") + "return".Length;
            var subStr = expr.Substring(indexOfReturn).Trim();

            return subStr;
        }

        private List<string> EvaluateFunctionCall(List<string> tokens, Scope executionScope)
        {
            int i = 0;
            var result = new List<string>();
            foreach (var tokenLine in tokens)
            {
                if (IsFunctionCall(tokenLine))
                {
                    var res = executionScope.CallFunction(tokenLine);

                    if (res != null) result.Add(res.ToString());
                    else
                    {
                        result.Add(tokenLine);
                    }
                }
                else
                {
                    result.Add(tokenLine);
                }
                i++;
            }

            return result;
        }

        private void ReplaceVariables(List<string> tokens, Scope executionScope)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                var currentToken = tokens[i];
                if (!IsFunctionCall(currentToken) && !"*+-/".Contains(currentToken) && !TypeSystem.IsRawValue(currentToken))
                {
                    tokens[i] = executionScope[currentToken].Value.ToString();
                }
            }
        }

        private string GetObjectProp(string str , Scope executionScope)
        {
            var props = str.Split('.').ToList();
            if (props[0] == str || str.StartsWith("Sys")) return str;
            var isObject = executionScope[props[0]].Value.GetType() == typeof(DynamicObject);    

            if(isObject)
            {
                var target = executionScope[props[0]];
                str = target.Value.GetProperty(str.Substring(str.IndexOf(".")+1)).Value.ToString();
            }

            return str;
        }

        private List<string> ExpressionParser(string str)
        {
            string ops = "+-/*";
            var result = new List<string>();
            var val = "";
            var op = "";
            bool inPranthesis = false;
            int prCount = 0;
            for (int i = 0; i < str.Length; i++)
            {
                var current = str[i];
                if (current != ' ')
                {
                    if (current == '(')
                    {
                        if (!inPranthesis) inPranthesis = true;
                        prCount++;
                    }
                    if (ops.IndexOf(current) != -1)
                    {
                        if (!inPranthesis)
                        {
                            if (val.Length > 0) result.Add(val);
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
                    if (current == ')')
                    {
                        prCount--;
                        if (prCount == 0)
                        {
                            result.Add(val);
                            val = "";
                            inPranthesis = false;
                        }
                    }

                }
            }
            if (val.Length > 0) result.Add(val);
            return result;
        }

        private dynamic ExpressionGen(List<string> code)
        {
            string lastOp = "";
            string ops = "+-/*";

            dynamic res = null;
            dynamic next;
            if (code.Count == 1 && !code[0].StartsWith("Sys"))
            {
                return TypeSystem.ExtractValueFromString(code[0]);
            }
            for (int i = 0; i < code.Count; i++)
            {
                var _char = code[i];
                if (ops.Contains(_char) && _char.Length == 1)
                {
                    lastOp = _char;
                    if (lastOp != string.Empty)
                    {
                        res = (res == null) ? TypeSystem.ExtractValueFromString(code[i - 1]) : res;
                        next = TypeSystem.ExtractValueFromString(code[i + 1]);
                        if (next.GetType() == typeof(string))
                        {
                            if (lastOp != "+")
                            {
                                throw new Exception("cannot do " + lastOp + " to type string");
                            }
                        }

                        res = ResolveValueBaseOnAction(res, next, lastOp);
                    }
                }
            }

            return res;
        }


        private dynamic ResolveValueBaseOnAction(dynamic val1, dynamic val2, string op)
        {
            switch (op)
            {
                case "+":
                    return val1 + val2;
                case "-": return val1 - val2;
                case "*": return val1 * val2;
                case "/": return val1 / val2;
            }

            return null;
        }

    }
}
