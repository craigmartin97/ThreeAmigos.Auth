using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace ThAmCo.Auth.Helpers
{
    public class RandomWordsGenerator
    {
        private IList<string> words = new List<string>();
        private readonly Random random = new Random();

        public RandomWordsGenerator(string url)
        {
            GetWords(url);
        }

        private void GetWords(string url)
        {
            using (var wc = new WebClient())
            {
                string file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Xml/temp.txt";
                wc.DownloadFile(url, file);
                using (StreamReader streamReader = new StreamReader(file))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        words.Add(line);
                    }
                }
            }
        }

        public string GetWord()
        {
            string word = words[random.Next(words.Count)].ToLower().Trim();
            return Regex.Replace(word, @"\s+", ""); //remove whitespace
        }
    }
}
