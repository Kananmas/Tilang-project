// See https://aka.ms/new-console-template for more informatiov



using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Syntax.Analyzer;

SyntaxAnalyzer analyzer = new SyntaxAnalyzer();
var codeFile = File.ReadAllText("C:\\Users\\ASUS\\Desktop\\projects\\Tilang-project\\Tilang-project\\code-sample.txt");


var res = analyzer.GenerateTokens(codeFile);
var expr = new ExprAnalyzer();


Processor processor = new Processor();

processor.Process(res);


Console.WriteLine();