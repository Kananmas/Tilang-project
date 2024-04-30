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
        private static string[] lineSeperator(string sourceCode)
        {
            var reformedString = codeReformer(sourceCode);
            var line = "";

            for(int i = 0; i < reformedString.Length; i++)
            {
                var character = reformedString[i];
                
                if(character == ';')
                {
                    
                }

            }
        }

        private static string codeReformer(string sourceCode)
        {
            return sourceCode.Replace("<" , " <").Replace("=" , " = ").Replace("{" , " {").Replace("}","};");
        }
    }
}
