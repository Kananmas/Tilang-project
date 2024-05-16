﻿using System.Linq;
using Tilang_project.Engine.Creators;
using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Structs
{
    public class TilangStructs
    {
        public Dictionary<string, TilangVariable> Properties { get; set; } = new Dictionary<string, TilangVariable>();
        public List<TilangFunction> Functions { get; set; } = new List<TilangFunction>();
        

        private ExprAnalyzer ExprAnalyzer { get; set; } = new ExprAnalyzer();
        public string TypeName { get; set; } = "";


        private TilangFunction GetFunction(string fnDes)
        {
            var res = Functions.Where((item) => item.FuncDefinition == fnDes).FirstOrDefault();
            if (res == null) throw new Exception("no such item found");
            return res;
        }



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


        public TilangStructs ParseStructFromString(string value , Processor pros)
        {
            var result = new TilangStructs();
            result.TypeName =  this.TypeName;
            result.Functions.AddRange(this.Functions);
            var defStart = value.IndexOf('{')+1;
            var defLen = value.LastIndexOf('}') - defStart - 1;
            var content = value.Substring(defStart, defLen).Trim();

            // add properties that user speciefied
            new SyntaxAnalyzer().SeperateFunctionArgs(content).ForEach((item) =>
            {
                string[] splits = { item.Substring(0 , item.IndexOf("=")).Trim() , item.Substring(item.IndexOf("=")+1).Trim() };

                var key = splits[0].Trim();
                var value = ExprAnalyzer.ReadExpression(splits[1].Trim() , pros);

                if(value == null)
                {
                    value = TypeSystem.DefaultVariable(GetProperty(key).TypeName);
                }

                value.VariableName = key;

                if(Properties.ContainsKey(key))
                {
                    result.Properties.Add(key, value);
                    return;
                }
                

            });

            // adds the properties that are not specified by user;
            if(result.Properties.Count < this.Properties.Count)
            {
                var dif =  result.Properties.Count;

                for(int i = dif; i < this.Properties.Count ; i++)
                {
                    var target = this.Properties.Values.ToArray()[i];
                    var copy = target.GetCopy();
                    result.Properties.Add(copy.VariableName , copy);
                }
            }

            return result;
        }


        public TilangVariable? CallMethod(string methodName , 
            List<TilangVariable> methodArgs , 
            Processor? parentProcess = null)
        {
            var fn = this.Functions.Where((fn) => fn.FunctionName == methodName).FirstOrDefault();
            var newProcess = new Processor();
            if(parentProcess != null) newProcess.Stack = new VariableStack(parentProcess.Stack.GetVariableStack() 
                , parentProcess.Stack.GetFunctionStack());

            var fnList = new List<int>();
            var varList = new List<int>();
            
            this.Functions.ForEach(fn =>
            {
              var arg = newProcess.Stack.AddFunction(fn);
                fnList.Add(arg);
            });

            this.Properties.Values.ToList().ForEach(prop =>
            {
                var argIndex = newProcess.Stack.SetInStack(prop);
                varList.Add(argIndex);
            });

            newProcess.ScopeName = methodName;

            var target = GetFunction(FunctionCreator.CreateFunctionDef(methodName , methodArgs));
            var processResult =  newProcess.FunctionProcess(fn, methodArgs);


            newProcess.Stack.ClearFnStackByIndexes(fnList);
            newProcess.Stack.ClearStackByIndexes(varList);

            return processResult;
        }


        public TilangStructs GetCopy()
        {
            var result = new TilangStructs();

            result.TypeName = this.TypeName;
            result.Functions.AddRange(this.Functions); 
            
            foreach(var prop in Properties)
            {
                result.Properties.Add(prop.Key, prop.Value.GetCopy());   
            }


            return result;
        }
    }
}
