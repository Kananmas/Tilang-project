using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Services.Creators;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.moudle_importer;

namespace Tilang_project.Engine.Module_Packet
{
    public class ModulePacket
    {
        private Processor ResolvedProcessor { get; set; }


        public ModulePacket(string filePath)
        {
            ResolvedProcessor = MoudleHandler.ImportMoudule(filePath);
        }

        public TilangVariable? Invoke(string call , Processor invoker)
        {
            if(SyntaxAnalyzer.IsFunctionCall(call))
            {
                var tokenizedCall = SyntaxAnalyzer.TokenizeFunctionCall(call);
                var args = TypeSystem.ParseFunctionArguments(tokenizedCall[1] , invoker);
                var fn = ResolvedProcessor.Stack.GetFunction(FunctionCreator.CreateFunctionDef(tokenizedCall[0] , args));
                return ResolvedProcessor.FunctionProcess(fn , args);
            }
            var varProcessTokens = new List<List<string>>()
            {
                new List<string> { call }
            };

            return ResolvedProcessor.Process(varProcessTokens);

        }
    }
}
