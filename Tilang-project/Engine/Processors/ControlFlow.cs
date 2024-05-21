using Tilang_project.Engine.Services.BoxingOps;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_Pipeline;
using Tilang_project.Utils.String_Extentions;

namespace Tilang_project.Engine.Processors
{
    public partial class Processor
    {
        private Processor CreateCFProcessor()
        {
            var newStack = new ProcessorStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
            var newProcess = new Processor();
            newProcess.Stack = newStack;
            newProcess.ScopeType = "control_flow";


            return newProcess;
        }
        private TilangVariable? ConditionalProcess(List<string> tokens)
        {
          

            if (tokens[0] == "if")
            {
                this.BoolCache.Clear();
            }

            if (tokens[0] == "else" && tokens[1] != "if")
            {
               return  HandleElse(tokens);
            }
            

            return HandleIfElseIf(tokens);
        }


        internal TilangVariable? HandleIfElseIf(List<string> tokens)
        {
            var newProcess = CreateCFProcessor();
            var condition = tokens[1].GetStringContent();
            var processBody = tokens[2].GetStringContent();
            var conditionStatus = exprAnalyzer.ReadExpression(condition, newProcess);

            if (UnBoxer.UnboxBool(conditionStatus) == true && !this.BoolCache.GetLatest())
            {
                var res = Pipeline.StartNew(processBody, newProcess);
                this.BoolCache.Append((bool)conditionStatus.Value);

                if (res != null)
                {
                    return res;
                }
            }

            this.BoolCache.Append((bool)conditionStatus.Value);
            return null;    
        }

        internal TilangVariable? HandleElse(List<string> tokens)
        {
            var newProcess = CreateCFProcessor();

            if (this.BoolCache.RunElse())
            {

                var body = tokens[1].GetStringContent();

                var res = Pipeline.StartNew(body, newProcess);

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
    }
}