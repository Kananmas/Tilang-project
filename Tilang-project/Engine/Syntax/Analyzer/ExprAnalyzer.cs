
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Syntax.Analyzer
{
    public class ExprAnalyzer
    {

        public void ReadExpression(List<string> tokens)
        {
            if (tokens.Count == 0) return;
            if (tokens.Count == 1)
            {
                var res = ResolveExpression(tokens[0]);
                return;
            }

            var lefSide = ResolveExpression(tokens[2]);
        }


        private TilangVariable ResolveExpression(string expression)
        {
            return ExpressionGen(ParseMathExpression(expression));
        }

        private List<string> ParseMathExpression(string str)
        {
            var ops = "+ - / * *= /= += -= == != || && = !".Split(" ").ToList();
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

                if (current != " ")
                {
                    if (current == "(")
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
                            if (val.Length > 0)
                            {
                                result.Add(val);
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
                    if (current == ")")
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
            if (val.Length > 0)
            {
                result.Add(val);
            }


            result.ForEach(val =>
            {
                if (TypeSystem.IsTypeCreation(val)) throw new Exception("cannot use operator with custom type");
            });

            return result;

        }


        private TilangVariable ExpressionGen(List<string> code)
        {
            string lastOp = "";
            string ops = "+-/*";

            dynamic res = null;
            dynamic next;
            if (code.Count == 1 && !code[0].StartsWith("Sys"))
            {
                return TypeSystem.ParsePrimitiveType(code[0]);
            }
            for (int i = 0; i < code.Count; i++)
            {
                var _char = code[i];
                if (ops.Contains(_char) && _char.Length == 1)
                {
                    lastOp = _char;
                    if (lastOp != string.Empty)
                    {
                        res = (res == null) ? TypeSystem.ParsePrimitiveType(code[i - 1]) : res;
                        next = TypeSystem.ParsePrimitiveType(code[i + 1]);
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
                case "&&":
                    result.TypeName = "bool";
                    result.Value = val1.Value && val2.Value;
                    return result;
            }

            return val1;

        }


        private TilangVariable ExecuteCode(string code)
        {
            return new TilangVariable();
        }
    }
}
