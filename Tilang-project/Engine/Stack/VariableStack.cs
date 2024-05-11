using Tilang_project.Engine.Structs;

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
            var item = Stack.Where((item) =>  item.VariableName.Equals(stackName)).FirstOrDefault();

            if (item != null) return item;
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
            Stack.Add( variable); 
            return Stack.Count - 1; 
        }

        public int AddFunction(TilangFunction function) 
        { 
            this.Functions.Add( function); 
            return Functions.Count - 1; 
        }

        public void ClearStackByIndexes(List<int> indexes)
        {
            foreach(var index in indexes)
            {
                Stack.RemoveAt(index);
            }
        }

        public void ClearFnStackByIndexes(List<int> indexes)
        {
            foreach (var index in indexes)
            {
                Stack.RemoveAt(index);
            }
        }
    }
}
