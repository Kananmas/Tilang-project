
using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Syntax.Analyzer
{
    public partial class ExprAnalyzer
    {
        public TilangVariable? ReadExpression(List<string> tokens, Processor? stack = null)
        {
            if (tokens.Count == 0) return null;
            if (tokens.Count == 1)
            {
                if (TypeSystem.IsRawValue(tokens[0])) return TypeSystem.ParseType(tokens[0], stack);
                if (SyntaxAnalyzer.IsTernaryOperation(tokens[0])) return HandleTernaryOperator(tokens[0], stack);
                var res = ResolveExpression(tokens[0], stack);
                return res;
            }

            if (SyntaxAnalyzer.IsFunctionCall(tokens[0]))
            {
                throw new Exception("cannot assign to a function call");
            }

            var rightSide = stack.Stack.GetFromStack(tokens[0], stack);
            var op = tokens[1];
            if (rightSide.Tag == "Constant")
                throw new Exception("Cannot assign Value To Constant");


            TilangVariable leftSide;
            if (TypeSystem.IsTypeCreation(tokens[2]))
            {
                leftSide = TypeSystem.ParseType(tokens[2], stack);
            }

            else
            {
                leftSide = ResolveExpression(tokens[2], stack);
            }


            rightSide.Assign(leftSide, op);


            return rightSide;
        }

        public TilangVariable? ReadExpression(string token, Processor stack)
        {
            var list = new List<string>
            {
                token
            };

            return ReadExpression(list, stack);
        }

    }
}