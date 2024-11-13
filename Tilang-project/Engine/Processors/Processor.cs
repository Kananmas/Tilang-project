using Tilang_project.Engine.Services.Creators;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;

public delegate TilangVariable ExecuteTry();
public delegate TilangVariable ExecuteCatch(string errorMessage);
public delegate TilangVariable ExecuteFinally();

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
        public bool InPipeLine = false;
        public bool IsTryCatched = false;

        public List<int> stackVarIndexs = new List<int>();
        public List<int> stackFnIndexes = new List<int>();
        public string ScopeType = "main";

        private BoolCache BoolCache = new BoolCache();

        private SyntaxAnalyzer analyzer = new SyntaxAnalyzer();
        private ExprAnalyzer exprAnalyzer = new ExprAnalyzer();

        private ExecuteTry executeTry;
        private ExecuteCatch executeCatch;
        private ExecuteFinally executeFinally;

        private TilangVariable? tryCatchExecution()
        {
            try
            {
                var res = executeTry?.Invoke();
                if (res != null)
                {
                    return res;
                }
            }
            catch (Exception e)
            {
                var res = executeCatch?.Invoke(e.Message);
                if (res != null)
                {
                    return res;
                }
            }
            finally
            {
                var res = executeFinally?.Invoke();
            }
            return null;
        }

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
            if (current.ScopeType == "loop")
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
                        case Keywords.FINALLY_KEYWORD:
                            break;
                        case Keywords.CATCH_KEYWORD:
                            break;
                        case Keywords.TRY_KEYWORD:
                            IsTryCatched = true;
                            HandleTryCatch(tokenList , i);
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
                            var fnIndex = Stack.AddFunction(fn);
                            stackFnIndexes.Add(fnIndex);
                            break;
                        case Keywords.SWITCH_KEYWORD:
                            break;
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
                            var result = LoopProcess(tokens);
                            if (result != null) { return result; }
                            break;
                        case Keywords.FOR_KEYWORD:
                            var newTokens = TranslateForLoop(tokens);
                            var forRes = ForLoopProcess(newTokens[0], newTokens[1],
                                newTokens[2], newTokens[3]);
                            if (forRes != null) return forRes;
                            break;
                        default:
                            if (IsTryCatched)
                            {
                                IsTryCatched = false;
                                var executionResult = tryCatchExecution();
                                if (executionResult != null) return executionResult;
                                break;
                            }
                            if (TypeSystem.PrimitiveDatatypes.Contains(tokens[0]) ||
                                TypeSystem.CustomTypes.ContainsKey(tokens[0]) ||
                                TypeSystem.IsArrayType(tokens[0]))
                            {
                                var newToks = new List<string>() { Keywords.VAR_KEYWORD };
                                newToks.AddRange(tokens);
                                tokenList[i] = newToks;
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

            if (IsTryCatched)
            {
                IsTryCatched = false;
                var executionResult = tryCatchExecution();
                if (executionResult != null) return executionResult;
            }

            if (this.PassLoop)
            {
                if (!InPipeLine) PassLoop = false;
                if (IsForLoop) return null;
                return Process(tokenList);
            }

            return null;
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
