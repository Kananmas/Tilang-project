using Tilang_project.LexicalTree;

namespace Tilang_project.Parser
{
    public class Parser
    {
        public List<string> LineSeparator(string line)
        {
            line = line.Replace("=", " = ");
            line = line.Replace("<", " <");
            line = line.Replace(">", "> ");
            line = line.Replace("}", "};");

            var result = new List<string>();
            var currentVal = "";

            var isInPrantesis = false;
            var prCount = 0;
            var isInCurvyBrackeys = false;
            var brackeysCount = 0;

            for(int i = 0; i < line.Length;i++)
            {
                var currentChar = line[i];

                if(currentChar == '(')
                {
                   if(!isInPrantesis)  isInPrantesis = true;
                   prCount++;
                }
                if(currentChar == '{') {
                    if(!isInCurvyBrackeys) isInCurvyBrackeys = true;
                    brackeysCount++;
                }
                if (currentChar != ';')
                {
                    currentVal += currentChar;
                }

                else
                {
                    if(!isInPrantesis && !isInCurvyBrackeys)
                    {
                        if(currentVal.Length > 0) result.Add(currentVal.Trim());
                        currentVal = "";
                    }
                    else
                    {
                        currentVal += currentChar;
                    }
                }



                if(currentChar == ')')
                {
                    prCount--;
                    if(prCount == 0)
                    {
                        isInPrantesis = false;
                    }
                }
                if(currentChar == '}')
                {
                    brackeysCount--;
                    if(brackeysCount == 0)
                    {
                        isInCurvyBrackeys = false;
                    }
                }
            }
            if(currentVal.Length > 0)
            {
                result.Add(currentVal);
            }
           

            return result;
        }


        public List<List<string>> LineByKeywords(List<string> lines)
        {
            var result = new List<List<string>>();
            foreach (var line in lines)
            {
                result.Add(ConfigureTokens(line));
            }

            return result;
        }

        private List<string> ConfigureTokens(string line)
        {
            var spaceIndex = line.IndexOf(' ');
            if (spaceIndex <= -1) return new List<string> { line };
            var statement = line.Substring(0 , spaceIndex);
            var result = new List<string>();

            result.Add(statement);

            switch(statement)
            {
                case "function":
                    var fnName = line.Substring(line.IndexOf(" "), line.IndexOf("(") 
                        - line.IndexOf(" ")).Trim();
                    result.Add(fnName);
                    var fnArgs =  line.Substring(line.IndexOf("(") , line.IndexOf(")")
                        - line.IndexOf("(")+ 1);
                    result.Add(fnArgs);
                    var fnBody = line.Substring(line.IndexOf("{"));
                    result.Add(fnBody);

                    if (line.IndexOf("<") == -1) break;

                    var fnReturnType = line.Substring(line.IndexOf("<") + 1, 
                        line.IndexOf(">") - line.IndexOf("<")-1);
                    result.Add(fnReturnType);
                    break;
                case "var":
                case "const":
                    result = ReformLine(line.Split(" ").ToList());
                    break;
                case "type":
                    result = ConfigureCustomType(line);
                    break;
                default:
                    result.Clear();
                    result.Add(line);
                    break;
            }
            return result;

        }

        private List<string> ReformLine(List<string> strings)
        {
            var result = new List<string>();
            var val = "";
            bool countered = false;

            foreach(var str in strings)
            {
                if(!countered)
                {
                   if(str != "")  result.Add(str);
                }
                else
                {
                    val += str;
                }
                if(str == "=")
                {
                    countered = true;
                }

            }
            result.Add(val);
            return result;
        }


        private List<string> ConfigureCustomType(string str)
        {
            var primaryTokens = str.Split(" ").ToList();
            var result = new List<string>();
            var val = "";

            result.Add(primaryTokens[0]);
            result.Add(primaryTokens[1]);

            for(int i = 2; i < primaryTokens.Count; i++)
            {
                val += primaryTokens[i] + (primaryTokens.Count - 1 == i ? "" : " ");
            }
               
            if (val != "") result.Add(val);
            return result;
        }

    }
}