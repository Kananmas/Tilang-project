
using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Services.BoxingOps;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Syntax.Analyzer
{
    public partial class ExprAnalyzer
    {
        private TilangVariable ResolveExpression(string expression, Processor stack)
        {
            var parsedExpression = TokenizeExpression(expression);
            var exprStack = stack.GetItemsFromStack(parsedExpression);


            return ConfigureResult(exprStack, stack);
        }

     
        private TilangVariable ConfigureResult(List<object> code, Processor stack)
        {
            var ops = Keywords.AllOperators;

            TilangVariable res = null;
            TilangVariable next;
            
            if (code.Count == 1)
            {
                return (TilangVariable)code[0];
            }

            if (code[0].GetType() == typeof(string) && (string)code[0] == "!")
            {
                var result = ConfigureResult(code.Skip(1).ToList(), stack);
                result.Value = ! (bool)result.Value;
                return result;
            }

            if (code.Count == 2)
            {
                return TypeSystem.ParseType(code[0] + ((TilangVariable) code[1]).Value.ToString() + "", stack);
            }

            for (int i = 0; i < code.Count; i++)
            {
                var _char = code[i];
                if (_char.GetType() == typeof(string) && ops.Contains(_char))
                {
                    string lastOp = (string)_char;


                    if (SyntaxAnalyzer.IsTernaryOperation(code.Skip(i - 1).ToList()))
                    {
                        return ResolveTernaryOperation(code.Skip(i - 1).ToList(), stack) ;
                    }


                    res = res == null ? ((TilangVariable) code[i - 1]).GetCopy() : res;
                    next =  (TilangVariable) code[i + 1];

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

                    if (Keywords.TwoSidedOperators.Contains(lastOp))
                    {
                        var rest = code.Skip(i + 1).ToList();
                        next = ConfigureResult(rest, stack);

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
