using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tilang_project.Engine.Structs
{
    public class TilangFunction
    {
        public List<TilangType> FunctionArguments { get; set; }
        public string Body { get; set; }
        public string FunctionName { get; set; }
        public string ReturnType { get; set; }

    }
}
