using System.Diagnostics;
using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Services.Creators
{
    public static class VariableCreator
    {
        public static TilangVariable CreateVariable(List<string> tokens, Processor processor)
        {
            var indexOfEqual = tokens.IndexOf(Keywords.EQUAL_ASSIGNMENT);
            var exprAnalyzer = new ExprAnalyzer();

            var Type = tokens[1];
            var Tag = tokens[0];
            var Name = tokens[2];

            if (Type == TypeSystem.FUNC_PTR_DATATYPE)
            {
                return CreateFuncPtr(tokens, processor);
            }

            var result = TypeSystem.DefaultVariable(Type);

            result.Tag = Tag == Keywords.CONST_KEYWORD ? "Constant" : "Variable";

            result.VariableName = Name;
            result.OwnerId = processor.scopeId;

            if (indexOfEqual == -1)
            {
                return result;
            }

            var rightSideRes = exprAnalyzer.ReadExpression(tokens[4], processor);

            result.Assign(rightSideRes, Keywords.EQUAL_ASSIGNMENT);

            return result;
        }

        public static TilangFuncPtr CreateFuncPtr(List<string> tokens, Processor processor)
        {
            if (tokens.Count < 5)
            {
                return new TilangFuncPtr()
                {
                    VariableName = tokens[2],
                    OwnerId = processor.scopeId,
                    Tag = tokens[0],
                    TypeName = TypeSystem.FUNC_PTR_DATATYPE,
                    funRef = new TilangFunction()
                };
            }
            var isLambda = SyntaxAnalyzer.IsLambda(tokens[4]);

            if (isLambda)
            {
                var lambdaToFunc = ExprAnalyzer.LambdaToFunc(tokens[4], tokens[2]);
                var newFunc = FunctionCreator.CreateFunction(lambdaToFunc, processor);

                return new TilangFuncPtr()
                {
                    VariableName = tokens[2],
                    OwnerId = processor.scopeId,
                    Tag = tokens[0],
                    TypeName = TypeSystem.FUNC_PTR_DATATYPE,
                    funRef = newFunc
                };
            }

            if (processor.Stack.GetFunctionStack().Any((item) => item.FunctionName == tokens[4]))
            {

                var targetFn = processor.Stack.GetFunctionStack()
                .Where(item => item.FunctionName == tokens[4]).First();

                return new TilangFuncPtr()
                {
                    VariableName = tokens[2],
                    OwnerId = processor.scopeId,
                    Tag = tokens[0],
                    TypeName = TypeSystem.FUNC_PTR_DATATYPE,
                    funRef = targetFn
                };
            }

            var exprAnalyzer = new ExprAnalyzer();
            var exprRes = exprAnalyzer.ReadExpression(tokens[4], processor);

            if (exprRes != null && exprRes.GetType() == typeof(TilangFuncPtr))
            {
                var result = (TilangFuncPtr)exprRes;
                if (result == null) throw new Exception("something went wrong");
                return new TilangFuncPtr()
                {
                    VariableName = tokens[2],
                    OwnerId = processor.scopeId,
                    Tag = tokens[0],
                    TypeName = TypeSystem.FUNC_PTR_DATATYPE,
                    funRef = result.funRef
                };
            }

            throw new Exception("unkown expression:" + tokens[4]);
        }
    }
}
