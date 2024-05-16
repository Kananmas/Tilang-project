using Tilang_project.Engine.Creators;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.Tilang_Console;

namespace Tilang_project.Engine.Processors
{
    public class Processor
    {
        public VariableStack Stack = new VariableStack();
        private BoolCache BoolCache = new BoolCache();

        public string ScopeName { get; set; } = "main";

        private SyntaxAnalyzer analyzer = new SyntaxAnalyzer();
        private ExprAnalyzer exprAnalyzer = new ExprAnalyzer();

        private bool IsSystemCall(string value)
        {
            if (value.StartsWith("Sys")) return true;
            return false;
        }

        private string TranslateForLoop(List<string> tokens)
        {
            var variablesStr = "";
            var conditionsStr = "";
            var loopVarChange = "";

            var bodyContent = tokens[2].Substring(1, tokens[2].Length - 2);

            var str = "#variables while(#conditions)\n {\n  #operation  #loop_var_change \n} ";

            var items = tokens[1].Substring(1, tokens[1].Length - 2).Split(";").ToList();

            if (items.Count % 3 != 0)
            {
                throw new Exception("invalid for loop creation");
            }
            var itemStep = items.Count / 3;

            for (int i = 0; i < itemStep; i++)
            {
                var currentItem = items[i];
                var res = "var " + currentItem.Replace("=", " = ") + ";\n";

                variablesStr += res;
            }

            for (int i = itemStep; i < itemStep * 2; i++)
            {
                var currentItem = items[i];
                var res = currentItem + "&&";

                conditionsStr += res;

            }

            conditionsStr = conditionsStr.Substring(0, conditionsStr.LastIndexOf("&&")).Trim();

            for (int i = itemStep * 2; i < itemStep * 3; i++)
            {
                loopVarChange += items[i] + ";\n";
            }

            str = str.Replace("#variables", variablesStr)
                .Replace("#conditions", conditionsStr).Replace("#loop_var_change", loopVarChange)
                .Replace("#operation", bodyContent);

            return str;
        }

        public static TilangVariable? HandleSysCall(string val, List<TilangVariable> varialables)
        {
            var tokens = val.Split('.');
            if (tokens[1] == "out")
            {
                switch (tokens[2])
                {
                    case "println":
                        Tilang_System.PrintLn(varialables);
                        return null;
                    case "print":
                        Tilang_System.Print(varialables);
                        return null;
                }
            }
            if (tokens[1] == "in")
            {
                switch (tokens[2])
                {
                    case "getKey":
                        return Tilang_System.GetKey();
                    case "getLine":
                        return Tilang_System.GetLine();
                }
            }
            return null;
        }

        private TilangVariable? FunctionProcess(TilangFunction fn, List<TilangVariable> argValues)
        {
            var newStack = new VariableStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
            var i = 0;
            var list = new List<int>();
            fn.FunctionArguments.ForEach(x =>
            {
                x.Assign(argValues[i++], "=");
                var assign = newStack.SetInStack(x);

                list.Add(assign);
            });

            var newProcess = new Processor();
            newProcess.Stack = newStack;
            newProcess.ScopeName = fn.FunctionName;
            var res = newProcess.Process(analyzer.GenerateTokens(fn.Body.Substring(1, fn.Body.Length - 2).Trim()));

            newProcess.Stack.ClearStackByIndexes(list);
            return res;
        }

        private TilangVariable? LoopProcess(List<string> tokens)
        {
            var condition = tokens[1].Substring(1, tokens[1].Length - 2).Trim();
            var processBody = tokens[2].Substring(1, tokens[2].Length - 2).Trim();

            var process = new Processor();
            process.Stack = new VariableStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
            process.ScopeName = "loop";

            TilangVariable var = null;
            var conditionRes = exprAnalyzer.ReadExpression(condition, process).Value;


            while (conditionRes)
            {

                var res = process.Process(analyzer.GenerateTokens(processBody));
                if (res != null)
                {
                    return res;
                }
                var = res;
                conditionRes = exprAnalyzer.ReadExpression(condition, process).Value;
            }

            return var;
        }

