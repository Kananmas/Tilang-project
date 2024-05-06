namespace Tilang_project.Engine.Structs
{
    public class TilangType
    {
        public string TypeName { get; set; }
        public dynamic Value { get; set; }

        public TilangType() { }

        public TilangType(string typeName, dynamic value)
        {
            TypeName = typeName;
            Value = value;
        }

        public TilangType GetSubproperties(List<string> keys)
        {
            if (Value.GetType() == typeof(TilangStructs))
            {
                var target = Value.GetProperty(keys[0]);
                if (keys.Count == 1)
                {
                    return target;
                }

                keys.RemoveAt(0);
                return target.GetSubproperties(keys);

            }

            else
            {
                throw new Exception($"property of ${keys[0]} is not a structure");
            }

        }
    }
}
