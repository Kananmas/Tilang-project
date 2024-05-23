using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.String_Extentions;

namespace Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer
{
    public partial class SyntaxAnalyzer
    {
        public static string[] GetImportNameAndPath(string importline)
        {
            var indexOfFrom = importline.IndexOf("from");
            var itemNamesIndex = 0 + Keywords.IMPORT_KEYWORD.Length;
            var pathIndex = indexOfFrom + 4;

            var path = importline.Slice(pathIndex);
            var items = importline.Slice(itemNamesIndex, indexOfFrom).Split(",").ToList();
            items.Add(path);

            return items.ToArray();
        }


        public static bool IsTernaryOperation(List<dynamic> token)
        {
            return token.Contains("?") && token.Contains(":") && token.IndexOf(":") > token.IndexOf("?");
        }

        public static bool IsTernaryOperation(string token)
        {
            return token.Contains("?") && token.Contains(":") && token.IndexOf(":") > token.IndexOf("?");
        }

        public static List<string> SplitBySperatorToken(string str)
        {
            var result = new List<string>();
            var ignoringIndex = new IgnoringRanges();
            ignoringIndex.AddIndexes(str);
            var parnthesisCount = 0;
            var currentStr = string.Empty;

            for (int i = 0; i < str.Length; i++)
            {
                var character = str[i];
                if ((character == '(' || character == '{' || character == '[') && !ignoringIndex.IsIgnoringIndex(i))
                {
                    parnthesisCount++;
                }
                if (character == Keywords.COMMA_TOKEN[0] && !ignoringIndex.IsIgnoringIndex(i))
                {
                    if (parnthesisCount == 0)
                    {
                        if (currentStr.Length > 0) result.Add(currentStr);
                        currentStr = string.Empty;
                    }
                    else
                    {
                        currentStr += character;
                    }
                }
                else
                {
                    currentStr += character;
                }

                if ((character == ')' || character == '}' || character == ']') && !ignoringIndex.IsIgnoringIndex(i))
                {
                    parnthesisCount--;
                }
            }

            if (currentStr != string.Empty)
            {
                result.Add(currentStr);
            }

            return result;
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
            if(split.Count == 0) return false;
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
            return tokens.Count == 2 && !IsIndexer(tokens[1]);
        }


        public static List<string> SeperateByBrackeyes(string text)
        {
            var lastIndex = 0;
            var result = new List<string>();
            var brackeyesCount = 0;
            var pranCount = 0;
            // cb:curvy brackeyes
            var cbCount = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '(') pranCount++;
                if (c == ')') pranCount--;
                if (c == '{') cbCount++;
                if (c == '}') cbCount--;

                if (pranCount == 0 && cbCount == 0)
                {
                  
                    if (c == '[')
                    {

                        if (brackeyesCount == 0)
                        {
                            var len = i - lastIndex;
                            result.Add(text.Substring(lastIndex, len));
                            lastIndex = i;
                        }

                        brackeyesCount++;
                    }

                    if (c == ']')
                    {
                        brackeyesCount--;

                        if (brackeyesCount == 0)
                        {
                            var len = i - lastIndex;
                            result.Add(text.Substring(lastIndex, len + 1));
                            lastIndex = i + 1;
                        }
                    }


                }

            }

            if (brackeyesCount > 0)
            {
                throw new Exception("invalid use of [] operator");
            }

            result = result.Where((item) => !string.IsNullOrEmpty(item)).ToList(); 

            return result;
        }
    }
}
