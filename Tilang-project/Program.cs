// See https://aka.ms/new-console-template for more informatiov
using System.Diagnostics;
using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Syntax.Analyzer.Syntax_analyzer;
using Tilang_project.Engine.Tilang_Pipeline;

// for-pc
var codeFile = File.ReadAllText("/home/kanan/Desktop/Projects/Tilang-project/Tilang-project/tests.ti");
// for-laptop

// var codeFile = File.ReadAllText("C:\\Users\\ASUS\\Desktop\\projects\\Tilang-project\\Tilang-project\\tests.ti");

Process currentProcess = Process.GetCurrentProcess();
Processor processor = new Processor();
SyntaxAnalyzer syntaxAnalyzer = new SyntaxAnalyzer();
var timer = Stopwatch.StartNew();
processor.Process(syntaxAnalyzer.GenerateTokens(codeFile));
timer.Stop();
long memoryUsed = currentProcess.WorkingSet64;


Console.WriteLine(timer.ToString());
Console.WriteLine(memoryUsed/(1024 * 1024));
// Console.ReadKey();