using Tilang_project.Engine.Services.Creators;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Processors
{
    public partial class Processor
    {
        public ProcessorStack Stack = new ProcessorStack();

        public Processor ParentProcessor { get; set; }

        public Guid scopeId = Guid.NewGuid();
        public bool PassLoop = false;
        public bool LoopBreak = false;
        public bool IsForLoop = false;

        public List<int> stackVarIndexs = new List<int>();
        public List<int> stackFnIndexes = new List<int>();
        public string ScopeType = "main";

        private BoolCache BoolCache = new BoolCache();

        private SyntaxAnalyzer analyzer = new SyntaxAnalyzer();
        private ExprAnalyzer exprAnalyzer = new ExprAnalyzer();


        private bool IsInsideALoop()
        {
            Processor current = this;
            if (current.ScopeType == "loop")
            {
                current.LoopBreak = true;
                return true;
            }

            while (current.ParentProcessor != null)
            {
                current = current.ParentProcessor;

                if (current.ScopeType == "loop")
                {
                    current.LoopBreak = true;
                    return true;
                }
            }

            return false;
        }

        public void StopLoop()
        {
            Processor current = this;
            if(current.ScopeType == "loop")
            {
                this.PassLoop = true;
                return;
            }

            while (current.ParentProcessor != null)
            {
                current = current.ParentProcessor;

                if (current.ScopeType == "loop")
                {
                    current.PassLoop = true;
                    return;
                }
            }
            throw new Exception("cannot use continue outside of a loop");
        }


        public TilangVariable? Process(List<List<string>> tokenList)
        {
            int i = 0;

            foreach (var tokens in tokenList)
            {
                // for handling continue key word
                if (this.PassLoop || this.LoopBreak)
                {
                    break;
                }

                if (tokens.Count > 0)
                {
                    var initialToken = tokens[0];
                    switch (initialToken)
                    {
                        case "": break;
                        case Keywords.TRY_KEYWORD:
                            break;
                        case Keywords.VAR_KEYWORD:
                        case Keywords.CONST_KEYWORD:
                            var variable = VariableCreator.CreateVariable(tokens, this);
                            var index = Stack.SetInStack(variable);
                            stackVarIndexs.Add(index);
                            break;
                        case Keywords.TYPE_KEYWORD:
                            TypeCreator.CreateDataStructrue(tokens, this);
                            break;
                        case Keywords.FUNCTION_KEYWORD:
                            var fn = FunctionCreator.CreateFunction(tokens, this);
                            fn.OwnerScope = this.scopeId;
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
                                var newTokens = TranslateForLoop(tokens);
                                var forRes = this.ForLoopProcess(newTokens[0], newTokens[1] , newTokens[2] , newTokens[3]); ;
                                if (forRes != null) return forRes;
                                break;
                            }
                            var result = LoopProcess(tokens);
                            if (result != null) { return result; }
                            break;
                        default:
                            if (TypeSystem.PrimitiveDatatypes.Contains(tokens[0]) ||
                                TypeSystem.CustomTypes.ContainsKey(tokens[0]) || TypeSystem.IsArrayType(tokens[0]))
                            {
                                var newTokens = new List<string>() { Keywords.VAR_KEYWORD };
                                newTokens.AddRange(tokens);
                                tokenList[i] = newTokens;
                                return Process(tokenList);
                            }
                            if (tokens[0] == Keywords.CONTINUE_KEYWORD)
                            {
                                StopLoop();
                                break;
                            }
                            if (tokens[0] == Keywords.BREAK_KEYWORD)
                            {
                                if (IsInsideALoop())
                                {
                                    return null;
                                }
                                throw new Exception("cannot use break outside of a loop");
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

                            var finalTry = exprAnalyzer.ReadExpression(tokens, this);
                            break;
                    }

                }
                i++;
            }

            if (this.PassLoop)
            {
                PassLoop = false;
                if (IsForLoop) return null;
                return Process(tokenList);
            }

            return null;
        }


        public List<dynamic> GetItemsFromStack(List<string> expressionTokens)
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
                var currentToken = expressionTokens[i];
                if (ops.Contains(currentToken)) { result.Add(currentToken); continue; }

                if (currentToken.StartsWith("."))
                {
                    if (result[i - 1].GetType() != typeof(string) && result[i - 1].Value.GetType() == typeof(TilangStructs))
                    {
                        result.Add(result[i - 1].Value.GetProperty(currentToken, this));
                        result = result.Skip(i).ToList();
                        continue;
                    }
                }

                if (SyntaxAnalyzer.IsIndexer(currentToken))
                {
                    var checkPrev = result.Count > 0 && result[result.Count - 1].GetType() != typeof(string);
                    var prev = checkPrev ? result[result.Count - 1] : null;
                    result.Add(TilangArray.UseIndexer(currentToken, this, prev));
                    result = prev != null ? result.Skip(1).ToList() : result;
                    continue;
                }
                if (TypeSystem.IsRawValue(currentToken))
                {
                    result.Add(TypeSystem.ParseType(currentToken, this));
                    continue;
                }
                var isFunctionCall = SyntaxAnalyzer.IsFunctionCall(currentToken);

                if (isSubExpression(currentToken))
                {
                    var str = expressionTokens[i].Substring(1, currentToken.Length - 2).Trim();
                    var list = new List<string>
                    {
                        str
                    };
                    result.Add(exprAnalyzer.ReadExpression(list, this) ?? TypeSystem.DefaultVariable("null"));
                    continue;
                }

                if (isFunctionCall)
                {
                    var list = SyntaxAnalyzer.TokenizeFunctionCall(currentToken);
                    var callResult = ResolveFunctionCall(list);
                    if (callResult == null) { throw new Exception("cannot use null in exprpession"); }
                    result.Add(callResult);
                    continue;
                }



                result.Add(Stack.GetFromStack(currentToken, this));
            }

            return result;
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

        private bool IsSystemCall(string value)
        {
            if (value.StartsWith("Sys")) return true;
            return false;
        }

    }
}
