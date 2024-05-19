using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;

namespace Tilang_project.Engine.Tilang_Pipeline
{
    public delegate void LineSplitEvent(string text, bool ended);
    public delegate void TokenCreationEvent(List<string> lines);
    public delegate void StartProcessEvent(List<List<string>> tokenList);
    public delegate void EndProcesssEvent(TilangVariable? result);
    public delegate void ClearProcessStackEvent(List<int> fnStack, List<int> varStack);

    public class Pipeline
    {
        public TilangVariable? ProcessResult;

        private SyntaxAnalyzer _syntaxAnalyzer = new SyntaxAnalyzer();
        private Processor _thread = new Processor();
        private bool Ended = false;

        public Pipeline()
        {
            ConfigurePipeline();
        }



        public LineSplitEvent OnLineSplited;
        public TokenCreationEvent OnTokenCreated;
        public StartProcessEvent OnStartProcesss;
        public EndProcesssEvent OnEndProcesss;
        public ClearProcessStackEvent OnClearProcessStack;


        public void Run(string text)
        {
            _syntaxAnalyzer.LineSeparator(text);
        }

        private void ConfigurePipeline()
        {
            OnClearProcessStack += (List<int> fnStack, List<int> varStack) =>
            {
                if (Ended)
                {
                    _thread.Stack.ClearStackByIndexes(varStack);
                    _thread.Stack.ClearFnStackByIndexes(fnStack);
                }
            };

            OnEndProcesss += (TilangVariable? result) =>
            {
                if (result != null) ProcessResult = result;

                OnClearProcessStack.Invoke(_thread.stackFnIndexes, _thread.stackVarIndexs);
            };

            OnStartProcesss += (List<List<string>> tokenList) =>
            {
                var processResult = _thread.Process(tokenList);
                OnEndProcesss.Invoke(processResult);
            };

            OnTokenCreated += (List<string> lines) =>
            {
                var tokens = _syntaxAnalyzer.TokenCreator(lines[0]);
                OnStartProcesss(new List<List<string>> { tokens });
            };


            OnLineSplited += (string res, bool ended) =>
            {

                OnTokenCreated.Invoke(new List<string>() { res });
                if (ended) this.Ended = true;
            };

            _thread.OnEndProcess = OnClearProcessStack;
            _syntaxAnalyzer.OnLineSplited = OnLineSplited;

        }


        public static TilangVariable? StartNew(string text)
        {
            var pipeline = new Pipeline();

            pipeline.Run(text);

            return pipeline.ProcessResult;
        }
    }
}
