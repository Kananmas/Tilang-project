using Tilang_project.Engine.Structs;

namespace Tilang_project.Engine.Tilang_TypeSystem
{
    public static class TypeSystem
    {
        public static string[] PrimitiveDatatype = ["char", "int", "bool", "float"];
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
            var specialChars = "\n\t\'\"";
            bool isValid = true;

            var content = value.Substring(0, value.Length - 1);

            if (content.Length > 1)
            {
                isValid = specialChars.Contains(content);
            }


            return !IsString(value) && value[0] == '\'' && value[value.Length - 1] != '\'' && isValid;
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

        public static TilangType ParseInt(string value)
        {
            return new TilangType("int", int.Parse(value));
        }

        public static TilangType ParseChar(string value)
        {
            return new TilangType("char", char.Parse(value));
        }


        public static TilangType ParseFloat(string value)
        {
            return new TilangType("float", float.Parse(value));
        }

        public static TilangType ParseBool(string value)
        {
            return new TilangType("bool", bool.Parse(value));
        }

        public static TilangType ParsePrimitiveType(string value)
        {
            if (IsBool(value)) return ParseBool(value);
            if (IsFloat(value)) return ParseFloat(value);
            if (IsInt(value)) return ParseInt(value);
            if (IsChar(value)) return ParseChar(value);
            throw new Exception($"unkown data type ${value}");
        }

        public static TilangStructs ParseCustomType(string value)
        {
            if (IsTypeCreation(value))
            {
                var typeName = value.Substring(0, value.IndexOf("{") - 1).Trim();

                var targetType = CustomTypes[typeName];

                return targetType.ParseStructFromString(value);
            }

            throw new Exception("");
        }

    }
}
