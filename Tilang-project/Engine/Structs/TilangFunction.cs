namespace Tilang_project.Engine.Structs
{
    public class TilangFunction
    {
        public List<TilangVariable> FunctionArguments { get; set; } = new List<TilangVariable>();
        public string Body { get; set; } = "";
        public string FunctionName { get; set; } = "";
        public string ReturnType { get; set; } = "";

        public string OwnerScope { get; set; } = "";
        public string FuncDefinition { get; set; } = "";

    }
}
