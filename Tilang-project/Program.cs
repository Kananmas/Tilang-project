// See https://aka.ms/new-console-template for more informatiov

using Tilang_project.Tilang_Interpertor;
using Tilang_project.Parser;

Parser parser = new Parser();
Intepretor lexicalTree = new Intepretor();

var codeFile = File.ReadAllText("C:\\Users\\ASUS\\Desktop\\projects\\Tilang-project\\Tilang-project\\code-sample.txt");


var lines = parser.LineSeparator(codeFile);
var tokenization = parser.LineByKeywords(lines);


var scope  = lexicalTree.GenerateCode(tokenization);

Console.WriteLine();