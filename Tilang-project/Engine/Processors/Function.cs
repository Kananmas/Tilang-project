
using Tilang_project.Engine.Services.BoxingOps;
using Tilang_project.Engine.Services.Creators;
using Tilang_project.Engine.Stack;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_Pipeline;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.Background_Functions;
using Tilang_project.Utils.String_Extentions;
using Tilang_project.Utils.Tilang_Console;

namespace Tilang_project.Engine.Processors
{
    public partial class Processor
    {
        public static TilangVariable? HandleSysCall(string val, List<TilangVariable> varialables)
        {
            var tokens = val.Split('.');
            if (tokens[1] == "out")
            {
                switch (tokens[2])
                {
                    case "println":
                        Tilang_System.PrintLn(varialables);
                        return null;
                    case "print":
                        Tilang_System.Print(varialables);
                        return null;
                }
            }
            if (tokens[1] == "in")
            {
                switch (tokens[2])
                {
                    case "getKey":
                        return Tilang_System.GetKey();
                    case "getLine":
                        return Tilang_System.GetLine();
                }
            }
            return null;
        }

        public TilangVariable? FunctionProcess(TilangFunction fn, List<TilangVariable> argValues)
        {
            var newStack = new ProcessorStack(this);
            fn.InjectFunctionArguments(this, argValues);
            var newProcess = new Processor();
            newProcess.Stack = newStack;
            newProcess.ScopeType = "function";
            newProcess.ParentProcessor = this;

            var res = Pipeline.StartNew(fn.Body.GetStringContent(), newProcess);
            newProcess.Stack.GrabageCollection(newProcess.scopeId);

            return res;
        }


        public TilangVariable? ResolveFunctionCall(List<string> tokens)
        {
            var fnName = tokens[0];
            var fnArgs = TypeSystem.ParseFunctionArguments(tokens[1], this);

            if (IsMethodCall(fnName + tokens[1]))
            {
                var fullStr = fnName + tokens[1];
                var objName = fullStr.Substring(0, fullStr.LastIndexOf("."));

                return UnBoxer.UnboxStruct(Stack.GetFromStack(objName, this))
                    .CallMethod(fnName.Substring(fnName.LastIndexOf(".") + 1).Trim(), fnArgs, this);
            }

            if (IsSystemCall(fnName))
            {
                return HandleSysCall(fnName, fnArgs);
            }

            if (Keywords.IsBackgroundFunction(fnName))
            {
                return BackgroundFunctions.CallBackgroundFunctions(fnName, fnArgs);
            }

            var calledFn = Stack.GetFunction(FunctionCreator.CreateFunctionDef(fnName, fnArgs));
            return FunctionProcess(calledFn, fnArgs);
        }
    }
}