using Tilang_project.Engine.Processors;
using Tilang_project.Engine.Services.BoxingOps;
using Tilang_project.Engine.Syntax.Analyzer;
using Tilang_project.Engine.Tilang_Keywords;
using Tilang_project.Engine.Tilang_TypeSystem;
using Tilang_project.Utils.String_Extentions;

namespace Tilang_project.Engine.Structs
{
    public class TilangArray
    {
        private List<TilangVariable> elements = new List<TilangVariable>();
        public string ElementType  = "";

        public int Length { get { return elements.Count; } }

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
                        elements[i].Assign(value, Keywords.EQUAL_ASSIGNMENT);
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


        public void Add(TilangVariable item)
        {
            if(item.TypeName == ElementType)
            {
                elements.Add(item.GetCopy());
            }
        }

        public void Remove(int i)
        {
            elements.RemoveAt(i);
        }


        public int IndexOf(TilangVariable var)
        { 
            int i = 0;
            foreach (TilangVariable item in elements)
            {
                if(item.Value.Equals(var.Value))
                {
                    return i;
                }
                i++;
            }

            return -1;
        }

        public  void Remove(TilangVariable item)
        {
            var indexOf = IndexOf(item);

            if (indexOf >= 0)
            {
                elements.RemoveAt(indexOf);
            }
        }

        public TilangArray GetCopy()
        {
            var result = new TilangArray();

            foreach(var item in elements)
            {
                result.Add(item.GetCopy());
            }

            return result;
        }

        public static TilangArray ParseArray(string arrayValue, Processor processor)
        {
            var syntaxAnalyzer = new SyntaxAnalyzer();
            var exprAnalyzer = new ExprAnalyzer();

            var result = new TilangArray();
            var elementType = TypeSystem.GetArrayType(arrayValue, null , processor);
            var elements = syntaxAnalyzer.SplitBySperatorToken(arrayValue.GetStringContent()).Select((item) =>
            {
                item = item.Trim();
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

        public static TilangVariable UseIndexer(string indexer, Processor processor, TilangVariable? prev = null)
        {
            TilangVariable? variable = prev;

            var parseIndexer = (string indexer) =>
            {
                var exprAnalyzer = new ExprAnalyzer();
                var ints = new List<int>();
                var splits = SyntaxAnalyzer.SeperateByBrackeyes(indexer);

                if (prev == null)
                {
                    if (SyntaxAnalyzer.IsFunctionCall(splits[0]))
                    {
                        variable = processor.ResolveFunctionCall
                        (SyntaxAnalyzer.TokenizeFunctionCall(splits[0]));
                    }
                    else if (TypeSystem.IsRawValue(splits[0]))
                    {
                        variable = TypeSystem.ParseType(splits[0], processor);
                    }
                    else
                    {
                        variable = processor.Stack.GetFromStack(splits[0].Replace(" ", ""), processor);
                    }
                }

                splits.Skip(1).ToList().ForEach((item) =>
                {
                    item = item.Substring(1, item.Length - 2).Trim();
                    ints.Add(UnBoxer.UnboxInt(exprAnalyzer.ReadExpression(item, processor)));
                });

                return ints;
            };



            var getValue = (List<int> indexes) =>
            {
                var res = variable;

                for (int i = 0; i < indexes.Count; i++)
                {
                    int currentIndex = indexes[i];
                    if (res.TypeName == "string")
                    {
                        var unboxed = UnBoxer.UnboxString(res);
                        var character = unboxed[currentIndex];
                        res = new TilangVariable("char", $"\'{character}\'");
                        continue;
                    }
                    res = UnBoxer.UnboxArray(res)[currentIndex];
                }

                return res;
            };

            var indexes = parseIndexer(indexer);

            return getValue(indexes);
        }
    }

}
