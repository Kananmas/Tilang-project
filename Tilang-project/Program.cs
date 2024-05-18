// See https://aka.ms/new-console-template for more informatiov
using System.Diagnostics;
using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;

SyntaxAnalyzer analyzer = new SyntaxAnalyzer();
Processor processor = new Processor();

// for-pc
var codeFile = File.ReadAllText("C:\\Users\\Kanan\\Desktop\\Projects\\personal\\Tilang-project\\Tilang-project\\code-sample.ti");
// for-laptop
// var codeFile = File.ReadAllText("C:\\Users\\ASUS\\Desktop\\projects\\Tilang-project\\Tilang-project\\code-sample.ti");

var timer = Stopwatch.StartNew();
var res = analyzer.GenerateTokens(codeFile);
processor.Process(res);
timer.Stop();

Console.WriteLine(timer.ToString());