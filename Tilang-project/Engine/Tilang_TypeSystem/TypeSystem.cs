using Tilang_project.Engine.Structs;

namespace Tilang_project.Engine.Tilang_TypeSystem
{
    public static class TypeSystem
    {
        public static string[] PrimitiveDatatype = ["char", "int", "bool", "float" , "string"];
        public static Dictionary<string , TilangStructs> CustomTypes = new Dictionary<string, TilangStructs> ();

        public static bool IsString(string value)
        {
            return value[0] == '"' && value[value.Length - 1] == '"';
        }
        public static bool IsInt(string value)
        {
            var legalChars = "1234567890";
            return !IsString(value) && value.ToCharArray().All(item => legalChars.Contains(item) && item != '.');
        }


        public static bool IsFloat(string value)
        {
            var legalChars = "0123456789.";
            return !IsString(value) && !IsInt(value) && value.ToCharArray().All(item => legalChars.Contains(item));
        }

        public static bool IsChar(string value)
        {
            var specialChars = "\n\t\'\"\\\0";
            bool isValid = true;

            var content = value.Substring(1, value.Length - 2);

            if (content.Length > 1)
            {
                isValid = specialChars.Contains(content);
            }


            return !IsString(value) && value[0] == '\'' && value[value.Length - 1] == '\'' && isValid;
        }

        public static bool IsBool(string value)
        {
            return !IsString(value) && value == "true" || value == "false";
        }

        public static bool IsTypeCreation(string value)
        {
            value = value.Replace("{", " {");
            var args = value.Split(" ");


            return CustomTypes.ContainsKey(args[0]);
        }

        public static TilangVariable ParseInt(string value)
        {
            return new TilangVariable("int", int.Parse(value));
        }

        public static TilangVariable ParseChar(string value)
        {
            return new TilangVariable("char", char.Parse(value.Substring(1 , value.Length-2)));
        }


        public static TilangVariable ParseFloat(string value)
        {
            return new TilangVariable("float", float.Parse(value));
        }

        public static TilangVariable ParseBool(string value)
        {
            return new TilangVariable("bool", bool.Parse(value));
        }

        public static TilangVariable ParseString(string value)
        {
            return new TilangVariable("string", value);
        }

        public static TilangVariable ParsePrimitiveType(string value)
        {
            if (IsBool(value)) return ParseBool(value);
            if (IsFloat(value)) return ParseFloat(value);
            if (IsInt(value)) return ParseInt(value);
            if (IsChar(value)) return ParseChar(value);
            if(IsString(value)) return ParseString(value);
            throw new Exception($"unkown data type ${value}");
        }

        public static TilangStructs ParseCustomType(string value)
        {
            if (IsTypeCreation(value))
            {
                var typeName = value.Substring(0, value.IndexOf("{")).Trim();

                var targetType = CustomTypes[typeName];

                return targetType.ParseStructFromString(value);
            }

            throw new Exception("");
        }

        public static TilangVariable ParseType(string value)
        {
            if(IsTypeCreation(value))
            {
                var typeName = value.Substring(0, value.IndexOf("{")).Trim();
                var res = ParseCustomType(value);
                return new TilangVariable(typeName , res);
            }
            else
            {
                return ParsePrimitiveType(value);
            }
        }

        public static TilangVariable DefaultVariable(string Type)
        {
            switch(Type) 
            {
                case "int":
                    return new TilangVariable(Type, 0);
                case "float":
                    return new TilangVariable(Type, (float)0);
                case "bool":
                    return new TilangVariable(Type, false);
                case "char":
                    return new TilangVariable(Type, '\0');
                case "string":
                    return new TilangVariable(Type, "");
                default:
                    {

                        if (CustomTypes.ContainsKey(Type))
                        {
                            var result = new TilangVariable();
                            var targetType = CustomTypes[Type];

                            result.TypeName = Type;
                            result.Value = targetType.GetCopy();

                            return result;
                        }


                        throw new Exception($"no type as {Type} exists");

                    }
            
            }
        }
    }
}
