﻿namespace Tilang_project.Engine.Syntax.Analyzer
{
    public class SyntaxAnalyzer
    {
        public List<List<string>> GenerateTokens(string text)
        {
            return SplitLines(text).Select((line) => TokenCreator(line)).ToList();
        }
        private string FormatLines(string text)
        {
            var result = text;

            result.Replace("<", " <");
            result.Replace("(", " { ");
            result.Replace("+=", " += ");
            result.Replace("-=", " -= ");
            result.Replace("*=", " *= ");
            result.Replace("/=", " /= ");




            return result;
        }

        private List<string> SplitLines(string text)
        {
            text = FormatLines(text);
            var lines = new List<string>();

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
                line.StartsWith("swtich") || line.StartsWith("function") || line.StartsWith("while");
            };

            for (int i = 0; i < text.Length; i++)
            {
                var currentChar = text[i];

                if (currentChar == '(')
                {
                    AddPranthesis();
                }
                if (currentChar == '{')
                {
                    AddBrackeyes();
                }

                if (currentChar != ';')
                {
                    currentValue += currentChar;
                }

                else
                {
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

                if (currentChar == ')')
                {
                    pranthisisCount--;
                    if (pranthisisCount == 0) { isInPranthesis = false; }
                }

                if (currentChar == '}')
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

            return lines;
        }


        private List<string> TokenCreator(string text)
        {
            var tokens = text.Split(" ").ToList();
            var assignments = "+= -= = /= *=";
            switch (tokens[0])
            {
                default:
                case "const":
                case "var":
                    {

                        List<string> result = new List<string>();
                        var encounterted = false;
                        var finalString = "";

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
                case "if":
                case "else":
                case "else if":
                case "function":
                    {
                        text = text.Replace("{", " {").Replace("(", " (");

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
}