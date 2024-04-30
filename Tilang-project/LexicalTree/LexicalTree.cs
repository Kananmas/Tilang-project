using Tilang_project.ExpressionEvaluator;
using Tilang_project.Properties;
using Tilang_project.Tilang_TypeSystem;
using Tilang_project.Tailang_Scope;

namespace Tilang_project.LexicalTree
{
    public class LexicalTree
    {
        private Scope currentScope;
        public Scope GenerateScope(List<List<string>> tokens, string scopeName = "main")
        {
            var result = new Scope();
            result.ScopeName = scopeName;

            currentScope = result;

            foreach (var token in tokens)
            {
                var firstItem = token[0];

                switch (firstItem)
                {
                    case "function":
                        var functionScope = CreateFunctionScope(token);
                        functionScope.ParentScope = result;
                        result.ChildScopes.Add(functionScope.ScopeName, functionScope);
                        currentScope = result;
                        break;
                    case "var":
                    case "const":
                        var propName = token[2];
                        var prop = GenerateProperty(token);
                        prop.PropType = token[0] == "var" ? "Variable" : "Constant";
                        result[propName] = prop;
                        currentScope = result;
                        break;
                    case "type":
                        TypeSystem.CreateCustomType(token[2], token[1], currentScope);
                        break;
                    default:
                        ExpressionEval expressionEval = new ExpressionEval();
                        if(currentScope.ScopeName == "main")
                        {
                            var line = new List<List<string>>();
                            line.Add(token);
                            expressionEval.ReadTheCode(line, currentScope);
                            currentScope = result;
                        }
                        break;

                }
            }


            return result;
        }

        public Scope CreateFunctionScope(List<string> tokens)
        {
            var parser = new Parser.Parser();
            var body = (string)tokens[3];
            var bodyToken = parser.LineByKeywords(parser.LineSeparator(body.Substring(1 , body.Length-2).Trim()));
            var scope = GenerateScope(bodyToken, tokens[1]);
            scope.Body =bodyToken;
            ResolveFunctionArguments(tokens[2]).ForEach((arg) =>
            {
                arg.PropType = "Argument";
                scope.Members[arg.Name] = arg;
            });

            return scope;
        }

        public List<Property> ResolveFunctionArguments(string argsString)
        {
            var content = argsString.Substring(1, argsString.Length - 2);
            var result = new List<Property>();

            var args = content.Split(',').Select((item) => item.Trim());

            foreach (var arg in args)
            {
                if (arg != "")
                {
                    var argDef = arg.Split(":");
                    var argName = argDef[0];
                    string argType = "";
                    dynamic argValue;
                    if (argDef[1].Split("=").Length > 1)
                    {
                        var splitTwo = argDef[1].Split("=");
                        argType = splitTwo[0].Trim();
                        argValue = TypeSystem.ConfigureValueByType(argType  , splitTwo[1].Trim() , currentScope);
                    }
                    else
                    {
                        argType = argDef[1];
                        argValue = TypeSystem.GenerateDefaultValueByType(argType);
                    }


                    result.Add(new Property
                    {
                        Name = argName,
                        Value = argValue,
                        Model = TypeSystem.ConfigureType(argType)
                    });
                }
            }

            return result;
        }

        public Property GenerateProperty(List<string> tokens)
        {
            var result = new Property();
            var exprHandler = new ExpressionEval();

            result.Name = tokens[2];
            result.Model = TypeSystem.ConfigureType(tokens[1]);

            if (tokens.Contains("=") && tokens.Count >= 5)
            {
                if (TypeSystem.IsCustomType(tokens[1]))
                {
                    result.Value = TypeSystem.ConfigureValueByType(tokens[1], tokens[4] , currentScope);
                    return result;
                }

                result.Value = TypeSystem.ConfigureValueByType(tokens[1], 
                    exprHandler.ReadExpression(tokens[4] , currentScope).ToString() , currentScope);
            }
            else
            {
                result.Value = TypeSystem.GenerateDefaultValueByType(tokens[1]);
            }

            return result;

        }
       
    }
}
