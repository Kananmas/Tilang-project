using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Services.Creators
{
    public static class TypeCreator
    {
        public static void CreateDataStructrue(List<string> tokens, Processor pros)
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
                if (item.Length == 0 || item.Length == 1) { return; }
                var toks = item.Split(' ').ToList();



                if (toks[0] == Keywords.FUNCTION_KEYWORD)
                {
                    item += "}";
                    toks = new SyntaxAnalyzer().GenerateTokens(item)[0];
                    var fn = FunctionCreator.CreateFunction(toks, pros);
                    result.Functions.Add(fn);
                    return;
                }
                if (toks[0] != Keywords.CONST_KEYWORD || toks[1] != Keywords.VAR_KEYWORD)
                {
                    var newToks = new List<string>();

                    newToks.Add(Keywords.VAR_KEYWORD);
                    newToks.AddRange(toks);

                    toks = newToks;
                }


                var res = VariableCreator.CreateVariable(toks, pros);
                result.Properties.Add(res.VariableName, res);
            });


            TypeSystem.CustomTypes.Add(typeName, result);
        }
    }
}
