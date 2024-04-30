using Tilang_project.Properties;

namespace Tilang_project
{
    public class DynamicObject
    {
        public string TypeName;
        private Dictionary<string, Property> props = new Dictionary<string, Property>();

        public Dictionary<string , Property> GetProps()
        {
            return props;
        }
        public DynamicObject() { }
        public DynamicObject(Dictionary<string, Property> props)
        {
            this.props = props;
        }

        public void AddProperty(string key, Property value)
        {
            if (!props.ContainsKey(key))
            {
                props.Add(key, value);
                return;
            }

            throw new Exception("property aleardy exists for " + key);
        }
        public Property GetProperty(string key)
        {
            var keys = key.Split('.');
            Property prop = props[keys[0]];

            if(key.Length > 1)
            {
                for (int i = 1; i < keys.Length; i++)
                {
                    var currentKey = keys[i];

                    if (prop.Value.GetType() == this.GetType())
                    {
                        var next = prop.Value.GetProperty(currentKey);
                        prop = next;
                    }
                }
            }
            
            return prop;
        }

        public dynamic GetPropertyValue(string key)
        {
            return GetProperty(key).Value;
        }

        public void SetPropertyValue(string key, dynamic value)
        {
            var prop = GetProperty(key);
            if (prop.Model != value.GetType())
            {
                throw new Exception("the type " + value.GetType().ToString() +
                    " unassignable to " + prop.Model.ToString());
            }
            prop.Value = value;

        }
    }
}
