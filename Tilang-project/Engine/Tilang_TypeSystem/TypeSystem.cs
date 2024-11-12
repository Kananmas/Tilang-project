using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Utils.String_Extentions;

namespace Tilang_project.Engine.Tilang_TypeSystem
{
    public static class TypeSystem
    {

        public static Dictionary<string, TilangStructs> CustomTypes = new Dictionary<string, TilangStructs>();

        public const string CHAR_DATATYPE = "char";
        public const string INT_DATATYPE = "int";
        public const string BOOL_DATATYPE = "bool";
        public const string FLOAT_DATATYPE = "float";
        public const string STRING_DATATYPE = "string";
        public const string NULL_DATATYPE = "null";
        public const string FUNC_PTR_DATATYPE = "funcPtr";

        public static string[] PrimitiveDatatypes = [CHAR_DATATYPE,
            INT_DATATYPE,
            BOOL_DATATYPE,
            FLOAT_DATATYPE,
            STRING_DATATYPE,
            NULL_DATATYPE , FUNC_PTR_DATATYPE];



        public static bool IsArray(string value)
        {
            if (value.Length <= 1) return false;
            return value[0] == '[' && value[value.Length - 1] == ']';
        }

        public static bool IsRawValue(string Value)
        {
            return IsString(Value) || IsBool(Value) || IsFloat(Value) ||
                IsInt(Value) || IsChar(Value) || IsTypeCreation(Value) ||
                IsNull(Value) || IsArray(Value);
        }

        public static bool IsNull(string value)
        {
            return value.Trim() == "null";
        }

        public static bool IsString(string value)
        {
            return value.StartsWith('\"') && value.EndsWith('\"') && !value.GetStringContent().Contains('\"');
        }
        public static bool IsInt(string value)
        {
            if (value.Length > 1 && (value.GetStringContent().Contains("+") || value.GetStringContent().Contains("-"))) return false;

            var legalChars = "1234567890-+";
            return !IsString(value) && value.ToCharArray()
                .All(item => legalChars.Contains(item) && item != '.');
        }


        public static bool IsFloat(string value)
        {
            if (value.Length > 1 && (value.GetStringContent().Contains("+") || value.GetStringContent().Contains("-"))) return false;

            if (value.EndsWith("-") || value.EndsWith("+")) return false;
            var legalChars = "0123456789.-+";
            return !IsString(value) && !IsInt(value) && value.ToCharArray().All(item => legalChars.Contains(item));
        }

        public static bool IsChar(string value)
        {
            if (value.Length < 3) return false;
            var specialChars = "\n\t\'\"\\\0";
            bool isValid = true;

            var content = value.Substring(1, value.Length - 2);

            if (content.Length > 1)
            {
                isValid = specialChars.Contains(content);
            }


            return !IsString(value) && value.StartsWith('\'') && value.EndsWith('\'') && isValid;
        }

        public static bool IsBool(string value)
        {
            return !IsString(value) && value == "true" || value == "false";
        }

        public static bool IsFuncPtr(string value) {
            return value == FUNC_PTR_DATATYPE;
        }

        public static bool IsTypeCreation(string value)
        {
            if (!value.Contains("{") || !value.Contains("}")) return false;
            value = value.Replace("{", " {");
            var args = value.Split(" ");


            return CustomTypes.ContainsKey(args[0]);
        }


        public static List<TilangVariable> ParseFunctionArguments(string args, Processor processor)
        {
            if (args == "") return new List<TilangVariable>();
            var isExpression = (string str) =>
            {
                var tokens = Keywords.AllOperators;

                return tokens.Any((item) => str.Contains(item)) && !IsTypeCreation(str);
            };

            var content = args.GetStringContent();

            var exprAnalyzer = new ExprAnalyzer();
            var fnArgs = SyntaxAnalyzer.SplitBySperatorToken(content).Select((item) =>
            {
                item = item.Trim();
                var variable = exprAnalyzer.ReadExpression(item, processor);

                return variable;

            }).ToList();


            return fnArgs ?? new List<TilangVariable>();
        }

        public static TilangVariable ParseInt(string value)
        {
            var parsedValue = int.Parse(value);

            if (parsedValue >= -127 && parsedValue <= 128)
            {
                return new TilangVariable(INT_DATATYPE, sbyte.Parse(value));
            }
            if (parsedValue >= -32767 && parsedValue <= 32768)
            {
                return new TilangVariable(INT_DATATYPE, short.Parse(value));
            }

            return new TilangVariable(INT_DATATYPE, parsedValue);
        }

