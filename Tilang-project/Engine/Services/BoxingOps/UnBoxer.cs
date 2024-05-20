using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Services.BoxingOps
{
    public static class UnBoxer
    {
        public static int UnboxInt(TilangVariable variable)
        {
            if (variable.TypeName == TypeSystem.INT_DATATYPE)
            {
                return (int)variable.Value;
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
            return variable.Value.GetType() == typeof(int) ? float.Parse(variable.Value.ToString()):((float) variable.Value);
        }


        public static string UnboxString(TilangVariable variable)
        {
            if (variable.TypeName == TypeSystem.STRING_DATATYPE)
            {
                return (string)variable.Value;
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
