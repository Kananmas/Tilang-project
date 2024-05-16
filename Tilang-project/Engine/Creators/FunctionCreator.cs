using Tilang_project.Engine.Structs;

namespace Tilang_project.Engine.Creators
{
    public static class FunctionCreator
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
            result.FuncDefinition = CreateFunctionDef(result);


            return result;
        }


        public static string CreateFunctionDef(TilangFunction fn)
        {
            var format = $"Func[{fn.FunctionName}](#args)";
            var argStr = "";

            for(var i = 0;  i < fn.FunctionArguments.Count; i++ )
            {
                var argType = fn.FunctionArguments[i].TypeName;
                argStr += argType + ",";
            }

            if(argStr.Length > 0) argStr = argStr.Substring(0 ,argStr.Length - 1);
            format = format.Replace("#args", argStr);

            return format;
        }

        public static string CreateFunctionDef(string fnName , List<TilangVariable> args)
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
