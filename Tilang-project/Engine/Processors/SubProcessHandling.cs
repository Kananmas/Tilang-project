using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Structs;

namespace Tilang_project.Engine.Processors
{
    public partial class Processor
    {
        public TilangVariable? FunctionProcess(TilangFunction fn, List<TilangVariable> argValues)
        {
            var newStack = new ProcessorStack(this);
            var list = fn.InjectFunctionArguments(this, argValues);
            var newProcess = new Processor();
            newProcess.Stack = newStack;
            newProcess.ScopeName = fn.FunctionName;

            var res = newProcess.Process(analyzer.GenerateTokens(fn.Body.Substring(1, fn.Body.Length - 2).Trim()));
            newProcess.Stack.ClearStackByIndexes(list);

            return res;
        }

        private TilangVariable? LoopProcess(List<string> tokens)
        {
            var condition = tokens[1].Substring(1, tokens[1].Length - 2).Trim();
            var processBody = tokens[2].Substring(1, tokens[2].Length - 2).Trim();

            var process = new Processor();
            process.Stack = new ProcessorStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
            process.ScopeName = "loop";

            TilangVariable var = null;
            var conditionRes = exprAnalyzer.ReadExpression(condition, process).Value;


            while (conditionRes)
            {

                var res = process.Process(analyzer.GenerateTokens(processBody));
                if (res != null)
                {
                    return res;
                }
                var = res;
                conditionRes = exprAnalyzer.ReadExpression(condition, process).Value;
            }

            return var;
        }

        private TilangVariable? ConditionalProcess(List<string> tokens)
        {
            var newStack = new ProcessorStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
            var newProcess = new Processor();
            newProcess.Stack = newStack;
            newProcess.ScopeName = this.ScopeName == "loop" ? "loop" : "if";

            if (tokens[0] == "if")
            {
                this.BoolCache.Clear();
            }

            if (tokens[0] == "else" && tokens[1] != "if")
            {
                if (this.BoolCache.RunElse())
                {

                    var body = tokens[1].Substring(1, tokens[1].Length - 2).Trim();

                    var res = newProcess.Process(analyzer.GenerateTokens(body));

                    if (res != null)
                    {
                        this.BoolCache.Clear();
                        return res;
                    }

                    this.BoolCache.Clear();
                    return null;
                }

                return null;
            }

            var condition = tokens[1].Substring(1, tokens[1].Length - 2).Trim();
            var processBody = tokens[2].Substring(1, tokens[2].Length - 2).Trim();
            var conditionStatus = exprAnalyzer.ReadExpression(condition, newProcess);


            newProcess.Stack = newStack;

            if (conditionStatus.Value == true && !this.BoolCache.GetLatest())
            {
                var res = newProcess.Process(analyzer.GenerateTokens(processBody));
                this.BoolCache.Append(conditionStatus.Value);

                if (res != null)
                {
                    return res;
                }
            }

            this.BoolCache.Append(conditionStatus.Value);

            return null;
        }
    }
}