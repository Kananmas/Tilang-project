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
            List<List<string>> slicedTokens = tokens.GetRange(i, i + 2 < tokens.Count-1 ? i + 2 : tokens.Count-1);

            var tryTokens = tokens[i];
            IsTryCatched = true;
            executeTry = () =>
            {
                var body = tryTokens[1].GetStringContent();
                var newProcessor = new Processor();
                newProcessor.Stack = new ProcessorStack(this);
                return newProcessor.Process(analyzer.GenerateTokens(body));
            };

            List<string>? catchTokens = slicedTokens.Where((item) => item[0] == Keywords.CATCH_KEYWORD).FirstOrDefault();
            if (catchTokens != null)
            {
                executeCatch = (string errorMessage) =>
                {

                    var body = catchTokens.Count == 3 ? catchTokens[2].GetStringContent() : catchTokens[1].GetStringContent();

                    var newProcessor = new Processor();
                    newProcessor.Stack = new ProcessorStack(this);

                    if (catchTokens.Count == 3)
                    {
                        newProcessor.Process(analyzer.GenerateTokens(catchTokens[1].GetStringContent()));
                        if (errorMessage.Length > 0)
                        {
                            var error = newProcessor.Stack.GetVariableStack().Where((item) => item.TypeName == TypeSystem.STRING_DATATYPE).First();
                            error.Value = errorMessage;
                        }
                    }

                    return newProcessor.Process(analyzer.GenerateTokens(body));
                };
            }

            List<string>? finallyTokens = slicedTokens.Where((item) => item[0] == Keywords.FINALLY_KEYWORD).FirstOrDefault();

            if (finallyTokens != null)
            {
                executeFinally = () =>
                {
                    var body = finallyTokens[1].GetStringContent();
                    var newProcessor = new Processor();
                    newProcessor.Stack = new ProcessorStack(this);
                    return newProcessor.Process(analyzer.GenerateTokens(body));
                };
            }

            if (finallyTokens == null && catchTokens == null) throw new Exception("either catch or finally must be use with try keyword");

        }
    }
}