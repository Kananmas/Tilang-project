namespace Tilang_project.Engine.Syntax.Analyzer
{
    public class SyntaxAnalyzer
    {
        public List<List<string>> GenerateTokens(string text)
        {
            return SplitLines(text).Select((line) => TokenCreator(line)).Select((item) => item = item.Where(item => item != "").ToList()).ToList();
        }
        private string FormatLines(string text)
        {
            var result = text;

            result = result.Replace("<", " <").Replace("if" , "if ").Replace("switch" , "switch ").Replace("while" , "while ").Replace("for" , "for ")
                .Replace("+=", " += ").Replace("-=", " -= ").Replace("*=", " *= ").Replace("/=", " /= ");

            return result;
        }


        public List<string> SplitLines(string text)
        {
            text = FormatLines(text);
            var lines = new List<string>();
            var ignoringIndex = new IgnoringRanges();

            ignoringIndex.AddIndexes(text, new List<char>() { '\"', '\'' });

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

            var shouldAdd = (string line) =>
            {
                line = line.Trim();

                return line.StartsWith("type") || line.StartsWith("for") || line.StartsWith("if") || line.StartsWith("else") || line.StartsWith("else if") ||
                line.StartsWith("switch") || line.StartsWith("function") || line.StartsWith("while");
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
                    if (!isInBrackeys && !isInPranthesis )
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
                        if (shouldAdd(currentValue.Trim()))
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

        public static bool IsIndexer(string str)
        {
            if (!str.Contains("[") || !str.Contains("]") || str.Length <= 1) return false;
            str = str.Replace(" ", "");
            str = str.Replace("[", " [");
            var split = str.Split(' ');

            return split[1][0] == '[' && split[1][split[1].Length - 1] == ']';
        }

        public static bool IsFunctionCall(string str)
        {

            if (!str.Contains("(") || !str.Contains(")") || str.Length <= 1) return false;
            if (str[0] == '(') return false;
            str = str.Replace(" ", "");
            var fnName = str.Substring(0, str.IndexOf("(")).Trim();
            var fnArgs = str.Substring(str.IndexOf("(")).Trim();
            if (fnName == "") return false;
            return fnArgs[0] == '(' && fnArgs[fnArgs.Length - 1] == ')';
        }

        public List<string> SeperateFunctionArgs(string str)
        {
            var result = new List<string>();
            var ignoringIndex = new IgnoringRanges();
            ignoringIndex.AddIndexes(str, new List<char>() { '\"', '\'' });
            var parnthesisCount = 0;
            var currentStr = string.Empty;

            for (int i = 0; i < str.Length; i++)
            {
                var character = str[i];
                if (character == '(' || character == '{' && !ignoringIndex.IsIgnoringIndex(i))
                {
                    parnthesisCount++;
                }
                if (character == ',' && !ignoringIndex.IsIgnoringIndex(i))
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

                if ((character == ')' || character == '}') && !ignoringIndex.IsIgnoringIndex(i))
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

        private List<string> TokenCreator(string text)
        {
            var tokens = text.Split(" ").Where((item) => item != "").ToList();
            var assignments = "+= -= = /= *=";

            if (tokens.Count == 0) return new List<string>();
            switch (tokens[0])
            {
                case "":
                    return new List<string>();
                default:
                   
                    var assingmentsTypes = assignments.Split(" ").ToList();

                    if (SyntaxAnalyzer.IsFunctionCall(text) && !assingmentsTypes.Any((item) => text.Contains(item)))
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
                        var result = new List<string>();
                        result.Add(text);

                        return result;
                    }

                    text = text.Replace(assingment, $" {assingment} ");

                    var left = text.Substring(0, text.IndexOf(assingment)).Trim();
                    var right = text.Substring(text.IndexOf(assingment) + assingment.Length).Trim();

                    return new List<string> { left, assingment, right };
                case "const":
                case "var":
                    {
                        List<string> result = new List<string>();
                        var encounterted = false;
                        var finalString = "";
                        tokens = text.Replace("=", " = ").Split(" ").Where(item => item != "").ToList();


                        foreach (var token in tokens)
                        {

                            if (!encounterted)
                            {
                                result.Add(token);
                            }
                            if (assignments.Contains(token))
                            {
                                if (!encounterted) encounterted = true;
                                else
                                {
                                    finalString += token;
                                }
                                continue;
                            }
                            if (encounterted)
                            {
                                finalString += token;
                            }
                        }

                        result.Add(finalString.Trim());


                        return result;
                    }
                case "for":
                case "while":
                case "switch":
                case "if":
                case "else":
                case "else if":
                case "function":
                    {
                        

                        text = text.Replace("{", " {");

                        var indexOfPranthesis = text.IndexOf("(");
                        var prevChar = indexOfPranthesis-1;

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

                        if(str.Trim().Length > 0) { result.Add(str.Trim()); }

                       

                        return result;
                    }
                case "type":
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
            }
        }


    }

    public class IgnoringRanges
    {
        private List<int> Ranges = new List<int>();


        public void AddIndexes(string text, List<char> items)
        {
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
            int i = 0;
            int j = 1;
            while(i < Ranges.Count && j < Ranges.Count)
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