        public static TilangVariable ParseNull()
        {
            return new TilangVariable(NULL_DATATYPE, null);
        }

        public static TilangVariable ParseChar(string value)
        {
            return new TilangVariable(CHAR_DATATYPE, $"{value}");
        }


        public static TilangVariable ParseFloat(string value)
        {
            return new TilangVariable(FLOAT_DATATYPE, float.Parse(value));
        }

        public static TilangVariable ParseBool(string value)
        {
            return new TilangVariable(BOOL_DATATYPE, bool.Parse(value));
        }

        public static TilangVariable ParseString(string value)
        {
            return new TilangVariable(STRING_DATATYPE, value);
        }

        public static TilangVariable ParseFuncPtr(string value) {
            return new TilangVariable(FUNC_PTR_DATATYPE , value);
        }

        public static TilangVariable ParsePrimitiveType(string value)
        {
            if (IsBool(value)) return ParseBool(value);
            if (IsFloat(value)) return ParseFloat(value);
            if (IsInt(value)) return ParseInt(value);
            if (IsChar(value)) return ParseChar(value);
            if (IsString(value)) return ParseString(value);
            if (IsNull(value)) return ParseNull();
            if (IsFuncPtr(value)) return ParseFuncPtr(value);
            throw new Exception($"unkown data type ${value}");
        }


        public static bool AreTypesCastable(string type1, string type2)
        {
            if (type1 == INT_DATATYPE && type2 == FLOAT_DATATYPE) return true;
            if (type1 == FLOAT_DATATYPE && type2 == INT_DATATYPE) return true;
            return false;
        }

        public static TilangVariable ParseArray(string value, Processor processor)
        {
            var parsedArray = TilangArray.ParseArray(value, processor);
            var arrayType = parsedArray.ElementType + "[]";

            return new TilangVariable(arrayType, parsedArray);
        }

        public static string GetArrayType(string value, string prevResult = "[]", Processor? processor = null)
        {
            string result = prevResult;
            var item = SyntaxAnalyzer.SplitBySperatorToken(value.Substring(1, value.Length - 2))[0];
            var exprAnalayzer = new ExprAnalyzer();


            item = item.Trim();
            var pres = IsRawValue(item) ? ParseType(item, processor)
                : exprAnalayzer.ReadExpression(item, processor);

            result = pres.TypeName + result;


            return result;
        }

        public static TilangStructs ParseCustomType(string value, Processor pros)
        {
            if (IsTypeCreation(value))
            {
                var typeName = value.Substring(0, value.IndexOf("{")).Trim();

                var targetType = CustomTypes[typeName];

                var res = targetType.ParseStructFromString(value, pros);


                return res;
            }

            throw new Exception("");
        }

        public static TilangVariable ParseType(string value, Processor pros)
        {

            if (IsArray(value)) return ParseArray(value, pros);
            if (IsTypeCreation(value))
            {
                var typeName = value.Substring(0, value.IndexOf("{")).Trim();
                var res = ParseCustomType(value, pros);
                return new TilangVariable(typeName, res);
            }
            else
            {
                return ParsePrimitiveType(value);
            }
        }

        public static bool IsArrayType(string type)
        {
            if (!type.Contains('[') || !type.Contains(']')) return false;
            var splitOne = type.Substring(0, type.IndexOf('[')).Trim();
            var splitTwo = type.Substring(type.IndexOf('[')).Trim();

            return PrimitiveDatatypes.Contains(splitOne) || CustomTypes.ContainsKey(splitOne)
            && splitTwo.ToCharArray().All((item) => item == '[' || item == ']');
        }

        public static TilangVariable DefaultVariable(string Type)
        {
            switch (Type)
            {
                case "int":
                    return new TilangVariable(Type, 0);
                case "float":
                    return new TilangVariable(Type, (float)0);
                case "bool":
                    return new TilangVariable(Type, false);
                case "char":
                    return new TilangVariable(Type, "\'\'");
                case "string":
                    return new TilangVariable(Type, "\"\"");
                case "funcRef":
                    return new TilangFuncPtr() { funRef = new TilangFunction() };
                case "null":
                    return new TilangVariable(Type, "null");
                default:
                    {
                        if (IsArrayType(Type))
                        {
                            var result = new TilangArray();
                            result.ElementType = Type.Substring(0, Type.LastIndexOf("["));

                            return new TilangVariable(Type, result);
                        }
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
