﻿using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;

namespace Tilang_project.Engine.Stack
{
    public class ProcessorStack
    {
        private List<TilangVariable> Stack { get; set; } = new List<TilangVariable>();
        private List<TilangFunction> Functions { get; set; } = new List<TilangFunction>();

        public ProcessorStack() { }
        public ProcessorStack(List<TilangVariable> stack, List<TilangFunction> functions)
        {
            Stack = stack;
            Functions = functions;
        }

        public ProcessorStack(Processor processor, bool pure = false)
        {
            if (!pure)
            {
                this.Stack = processor.Stack.GetVariableStack();
                this.Functions = processor.Stack.GetFunctionStack();
                return;
            }

            this.Stack = processor.Stack.GetVariableStackPure();
            this.Functions = processor.Stack.GetFunctionStackPure();
        }

        public List<TilangVariable> GetVariableStack()
        {
            return Stack;
        }

        public List<TilangFunction> GetFunctionStack()
        {
            return this.Functions;
        }

        public List<TilangFunction> GetFunctionStackPure()
        {
            return this.Functions.Select(item => item.GetCopy()).ToList();
        }

        public List<TilangVariable> GetVariableStackPure()
        {
            return this.Stack.Select(item => item.GetCopy()).ToList();
        }

        public TilangVariable? GetFromStack(string stackName, Processor processor)
        {
            if (Stack.Count == 0) return null;
            TilangVariable item;
            var stackNames = stackName.Replace(" ", "")
                .Split(".").Where(item => item != "").ToList();
            string targetName = stackName;
            if (stackNames.Count > 1)
            {
                targetName = stackNames[0];
            }

            if (SyntaxAnalyzer.IsIndexer(targetName))
            {
                item = TilangArray.UseIndexer(targetName, processor);
            }
            else
            {
                item = Stack.Where((item) => item.VariableName.Equals(targetName)).LastOrDefault();
            }




            if (item != null)
            {
                if (stackNames.Count > 1)
                {
                    stackNames.RemoveAt(0);
                    return item.GetSubproperty(stackNames, processor);
                }
                return item;
            }

            return null;
        }

        public TilangFunction GetFunction(string defination)
        {
            var item = Functions.Where((item) => item.FuncDefinition.Equals(defination)).FirstOrDefault();

            if (item != null) return item;

            var isFunPtr = Stack.Where((item) =>
            {
                if (!(item.GetType() == typeof(TilangFuncPtr))) return false;
                var funcPtr = (TilangFuncPtr)item;
                return funcPtr.funRef.FuncDefinition == defination || defination.Contains(funcPtr.VariableName);
            }).FirstOrDefault();

            if (isFunPtr != null) return ((TilangFuncPtr)isFunPtr).funRef;
            var start = defination.IndexOf('[') + 1;
            var len = defination.LastIndexOf(']') - start;
            var fnName = defination.Substring(start, len).Trim();
            var finalTry = GetFromStack(fnName, new Processor()
            {
                Stack = this,
            });
            if (finalTry != null)
            {
                var result = (TilangFuncPtr)finalTry;
                return result.funRef;
            }

            throw new Exception($"no function {defination} exists");
        }

        public int SetInStack(TilangVariable variable)
        {
            if (hasVariable(variable))
            {
                throw new Exception($"{variable.VariableName} doesn't exits in current context");
            }
            Stack.Add(variable);
            return Stack.Count - 1;
        }

        public int AddFunction(TilangFunction function)
        {
            if (hasFunction(function))
                throw new Exception("cannot define a function twice in same scope");
            this.Functions.Add(function);
            return Functions.Count - 1;
        }


        private bool hasVariable(TilangVariable variable)
        {
            var result = Stack.Where((item) => item.VariableName == variable.VariableName &&
            item.OwnerId == variable.OwnerId).FirstOrDefault();

            return result != null;
        }

        private bool hasFunction(TilangFunction fn)
        {
            var result = Functions.Where((item) => item.FuncDefinition == fn.FuncDefinition &&
            item.OwnerScope == fn.OwnerScope).FirstOrDefault();

            return result != null;
        }

        public void GrabageCollection(Guid ScopeId)
        {
            var t1 = Task.Run(() =>
            {
                Stack = Stack.Where((item) => item.OwnerId != ScopeId).ToList();
            });

            var t2 = Task.Run(() =>
            {
                Functions = Functions.Where((item) => item.OwnerScope != ScopeId).ToList();
            });


            Task.WaitAll([t1, t2]);
        }

        public void ClearVariables(List<int> indexes)
        {
            var list = new List<TilangVariable>();
            foreach (var index in indexes)
            {
                list.Add(Stack[index]);
            }

            list.ForEach(item =>
            {
                Stack.Remove(item);
            });
        }

        public void ClearFunctions(List<int> indexes)
        {
            var list = new List<TilangFunction>();
            foreach (var index in indexes)
            {
                list.Add(Functions[index]);
            }

            list.ForEach(item =>
            {
                Functions.Remove(item);
            });
        }



    }
}
