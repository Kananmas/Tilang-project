using System.Diagnostics;
using Tilang_project.Engine.Services.BoxingOps;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_Pipeline;
using Tilang_project.Utils.String_Extentions;

namespace Tilang_project.Engine.Processors
{
     
    public partial class Processor
    {
        private string[] TranslateForLoop(List<string> tokens)
        {
            var variablesStr = "";
            var conditionsStr = "";
            var loopVarChange = "";

            var bodyContent = tokens[2].GetStringContent();
            var items = tokens[1].GetStringContent().Split(";").ToList();

            if (items.Count % 3 != 0)
            {
                throw new Exception("invalid for loop condition");
            }
            var itemStep = items.Count / 3;

            var t1 = Task.Run(() =>
            {
                variablesStr = getVariables(items, itemStep);
            });
            var t2 = Task.Run(() => {
                conditionsStr = getConditionsStr(items, itemStep);
            });
            var t3 = Task.Run(() =>
            {
                loopVarChange = getAssignmentStr(items, itemStep);
            });


            Task.WaitAll([t1 , t2, t3]);

            return [variablesStr , conditionsStr , tokens[2] , loopVarChange];
        }

        
        internal string getVariables(List<string> items , int step)
        {
            string variablesStr = string.Empty;
            for (int i = 0; i < step; i++)
            {
                var currentItem = items[i];
                var res = "var " + currentItem.Replace(Keywords.EQUAL_ASSIGNMENT, " = ") + ";\n";

                variablesStr += res;
            }

            return variablesStr;
        }


        internal string getConditionsStr(List<string> items , int step)
        {
            string conditionsStr = string.Empty;    
            for (int i = step; i < step * 2; i++)
            {
                var currentItem = items[i];
                var res = currentItem + "&&";

                conditionsStr += res;

            }

            conditionsStr = conditionsStr.Substring(0, conditionsStr.LastIndexOf("&&")).Trim();


            return conditionsStr;
        }

        internal string getAssignmentStr(List<string> items , int step)
        {
            string assignmentStr = string.Empty;


            for (int i = step * 2; i < step * 3; i++)
            {
                assignmentStr += items[i] + ";\n";
            }


            return assignmentStr;
        }


        private TilangVariable? ForLoopProcess(string vars, string conditions, string body, string loopOp)
        {
            var newProcess = new Processor();
            newProcess.Stack = new ProcessorStack(this);
            newProcess.ScopeType = "loop";
            ParentProcessor = this;
            newProcess.IsForLoop = true;
            var bodyTokens = analyzer.GenerateTokens(body.GetStringContent());
            // inject variables 
            newProcess.Process(this.analyzer.GenerateTokens(vars));

            

            while ((bool)newProcess.Process(this.analyzer.GenerateTokens("return " + conditions)).Value)
            {
                var processRes = newProcess.Process(bodyTokens);
                if (newProcess.LoopBreak) break;
                if (processRes != null) { 
                    newProcess.ClearStack();
                    return processRes; 
                }
                newProcess.Process(analyzer.GenerateTokens(loopOp));
            }

            newProcess.ClearStack();

            return null;
        }


        private TilangVariable? LoopProcess(List<string> tokens)
        {
            var condition = (tokens[1].GetStringContent());
            var processBody = analyzer.GenerateTokens(tokens[2].GetStringContent());

            var process = new Processor();
            process.Stack = new ProcessorStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
            process.ScopeType = "loop";
            process.ParentProcessor = this;

            TilangVariable var = null;
            var conditionRes = exprAnalyzer.ReadExpression(condition, process).Value;

            while ((bool)conditionRes && !process.LoopBreak)
            {
                var res = process.Process(processBody);
                if(process.LoopBreak) break;
                if (res != null)
                {
                    return res;
                }
                var = res;
                conditionRes = exprAnalyzer.ReadExpression(condition, process).Value;
            }

            return var;
        }

       
    }
}