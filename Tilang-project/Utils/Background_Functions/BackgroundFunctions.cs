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
                int result = item.TypeName == "string" ? UnBoxer.UnboxString(item).Length - 2 : UnBoxer.UnboxArray(item).Length;
                return new TilangVariable("int", result);
            }
            throw new NotImplementedException();
        }

        private static void Add(TilangVariable Target, TilangVariable item)
        {
            UnBoxer.UnboxArray(item).Add(item);
        }

        private static void Remove(TilangVariable Target, TilangVariable item)
        {
            UnBoxer.UnboxArray(item).Remove(item);
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
                    Remove(fnArgs[0], fnArgs[1]);
                    return null;
                default:
                    return null;
            }
        }
    }
}
