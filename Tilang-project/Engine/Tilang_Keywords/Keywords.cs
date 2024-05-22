namespace Tilang_project.Engine.Tilang_Keywords
{
    public static class Keywords
    {
        public static readonly List<string> AllOperators = "> < >= <= + - | / * *= /= += -= == != || && = & ! ? : %".Split(" ").ToList();
        public static readonly List<string> ArithmeticOperators = " + - / * *= /= += -=".Split(" ").ToList();
        public static readonly List<string> LogicalOperators = "> < >= <= == != || && ! ?".Split(" ").ToList();
        public static readonly List<string> AssignmentOperators = "+= -= /= *= =".Split(" ").ToList();
        public static readonly List<string> TwoSidedOperators = "&& ||".Split(" ").ToList();

        public const string VAR_KEYWORD = "var";
        public const string CONST_KEYWORD = "const";
        public const string SWITCH_KEYWORD = "switch";
        public const string WHILE_KEYWORD = "while";
        public const string CASE_KEYWORD = "case";
        public const string CONTINUE_KEYWORD = "continue";
        public const string BREAK_KEYWORD = "break";
        public const string FUNCTION_KEYWORD = "function";
        public const string FOR_KEYWORD = "for";
        public const string TYPE_KEYWORD = "type";
        public const string SYSTEM_KEYWORD = "Sys";
        public const string TRY_KEYWORD = "try";
        public const string CATCH_KEYWORD = "catch";
        public const string FINALLY_KEYWORD = "finally";
        public const string IF_KEYWORD = "if";
        public const string ELSE_KEYWORD = "else";
        public const string ELSE_IF_KEYWORD = "else if";
        public const string RETURN_KEYWORD = "return";


        public const string DOUBLE_QUOET_RP = "DOUBLE_QUOET";
        public const string SINGLE_QUOET_RP = "SINGLE_QUOET";


        public const string EQUAL_ASSIGNMENT = "=";
        public const string DOUBLE_DOT_TOKEN = ":";
        public const string ACCESSOR_TOKEN = ".";
        public const string COMMA_TOKEN = ",";


        public const string LEN_BG_FUNCTION = "len";
        public const string ADD_BG_FUNCTION = "add";
        public const string REMOVE_BG_FUNCTION = "remove";
        public const string TO_CHAR_BG_FUNCTION = "toCharArray";
        public const string TO_INT_BG_METHOD = "toInt";
        public const string TO_STRING_BG_METHOD = "toString";
        public const string TO_FLOAT_BG_METHOD = "toFloat";
        public const string GET_CHAR_CODE = "getCharCode";


        public static bool IsControlFlow(string text)
        {
            if (text.StartsWith(IF_KEYWORD) || 
                text.StartsWith(ELSE_IF_KEYWORD) || text.StartsWith(ELSE_KEYWORD)) { return true; }
            return false;
        }



        public static bool IsBackgroundFunction(string fnName)
        {
            return fnName == LEN_BG_FUNCTION || fnName == REMOVE_BG_FUNCTION 
                || fnName == TO_CHAR_BG_FUNCTION || fnName == ADD_BG_FUNCTION ||
                fnName == TO_INT_BG_METHOD || fnName == TO_FLOAT_BG_METHOD 
                || fnName == TO_CHAR_BG_FUNCTION || fnName == TO_STRING_BG_METHOD ||
                fnName == GET_CHAR_CODE;
        }


        public static readonly List<string> AllKeywords = new List<string>()
        {
            VAR_KEYWORD, CONST_KEYWORD, SWITCH_KEYWORD, CONTINUE_KEYWORD,
            BREAK_KEYWORD,FUNCTION_KEYWORD, FOR_KEYWORD, TYPE_KEYWORD,
            SYSTEM_KEYWORD, TRY_KEYWORD, CATCH_KEYWORD, FINALLY_KEYWORD,WHILE_KEYWORD,
            CASE_KEYWORD , IF_KEYWORD , ELSE_KEYWORD , ELSE_IF_KEYWORD
        };


        public static bool IsKeyword(string word)
        {
            return AllKeywords.Contains(word) || AllOperators.Contains(word);
        }

        public static bool IsBlocked(string word)
        {
            string[] blockKeywords = { TYPE_KEYWORD , TRY_KEYWORD , CATCH_KEYWORD , FINALLY_KEYWORD ,
                FUNCTION_KEYWORD , FOR_KEYWORD , WHILE_KEYWORD , ELSE_IF_KEYWORD , ELSE_KEYWORD , IF_KEYWORD  };

            word = word.Trim();

            for (int i = 0; i < blockKeywords.Length; i++)
            {
                var currentWord = blockKeywords[i];

                if (word.StartsWith(currentWord)) return true;
            }


            return false;
        }
    }
}
