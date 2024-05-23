using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tilang_project.Engine.Tilang_Keywords;

namespace Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer
{
    public partial class SyntaxAnalyzer
    {
        public void LineSeparator(string text)
        {
            var ignoringIndex = new IgnoringRanges();
            bool ended = false;
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
                        OnLineSplited.Invoke(currentValue.Trim());
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
                            OnLineSplited.Invoke(currentValue.Trim());
                            currentValue = string.Empty;
                        }
                    }
                }

            }

            if (currentValue.Trim() != string.Empty) OnLineSplited.Invoke(currentValue.Trim());


        }
    }
}
