using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Structs
{
    public class TilangFunction
    {
        public List<TilangVariable> FunctionArguments { get; set; } = new List<TilangVariable>();
        public string Body { get; set; } = "";
        public string FunctionName { get; set; } = "";
        public string ReturnType { get; set; } = "";

        public Guid OwnerScope { get; set; } = Guid.Empty;
        public string FuncDefinition { get; set; } = "";
        
        public TilangFunction GetCopy()
        {
            var copy = new TilangFunction();
            copy.Body = Body;
            copy.FunctionArguments = FunctionArguments.Select((item) => item.GetCopy()).ToList();
            copy.FunctionName = FunctionName;
            copy.ReturnType = ReturnType;
            return copy;
        }

        public void ResetVariable()
        {
            for(int i=0;i<FunctionArguments.Count;i++)
            {
                var v = FunctionArguments[i];
                FunctionArguments[i] = TypeSystem.DefaultVariable(v.TypeName);
            }
        }


        public List<int> InjectFunctionArguments(Processor processor , List<TilangVariable> variables)
        {
            var list = new List<int>();
            var i = 0;

            this.FunctionArguments.ForEach(x =>
            {
                if (x.TypeName == "null") return;
                x.Assign(variables[i++], Keywords.EQUAL_ASSIGNMENT);
                var copy = x.GetCopy();
                copy.VariableName = x.VariableName;
                copy.OwnerId = processor.scopeId;
                var assign = processor.Stack.SetInStack(copy);

                list.Add(assign);
            });


            return list;
        }
    }
}
