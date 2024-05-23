using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Syntax.Analyzer
{
    public partial class ExprAnalyzer
    {
        private readonly string[] ops = ["+", "!", "-", "?", "*", "%", "/", "<", ">", "+=", "-=", "/=", "*=", "==", "!=", "<=", ">=", "||", "&&"];
        internal string reformText(string text)
        {
            List<string> spaceOps = [
                   "+=",
                    "-=",
                    "/=",
                    "*=",
                    "==",
                    "!=",
                    "<=",
                    ">=",
                    "||",
                    "&&"];
            foreach (var op in spaceOps)
            {
                text = text.Replace(op, $" {op} ");
            }

            return text;
        }
        private List<string> TokenizeExpression(string text)
        {
            var result = new List<string>();
            var ignoreRanges = new IgnoringRanges();
            ignoreRanges.AddIndexes(text);

            text = reformText(text);

            var stringCache = "";

            var pranCount = 0;
            var brackCount = 0;
            var prevIndex = 0;
            var curvyBCount = 0;

            var addToResult = (string op, int index) =>
            {
                var len = index - prevIndex;
                var str = text.Substring(prevIndex, len).Trim();
                if (len > 0 && str.Length > 0)
                {
                    result.Add(str);
                    stringCache = "";
                }
                result.Add(op);

            };


            var checkChar = (char ch) =>
            {
                switch (ch)
                {
                    case '(':
                        pranCount++; return;
                    case ')':
                        pranCount--; return;
                    case '[':
                        brackCount++; return;
                    case ']':
                        brackCount--; return;
                    case '{':
                        curvyBCount++; return;
                    case '}':
                        curvyBCount--; return;
                }
            };

            var shouldAdd = () =>
            {
                return pranCount == 0 && brackCount == 0 && curvyBCount == 0;
            };


            for (int i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];
                string doubleChar = i > 0 && i < text.Length - 1 ? "" + text[i] + text[i + 1] : "";

                if (!ignoreRanges.IsIgnoringIndex(i))
                {
                    checkChar(currentChar);

                    if (doubleChar.Trim().Length > 1 && ops.Contains(doubleChar.Trim()) && shouldAdd())
                    {
                        addToResult(doubleChar, i);
                        prevIndex = i + 2;
                        i++;
                        continue;
                    }
                    if (ops.Contains(currentChar.ToString()) && shouldAdd())
                    {
                        addToResult(currentChar.ToString(), i);
                        prevIndex = i + 1;
                        continue;
                    }

                }

                stringCache += currentChar;
            }

            if (stringCache.Trim().Length > 0) result.Add(stringCache.Trim());
            var finalResult = new List<string>(result);


            if (result.Count % 2 == 0)
            {
                int current = 0;
                foreach (var item in result)
                {
                    if (current > 0 && current < result.Count - 1)
                    {
                        var firstItem = result[current];
                        var secondItem = result[current + 1];

                        var combine = firstItem + secondItem;

                        if (TypeSystem.IsRawValue(combine))
                        {
                            finalResult.RemoveAt(current + 1);
                            finalResult[current] = combine;
                        }
                    }

                    current++;
                }
            }


            return finalResult;

        }
    }
}