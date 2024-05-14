using Tilang_project.Engine.Structs;

namespace Tilang_project.Utils.Tilang_Console
{
    public static class Tilang_System
    {
        public static void PrintLn(List<TilangVariable> vars)
        {
            var str = "";

            vars.ForEach(x => str += x.Value.ToString());

            Console.WriteLine(str);
        }

        public static void Print(List<TilangVariable> vars)
        {
            var str = "";

            vars.ForEach(x => str += x.Value.ToString());

            Console.Write(str);
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
    }
}
