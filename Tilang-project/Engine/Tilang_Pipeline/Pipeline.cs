using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;

namespace Tilang_project.Engine.Tilang_Pipeline
{
    public delegate void LineSplitEvent(string text);
    public delegate void TokenCreationEvent(List<string> lines);
    public delegate void StartProcessEvent(List<List<string>> tokenList);
    public delegate void EndProcesssEvent(TilangVariable? result);
    public delegate void ClearProcessStackEvent(bool ended);

    public class Pipeline
    {
        public TilangVariable? ProcessResult;

        private SyntaxAnalyzer _syntaxAnalyzer = new SyntaxAnalyzer();
        private Processor _thread = new Processor();

        public Pipeline()
        {
            _thread.InPipeLine = true;
            ConfigurePipeline();
        }


        public Pipeline(Processor processor)
        {
            _thread = processor;
            _thread.InPipeLine = true;
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


        private void HandleClearProcessStack(bool ended)
        {
            if (ended)
            {
                _thread.Stack.ClearStackByIndexes(_thread.stackVarIndexs);
                _thread.Stack.ClearFnStackByIndexes(_thread.stackFnIndexes);
            }
        }

        private void HandleLineSplited(string text)
        {
            //try
            //{
              if(ProcessResult == null)  OnTokenCreated.Invoke(new List<string>() { text });
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine(ex.Message + "At " + text);
            //}
        }


        private void HandleTokenCreated(List<string> lines)
        {
            var tokens = _syntaxAnalyzer.TokenCreator(lines[0]);
            OnStartProcesss(new List<List<string>> { tokens });
        }

        private void HandleStartProcesss(List<List<string>> tokenList)
        {
            var processResult = _thread.Process(tokenList);
            if (_thread.PassLoop)
            {
                _thread.PassLoop = false; 
                return;
            }
            OnEndProcesss.Invoke(processResult);

        }

        private void HandleEndProcess(TilangVariable? result)
        {
            if (result != null) ProcessResult = result;
        }



        private void ConfigurePipeline()
        {
            OnClearProcessStack += HandleClearProcessStack;
            OnEndProcesss += HandleEndProcess;
            OnStartProcesss += HandleStartProcesss;
            OnTokenCreated += HandleTokenCreated;
            OnLineSplited += HandleLineSplited;

            _syntaxAnalyzer.OnLineSplited = OnLineSplited;
            _syntaxAnalyzer.OnClearProcessStack = OnClearProcessStack;
        }


        public static TilangVariable? StartNew(string text)
        {
            var pipeline = new Pipeline();

            pipeline.Run(text);

            return pipeline.ProcessResult;
        }

        public static TilangVariable? StartNew(string text , Processor processor)
        {
            var pipeline = new Pipeline(processor);

            pipeline.Run(text);

            return pipeline.ProcessResult;
        }
    }
}
