using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;

namespace Tilang_project.Utils.moudle_importer
{
    public static class MoudleHandler
    {
        public static Processor ImportMoudule(string path)
        {
            var file = File.ReadAllText(path);
            var fileProcessor = new Processor();
            var syntaxAnalayzer = new SyntaxAnalyzer();
            fileProcessor.Process(syntaxAnalayzer.GenerateTokens(file));


            return fileProcessor;

        }
    }
}
