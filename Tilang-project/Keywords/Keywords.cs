
namespace Tilang_project.Keywords
{
    public class TiLangKeywords
    {
        public string[] keywords = {
            "function",
            "int",
            "float",
            "string",
            "char",
            ":",
            "return",
            "var",
            "const",
            "if",
            "else",
            "else if",
            "switch",
        };

     
        public List<string> CustomDataTypes = new List<string>();


        public string this[int index]
        {
            get
            {
                if (index >= 0 && index < keywords.Length)
                    return keywords[index];
                else throw new IndexOutOfRangeException();
            }
        }


        public bool IsKeyword(string word, out int wordIndex)
        {
            for (int i = 0; i < keywords.Length; i++)
            {
                wordIndex = i;
                if (keywords[i] == word) return true;
            }


            wordIndex = -1;
            return false;
        }
    }
}
