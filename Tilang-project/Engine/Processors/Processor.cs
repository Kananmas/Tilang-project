using Tilang_project.Engine.Creators;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.Background_Functions;
using Tilang_project.Utils.Tilang_Console;

namespace Tilang_project.Engine.Processors
{
    public class Processor
    {
        public ProcessorStack Stack = new ProcessorStack();
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
                throw new Exception("invalid for loop condition");
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

        public TilangVariable? FunctionProcess(TilangFunction fn, List<TilangVariable> argValues)
        {
            var newStack = new ProcessorStack(this);
            var i = 0;
            var list = new List<int>();
            fn.FunctionArguments.ForEach(x =>
            {
                if (x.TypeName == "null") return;
                x.Assign(argValues[i++], "=");
                var assign = newStack.SetInStack(x.GetCopy());

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
            process.Stack = new ProcessorStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
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
            var newStack = new ProcessorStack(Stack.GetVariableStack(), Stack.GetFunctionStack());
            var newProcess = new Processor();
            newProcess.Stack = newStack;
            newProcess.ScopeName = this.ScopeName == "loop" ? "loop" : "if";

            if (tokens[0] == "if")
            {
                this.BoolCache.Clear();
            }

            if (tokens[0] == "else" && tokens[1] != "if")
            {
                if (this.BoolCache.RunElse())
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
                        case Keywords.VAR_KEYWORD:
                        case Keywords.CONST_KEYWORD:
                            var variable = VariableCreator.CreateVariable(tokens, this);
                            variable.OwnerScope = this.ScopeName;
                            var index = Stack.SetInStack(variable);
                            stackVarIndexs.Add(index);
                            break;
                        case Keywords.TYPE_KEYWORD:
                            TypeCreator.CreateDataStructrue(tokens, this);
                            break;
                        case Keywords.FUNCTION_KEYWORD:
                            var fn = FunctionCreator.CreateFunction(tokens, this);
                            fn.OwnerScope = this.ScopeName;
                            var fnIndex = Stack.AddFunction(fn);
                            stackFnIndexes.Add(fnIndex);
                            break;
                        case Keywords.SWITCH_KEYWORD:
                        case Keywords.IF_KEYWORD:
                        case Keywords.ELSE_IF_KEYWORD:
                        case Keywords.ELSE_KEYWORD:
                            var tokenMirror = new List<string>();
                            if (tokens[0] == Keywords.ELSE_KEYWORD && tokens[1] == Keywords.IF_KEYWORD)
                            {
                                tokenMirror = tokens.Skip(1).ToList();
                            }
                            if (tokens[0] == Keywords.ELSE_KEYWORD && tokens[1] != Keywords.IF_KEYWORD)
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
                        case Keywords.WHILE_KEYWORD:
                        case Keywords.FOR_KEYWORD:
                            if (tokens[0] == Keywords.FOR_KEYWORD)
                            {
                                var newTokens = analyzer.GenerateTokens(TranslateForLoop(tokens));
                                var forRes = Process(newTokens);
                                if (forRes != null) return forRes;
                                break;
                            }
                            var result = LoopProcess(tokens);
                            if (result != null) { return result; }
                            break;
                        default:

                            if (tokens[0] == Keywords.CONTINUE_KEYWORD) break;
                            if (tokens[0] == Keywords.BREAK_KEYWORD)
                            {
                                if (ScopeName == "loop" || ScopeName == Keywords.SWITCH_KEYWORD) return null;

                                throw new Exception("cannot use break outside of a loop or switch statement");
                            }

                            if (tokens[0].StartsWith(Keywords.RETURN_KEYWORD))
                            {
                                var expr = tokens[0].Substring(Keywords.RETURN_KEYWORD.Length).Trim();

                                return exprAnalyzer.ReadExpression(expr, this);
                            };

                            var target = tokens.Count > 2 ? tokens[0] + tokens[1] : tokens[0];

                            if (SyntaxAnalyzer.IsFunctionCall(target))
                            {
                                List<string> items = SyntaxAnalyzer.TokenizeFunctionCall(target);
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


        public List<dynamic> ReplaceItemsFromStack(List<string> expressionTokens)
        {
            var ops = Keywords.AllOperators;
            var result = new List<dynamic>();

            var isSubExpression = (string value) =>
            {
                if (value == "") return false;
                return value[0] == '(' && value[value.Length - 1] == ')';
            };


            for (var i = 0; i < expressionTokens.Count; i += 1)
            {
                if (ops.Contains(expressionTokens[i])) { result.Add(expressionTokens[i]); continue; }

                if(expressionTokens[i].StartsWith(".")) {
                    if(result[i-1].GetType() != typeof(string) && result[i-1].Value.GetType() == typeof(TilangStructs)) {
                        result.Add(result[i-1].Value.GetProperty( expressionTokens[i] ,this));
                        result = result.Skip(i).ToList();
                        continue;
                    } 
                }

                if (SyntaxAnalyzer.IsIndexer(expressionTokens[i]))
                {
                    var prev = result[result.Count-1].GetType() != typeof(string) ? result[result.Count-1]:null;
                    result.Add(TilangArray.UseIndexer(expressionTokens[i], this , prev));
                    result = result.Skip(1).ToList();
                    continue;
                }
                if (TypeSystem.IsRawValue(expressionTokens[i]))
                {
                    result.Add(TypeSystem.ParseType(expressionTokens[i],this));
                    continue;
                }
                var isFunctionCall = SyntaxAnalyzer.IsFunctionCall(expressionTokens[i]);

                if (isSubExpression(expressionTokens[i]))
                {
                    var str = expressionTokens[i].Substring(1, expressionTokens[i].Length - 2).Trim();
                    var list = new List<string>();
                    list.Add(str);
                    result.Add(exprAnalyzer.ReadExpression(list, this) ?? TypeSystem.DefaultVariable("null"));
                    continue;
                }

                if (isFunctionCall)
                {
                    var list = SyntaxAnalyzer.TokenizeFunctionCall(expressionTokens[i]);
                    var callResult = ResolveFunctionCall(list);
                    if (callResult == null) { throw new Exception("cannot use null in exprpession"); }
                    result.Add(callResult);
                    continue;
                }

                

               result.Add(Stack.GetFromStack(expressionTokens[i], this)); 
            }

            return result;
        }

        public TilangVariable? ResolveFunctionCall(List<string> tokens)
        {
            var fnName = tokens[0];
            var fnArgs = TypeSystem.ParseFunctionArguments(tokens[1], this);

            if (IsMethodCall(fnName + tokens[1]))
            {
                var fullStr = fnName + tokens[1];
                var objName = fullStr.Substring(0, fullStr.LastIndexOf("."));

                return Stack.GetFromStack(objName, this).Value.CallMethod(fnName.Substring(fnName.LastIndexOf(".") + 1).Trim(), fnArgs, this);
            }

            if (IsSystemCall(fnName))
            {
                return HandleSysCall(fnName, fnArgs);
            }

            if(Keywords.IsBackgroundFunction(fnName))
            {
                return BackgroundFunctions.CallBackgroundFunctions(fnName , fnArgs);
            }

            var calledFn = Stack.GetFunction(FunctionCreator.CreateFunctionDef(fnName, fnArgs));
            return FunctionProcess(calledFn, fnArgs);
        }

        private bool IsMethodCall(string fnName)
        {
            if (IsSystemCall(fnName)) return false;
            // this checks call of normal functions like fn(2.2) 
            if (!fnName.Substring(0, fnName.IndexOf("(")).Contains(".")) return false;

            var objName = fnName.Substring(0, fnName.LastIndexOf(".")).Trim();

            var target = Stack.GetFromStack(objName, this);

            if (target.Value.GetType() == typeof(TilangStructs))
            {
                return true;
            }

            return false;
        }
    }
}
