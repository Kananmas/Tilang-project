
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_TypeSystem;

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
        

        private List<string> ParseMathExpression(string str)
        {
            string ops = "+-/*";
            var result = new List<string>();
            var val = "";
            var op = "";
            bool inPranthesis = false;
            int prCount = 0;
            for (int i = 0; i < str.Length; i++)
            {
                var current = str[i];
                if (current != ' ')
                {
                    if (current == '(')
                    {
                        if (!inPranthesis) inPranthesis = true;
                        prCount++;
                    }
                    if (ops.IndexOf(current) != -1)
                    {
                        if (!inPranthesis)
                        {
                            if (val.Length > 0)
                            {
                                result.Add(val);
                            }
                            op += current;
                            result.Add(op);

                            val = "";
                            op = "";
                        }
                        else
                        {
                            val += current;
                        }
                    }
                    else
                    {
                        val += current;
                    }
                    if (current == ')')
                    {
                        prCount--;
                        if (prCount == 0)
                        {
                            result.Add(val);
                            val = "";
                            inPranthesis = false;
                        }
                    }

                }
            }
            if (val.Length > 0)
            {
                result.Add(val);
            }


            result.ForEach(val =>
            {
                if (TypeSystem.IsTypeCreation(val)) throw new Exception("cannot use operator with custom type");
            });

            return result;

        }


        private TilangType ExecuteCode(string code)
        {
            return new TilangType();
        }
    }
}
