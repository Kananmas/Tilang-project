// See https://aka.ms/new-console-template for more informatiov
using System.Diagnostics;
using Tilang_project.Engine.Tilang_Pipeline;

// for-pc
//var codeFile = File.ReadAllText("C:\\Users\\Kanan\\Desktop\\Projects\\personal\\Tilang-project\\Tilang-project\\code-sample.ti");
// for-laptop

var codeFile = File.ReadAllText("C:\\Users\\ASUS\\Desktop\\projects\\Tilang-project\\Tilang-project\\tests.ti");

Process currentProcess = Process.GetCurrentProcess();
var timer = Stopwatch.StartNew();
Pipeline.StartNew(codeFile);
timer.Stop();
long memoryUsed = currentProcess.WorkingSet64;


Console.WriteLine(timer.ToString());
Console.WriteLine(memoryUsed/(1024 * 1024));