using Tilang_project.ExpressionEvaluator;
using Tilang_project.Properties;
using Tilang_project.System_utils;
using Tilang_project.Tilang_TypeSystem;

namespace Tilang_project.Tailang_Scope
{
    public class Scope
    {
        public string ScopeName { get; set; } = "";
        public List<List<string>> Body { get; set; }
        public Dictionary<string, Property> Members = new Dictionary<string, Property>();
        public Dictionary<string, Scope> ChildScopes = new Dictionary<string, Scope>();
        public Scope? ParentScope { get; set; }

        public Property? this[string name]
        {
            get
            {
                if (Members.ContainsKey(name)) return Members[name];
                else
                {
                    if (ParentScope == null) throw new Exception($"no such item with name:{name} is defined");
                    else
                    {
                        return ParentScope[name];
                    }
                }
            }

            set
            {
                if (!Members.ContainsKey(name)) Members.Add(name, value);
                else
                {
                    throw new Exception($"property of {name} is already defined");
                }
            }
        }



        public void UpdateValue(string name, object newValue, string op)
        {
            var splits = name.Split('.');
            var prop = this[splits[0]];
            if (prop.Value.GetType() == typeof(DynamicObject))
            {
               if(splits.Length > 1) prop = prop.Value.GetProperty(name.Substring(name.IndexOf('.')+1)) ;
            }
            if (prop.PropType == "Constant")
            {
                throw new Exception("cannot assign to constant variable");
            }
            if (prop.Model.Name == newValue.GetType().Name)
            {
                switch (op)
                {
                    case "=":
                        prop.Value = newValue;
                        break;
                    case "+=":
                        prop.Value += newValue;
                        break;
                    case "-=":
                        prop.Value -= newValue;
                        break;
                    case "*=":
                        prop.Value *= newValue;
                        break;
                    case "/=":
                        prop.Value /= newValue;
                        break;
                }
            }
            else
            {
                throw new Exception("unable to assign type " + newValue.GetType().Name + " to " + prop.Value.GetType().Name);
            }
        }

        public dynamic CallFunction(string functionCall)
        {
            var expressionEval = new ExpressionEval();
            var start = functionCall.IndexOf("(");
            var len = functionCall.Length - start - 1;
            var functionName = functionCall.Substring(0, start);
            var args = ArguemntExpressionParser(functionCall.Substring(start + 1, len - 1))
                .Select((item) => expressionEval.ReadExpression(item.Trim(), this)).ToList();
            Scope child;
            ChildScopes.TryGetValue(functionName, out child);



            if (child != null)
            {
                bool injected = false;
                if (args.Count > 0 && !args.Contains(null))
                {
                    child.InjectArguments(args);
                    injected = true;
                }

                var res = expressionEval.ReadTheCode(child.Body, child);

                if (injected)
                {
                    child.RemoveArguments();
                }

                return res;
            }
            if (functionName.StartsWith("Sys"))
            {
                var types = functionName.Split(".");
                if (types[1] == "out")
                {
                    if (types[2] == "println") SystemUtils.Print(args);

                }
                if (types[1] == "in")
                {
                    if (types[2] == "getKey") return SystemUtils.KeyInput();
                    if (types[2] == "stringInput") return SystemUtils.StringInput(); 
                }

                return null;
            }
            else
            {
                throw new Exception("no such function exist");
            }

        }

        public void InjectArguments(List<dynamic> arguments)
        {
            int i = 0;
            foreach (var item in Members)
            {
                if (item.Value.PropType == "Argument")
                {
                    if (arguments[i].GetType().Name == item.Value.Value.GetType().Name)
                    {
                        item.Value.Value = arguments[i++];
                    }
                    else
                    {
                        throw new Exception("cannot convert " + arguments[i].GetType().Name + " to " + item.Value.Value.GetType().Name);
                    }
                }
            }
        }

        public void RemoveArguments()
        {
            foreach (var item in Members)
            {
                if (item.Value.PropType == "Argument")
                {
                    item.Value.Value = TypeSystem.GenerateDefaultValueByType(item.Value.Model.ToString());
                }
            }
        }

        private List<string> ArguemntExpressionParser(string str)
        {
            string ops = ",";
            var result = new List<string>();
            var val = "";
            bool inPranthesis = false;
            int prCount = 0;
            for (int i = 0; i < str.Length; i++)
            {
                var current = str[i];
                if (current != ' ')
                {
                    if (current == '(')
                    {
                        if (!inPranthesis) inPranthesis = true;
                        prCount++;
                    }
                    if (ops.IndexOf(current) != -1)
                    {
                        if (!inPranthesis)
                        {
                            if (val.Length > 0) result.Add(val);
                            val = "";
                        }
                        else
                        {
                            val += current;
                        }
                    }
                    else
                    {
                        val += current;
                    }
                    if (current == ')')
                    {
                        prCount--;
                        if (prCount == 0)
                        {
                            inPranthesis = false;
                        }
                    }

                }
            }
            if (val.Length > 0) result.Add(val);
            return result;
        }
    }
}