using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.String_Extentions;

namespace Tilang_project.Engine.Services.BoxingOps
{
    public static class UnBoxer
    {
        public static int UnboxInt(TilangVariable variable)
        {
            if (variable.TypeName == TypeSystem.INT_DATATYPE)
            {
                var res = Convert.ToInt32(variable.Value);

                if(res >= -127  && res <= 128)
                {
                    return Convert.ToSByte(variable.Value);
                }

                if(res >= -32767 && res <= 32768)
                {
                    return Convert.ToInt16(variable.Value);
                }

                return res;
            }

            throw new Exception("variable is not the type of int");
        }

        public static float UnboxFloat(TilangVariable variable)
        {
            if (variable.TypeName == TypeSystem.FLOAT_DATATYPE)
            {
                return (float)variable.Value;
            }

            throw new Exception("variable is not the type of float");
        }

        public static float ForceUnboxFloat(TilangVariable variable)
        {
            return float.Parse(variable.Value.ToString());
        }

        public static bool UnboxCompare(object val1 , object val2 , string op)
        {
            switch(op)
            {
                case "||":
                    return ((bool)val1) || ((bool)val2);
                case ">=":
                    return  float.Parse(val1.ToString()) >= float.Parse(val2.ToString());
                case ">":
                    return float.Parse(val1.ToString()) > float.Parse(val2.ToString());
                case "<=":
                    return float.Parse(val1.ToString()) <= float.Parse(val2.ToString());
                case "<":
                    return  float.Parse(val1.ToString()) < float.Parse(val2.ToString());
                case "&&":
                    return ((bool)val1) && ((bool)val2);
                case "==":
                    return ((dynamic)val1) == ((dynamic)val2);
                case "!=":
                    return ((dynamic)val1) != ((dynamic)val2);

            }
            throw new Exception();
        }


        public static string UnboxString(TilangVariable variable)
        {
            if (variable.TypeName == TypeSystem.STRING_DATATYPE)
            {
                return (string)variable.Value.ToString().GetStringContent();
            }

            throw new Exception("variable is not the type of string");
        }

        public static TilangStructs UnboxStruct(TilangVariable variable)
        {
            if (TypeSystem.CustomTypes.ContainsKey(variable.TypeName))
            {
                return (TilangStructs)variable.Value;
            }

            throw new Exception("no such type exist");
        }

        public static bool UnboxBool(TilangVariable variable)
        {
            return (bool)variable.Value;
        }

        public static char UnboxChar(TilangVariable variable)
        {
            return (char)variable.Value;
        }

        public static TilangArray UnboxArray(TilangVariable variable)
        {
            return (TilangArray)variable.Value;
        }


    }
}
