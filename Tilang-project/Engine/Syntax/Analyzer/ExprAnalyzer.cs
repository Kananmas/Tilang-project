using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tilang_project.Engine.Structs;

namespace Tilang_project.Engine.Syntax.Analyzer
{
    public class ExprAnalyzer
    {
        public void ReadExpression(List<string> tokens)
        {
            if(tokens.Count == 0) return;
            if(tokens.Count == 1)
            {
               var res = ParseMathExpression(tokens[0]);
                return;
            }

            var lefSide = ParseMathExpression(tokens[2]);
        }
        

        private List<string> ParseMathExpression(string code)
        {
         

        }


        private TilangType ExecuteCode(string code)
        {
            return new TilangType();
        }
    }
}
