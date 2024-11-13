using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.String_Extentions;

namespace Tilang_project.Engine.Processors {
    public partial class Processor {
        public List<object> GetItemsFromStack(List<string> expressionTokens)
        {
            var ops = Keywords.AllOperators;
            var result = new List<dynamic>();

            var isSubExpression = (string value) =>
            {
                if (value == "") return false;
                return value[0] == '(' && value[value.Length - 1] == ')';
            };

            for (var i = 0; i < expressionTokens.Count; i += 1)
            {
                var currentToken = expressionTokens[i];
                if (ops.Contains(currentToken)) { result.Add(currentToken); continue; }

                if (TypeSystem.IsRawValue(currentToken))
                {
                    result.Add(TypeSystem.ParseType(currentToken, this));
                    continue;
                }

                if (SyntaxAnalyzer.IsFunctionCall(currentToken))
                {
                    var list = SyntaxAnalyzer.TokenizeFunctionCall(currentToken);
                    var callResult = ResolveFunctionCall(list);
                    if (callResult == null) { throw new Exception("cannot use null in exprpession"); }
                    result.Add(callResult);
                    continue;
                }

                if (SyntaxAnalyzer.IsLambda(currentToken))
                {
                    var item = TilangFuncPtr.CreateFuncPtr(currentToken, this);
                    result.Add(item);
                    continue;
                }

                if (isSubExpression(currentToken))
                {
                    var str = expressionTokens[i].GetStringContent();
                    var list = new List<string>
                    {
                        str
                    };
                    result.Add(exprAnalyzer.ReadExpression(list, this)
                        ?? TypeSystem.DefaultVariable("null"));
                    continue;
                }



                if (SyntaxAnalyzer.IsIndexer(currentToken))
                {
                    var indexerSplits = SyntaxAnalyzer.SeperateByBrackeyes(currentToken);
                    var left = indexerSplits[0];
                    var leftSide = exprAnalyzer.ReadExpression(left, this);
                    var value = TilangArray.UseIndexer(currentToken, this, leftSide);
                    result.Add(value);
                    continue;
                }


                if (currentToken.StartsWith("."))
                {
                    if (result[i - 1].GetType() != typeof(string)
                        && result[i - 1].Value.GetType() == typeof(TilangStructs))
                    {
                        result.Add(result[i - 1].Value.GetProperty(currentToken, this));
                        result = result.Skip(i).ToList();
                        continue;
                    }
                }


                if (this.Stack.GetFunctionStack().Any(item => item.FunctionName == currentToken))
                {
                    if (expressionTokens.Count != 1) throw new Exception("cannot use func ref with other operators");
                    var target = Stack.GetFunctionStack().Where(item => item.FunctionName == currentToken).First();
                    result.Add(new TilangFuncPtr()
                    {
                        funRef = target,
                    });
                    continue;
                }

                result.Add(Stack.GetFromStack(currentToken, this));
            }

            return result;
        }
    }
}