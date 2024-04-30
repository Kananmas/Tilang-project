namespace Tilang_project.System_utils
{
    public static class SystemUtils
    {
        public static void Print(string text)
        {
            Console.WriteLine(text);
        }

        public static void Print(List<dynamic> list)
        {
            var text = "";
            var rawText = "";
            foreach (dynamic item in list)
            {
                rawText += item != null ? item.ToString():"";
            }

            for(int i = 0; i < rawText.Length; i++)
            {
                if (rawText[i] != '"')
                {
                    text += rawText[i];
                }
            }

            Print(text);
        }

        public static char KeyInput()
        {
            return Console.ReadKey().KeyChar;
        }

        public static string StringInput()
        {
            return Console.ReadLine();
        }
    }
}
