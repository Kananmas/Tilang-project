// See https://aka.ms/new-console-template for more informatiov

using Tilang_project.LexicalTree;
using Tilang_project.Parser;

Parser parser = new Parser();
LexicalTree lexicalTree = new LexicalTree();

var codeFile = File.ReadAllText("C:\\Users\\ASUS\\Desktop\\projects\\Tilang-project\\Tilang-project\\code-sample.txt");


var lines = parser.LineSeparator(codeFile);
var tokenization = parser.LineByKeywords(lines);


var scope  = lexicalTree.GenerateScope(tokenization);

Console.WriteLine();