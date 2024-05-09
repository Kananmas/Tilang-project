using System.Linq;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Structs
{
    public class TilangStructs
    {
        public Dictionary<string, TilangVariable> Properties { get; set; } = new Dictionary<string, TilangVariable>();
        public List<TilangFunction> Functions { get; set; } = new List<TilangFunction>();

        public string TypeName { get; set; }



        public TilangVariable GetProperty(string name)
        {
            if (name.IndexOf(".") == -1)
            {
                return Properties.Where((prop) => prop.Key == name).FirstOrDefault().Value;
            }

            var argList = name.Split('.').ToList();
            var res = Properties.Where((prop) => prop.Key == argList[0]).FirstOrDefault().Value;
            argList.RemoveAt(0);
            return res.GetSubproperties(argList);

        }


        public TilangStructs ParseStructFromString(string value)
        {
            var result = new TilangStructs();
            result.TypeName =  this.TypeName;
            result.Functions.AddRange(this.Functions);
            var defStart = value.IndexOf('{');
            var defLen = value.IndexOf('}') - defStart - 1;
            var content = value.Substring(defStart, defLen).Trim();

            content.Split(',').Select((item) => item.Trim()).ToList().ForEach((item) =>
            {
                var splits = item.Split("=");

                var key = splits[0].Trim();
                var value = splits[1].Trim();

                if(Properties.ContainsKey(key))
                {
                    result.Properties.Add(key, TypeSystem.ParseType(value));
                }
                

            });

            return result;
        }


        public TilangStructs GetCopy()
        {
            var result = new TilangStructs();

            result.TypeName = this.TypeName;
            result.Functions.AddRange(this.Functions); 
            
            foreach(var prop in Properties)
            {
                result.Properties.Add(prop.Key, prop.Value);   
            }


            return result;
        }
    }
}
