using Google.Cloud.Vision.V1;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseVisionJSON
{
    class Program
    {
        static void Main(string[] args)
        {
            //JObject o1 = JObject.Parse(File.ReadAllText(@"output-1.json"));
            //JArray oList = JArray.Parse(File.ReadAllText(@"output-1.json"));
            //JToken o = oList.ElementAt(0);
            //var data = o["responses"][0]["fullTextAnnotation"];
            //var page = data["pages"][0];
            //var blocks = page["blocks"]

            var o = Welcome.FromJson(File.ReadAllText(args[0]));
            var page = o.Responses[0].FullTextAnnotation.Pages[0];

            SortedDictionary<double, List<Word>> wordDict = new SortedDictionary<double, List<Word>>();
            foreach (var block in page.Blocks)
            {
                foreach (var paragraph in block.Paragraphs)
                {
                    foreach (var word in paragraph.Words)
                    {
                        var word_y = word.BoundingBox.NormalizedVertices[3].Y;
                        List<Word> wordList = null;
                        if (!wordDict.TryGetValue(word_y, out wordList))
                        {
                            wordList = new List<Word>();
                            wordDict[word_y] = wordList;
                            wordList.Add(word);
                        }
                        else
                        {
                            wordList.Add(word);
                        }
                    }
                }

            }

            Dictionary<int, List<Word>> linesDict = new Dictionary<int, List<Word>>();
            linesDict[0] = new List<Word>();
            //List<Word> lineWordList = new List<Word>();

            foreach (var word_y in wordDict.Keys)
            {
                var wordList = wordDict[word_y];
                //Console.WriteLine(word_y +"<->"+ wordList.Count);
                int linePos = linesDict.Keys.Count - 1;
                List<Word> lineWordList = linesDict[linePos];
                if(lineWordList.Count > 0)
                {
                    Word wordInCurrentLine = lineWordList.ElementAt(0);
                    Word wordToAdd = wordList.ElementAt(0);
                    if(wordToAdd.BoundingBox.NormalizedVertices[0].Y > wordInCurrentLine.BoundingBox.NormalizedVertices[3].Y)
                    {
                        linePos++;
                        linesDict[linePos] = wordList;

                    }
                    else
                    {
                        lineWordList.AddRange(wordList);
                    }
                     
                }
                else
                {
                    lineWordList.AddRange(wordList);
                }

                
            }

            foreach (var linePos in linesDict.Keys)
            {
                List<Word> listOfWords = linesDict[linePos];
                var orderedList = listOfWords.OrderBy(w => w.BoundingBox.NormalizedVertices[0].X);

                foreach (var lineWord in orderedList)
                {
                    foreach (var symbol in lineWord.Symbols)
                    {
                        Console.Write(symbol.Text);

                        if (symbol.Property != null && symbol.Property.DetectedBreak != null)
                        {
                            switch (symbol.Property.DetectedBreak.Type)
                            {
                                case TypeEnum.Space:
                                case TypeEnum.EolSureSpace:
                                case TypeEnum.LineBreak:
                                    Console.Write("\t");
                                    break;


                            }
                        }
                    }               
                }
                Console.WriteLine();
            }
            string s = "";
        }
    }
}