        private TilangVariable? ConditionalProcess(List<string> tokens)
        {
            var newStack = new VariableStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
            var newProcess = new Processor();
            newProcess.Stack = newStack;
            newProcess.ScopeName = this.ScopeName == "loop" ? "loop" : "if";

            if (tokens[0] == "if")
            {
                this.BoolCache.Clear();
            }

            if (tokens[0] == "else" && tokens[1] != "if")
            {
                var body = tokens[1].Substring(1, tokens[1].Length - 2).Trim();

                var res = newProcess.Process(analyzer.GenerateTokens(body));

                if (res != null)
                {
                    this.BoolCache.Clear();
                    return res;
                }

                this.BoolCache.Clear();
                return null;
            }

            var condition = tokens[1].Substring(1, tokens[1].Length - 2).Trim();
            var processBody = tokens[2].Substring(1, tokens[2].Length - 2).Trim();
            var conditionStatus = exprAnalyzer.ReadExpression(condition, newProcess);


            newProcess.Stack = newStack;

            if (conditionStatus.Value == true && !this.BoolCache.GetLatest())
            {
                var res = newProcess.Process(analyzer.GenerateTokens(processBody));
                this.BoolCache.Append(conditionStatus.Value);

                if (res != null)
                {

                    return res;
                }
            }

            this.BoolCache.Append(conditionStatus.Value);

            return null;
        }

        public TilangVariable? Process(List<List<string>> tokenList)
        {

            List<int> stackVarIndexs = new List<int>();
            List<int> stackFnIndexes = new List<int>();



            foreach (var tokens in tokenList)
            {

                if (tokens.Count > 0)
                {
                    var initialToken = tokens[0];


                    switch (initialToken)
                    {
                        case "": break;
                        case "var":
                        case "const":
                            var variable = VariableCreator.CreateVariable(tokens, this);
                            variable.OwnerScope = this.ScopeName;
                            var index = Stack.SetInStack(variable);
                            stackVarIndexs.Add(index);
                            break;
                        case "type":
                            TypeCreator.CreateDataStructrue(tokens, this);
                            break;
                        case "function":
                            var fn = FunctionCreator.CreateFunction(tokens);
                            fn.OwnerScope = this.ScopeName;
                            var fnIndex = Stack.AddFunction(fn);
                            stackFnIndexes.Add(fnIndex);
                            break;
                        case "switch":
                        case "if":
                        case "else if":
                        case "else":
                            var tokenMirror = new List<string>();
                            if (tokens[0] == "else" && tokens[1] == "if")
                            {
                                tokenMirror = tokens.Skip(1).ToList();
                            }
                            if (tokens[0] == "else" && tokens[1] != "if")
                            {
                                if (this.BoolCache.Length == 0) throw new Exception("no condition is defined for else");
                                tokenMirror.AddRange(tokens);
                            }
                            var res = ConditionalProcess(tokenMirror.Count > 0 ? tokenMirror : tokens);
                            if (res != null)
                            {
                                return res;
                            }
                            break;
                        case "while":
                        case "for":
                            if (tokens[0] == "for")
                            {
                                var newTokens = analyzer.GenerateTokens(TranslateForLoop(tokens));

                                return Process(newTokens);
                            }
                            var result = LoopProcess(tokens);
                            if (result != null) { return result; }
                            break;
                        default:
                            if (tokens[0] == "break")
                            {
                                if (ScopeName == "loop" || ScopeName == "switch") return null;

                                throw new Exception("cannot use break outside of a loop or switch statement");
                            }
                            if (tokens[0].StartsWith("return"))
                            {
                                var expr = tokens[0].Substring(0 + "return".Length).Trim();

                                return exprAnalyzer.ReadExpression(expr, this);
                            };

                            var target = tokens.Count > 2 ? tokens[0] + tokens[1] : tokens[0];

                            if (SyntaxAnalyzer.IsFunctionCall(target))
                            {
                                if(IsMethodCall(target))
                                {

                                }

                                List<string> items = new List<string>() { tokens[0].Substring(0 , tokens[0].IndexOf("(")).Trim()
                                    , tokens[0].Substring(tokens[0].IndexOf("(")).Trim() };
                                var callRes = ResolveFunctionCall(items);
                                if (callRes != null)
                                {
                                    return callRes;
                                }
                                break;
                            }

                            exprAnalyzer.ReadExpression(tokens, this);
                            break;
                    }
                }

            }
            var t1 = Task.Run(() =>
            {
                Stack.ClearStackByIndexes(stackVarIndexs);
            });
            var t2 = Task.Run(() =>
            {
                Stack.ClearFnStackByIndexes(stackFnIndexes);
            });

            Task.WaitAll([t1, t2]);

            return null;
        }


