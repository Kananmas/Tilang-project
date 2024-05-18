using System.Security.Cryptography.X509Certificates;
using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_TypeSystem;

namespace Tilang_project.Engine.Structs
{
    public class TilangArray
    {
        private List<TilangVariable> elements = new List<TilangVariable>();
        public string ElementType = "";

        public TilangVariable this[int i]
        {
            get
            {
                if (i >= 0 && i < elements.Count) return elements[i];
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (value.GetType() == typeof(TilangVariable))
                {
                    if (value.TypeName == ElementType)
                    {
                        elements[i].Assign(value, "=");
                    }

                    throw new InvalidDataException();
                }
                throw new Exception("invalid data");
            }
        }

        public void SetElements(List<TilangVariable> elements)
        {
            this.elements = elements;
        }


        public static TilangArray ParseArray(string arrayValue, Processor processor)
        {
            var syntaxAnalyzer = new SyntaxAnalyzer();
            var exprAnalyzer = new ExprAnalyzer();

            var result = new TilangArray();
            var elementType = TypeSystem.GetArrayType(arrayValue, null, processor);
            var elements = syntaxAnalyzer.SplitBySperatorToken(arrayValue.Substring(1, arrayValue.Length - 2)).Select((item) =>
            {
                item = item.Trim();
                if (TypeSystem.IsArray(item))
                {
                    return TypeSystem.ParseArray(item, processor);
                }
                if (TypeSystem.IsRawValue(item))
                {
                    return TypeSystem.ParseType(item, processor);
                }

                return exprAnalyzer.ReadExpression(item, processor);
            });
            result.SetElements(elements.ToList());
            result.ElementType = elementType;

            return result;
        }

        public static TilangVariable UseIndexer(string indexer, Processor processor, TilangVariable prev = null)
        {
            TilangVariable variable = prev;

            var parseIndexer = (string indexer) =>
            {
                var exprAnalyzer = new ExprAnalyzer();
                var ints = new List<int>();
                indexer = indexer.Replace("[", "#here[");
                var splits = indexer.Split("#here").Select((item) => item = item.Trim()).ToList();

                if (prev == null)
                {
                    if (SyntaxAnalyzer.IsFunctionCall(splits[0]))
                    {
                        variable = processor.ResolveFunctionCall(SyntaxAnalyzer.TokenizeFunctionCall(splits.Slice(0, 1)[0]));
                    }
                    else
                    {
                        variable = processor.Stack.GetFromStack(splits[0].Replace(" ", ""), processor);
                    }
                }

                splits.Skip(1).ToList().ForEach((item) =>
                {
                    item = item.Substring(1, item.Length - 2).Trim();
                    ints.Add(exprAnalyzer.ReadExpression(item, processor).Value);
                });

                return ints;
            };



            var getValue = (List<int> indexes) =>
            {
                var res = variable;

                for (int i = 0; i < indexes.Count; i++)
                {
                    if (res.TypeName == "string")
                    {
                        res = new TilangVariable("char", res.Value[indexes[i]]);
                        continue;
                    }
                    res = res.Value[indexes[i]];
                }

                return res;
            };

            var indexes = parseIndexer(indexer);

            return getValue(indexes);
        }
    }

}
