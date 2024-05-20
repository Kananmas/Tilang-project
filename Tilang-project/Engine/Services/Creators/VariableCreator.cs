﻿using System.Diagnostics;
using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Structs;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Services.Creators
{
    public static class VariableCreator
    {
        public static TilangVariable CreateVariable(List<string> tokens, Processor precessor)
        {
            var indexOfEqual = tokens.IndexOf(Keywords.EQUAL_ASSIGNMENT);
            var exprAnalyzer = new ExprAnalyzer();

            var Type = tokens[1];
            var Tag = tokens[0];
            var Name = tokens[2];

            var result = TypeSystem.DefaultVariable(Type);

            result.Tag = Tag == Keywords.CONST_KEYWORD ? "Constant" : "Variable";

            result.VariableName = Name;


            if (indexOfEqual == -1)
            {
                return result;
            }

            result.Value = exprAnalyzer.ReadExpression(tokens[4], precessor).Value;

            return result;
        }
    }
}