        public void ReplaceItemsFromStack(List<string> expressionTokens)
        {
            var isSubExpression = (string value) =>
            {
                if (value == "") return false;
                return value[0] == '(' && value[value.Length - 1] == ')';
            };
            for (var i = 0; i < expressionTokens.Count; i += 2)
            {
                var isFunctionCall = SyntaxAnalyzer.IsFunctionCall(expressionTokens[i]);

                if (isSubExpression(expressionTokens[i]))
                {
                    var str = expressionTokens[i].Substring(1, expressionTokens[i].Length - 2).Trim();
                    var list = new List<string>();
                    list.Add(str);
                    expressionTokens[i] = exprAnalyzer.ReadExpression(list, this).Value.ToString();
                }

                if (isFunctionCall)
                {
                    var fnName = expressionTokens[i].Substring(0, expressionTokens[i].IndexOf("(")).Trim();
                    var fnArgs = expressionTokens[i].Substring(expressionTokens[i].IndexOf("(")).Trim();

                    var list = new List<string>();
                    list.Add(fnName);
                    list.Add(fnArgs);

                    var callResult = ResolveFunctionCall(list)?.Value;
                    if (callResult == null) { throw new Exception("cannot use null in exprpession"); }
                    expressionTokens[i] = callResult.ToString();
                    continue;
                }
                else
                {
                    if (TypeSystem.IsRawValue(expressionTokens[i]))
                    {
                        continue;
                    }
                    else
                    {
                        expressionTokens[i] = Stack.GetFromStack(expressionTokens[i]).Value.ToString();
                    }
                }
            }
        }

        private TilangVariable? ResolveFunctionCall(List<string> tokens)
        {
            var isExpression = (string str) =>
            {
                var tokens = "+ - / * == || && != < > >= <= ! % ? :".Split(" ");

                return tokens.Any((item) => str.Contains(item)) || SyntaxAnalyzer.IsFunctionCall(str);
            };

            var fnName = tokens[0];

            var fnArgs = analyzer.SeperateFunctionArgs(tokens[1].Substring(1, tokens[1].Length - 2).Trim()).Select((item) =>
            {
                item = item.Trim();
                if (isExpression(item))
                {
                    var variable = exprAnalyzer.ReadExpression(item, this);

                    return variable;
                }
                if (TypeSystem.IsRawValue(item))
                {
                    item = item.Trim();

                    return TypeSystem.ParseType(item, this);
                }

                return Stack.GetFromStack(item);

            }).ToList();

            if (IsSystemCall(fnName))
            {
                return HandleSysCall(fnName, fnArgs);
            }

            var calledFn = Stack.GetFunction(FunctionCreator.CreateFunctionDef(fnName , fnArgs));
            return FunctionProcess(calledFn, fnArgs);
        }

        private bool IsMethodCall(string fnName)
        {
            if (IsSystemCall(fnName)) return false;   
            if (!fnName.Contains(".")) return false;
            var splits = fnName.Split('.');

            var objName = splits[0];
            var methodName = splits[1];

            var target = Stack.GetFromStack(objName);

            if (target.Value.GetType() == typeof(TilangStructs))
            {
                return true && SyntaxAnalyzer.IsFunctionCall(methodName);
            }

            return false;
        }
    }
}
