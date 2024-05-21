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
        private string TranslateForLoop(List<string> tokens)
        {
            var variablesStr = "";
            var conditionsStr = "";
            var loopVarChange = "";

            var bodyContent = tokens[2].GetStringContent();

            var str = "#variables while(#conditions)\n {\n  #operation  #loop_var_change \n} ";

            var items = tokens[1].GetStringContent().Split(";").ToList();

            if (items.Count % 3 != 0)
            {
                throw new Exception("invalid for loop condition");
            }
            var itemStep = items.Count / 3;

            for (int i = 0; i < itemStep; i++)
            {
                var currentItem = items[i];
                var res = "var " + currentItem.Replace(Keywords.EQUAL_ASSIGNMENT, " = ") + ";\n";

                variablesStr += res;
            }

            for (int i = itemStep; i < itemStep * 2; i++)
            {
                var currentItem = items[i];
                var res = currentItem + "&&";

                conditionsStr += res;

            }

            conditionsStr = conditionsStr.Substring(0, conditionsStr.LastIndexOf("&&")).Trim();

            for (int i = itemStep * 2; i < itemStep * 3; i++)
            {
                loopVarChange += items[i] + ";\n";
            }

            str = str.Replace("#variables", variablesStr)
                .Replace("#conditions", conditionsStr).Replace("#loop_var_change", loopVarChange)
                .Replace("#operation", bodyContent);

            return str;
        }

       

        private TilangVariable? LoopProcess(List<string> tokens)
        {
            var condition = tokens[1].GetStringContent();
            var processBody = tokens[2].GetStringContent();

            var process = new Processor();
            process.Stack = new ProcessorStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
            process.ScopeType = "loop";

            TilangVariable var = null;
            var conditionRes = exprAnalyzer.ReadExpression(condition, process).Value;


            while ((bool)conditionRes)
            {
                if (process.ScopeType == "loop(breaked)")
                {
                    break;
                }
                var res = Pipeline.StartNew(processBody , process);
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