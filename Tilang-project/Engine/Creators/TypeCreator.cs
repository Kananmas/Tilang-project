using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Creators
{
    public static class TypeCreator
    {
        public static void CreateDataStructrue(List<string> tokens , Processor pros)
        {
            var typeName = tokens[1];
            var implentation = tokens[2];

            var result = new TilangStructs();
            result.TypeName = typeName;

            if (TypeSystem.CustomTypes.ContainsKey(typeName))
            {
                throw new Exception($"{typeName} is already defined");
            }

            implentation.Substring(1, implentation.Length - 2).Split(";").ToList().ForEach((item) =>
            {
                item = item.Trim();
                if (item.Length == 0 || item.Length == 1) { return;  }
                var toks = item.Split(' ').ToList();



                if (toks[0] == "function")
                {
                    item += "}";
                    toks = new SyntaxAnalyzer().GenerateTokens(item)[0];
                    var fn = FunctionCreator.CreateFunction(toks);
                    result.Functions.Add(fn);
                    return;
                }
                if (toks[0] != "const" || toks[1] != "var")
                {
                    var newToks = new List<string>();

                    newToks.Add("var");
                    newToks.AddRange(toks);

                    toks = newToks;
                }


                var res = VariableCreator.CreateVariable(toks , pros);
                result.Properties.Add(res.VariableName, res);
            });


            TypeSystem.CustomTypes.Add(typeName, result);
        }
    }
}
