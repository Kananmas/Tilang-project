using Tilang_project.Engine.Services.BoxingOps;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_Pipeline;
using Tilang_project.Utils.String_Extentions;

namespace Tilang_project.Engine.Processors
{
    public partial class Processor
    {
        // cf stands for control flow
        private Processor CreateCFProcessor()
        {
            var newStack = new ProcessorStack(this);
            var newProcess = new Processor
            {
                Stack = newStack,
                ScopeType = "control_flow",
                ParentProcessor = this
            };

            return newProcess;
        }
        private TilangVariable? ConditionalProcess(List<string> tokens)
        {
            if (tokens[0] == "if")
            {
                BoolCache.Clear();
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
            var conditionStatus = exprAnalyzer.ReadExpression(condition, newProcess);



            if (UnBoxer.UnboxBool(conditionStatus) == true && !this.BoolCache.GetLatest())
            {
                var res = Pipeline.StartNew(tokens[2].GetStringContent(),newProcess);
                BoolCache.Append((bool)conditionStatus.Value);

                if (res != null)
                {
                    newProcess.ClearStack();
                    return res;
                }
            }

            BoolCache.Append((bool)conditionStatus.Value);
            newProcess.ClearStack();
            return null;    
        }

        internal TilangVariable? HandleElse(List<string> tokens)
        {
            var newProcess = CreateCFProcessor();

            if (BoolCache.RunElse())
            {
                var res = Pipeline.StartNew(tokens[1].GetStringContent() ,newProcess);

                if (res != null)
                {
                    BoolCache.Clear();
                    newProcess.ClearStack();
                    return res;
                }

                BoolCache.Clear();
                newProcess.ClearStack();
                return null;
            }

            return null;
        }
    }
}