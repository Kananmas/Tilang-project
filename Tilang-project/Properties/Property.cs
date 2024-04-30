namespace Tilang_project.Properties
{
    public class Property
    {
        public string Name { get; set; }
        public dynamic Value { get; set; }
        public Type Model { get; set; }

        public string PropType { get; set; } = "Variable";

    }

    public class FunctionProperty : Property
    {
        public string FunctionBody { get; set; } = "";
        public List<Property> Arguments { get; set; } = new List<Property>();

    }
}
