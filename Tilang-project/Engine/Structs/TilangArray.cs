using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tilang_project.Engine.Structs
{
    public class TilangArray
    {
        private List<TilangVariable> elements = new List<TilangVariable>();
        public string ElementType = "";


        public static TilangArray ParseArray(string arrayValue , string arrayType)
        {
           var declTokens = arrayType.Replace(" " , "").Replace("[", " [").Split("");

            return new TilangArray();
        }
    }

}
