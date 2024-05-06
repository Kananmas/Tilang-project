namespace Tilang_project.Engine.Structs
{
    public class TilangStructs
    {
        public Dictionary<string, TilangType> Properties { get; set; } = new Dictionary<string, TilangType>();
        public List<TilangFunction> Functions { get; set; } = new List<TilangFunction>();

        public string TypeName { get; set; }



        public TilangType GetProperty(string name)
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
            var defStart = value.IndexOf('{');
            var defLen = value.IndexOf('}') - defStart - 1;
            var content = value.Substring(defStart, defLen).Trim();

            content.Split(',').Select((item) => item.Trim()).ToList().ForEach((item) =>
            {
                var splits = item.Split("=");

                var key = splits[0].Trim();
                var value = splits[1].Trim();


            });

            return result;
        }
    }
}
