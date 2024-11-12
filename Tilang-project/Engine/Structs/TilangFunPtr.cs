using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Services.Creators;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Structs
{
    public class TilangFuncPtr : TilangVariable
    {
        public TilangFunction funRef;

        public TilangFuncPtr()
        {
            this.TypeName = TypeSystem.FUNC_PTR_DATATYPE;
        }

        public static TilangFuncPtr CreateFuncPtr(string text, Processor processor)
        {
            var lambdaToFunc = ExprAnalyzer.LambdaToFunc(text, "unknown");
            var function = FunctionCreator.CreateFunction(lambdaToFunc, processor);
            var item = new TilangFuncPtr()
            {
                funRef = function,
                OwnerId = processor.scopeId
            };

            return item;
        }
    }
}