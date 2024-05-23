using Tilang_project.Engine.Services.BoxingOps;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.String_Extentions;

namespace Tilang_project.Utils.Background_Functions
{
    public static class BackgroundFunctions
    {
        private static TilangVariable Len(TilangVariable item)
        {
            if (TypeSystem.IsArrayType(item.TypeName) || item.TypeName == "string")
            {
                int result = item.TypeName == "string" ? UnBoxer.UnboxString(item).Length : UnBoxer.UnboxArray(item).Length;
                return new TilangVariable(TypeSystem.INT_DATATYPE, result);
            }
            throw new NotImplementedException();
        }

        private static TilangVariable ToInt(TilangVariable item)
        {
            switch(item.TypeName)
            {
                case TypeSystem.INT_DATATYPE: return item;
                case TypeSystem.STRING_DATATYPE: return TypeSystem.ParseInt(((string)item.Value).GetStringContent());
                case TypeSystem.BOOL_DATATYPE: var result = (bool)item.Value ? 1 : 0;
                    return new TilangVariable(TypeSystem.INT_DATATYPE , result );
                case TypeSystem.FLOAT_DATATYPE: return new TilangVariable(TypeSystem.INT_DATATYPE, (int)item.Value);
                default:
                    if(TypeSystem.CustomTypes.ContainsKey(item.TypeName))
                    {
                        throw new Exception("cannot cast object to int");
                    }
                    throw new Exception("unkown data type");
            }
        }

        private static TilangVariable ToString(TilangVariable item)
        {
            return new TilangVariable(TypeSystem.STRING_DATATYPE , "\"" + item.Value.ToString() + "\"");
        }

        private static TilangVariable ToCharCode(TilangVariable item)
        {
            if (item.TypeName != TypeSystem.CHAR_DATATYPE) throw new Exception("function only gets character as argument");
            var c = (int) item.ToString().GetStringContent()[0];

            return new TilangVariable(TypeSystem.INT_DATATYPE , c);
        }

        private static TilangVariable ToFloat(TilangVariable item)
        {
            switch(item.TypeName)
            {
                case TypeSystem.FLOAT_DATATYPE : return item;
                case TypeSystem.CHAR_DATATYPE:
                    return new TilangVariable(TypeSystem.FLOAT_DATATYPE, (float) ToCharCode(item).Value);
                case TypeSystem.INT_DATATYPE:
                    return new TilangVariable(TypeSystem.FLOAT_DATATYPE , (float) item.Value);
                case TypeSystem.STRING_DATATYPE:
                    return new TilangVariable(TypeSystem.FLOAT_DATATYPE , float.Parse(item.Value.ToString()));
            }
            throw new Exception("unkown data type");
        }

        private static void Add(TilangVariable Target, TilangVariable item)
        {
            UnBoxer.UnboxArray(Target).Add(item);
        }

        private static void Remove(TilangVariable Target, TilangVariable item)
        {
            UnBoxer.UnboxArray(Target).Remove(item);
        }

        private static void Remove(TilangVariable Target, int index)
        {
            UnBoxer.UnboxArray(Target).Remove(index);
        }

        private static TilangVariable ToCharArray(TilangVariable target)
        {
            if (target.TypeName == "string")
            {
                var tilangArr = new TilangArray();
                var unBoxedValue = UnBoxer.UnboxString(target);
                string value = unBoxedValue.GetStringContent();

                tilangArr.ElementType = "char[]";

                tilangArr.SetElements(value.ToCharArray().Select((item) =>
                {
                    return new TilangVariable("char", $"\'{item}\'");
                }).ToList());


                var result = new TilangVariable("char[]", tilangArr);

                return result;
            }

            throw new Exception("cannot turn non string to char array");
        }



        public static TilangVariable? CallBackgroundFunctions(string fnName, List<TilangVariable> fnArgs)
        {
            switch (fnName)
            {
                case Keywords.LEN_BG_FUNCTION:
                    return Len(fnArgs[0]);
                case Keywords.TO_CHAR_BG_FUNCTION:
                    return ToCharArray(fnArgs[0]);
                case Keywords.ADD_BG_FUNCTION:
                    Add(fnArgs[0], fnArgs[1]);
                    return null;
                case Keywords.REMOVE_BG_FUNCTION:
                    if (fnArgs[1].TypeName == TypeSystem.INT_DATATYPE) 
                        Remove(fnArgs[0] , UnBoxer.UnboxInt(fnArgs[1]));
                    Remove(fnArgs[0], fnArgs[1]);
                    return null;
                case Keywords.TO_STRING_BG_METHOD: return ToString(fnArgs[0]);
                case Keywords.TO_INT_BG_METHOD:return ToInt(fnArgs[0]);
                case Keywords.TO_FLOAT_BG_METHOD:return ToFloat(fnArgs[0]);
                case Keywords.GET_CHAR_CODE:return ToCharCode(fnArgs[0]);
                default:
                    return null;
            }
        }
    }
}
