using Tilang_project.Engine.Structs;

namespace Tilang_project.Engine.Stack
{
    public class VariableStack
    {
        private Dictionary<string, TilangVariable> Stack { get; set; } = new Dictionary<string, TilangVariable>();
        private Dictionary<string, TilangFunction> Functions { get; set; } = new Dictionary<string, TilangFunction>();


        public TilangVariable GetFromStack(string stackName)
        {
            if (Stack.ContainsKey(stackName)) return Stack[stackName];
            throw new Exception($"no variable named {stackName} exists");
        }

        public int SetInStack(string stackName, TilangVariable variable) { Stack.Add(stackName, variable); return Stack.Count - 1; }
        public int AddFunction(string stackName, TilangFunction function) { this.Functions.Add(stackName, function); return Functions.Count - 1; }


        public void ClearStackByIndexes(List<int> indexes)
        {
            var keys = Stack.Keys.ToList();
            foreach(var index in indexes)
            {
                Stack.Remove(keys[index]);
            }
        }

        public void ClearFnStackByIndexes(List<int> indexes)
        {
            var keys = Functions.Keys.ToList();
            foreach (var index in indexes)
            {
                Stack.Remove(keys[index]);
            }
        }
    }
}
