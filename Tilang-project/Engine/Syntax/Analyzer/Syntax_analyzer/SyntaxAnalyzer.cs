using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_Pipeline;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer
{
    public partial class SyntaxAnalyzer
    {

        public LineSplitEvent OnLineSplited { get; set; }
        public ClearProcessStackEvent OnClearProcessStack { get; set; }
        
        public List<List<string>> GenerateTokens(string text)
        {
            return SplitLines(text).Select((line) => TokenCreator(line)).Select((item) => item = item.Where(item => item != "").ToList()).ToList();
        }
        private string FormatLines(string text)
        {
            var result = text;

            result = result.Replace("<", " <").Replace("if(", "if (").Replace("switch", "switch ")
                .Replace("while(", "while (").Replace("for(", "for (")
                .Replace("\\\'", Keywords.SINGLE_QUOET_RP).Replace("\\\"", Keywords.DOUBLE_QUOET_RP)
                .Replace("+=", " += ").Replace("-=", " -= ").Replace("*=", " *= ").Replace("/=", " /= ").Replace("\t", " ");

            return result;
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

        public List<string> TokenCreator(string text)
        {
            var tokens = text.Split(" ").Where((item) => item != "").ToList();
            if (text == string.Empty) return new List<string>();
            if (tokens.Count == 0) throw new Exception("use space in your line asshole");
            if (text.StartsWith("return "))
            {
                return new List<string> { text };
            }
            switch (tokens[0])
            {
                case "":
                    return new List<string>();
                default:
                    if (TypeSystem.PrimitiveDatatypes.Contains(tokens[0])
                        || TypeSystem.IsArrayType(tokens[0]) || 
                        TypeSystem.CustomTypes.ContainsKey(tokens[0])) return TokenCreator("var " + text);
                    return TokenizeAssignments(text);
                case Keywords.CONST_KEYWORD:
                case Keywords.VAR_KEYWORD:
                    {
                        return TokenizeVarAndConsts(text);
                    }
                case Keywords.FOR_KEYWORD:
                case Keywords.WHILE_KEYWORD:
                case Keywords.SWITCH_KEYWORD:
                case Keywords.IF_KEYWORD:
                case Keywords.ELSE_KEYWORD:
                case Keywords.ELSE_IF_KEYWORD:

                case Keywords.FUNCTION_KEYWORD:
                    {
                        return TokenizeBlocks(text);
                    }
                case Keywords.TYPE_KEYWORD:
                    {
                        return TokenizeTypes(ref tokens);
                    }
                case Keywords.RETURN_KEYWORD:
                    return new List<string> { text };
            }
        }

        private List<string> TokenizeAssignments(string text)
        {

            var assingmentsTypes = Keywords.AssignmentOperators;

            if (IsFunctionCall(text) && !assingmentsTypes.Any((item) => text.Contains(item)) || text.StartsWith("Sys"))
            {
                return new List<string> { text };
            }

            var assingment = "";

            foreach (var ass in assingmentsTypes)
            {
                if (text.Contains(ass))
                {
                    assingment = ass; break;
                }
            }

            if (assingment == string.Empty)
            {
                var result = new List<string>
                {
                    text
                };

                return result;
            }

            var target = text[text.IndexOf(assingment) - 1] + assingment;
            var replacement = text[text.IndexOf(assingment) - 1] + " " + assingment;

            text = text.Replace(target, replacement);

            var left = text.Substring(0, text.IndexOf(assingment)).Trim();
            var right = text.Substring(text.IndexOf(assingment) + assingment.Length).Trim();

            return new List<string> { left, assingment, right };
        }

        private List<string> TokenizeVarAndConsts(string text)
        {
            List<string> result = new List<string>();
            var indexOfEqual = text.IndexOf("=");


            if (indexOfEqual == -1) return text.Split(" ").ToList();

            var left = text.Substring(0, indexOfEqual).Trim().Split(" ");
            var right = text.Substring(indexOfEqual + 1).Trim();


            result.AddRange(left);
            result.Add(Keywords.EQUAL_ASSIGNMENT);
            result.Add(right);



            return result;
        }


        private List<string> TokenizeTypes(ref List<string> tokens)
        {
            var result = new List<string>();
            var finalToken = "";
            var countered = false;

            for (int i = 0; i < tokens.Count; i++)
            {
                var currentToken = tokens[i];
                if (currentToken.StartsWith("{"))
                {
                    countered = true;
                }
                if (!countered)
                {
                    result.Add(currentToken);
                }
                else
                {
                    finalToken += currentToken + " ";
                }


            }

            result.Add(finalToken.Trim());

            return result;
        }




        private List<string> TokenizeBlocks(string text)
        {
            text = text.Replace("{", " {");



            var ignoreIndexes = new IgnoringRanges();

            ignoreIndexes.AddIndexes(text);

            var indexOfPranthesis = text.IndexOf("(");
            var prevChar = indexOfPranthesis - 1;

            if (indexOfPranthesis > -1 && text[prevChar].ToString() != " ")
            {
                var targetItem = "" + text[prevChar] + text[indexOfPranthesis];
                var replacingItem = text[prevChar] + " " + text[indexOfPranthesis];

                text = text.Replace(targetItem, replacingItem);

            }


            var result = new List<string>();
            var str = "";

            var isPranthesis = false;
            var isBrackeyes = false;

            var parCount = 0;
            var brackCount = 0;

            for (int i = 0; i < text.Length; i++)
            {
                var currentToken = text[i];
                if (currentToken == '{')
                {
                    isBrackeyes = true;
                    brackCount++;
                }
                if (currentToken == '(')
                {
                    isPranthesis = true;
                    parCount++;
                }

                if (currentToken == ' ')
                {

                    if (!isPranthesis && !isBrackeyes)
                    {
                        if (str.Length > 0)
                        {
                            result.Add(str.Trim());
                            str = "";
                        }
                    }
                    else
                    {
                        str += currentToken;
                    }

                }
                else
                {
                    str += currentToken;
                }

                if (currentToken == ')')
                {
                    parCount--;
                    isPranthesis = parCount != 0;
                    if (!isPranthesis && !isBrackeyes)
                    {
                        result.Add(str.Trim());
                        str = "";
                    }
                }

                if (currentToken == '}')
                {
                    brackCount--;
                    isBrackeyes = brackCount != 0;

                    if (!isBrackeyes && !isPranthesis)
                    {
                        result.Add(str.Trim());
                        str = "";
                    }
                }

            }

            if (str.Trim().Length > 0) { result.Add(str.Trim()); }

            // handling in line if or else statemnts
            if (Keywords.IsControlFlow(text) && !text.Contains("{"))
            {
                var rest = "{ #body }";
                var body = "";
                var skip = text.StartsWith(Keywords.ELSE_IF_KEYWORD) ? 3 :
                    text.StartsWith(Keywords.ELSE_KEYWORD) ? 1 : 2;

                result.Skip(skip).ToList().ForEach(x =>
                {
                    body += x + " ";
                });
                body += ";";

                rest = rest.Replace("#body", body);

                result.RemoveRange(skip, result.Count - skip);
                result.Add(rest);
            }


            return result;
        }


    }

    public class IgnoringRanges
    {
        private List<int> Ranges = new List<int>();


        public void AddIndexes(string text)
        {
            List<char> items = new List<char>() { '\'', '\"' };

            for (int i = 0; i < text.Length; i++)
            {
                var _char = text[i];

                if (items.Contains(_char))
                {
                    Ranges.Add(i);
                }
            }

            if (Ranges.Count % 2 != 0)
            {
                throw new Exception("invalid use \' or \" ");
            }
        }

        public bool IsIgnoringIndex(int index)
        {
            if (Ranges.Count == 0) return false;
            int i = 0;
            int j = 1;
            while (i < Ranges.Count && j < Ranges.Count)
            {

                if (index >= Ranges[i] && index <= Ranges[j])
                {
                    return true;
                }

                i += 2;
                j += 2;
            }

            return false;
        }
    }
}
