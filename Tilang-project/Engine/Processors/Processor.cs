using Tilang_project.Engine.Creators;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Processors
{
    public class Processor
    {
        public VariableStack Stack = new VariableStack();

        private SyntaxAnalyzer analyzer = new SyntaxAnalyzer();
        private ExprAnalyzer exprAnalyzer = new ExprAnalyzer();

        private TilangVariable? SubProcess(TilangFunction fn, List<TilangVariable> argValues)
        {
            var newStack = new VariableStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
            var i = 0;
            fn.FunctionArguments.ForEach(x =>
            {
                x.Assign(argValues[i++], "=");
                newStack.SetInStack(x);
            });

            var newProcess = new Processor();
            newProcess.Stack = newStack;
            return newProcess.Process(analyzer.GenerateTokens(fn.Body.Substring(1, fn.Body.Length - 2).Trim()));
        }

        public TilangVariable? Process(List<List<string>> tokenList)
        {

            List<int> stackVarIndexs = new List<int>();
            List<int> stackFnIndexes = new List<int>();


            foreach (var tokens in tokenList)
            {

                if (tokens.Count > 0)
                {
                    var initialToken = tokens[0];


                    switch (initialToken)
                    {
                        case "": break;
                        case "var":
                        case "const":
                            var variable = VariableCreator.CreateVariable(tokens);
                            var index = Stack.SetInStack(variable);
                            stackVarIndexs.Add(index);
                            break;
                        case "type":
                            TypeCreator.CreateDataStructrue(tokens);
                            break;
                        case "function":
                            var fn = FunctionCreator.CreateFunction(tokens);
                            var fnIndex = Stack.AddFunction(fn);
                            stackFnIndexes.Add(fnIndex);
                            break;
                        case "switch":
                        case "if":
                        case "else if":
                        case "else":
                            break;
                        case "while":
                        case "for":
                            break;
                        default:
                            if (tokens[0].StartsWith("return"))
                            {
                                var expr = tokens[0].Substring(0 + "return".Length).Trim();

                                return exprAnalyzer.ReadExpression(expr, this);
                            };

                            var target = tokens[0] + tokens[1] ?? "";

                            if (SyntaxAnalyzer.IsFunctionCall(target))
                            {
                                return ResolveFunctionCall(tokens);
                            }

                            exprAnalyzer.ReadExpression(tokens, this);
                            break;
                    }
                }

            }
            var t1 = Task.Run(() =>
            {
                Stack.ClearStackByIndexes(stackVarIndexs);
            });
            var t2 = Task.Run(() =>
            {
                Stack.ClearFnStackByIndexes(stackFnIndexes);
            });

            Task.WaitAll([t1, t2]);

            return null;
        }


        public void ReplaceItemsFromStack(List<string> expressionTokens)
        {
            var isSubExpression = (string value) =>
            {
                return value[0] == '(' && value[value.Length - 1] == ')';
            };
            for (var i = 0; i < expressionTokens.Count; i += 2)
            {
                var isFunctionCall = SyntaxAnalyzer.IsFunctionCall(expressionTokens[i]);

                if (isSubExpression(expressionTokens[i]))
                {
                    var str = expressionTokens[i].Substring(1, expressionTokens[i].Length - 2).Trim();
                    var list = new List<string>();
                    list.Add(str);
                    expressionTokens[i] = exprAnalyzer.ReadExpression(list, this).Value.ToString();
                }

                if (isFunctionCall)
                {
                    var toks = expressionTokens[i].Replace("(", " (").Split(" ");
                    var callResult = ResolveFunctionCall(toks.ToList())?.Value;
                    if (callResult == null) { throw new Exception("cannot use null in exprpession"); }
                    expressionTokens[i] = callResult.ToString();
                    continue;
                }
                else
                {
                    if (TypeSystem.IsRawValue(expressionTokens[i]))
                    {
                        continue;
                    }
                    else
                    {
                        expressionTokens[i] = Stack.GetFromStack(expressionTokens[i]).Value.ToString();
                    }
                }
            }
        }

        private TilangVariable? ResolveFunctionCall(List<string> tokens)
        {
            var isExpression = (string str) =>
            {
                var tokens = "+ - / *".Split(" ");

                return tokens.Any((item) => str.Contains(item));
            };

            var fnName = tokens[0];
            var fnArgs = tokens[1].Substring(1, tokens[1].Length - 2).Split(",").Select((item) =>
            {
                if (isExpression(item.Trim()))
                {
                    var variable = exprAnalyzer.ReadExpression(item, this);

                    return variable;
                }
                if (TypeSystem.IsRawValue(item))
                {
                    item = item.Trim();

                    return TypeSystem.ParseType(item);
                }

                return Stack.GetFromStack(item);

            }).ToList();
            var calledFn = Stack.GetFunction(fnName);
            return SubProcess(calledFn, fnArgs);
        }
    }
}
