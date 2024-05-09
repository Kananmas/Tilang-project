using Tilang_project.Engine.Creators;
using Tilang_project.Engine.Stack;

namespace Tilang_project.Engine.Processors
{
    public class Processor
    {
        public VariableStack Stack = new VariableStack();

        public void Process(List<List<string>> tokenList)
        {

            List<int> stackVarIndexs = new List<int>();
            List<int> stackFnIndexes = new List<int>();

            foreach (var tokens in tokenList)
            {
                var initialToken = tokens[0];

                switch (initialToken)
                {
                    case "":break;
                    case "var":
                    case "const":
                        var variable = VariableCreator.CreateVariable(tokens);
                        var index = Stack.SetInStack(variable.VariableName, variable);
                        stackVarIndexs.Add(index);
                        break;
                    case "type":
                        TypeCreator.CreateDataStructrue(tokens);
                        break;
                    case "function":
                        var fn = FunctionCreator.CreateFunction(tokens);
                        var fnIndex = Stack.AddFunction(fn.FunctionName, fn);
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
