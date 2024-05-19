using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Structs
{
    public class TilangVariable
    {
        public string OwnerScope;
        public string VariableName;
        public string  Tag = "Variable";
        public string TypeName { get; set; } = "";
        public dynamic Value { get; set; } = "";

        public TilangVariable() { }

        public TilangVariable(string typeName, dynamic value)
        {
            TypeName = typeName;
            Value = value;
        }

        public TilangVariable GetCopy()
        {
            var copy = new TilangVariable();    

            copy.OwnerScope = OwnerScope;
            copy.VariableName = VariableName;
            copy.Value = Value.GetType() == typeof(TilangStructs) ? Value.GetCopy():Value;
            copy.Tag = Tag;
            copy.TypeName = TypeName;


            return copy;
        }

        public TilangVariable GetSubproperty(List<string> keys, Processor processor)
        {
            if (Value.GetType() == typeof(TilangStructs))
            {
                var target = new TilangVariable();
                if(SyntaxAnalyzer.IsIndexer(keys[0])) {
                    var fnList = Value.InjectFnsToStack(processor);
                    var varList = Value.InjectVarsToStack(processor);

                    target = TilangArray.UseIndexer(keys[0], processor);

                    processor.Stack.ClearFnStackByIndexes(fnList);
                    processor.Stack.ClearStackByIndexes(varList);
                }
                else {
                    if(SyntaxAnalyzer.IsFunctionCall(keys[0])) {
                        var callTokens = SyntaxAnalyzer.TokenizeFunctionCall(keys[0]);
                        var args = TypeSystem.ParseFunctionArguments(
                            callTokens[1].Substring(1, callTokens[1].Length-2).Trim() , processor);
                        target = Value.CallMethod(callTokens[0],args ,processor);
                    }
                    else  target = Value.GetProperty(keys[0],processor);
                }
                if (keys.Count == 1)
                {
                    return target;
                }

                keys.RemoveAt(0);
                return target.GetSubproperty(keys,processor);

            }
            else
            {
                throw new Exception($"property of ${keys[0]} is not a structure");
            }

        }

        public void Assign(TilangVariable value , string op)
        {
            TilangVariable target = value;
            if (value.Tag == "Constant")
                throw new Exception("cannot assign value to constant");
            if(value.TypeName != this.TypeName)
            {
                if(this.TypeName == "string" || value.TypeName == "string")
                {
                    var newTarget = new TilangVariable("string" , "\"" + value.Value.ToString() + "\"") ;
                   target = newTarget;
                }

                else  throw new Exception("cannot assign two types two each other");
            }
            switch(op) 
            {
                case "+":
                case "+=":
                    Value += target.Value;
                    return;
                case "-":
                case "-=":
                    Value -= target.Value;
                    return;
                case "/":
                case "/=":
                    Value /= target.Value;
                    return;
                case "*":
                case "*=":
                    Value *= target.Value;
                    return;
                case "=":
                    Value = target.Value;
                    return;
            }
        }
    }
}
