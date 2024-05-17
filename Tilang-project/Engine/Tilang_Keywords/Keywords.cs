using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tilang_project.Engine.Tilang_Keywords
{
    public static class Keywords
    {
        public static readonly List<string> AllOperators = "> < >= <= + - / * *= /= += -= == != || && = & ! ? : %".Split(" ").ToList();
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

            for(int i = 0; i < blockKeywords.Length; i++)
            {
                var currentWord = blockKeywords[i];

                if(word.StartsWith(currentWord)) return true;
            }


            return false;
        }
    }
}
