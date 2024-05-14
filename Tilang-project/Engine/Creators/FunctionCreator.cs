﻿using Tilang_project.Engine.Structs;

namespace Tilang_project.Engine.Creators
{
    public class FunctionCreator
    {
        public static TilangFunction CreateFunction(List<string> Tokens)
        {
            var type = Tokens[3].Substring(1, Tokens[3].Length - 2).Trim();
            var functionName = Tokens[1];
            var body = Tokens[4];

            var result = new TilangFunction();
            var argDefs = Tokens[2].Substring(1, Tokens[2].Length - 2);

            if(argDefs.Length > 0)
            {
                argDefs.Split(",").ToList().ForEach((item) =>
                {
                    item = item.Trim();
                    var split = item.Split(":");

                    var name = split[0];
                    var type = split[1];
                    var Tag = "Var";

                    var toks = new List<string>() { Tag, type, name };

                    var tilangVar = VariableCreator.CreateVariable(toks);

                    result.FunctionArguments.Add(tilangVar);
                });
            }
           
            result.FunctionName = functionName;
            result.ReturnType = type;
            result.Body = body;


            return result;
        }
    }
}