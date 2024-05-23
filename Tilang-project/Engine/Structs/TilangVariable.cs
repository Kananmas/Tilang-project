using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Services.BoxingOps;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Structs
{
    public class TilangVariable
    {
        public Guid OwnerId;
        public string VariableName;
        public string  Tag = "Variable";
        public string TypeName { get; set; } = "";
        public object Value { get; set; } = "";

        public TilangVariable() { }

        public TilangVariable(string typeName, object value)
        {
            TypeName = typeName;
            Value = value;
        }

        public TilangVariable GetCopy()
        {
            var copy = new TilangVariable();    
            copy.Value = Value.GetType() == typeof(TilangStructs) ?
                ((TilangStructs) Value).GetCopy():Value;
            copy.Tag = Tag;
            copy.TypeName = TypeName;
            copy.VariableName = VariableName;

            return copy;
        }

        public TilangVariable GetSubproperty(List<string> keys, Processor processor)
        {
            if (Value.GetType() == typeof(TilangStructs))
            {
                var target = new TilangVariable();
                if(SyntaxAnalyzer.IsIndexer(keys[0])) {
                    var fnList = ((TilangStructs)Value).InjectFnsToStack(processor);
                    var varList = ((TilangStructs)Value).InjectVarsToStack(processor);

                    target = TilangArray.UseIndexer(keys[0], processor);

                    processor.Stack.ClearFunctions(fnList);
                    processor.Stack.ClearVariables(varList);
                }
                else {
                    if(SyntaxAnalyzer.IsFunctionCall(keys[0])) {
                        var callTokens = SyntaxAnalyzer.TokenizeFunctionCall(keys[0]);
                        var args = TypeSystem.ParseFunctionArguments(
                            callTokens[1].Substring(1, callTokens[1].Length-2).Trim() , processor);
                        target = ((TilangStructs)Value).CallMethod(callTokens[0],args ,processor);
                    }
                    else  target = ((TilangStructs)Value).GetProperty(keys[0],processor);
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
            if(value.TypeName != this.TypeName && !TypeSystem.AreTypesCastable(TypeName , target.TypeName))
            {
                if(this.TypeName == "string" || value.TypeName == "string" && op != "=" )
                {
                    var newTarget = new TilangVariable("string" , "\"" + value.Value.ToString() + "\"") ;
                    if(target.TypeName == "string" && TypeName != "string")
                    {
                        this.TypeName = "string";
                    }
                   target = newTarget;
                }

                else  throw new Exception("cannot assign two types two each other");
            }
            switch(op) 
            {
                case "+":
                case "+=":
                    Value = Boxer.BoxingSum(this , target);
                    return;
                case "-":
                case "-=":
                    Value = Boxer.BoxingSub(this , target);
                    return;
                case "/":
                case "/=":
                    Value = Boxer.BoxingDiv(this, target);
                    return;
                case "*":
                case "*=":
                    Value = Boxer.BoxingMulti(this , target);
                    return;
                case "=":
                    Value = target.Value;
                    return;
            }
        }
    }
}
