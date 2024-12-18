﻿using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Services.Creators;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Structs
{
    public class TilangStructs
    {
        public Dictionary<string, TilangVariable> Properties { get; set; } = new Dictionary<string, TilangVariable>();
        public List<TilangFunction> Functions { get; set; } = new List<TilangFunction>();

        private Guid ObjectId { get; set; } = Guid.NewGuid();


        private ExprAnalyzer ExprAnalyzer { get; set; } = new ExprAnalyzer();
        public string TypeName { get; set; } = "";


        private TilangFunction GetFunction(string fnDes)
        {
            var start = fnDes.IndexOf('[') + 1;
            var len = fnDes.LastIndexOf(']') - start;
            var fnName = fnDes.Substring( start , len ).Trim();
            var res = Functions.Where((item) => item.FuncDefinition == fnDes || 
            (item.FunctionName == fnName)).FirstOrDefault();
            if (res == null) throw new Exception("no such item found");
            return res;
        }



        public TilangVariable GetProperty(string name, Processor processor)
        {
            if (name.StartsWith(Keywords.ACCESSOR_TOKEN))
            {
                name = name.Substring(1);
            }
            if (name.IndexOf(".") == -1)
            {
                return Properties.Where((prop) => prop.Key == name).FirstOrDefault().Value;
            }

            var argList = name.Split('.').Select(item => item.Trim()).Where((item) => item != "").ToList();
            var res = Properties.Where((prop) => prop.Key == argList[0]).FirstOrDefault().Value;
            argList.RemoveAt(0);
            return res.GetSubproperty(argList, processor);

        }

        public string ToString()
        {
            var keysStr = "";
            foreach (var kvp in this.Properties)
            {
                keysStr += kvp.Key + " = " + kvp.Value.Value.ToString() + Keywords.COMMA_TOKEN;
            }
            if (keysStr.Length > 0) keysStr = keysStr.Substring(0, keysStr.Length - 1).Trim();
            var result = this.TypeName + " { " + keysStr + " } ";

            return result;
        }

        public TilangStructs ParseStructFromString(string value, Processor pros)
        {
            var result = new TilangStructs();
            result.TypeName = this.TypeName;
            result.Functions.AddRange(this.Functions);

            var defStart = value.IndexOf('{') + 1;
            var defLen = value.LastIndexOf('}') - defStart - 1;
            if (!value.EndsWith(" }")) defLen += 1;
            var content = value.Substring(defStart, defLen).Trim();

            // add properties that user speciefied
            SyntaxAnalyzer.SplitBySperatorToken(content).ForEach((item) =>
            {
                string[] splits = { item.Substring(0, item.IndexOf(Keywords.EQUAL_ASSIGNMENT))
                    .Trim(), item.Substring(item.IndexOf(Keywords.EQUAL_ASSIGNMENT) + 1).Trim() };
                var key = splits[0].Trim();
                var value = ExprAnalyzer.ReadExpression(splits[1].Trim(), pros);

                if (value == null)
                {
                    value = TypeSystem.DefaultVariable(GetProperty(key, pros).TypeName);
                }

                value.VariableName = key;
                value.OwnerId = ObjectId;

                if (Properties.ContainsKey(key))
                {
                    result.Properties.Add(key, value);
                    return;
                }


            });

            // adds the properties that are not specified by user;
            if (result.Properties.Count < this.Properties.Count)
            {
                var dif = result.Properties.Count;

                for (int i = dif; i < this.Properties.Count; i++)
                {
                    var target = this.Properties.Values.ToArray()[i];
                    var copy = target.GetCopy();
                    result.Properties.Add(copy.VariableName, copy);
                }
            }

            return result;
        }


        public TilangVariable? CallMethod(string methodName,
            List<TilangVariable> methodArgs,
            Processor? parentProcess = null)
        {
            var fn = this.Functions.Where((fn) => fn.FunctionName == methodName).FirstOrDefault();
            var newProcess = new Processor();
            if (parentProcess != null) newProcess.Stack = new ProcessorStack(parentProcess);

            InjectFnsToStack(newProcess);
            InjectVarsToStack(newProcess);

            newProcess.ScopeType = "function";

            var target = GetFunction(FunctionCreator.CreateFunctionDef(methodName, methodArgs));
            var processResult = newProcess.FunctionProcess(fn, methodArgs);

            newProcess.Stack.GrabageCollection(this.ObjectId);

            return processResult;
        }

        public List<int> InjectVarsToStack(Processor processor)
        {
            var varList = new List<int>();

            this.Properties.Values.ToList().ForEach(prop =>
            {
                var copy = prop.GetCopy();
                copy.VariableName = prop.VariableName;
                copy.OwnerId = ObjectId;
                var argIndex = processor.Stack.SetInStack(prop);
                varList.Add(argIndex);
            });

            return varList;
        }

        public List<int> InjectFnsToStack(Processor processor)
        {
            var fnList = new List<int>();

            this.Functions.ForEach(fn =>
           {
               var copy = fn.GetCopy();
               copy.OwnerScope = ObjectId;
               copy.FuncDefinition = fn.FuncDefinition;
               var arg = processor.Stack.AddFunction(copy);
               fnList.Add(arg);
           });


            return fnList;
        }


        public TilangStructs GetCopy()
        {
            var result = new TilangStructs();

            result.TypeName = this.TypeName;
            result.Functions.AddRange(this.Functions);

            foreach (var prop in Properties)
            {
                result.Properties.Add(prop.Key, prop.Value.GetCopy());
            }


            return result;
        }
    }
}
