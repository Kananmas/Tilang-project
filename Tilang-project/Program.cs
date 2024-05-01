// See https://aka.ms/new-console-template for more informatiov

using Tilang_project.Tilang_Interpertor;
using Tilang_project.Parser;

Parser parser = new Parser();
Intepretor lexicalTree = new Intepretor();

var codeFile = File.ReadAllText("C:\\Users\\ASUS\\Desktop\\projects\\Tilang-project\\Tilang-project\\code-sample.txt");



var tokens = parser.GenerateLexicalTree(codeFile);
var scope  = lexicalTree.GenerateCode(tokens);

Console.WriteLine();