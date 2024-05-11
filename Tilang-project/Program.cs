// See https://aka.ms/new-console-template for more informatiov



using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Syntax.Analyzer;

SyntaxAnalyzer analyzer = new SyntaxAnalyzer();
var codeFile = File.ReadAllText("C:\\Users\\ASUS\\Desktop\\projects\\Tilang-project\\Tilang-project\\code-sample.txt");


//var res = analyzer.GenerateTokens(codeFile);
//Processor processor = new Processor();

//processor.Process(res);


var ExprAn = new ExprAnalyzer();

var strList = new List<string>();

strList.Add("1+2+4!=4");

ExprAn.ReadExpression(strList);


Console.WriteLine();