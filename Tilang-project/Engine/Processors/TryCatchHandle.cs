using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.String_Extentions;

namespace Tilang_project.Engine.Processors
{
    public partial class Processor
    {
        public void HandleTryCatch(List<List<string>> tokens, int i)
        {


            var tryTokens = tokens[i];
            IsTryCatched = true;
            executeTry = () =>
            {
                var body = tryTokens[1].GetStringContent();
                var newProcessor = new Processor();
                newProcessor.Stack = new ProcessorStack(this);
                return newProcessor.Process(analyzer.GenerateTokens(body));
            };

            List<string> catchTokens;
            if (tokens[i + 1][0] == Keywords.CATCH_KEYWORD)
            {
                catchTokens = tokens[i + 1];
                executeCatch = (string errorMessage) =>
                {

                    var body = catchTokens.Count == 3 ? catchTokens[2].GetStringContent() : catchTokens[1].GetStringContent();
                    
                    var newProcessor = new Processor();
                    newProcessor.Stack = new ProcessorStack(this);

                    if(catchTokens.Count == 3) {
                      newProcessor.Process(analyzer.GenerateTokens(catchTokens[1].GetStringContent()));  
                      if(errorMessage.Length > 0) {
                         var error = newProcessor.Stack.GetVariableStack().Where((item) => item.TypeName == TypeSystem.STRING_DATATYPE ).First();
                         error.Value = errorMessage;
                      }
                    }
                    
                    return newProcessor.Process(analyzer.GenerateTokens(body));
                };
            }

            List<string> finallyTokens;

            if (tokens[i + 2][0] == Keywords.FINALLY_KEYWORD)
            {
                finallyTokens = tokens[i + 2];
                executeFinally = () =>
                {
                    var body = finallyTokens[1].GetStringContent();
                    var newProcessor = new Processor();
                    newProcessor.Stack = new ProcessorStack(this);
                    return newProcessor.Process(analyzer.GenerateTokens(body));
                };
            }



        }
    }
}