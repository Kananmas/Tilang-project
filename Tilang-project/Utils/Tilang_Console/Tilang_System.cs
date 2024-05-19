using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_Keywords;

namespace Tilang_project.Utils.Tilang_Console
{
    public static class Tilang_System
    {
        public static void PrintLn(List<TilangVariable> vars)
        {
            var str = "";

            vars.ForEach(x => str += x.Value.ToString());

            Console.WriteLine(ReformString(str.Replace(Keywords.SINGLE_QUOET_RP, "\'").Replace(Keywords.DOUBLE_QUOET_RP , "\"")));
        }

        public static void Print(List<TilangVariable> vars)
        {
            var str = "";

            vars.ForEach(x => str += x.Value.ToString());

            Console.Write(ReformString(str));
        }


        public static TilangVariable GetKey()
        {
            var key = Console.ReadKey();

            return new TilangVariable("char", $"\'{key.KeyChar}\'");
        }

        public static TilangVariable GetLine()
        {
            var key = Console.ReadLine();

            return new TilangVariable("string", $"\"{key}\"" ?? "");
        }

        private static string ReformString(string str)
        {
            var result = "";

            for(int i=0; i<str.Length; i++)
            {
                if(str[i] != '\"')
                {
                    result += str[i];
                }
            }

            return result;
        }
    }
}
