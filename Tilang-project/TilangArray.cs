using Tilang_project.Properties;
using Tilang_project.Tilang_TypeSystem;

namespace Tilang_project
{
    public class TilangArray
    {
        public Type ArrayModel { get; set; }
        public List<Property> Elements { get; set; } = new List<Property>();

        public dynamic this[int index]
        {
            get { return Elements[index].Value; }
        }


        public Type GetType()
        {
            return ArrayModel;
        }

        public int Length { get { return Elements.Count; } }

        public void ParseElements(string values)
        {
            values.Substring(1, values.IndexOf("]") - 1).Trim()
            .Split(",").ToList().ForEach(item =>
            {
                AddElement(item);
            });
        }

        public void AddElement(string Value)
        {
            var value = TypeSystem.ExtractValueFromString(Value.Trim()) ;
            if (value.GetType() != ArrayModel)
            {
                throw new Exception($"cannot assign {value.GetType().Name} to {ArrayModel.Name}");
            }

            var propValue = new Property();

            propValue.Name = this.Length.ToString();
            propValue.Value = value;
            propValue.Model = ArrayModel;

            Elements.Add(propValue);
        }

        public void Remove(string Value)
        {
            var value = TypeSystem.ExtractValueFromString(Value.Trim());
            if (value.GetType() != ArrayModel)
            {
                throw new Exception($"cannot assign {value.GetType().Name} to {ArrayModel.Name}");
            }

            Elements.Remove(value);
        }


        public  static TilangArray CreateArray(string Type , string Values) {
            var result = new TilangArray();

            result.ArrayModel = TypeSystem.ConfigureType(Type);
            result.ParseElements(Values);

            return result;
        }


        public static TilangArray CreateArray(string Values)
        {
            var result = new TilangArray();
            var type = Values.Substring(1 , Values.Length - 2).Split(",").ToList()[0].Trim();

            result.ArrayModel = TypeSystem.ExtractValueFromString(type).GetType();
            result.ParseElements(Values);

            return result;
        }
    }
}
