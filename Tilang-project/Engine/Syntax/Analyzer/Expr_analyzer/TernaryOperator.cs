using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;

namespace Tilang_project.Engine.Syntax.Analyzer
{
    public partial class ExprAnalyzer
    {

        private TilangVariable? HandleTernaryOperator(string token, Processor stack)
        {
            var indexOfQuestionMark = token.IndexOf("?");

            var conditionSide = token.Substring(0, indexOfQuestionMark).Trim();
            var operationsSide = token.Substring(indexOfQuestionMark + 1).Trim();

            var lefSide = operationsSide.Substring(0, operationsSide.IndexOf(':')).Trim();
            var rightSide = operationsSide.Substring(operationsSide.IndexOf(':') + 1).Trim();

            var conditionSideResult = ReadExpression(conditionSide, stack);

            if ((bool)conditionSideResult.Value == true)
            {
                return ReadExpression(lefSide, stack);
            }

            return ReadExpression(rightSide, stack);
        }

        private TilangVariable? ResolveTernaryOperation(List<object> code, Processor stack)
        {

            var op = "";

            code.ForEach(val =>
            {
                op += val.ToString();
            });

            if (SyntaxAnalyzer.IsTernaryOperation(code))
            {
                return HandleTernaryOperator(op, stack);
            }

            throw new Exception("invalid ternary operation");
        }

    }
}