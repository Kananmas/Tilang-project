using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer
{
    public partial class SyntaxAnalyzer
    {
        public static bool IsTernaryOperation(List<dynamic> token)
        {
            return token.Contains("?") && token.Contains(":") && token.IndexOf(":") > token.IndexOf("?");
        }

        public static bool IsTernaryOperation(string token)
        {
            return token.Contains("?") && token.Contains(":") && token.IndexOf(":") > token.IndexOf("?");
        }

        public static List<string> TokenizeFunctionCall(string functionCall)
        {
            var fnName = functionCall.Substring(0, functionCall.IndexOf("(")).Trim();
            var fnArgs = functionCall.Substring(functionCall.IndexOf("(")).Trim();

            var list = new List<string>
            {
                fnName,
                fnArgs
            };
            return list;
        }

        public static bool IsIndexer(string str)
        {
            if (str.StartsWith('[') && str.EndsWith("]") || str.StartsWith("!")) return false;
            if (str.StartsWith("(") && str.EndsWith(")")) return false;
            if (!str.Contains("[") || !str.Contains("]") || str.Length <= 1) return false;

            if (str.StartsWith("("))
            {
                var section = str.Substring(0, str.LastIndexOf(")") + 1);
                str = str.Replace(section, "SECTION");
            }

            var split = SeperateByBrackeyes(str);

            if (split[0].ToCharArray().Any((item) => Keywords.AllOperators.Contains(item.ToString()))) return false;

            return split.Skip(1).All((item) => item.StartsWith("[") && item.EndsWith("]"));
        }

        public static bool IsFunctionCall(string str)
        {
            if (string.IsNullOrEmpty(str) || TypeSystem.IsArray(str)) return false;
            if (str.StartsWith("Sys.out") || str.StartsWith("Sys.in")) return true;
            if (TypeSystem.IsRawValue(str)) return false;
            if (!str.Contains("(") || !str.Contains(")")) return false;
            if (str[0] == '(') return false;
            var isExpression = str.Substring(0, str.IndexOf("("))
                .ToCharArray().Any(item => Keywords.AssignmentOperators.Contains(item.ToString())
                || Keywords.LogicalOperators.Contains(item.ToString()) || item == '[');
            if (isExpression) return false;

            var tokens = TokenizeFunctionCall(str);

            // Return true if there's a match, false otherwise
            return tokens.Count == 2;
        }


        public static List<string> SeperateByBrackeyes(string text)
        {
            var stringLeft = text.Substring(0, text.IndexOf("["));
            var stringRight = text.Substring(text.IndexOf("["));
            var lastIndex = 0;
            var result = new List<string>() { stringLeft };
            var brackeyesCount = 0;
            for (int i = 0; i < stringRight.Length; i++)
            {
                char c = stringRight[i];

                if (c == '[')
                {
                    brackeyesCount++;
                }

                if (c == ']')
                {
                    brackeyesCount--;
                }

                if (brackeyesCount == 0)
                {
                    var len = i - lastIndex;
                    result.Add(stringRight.Substring(lastIndex, len + 1));
                    lastIndex = i + 1;
                }
            }


            return result;
        }
    }
}
