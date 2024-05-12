﻿using Tilang_project.Engine.Structs;

namespace Tilang_project.Engine.Stack
{
    public class VariableStack
    {
        private List<TilangVariable> Stack { get; set; } = new List<TilangVariable>();
        private List<TilangFunction> Functions { get; set; } = new List<TilangFunction>();

        public VariableStack() { }
        public VariableStack(List<TilangVariable> stack, List<TilangFunction> functions)
        {
            Stack = stack;
            Functions = functions;
        }

        public List<TilangVariable> GetVariableStack()
        {
            return Stack;
        }

        public List<TilangFunction> GetFunctionStack()
        {
            return this.Functions;
        }

        public TilangVariable GetFromStack(string stackName)
        {
            var stackNames = stackName.Replace(" ", "").Split(".").ToList();
            string targetName = stackName;
            if (stackNames.Count > 1)
            {
                targetName = stackNames[0];
                stackNames.RemoveAt(0);
            }

            var item = Stack.Where((item) => item.VariableName.Equals(stackName)).FirstOrDefault();

            if (item != null) return item;

            if (stackNames.Count > 1)
            {
                return item.GetSubproperties(stackNames);
            }

            throw new Exception($"no variable named {stackName} exists");
        }

        public TilangFunction GetFunction(string stackName)
        {
            var item = Functions.Where((item) => item.FunctionName.Equals(stackName)).FirstOrDefault();

            if (item != null) return item;
            throw new Exception($"no variable named {stackName} exists");
        }

        public int SetInStack(TilangVariable variable)
        {
            Stack.Add(variable);
            return Stack.Count - 1;
        }

        public int AddFunction(TilangFunction function)
        {
            this.Functions.Add(function);
            return Functions.Count - 1;
        }

        public void ClearStackByIndexes(List<int> indexes)
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

        public void ClearFnStackByIndexes(List<int> indexes)
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
