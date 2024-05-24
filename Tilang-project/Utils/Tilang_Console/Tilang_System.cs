using System.Diagnostics;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.String_Extentions;

namespace Tilang_project.Utils.Tilang_Console
{
    public static class Tilang_System
    {
        public static TilangVariable RunCommand(List<TilangVariable> commands) {
            
            var exe = commands[0].Value.ToString().GetStringContent();
            var str = "";
            var result = "";

            commands.GetRange(1,commands.Count-1).ForEach(x => str += x.Value.ToString());
            str = ReformString(str);

            var process = new Process();
            var startInfo = new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = str,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            result = process.StandardOutput.ReadToEnd();


            return new TilangVariable(TypeSystem.STRING_DATATYPE , $"\"{result.Trim()}\"");
        }


        public static void PrintLn(List<TilangVariable> vars)
        {
            var str = "";

            vars.ForEach(x => str += x.Value.ToString());

            Console.WriteLine(ReformString(str));
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

        public static string ReformString(string str)
        {
            var result = "";
            str = str.Replace("'", "").Replace("\'" , "").Replace("\"","");

            for(int i=0; i<str.Length; i++)
            {
                if(str[i] != '\"' || str[i] != '\'')
                {
                    result += str[i];
                }
            }

            return result.Replace(Keywords.SINGLE_QUOET_RP, "\'").Replace(Keywords.DOUBLE_QUOET_RP, "\"");
        }
    }
}
