using Tilang_project.ExpressionEvaluator;
using Tilang_project.Properties;
using Tilang_project.Tailang_Scope;

namespace Tilang_project.Tilang_TypeSystem
{
    public static class TypeSystem
    {
        private static List<string> CustomTypes = new List<string>();


        private static Dictionary<string, DynamicObject> objRepresentation = new Dictionary<string, DynamicObject>();
        public static string[] primitiveTypes = { "string", "int", "bool", "float" };


        public static bool IsString(string str)
        {
            return str[0] == '"' && str[str.Length - 1] == '"';
        }

        public static bool IsRawValue(string str)
        {
            return IsBool(str) || IsInt(str) || IsString(str) || IsFloat(str);
        }

        private static bool IsNumber(string str)
        {
            var legalChars = "0123456789";
            return str.ToCharArray().ToList().All(x => legalChars.Contains(x) || x == '.');
        }

        public static bool IsInt(string str)
        {
            return str.IndexOf('.') == -1 && !IsString(str) && IsNumber(str);
        }

        public static bool IsBool(string str)
        {
            return !IsString(str) && (str == "false" || str == "true");
        }

        public static bool IsFloat(string str)
        {
            return IsNumber(str) && !IsInt(str);
        }

        public static dynamic ExtractValueFromString(string str)
        {
            if (IsBool(str))
            {
                return str == "true";
            }
            if (IsFloat(str))
            {
                return float.Parse(str);
            }
            if (IsInt(str))
            {
                return int.Parse(str);
            }
            if (str.Length == 0) return string.Empty;
            return FilterString(str);
        }

        public static Type ConfigureType(string type)
        {
            switch (type)
            {
                case "int":
                    return typeof(int);
                case "float":
                    return typeof(float);
                case "string":
                    return typeof(string);
                case "bool":
                    return typeof(bool);
            }
            if (IsCustomType(type))
            {
                var customObj = new DynamicObject();
                return customObj.GetType();
            }

            return typeof(object);
        }


        public static dynamic? ConfigureValueByType(string type, string value , Scope scope)
        {
            switch (type)
            {
                case "int":
                    return int.Parse(value);
                case "float":
                    return float.Parse(value);
                case "string":
                    return FilterString(value);
                case "bool":
                    return bool.Parse(value);
            }
            if (IsCustomType(type))
            {
                var dynamicObj = GenerateDefaultValueByType(type);
                if(value.Contains('{'))
                {
                    value = value.Substring(value.IndexOf('{')).Trim();
                }
                var dynamicObjVals = GenerateObjectRep(value, dynamicObj ,scope);

                foreach (var dynamicObjVal in dynamicObjVals)
                {
                    var key = dynamicObjVal.Key;
                    var val = dynamicObjVal.Value;

                    dynamicObj.SetPropertyValue(key, val);
                }

                return dynamicObj;
            }

            return null;
        }

        private static Dictionary<string, dynamic> GenerateObjectRep(string str, DynamicObject obj,Scope scope)
        {
            var result = new Dictionary<string, dynamic>();
            str = str.Substring(1, str.Length - 2);
            var val = "";
            var inBrackeys = false;
            var BrackeysCount = 0;
            var exprHandler = new ExpressionEval();

            for (int i = 0; i < str.Length; i++)
            {
                var _char = str[i];
                if (_char == '{')
                {
                    if (!inBrackeys) inBrackeys = true;
                    BrackeysCount++;
                }
                if (_char == '}')
                {
                    BrackeysCount--;
                    if (BrackeysCount == 0)
                    {
                        inBrackeys = false;
                    }
                }
                if (str[i] != ',' && i != str.Length - 1)
                {
                    val += _char;
                }

                else
                {
                    if (!inBrackeys)
                    {
                        val = val.Trim();

                        var left = val.Substring(0 , val.IndexOf('=')).Trim();
                        var right = val.Substring(val.IndexOf('=') + 1).Trim().Replace("{", " {");

                        var rightTokens = right.Split(" ");
                        if (rightTokens.Length == 1)
                        {
                            result.Add(left, ExtractValueFromString(exprHandler.ReadExpression(right , scope).ToString()));
                        }
                        else
                        {
                            var value = rightTokens[1].Substring(0, rightTokens[1].LastIndexOf('}')+1).Trim();
                            result.Add(left, ConfigureValueByType(rightTokens[0],value, scope));
                        }
                        val = "";
                    }
                    else
                    {
                        val += _char;
                    }
                }
            }


            return result;
        }

        public static bool IsCustomType(string Type)
        {
            return CustomTypes.Contains(Type);
        }

        public static dynamic GenerateDefaultValueByType(string type)
        {
            switch (type)
            {
                case "int":
                    return 0;
                case "float":
                    return 0.0f;
                case "string":
                    return "";
                case "bool":
                    return false;
            }
            if (IsCustomType(type))
            {
                var target = objRepresentation[type];
                var dynamicObject = new DynamicObject(target.GetProps());


                return dynamicObject;
            }

            return null;
        }

        public static void CreateCustomType(string typeDiscription, string TypeName , Scope scope)
        {
            string val = "";
            typeDiscription = typeDiscription.Substring(1, typeDiscription.Length - 2);
            var dynamicObj = new DynamicObject();
            dynamicObj.TypeName = TypeName;

            for (int i = 0; i < typeDiscription.Length; i++)
            {
                if (typeDiscription[i] != ';')
                {
                    val += typeDiscription[i];
                }
                else
                {
                    var tokens = val.Trim().Split(" ").ToList();
                    bool isConstant = false;

                    if (tokens[0] == "const")
                    {
                        isConstant = true;
                        tokens.RemoveAt(0);
                    }

                    var propertyName = tokens[1];
                    var propertyType = tokens[0];
                    var property = new Property();

                    property.Name = propertyName;
                    property.PropType = !isConstant ? "Variable" : "Constant";
                    property.Model = ConfigureType(propertyType);


                    property.Value = GenerateDefaultValueByType(propertyType);

                    if (tokens.Count >= 3 && tokens[2] == "=")
                    {
                        property.Value = ConfigureValueByType(tokens[3], propertyType , scope);
                    }

                    dynamicObj.AddProperty(propertyName, property);
                    val = "";
                    continue;

                }
            }

            objRepresentation.Add(TypeName, dynamicObj);
            CustomTypes.Add(TypeName);
        }

        private static string FilterString(string value) 
        {
            var res = "";

            for(int i = 0; i < value.Length; i++)
            {
                if (value[i] != '\"')
                {
                    res += value[i];
                }
            }


            return '\"' + res + '\"';
        
        }
    }
}
