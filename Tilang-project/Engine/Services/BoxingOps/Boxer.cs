using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.String_Extentions;
using Tilang_project.Utils.Tilang_Console;

namespace Tilang_project.Engine.Services.BoxingOps
{
    public static class Boxer
    {
        public static object BoxingSum(TilangVariable val1, TilangVariable val2)
        {
            if(val1.TypeName == TypeSystem.STRING_DATATYPE || val2.TypeName == TypeSystem.STRING_DATATYPE)
            {
                return  '\"' +Tilang_System.ReformString(val1.Value.ToString() + val2.Value.ToString()) + '\"';
            }
            object res = UnBoxer.ForceUnboxFloat(val1) + UnBoxer.ForceUnboxFloat(val2);

            return res;
        }

        public static object BoxingSub(TilangVariable val1, TilangVariable val2)
        {
            object res = UnBoxer.ForceUnboxFloat(val1) - UnBoxer.ForceUnboxFloat(val2);

            return res;
        }


        public static object BoxingMulti(TilangVariable val1 ,  TilangVariable val2)
        {
            var res = UnBoxer.ForceUnboxFloat(val1) * UnBoxer.ForceUnboxFloat(val2);
            return res;
        }


        public static object BoxingDiv(TilangVariable val1, TilangVariable val2)
        {
            var res = UnBoxer.ForceUnboxFloat(val1) / UnBoxer.ForceUnboxFloat(val2);
            return res;
        }
    }
}
