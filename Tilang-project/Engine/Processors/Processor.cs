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
            var analyzer = new SyntaxAnalyzer();


            newProcess.Process(analyzer.GenerateTokens(fn.Body.Substring(1, fn.Body.Length - 2).Trim()));

            return null;
        }

        public void Process(List<List<string>> tokenList)
        {

            List<int> stackVarIndexs = new List<int>();
            List<int> stackFnIndexes = new List<int>();

            var exprAnalyzer = new ExprAnalyzer();

            foreach (var tokens in tokenList)
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
                        var indexOfEqual = tokens.IndexOf("=");
                        if (indexOfEqual == -1)
                        {
                            var target = tokens[0] + tokens[1] ?? "";

                            if (SyntaxAnalyzer.IsFunctionCall(target))
                            {

                                var fnName = tokens[0];
                                var fnArgs = tokens[1].Substring(1, tokens[1].Length-2).Split(",").Select((item) =>
                                {
                                    if (TypeSystem.IsRawValue(item))
                                    {
                                        item = item.Trim();

                                        var variable = TypeSystem.ParseType(item);

                                        return variable;
                                    }

                                    return Stack.GetFromStack(item);

                                }).ToList();
                                var calledFn = Stack.GetFunction(fnName);
                                SubProcess(calledFn, fnArgs);
                            }
                            else
                            {
                                exprAnalyzer.ReadExpression(tokens);
                            }
                        }
                        else
                        {

                        }

                        break;

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
        }
    }
}
