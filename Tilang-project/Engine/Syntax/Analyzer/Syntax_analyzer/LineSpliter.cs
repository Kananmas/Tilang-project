using Tilang_project.Engine.Tilang_Keywords;

namespace Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer
{
    public partial class SyntaxAnalyzer
    {
        private List<string> SplitLines(string text)
        {
            var lines = new List<string>();
            var ignoringIndex = new IgnoringRanges();
            text = FormatLines(text);
            ignoringIndex.AddIndexes(text);

            bool isInBrackeys = false;
            bool isInPranthesis = false;

            string currentValue = "";

            int pranthisisCount = 0;
            int brackeyesCount = 0;

            var AddPranthesis = () =>
            {
                if (!isInPranthesis) { isInPranthesis = true; }
                pranthisisCount++;
            };

            var AddBrackeyes = () =>
            {
                if (!isInBrackeys) { isInBrackeys = true; }
                brackeyesCount++;
            };

            for (int i = 0; i < text.Length; i++)
            {
                var currentChar = text[i];

                if (currentChar == '(' && !ignoringIndex.IsIgnoringIndex(i))
                {
                    AddPranthesis();
                }
                if (currentChar == '{' && !ignoringIndex.IsIgnoringIndex(i))
                {
                    AddBrackeyes();
                }

                if (currentChar != ';')
                {
                    currentValue += currentChar;
                }

                else
                {
                    if (ignoringIndex.IsIgnoringIndex(i)) continue;
                    if (!isInBrackeys && !isInPranthesis)
                    {
                        lines.Add(currentValue.Trim());
                        currentValue = string.Empty;
                    }
                    else
                    {
                        currentValue += currentChar;
                    }
                }

                if (currentChar == ')' && !ignoringIndex.IsIgnoringIndex(i))
                {
                    pranthisisCount--;
                    if (pranthisisCount == 0) { isInPranthesis = false; }
                }

                if (currentChar == '}' && !ignoringIndex.IsIgnoringIndex(i))
                {
                    brackeyesCount--;
                    if (brackeyesCount == 0)
                    {
                        isInBrackeys = false;
                        if (Keywords.IsBlocked(currentValue.Trim()))
                        {
                            lines.Add(currentValue.Trim());
                            currentValue = string.Empty;
                        }
                    }
                }

            }

            if (currentValue.Trim() != string.Empty) lines.Add(currentValue.Trim());

            return lines;
        }
    }
}
