using Tilang_project.Engine.Structs;

namespace Tilang_project.Engine.Services.BoxingOps
{
    public static class Boxer
    {
        public static object BoxingSum(TilangVariable val1, TilangVariable val2)
        {
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
