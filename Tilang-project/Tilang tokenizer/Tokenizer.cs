using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Tilang_project.Tilang_tokenizer
{
    public static class Tokenizer
    {

        public static List<string> Tokenization(string sourceCode)
        {

        }



        private static string[] lineSeperator(string sourceCode)
        {
            sourceCode = codeReformer(sourceCode);
            var result = new List<string>();
            var currentVal = "";

            var isInPrantesis = false;
            var prCount = 0;
            var isInCurvyBrackeys = false;
            var brackeysCount = 0;

            for (int i = 0; i < sourceCode.Length; i++)
            {
                var currentChar = sourceCode[i];

                if (currentChar == '(')
                {
                    if (!isInPrantesis) isInPrantesis = true;
                    prCount++;
                }
                if (currentChar == '{')
                {
                    if (!isInCurvyBrackeys) isInCurvyBrackeys = true;
                    brackeysCount++;
                }
                if (currentChar != ';')
                {
                    currentVal += currentChar;
                }

                else
                {
                    if (!isInPrantesis && !isInCurvyBrackeys)
                    {
                        if (currentVal.Length > 0) result.Add(currentVal.Trim());
                        currentVal = "";
                    }
                    else
                    {
                        currentVal += currentChar;
                    }
                }



                if (currentChar == ')')
                {
                    prCount--;
                    if (prCount == 0)
                    {
                        isInPrantesis = false;
                    }
                }
                if (currentChar == '}')
                {
                    brackeysCount--;
                    if (brackeysCount == 0)
                    {
                        isInCurvyBrackeys = false;
                    }
                }
            }
            if (currentVal.Length > 0)
            {
                result.Add(currentVal);
            }


            return result.ToArray();
        }

        private static string codeReformer(string sourceCode)
        {
            return sourceCode.Replace("<" , " <").Replace("=" , " = ").Replace("{" , " {").Replace("}"," };");
        }
    }
}
