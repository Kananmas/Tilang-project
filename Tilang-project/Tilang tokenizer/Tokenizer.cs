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
            var spaceIndex = sourceCode.IndexOf(' ');
            if (spaceIndex <= -1) return new List<string> { sourceCode };
            var statement = sourceCode.Substring(0, spaceIndex);
            var result = new List<string>();

            result.Add(statement);

            switch (statement)
            {
                case "function":
                    var fnName = sourceCode.Substring(sourceCode.IndexOf(" "), sourceCode.IndexOf("(")
                        - sourceCode.IndexOf(" ")).Trim();
                    result.Add(fnName);
                    var fnArgs = sourceCode.Substring(sourceCode.IndexOf("("), sourceCode.IndexOf(")")
                        - sourceCode.IndexOf("(") + 1);
                    result.Add(fnArgs);
                    var fnBody = sourceCode.Substring(sourceCode.IndexOf("{"));
                    result.Add(fnBody);

                    if (sourceCode.IndexOf("<") == -1) break;

                    var fnReturnType = sourceCode.Substring(sourceCode.IndexOf("<") + 1,
                        sourceCode.IndexOf(">") - sourceCode.IndexOf("<") - 1);
                    result.Add(fnReturnType);
                    break;
                case "var":
                case "const":
                    result = Reform(sourceCode.Split(" ").ToList());
                    break;
                case "type":
                    result = ConfigureCustomType(sourceCode);
                    break;
                default:
                    result.Clear();
                    result.Add(sourceCode);
                    break;
            }
            return result;
        }



        private static List<string> Reform(List<string> subTokens)
        {
            var result = new List<string>();
            var indexOfEqual = subTokens.IndexOf("=");
            string value = "";
            if (indexOfEqual != -1)
            {
                for(int i = 0 ; i < subTokens.Count; i++)
                {
                    if(i < indexOfEqual)
                    {
                        result.Add(subTokens[i]);
                    }
                    else
                    {
                        value += subTokens[i];
                    }
                }

                if(value.Length > 0)
                {
                    result.Add(value);
                }
            }

            
            return subTokens;
        }



        private static string[] sourceCodeSeperator(string sourceCode)
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
