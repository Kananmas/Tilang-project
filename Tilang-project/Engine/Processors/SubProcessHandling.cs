using Tilang_project.Engine.Services.BoxingOps;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Structs;
using Tilang_project.Utils.String_Extentions;

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

            var res = newProcess.Process(analyzer.GenerateTokens(fn.Body.GetStringContent()));
            newProcess.Stack.ClearStackByIndexes(list);

            return res;
        }

        private TilangVariable? LoopProcess(List<string> tokens)
        {
            var condition = tokens[1].GetStringContent();
            var processBody = tokens[2].GetStringContent();

            var process = new Processor();
            process.Stack = new ProcessorStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
            process.ScopeName = "loop";

            TilangVariable var = null;
            var conditionRes = exprAnalyzer.ReadExpression(condition, process).Value;


            while ((bool)conditionRes)
            {
                if (process.ScopeName == "loop(breaked)")
                {
                    break;
                }
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
            newProcess.ScopeName = this.ScopeName == "loop" ? "loop" : "loop.if";

            if (tokens[0] == "if")
            {
                this.BoolCache.Clear();
            }

            if (tokens[0] == "else" && tokens[1] != "if")
            {
                if (this.BoolCache.RunElse())
                {

                    var body = tokens[1].GetStringContent();

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

            var condition = tokens[1].GetStringContent();
            var processBody = tokens[2].GetStringContent();
            var conditionStatus = exprAnalyzer.ReadExpression(condition, newProcess);


            newProcess.Stack = newStack;

            if (UnBoxer.UnboxBool(conditionStatus) == true && !this.BoolCache.GetLatest())
            {
                var res = newProcess.Process(analyzer.GenerateTokens(processBody));
                if (newProcess.ScopeName.EndsWith("(passed)")) ScopeName += "(passed)";
                this.BoolCache.Append((bool)conditionStatus.Value);

                if (res != null)
                {
                    return res;
                }
            }

            this.BoolCache.Append((bool)conditionStatus.Value);

            return null;
        }
    }
}