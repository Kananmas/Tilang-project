using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_Keywords;

namespace Tilang_project.Engine.Services.Creators
{
    public static class FunctionCreator
    {
        public static TilangFunction CreateFunction(List<string> Tokens, Processor processor)
        {
            var type = Tokens[3].Substring(1, Tokens[3].Length - 2).Trim();
            var functionName = Tokens[1];
            var body = Tokens[4];

            var result = new TilangFunction();
            var argDefs = Tokens[2].Substring(1, Tokens[2].Length - 2);

            if (argDefs.Length > 0)
            {
                argDefs.Split(",").ToList().ForEach((item) =>
                {
                    item = item.Trim();
                    var split = item.Split(":");
                    var value = "";

                    if (split[1].Contains("="))
                    {
                        value = split[1].Split('=')[1].Trim();
                        split[1] = split[1].Substring(0, split[1].IndexOf("=")).Trim();
                    }

                    var name = split[0].Trim();
                    var type = split[1].Trim();
                    var Tag = Keywords.VAR_KEYWORD;

                    var toks = new List<string>() { Tag, type, name };

                    if (value != null && value != string.Empty) { toks.Add("="); toks.Add(value); }

                    var tilangVar = VariableCreator.CreateVariable(toks, processor);

                    result.FunctionArguments.Add(tilangVar);
                });
            }

            result.FunctionName = functionName;
            result.ReturnType = type;
            result.Body = body;
            result.FuncDefinition = CreateFunctionDef(result);


            return result;
        }


        public static string CreateFunctionDef(TilangFunction fn)
        {
            var format = $"Func[{fn.FunctionName}](#args)";
            var argStr = "";

            for (var i = 0; i < fn.FunctionArguments.Count; i++)
            {
                var argType = fn.FunctionArguments[i].TypeName;
                argStr += argType + ",";
            }

            if (argStr.Length > 0) argStr = argStr.Substring(0, argStr.Length - 1);
            format = format.Replace("#args", argStr);

            return format;
        }

        public static string CreateFunctionDef(string fnName, List<TilangVariable> args)
        {
            var format = $"Func[{fnName}](#args)";
            var argStr = "";

            for (var i = 0; i < args.Count; i++)
            {
                var argType = args[i].TypeName;
                argStr += argType + ",";
            }

            if (argStr.Length > 0) argStr = argStr.Substring(0, argStr.Length - 1);
            format = format.Replace("#args", argStr);

            return format;
        }
    }
}
