using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OldChatCleanup
{
    class ChatFile
    {
        public static List<string> nameTags = new List<string>();
        public static List<Tuple<int, string, string>> ReadChatfile(string fileName, MainWindow mw)
        {
            List<Tuple<int, string, string>> annotatedChatLines = new List<Tuple<int, string, string>>();
            int lineCounter = 0;

            nameTags.Clear();

            List<string> FileLines = FixSplitLines(File.ReadAllText(fileName));

            foreach (string line in FileLines)
            {
                lineCounter++;
                string chatLine = Regex.Replace(line, @"\s+", string.Empty);



                if (line.Length > 1)
                {
                    int subCount = 30;
                    if(line.Length < 31)
                    {
                        subCount = line.Length / 2;
                    }
                    if (!line.Contains(@" -> "))
                    {
                        annotatedChatLines.Add(new Tuple<int, string, string>(lineCounter, line, GetHashString(chatLine.Substring(subCount))));
                    }
                }
                annotatedChatLines = CleanChatFile(annotatedChatLines);


            }
            nameTags = nameTags.Distinct().ToList();

            foreach(Tuple<int, string, string> nameLine in annotatedChatLines )
            {
                var nameMatch = Regex.Match(nameLine.Item2, @"^([ -\(\+-~]+: )");

                foreach (Capture match in nameMatch.Groups)
                {
                    if (match.Length > 2)
                    {
                        nameTags.Add(match.Value.Trim());
                    }
                }
            }
            //var nameMatch = Regex.Match(line, @"^([A-Za-z\~\s]+:\s)");
            nameTags = nameTags.Distinct().ToList();

            foreach (string name in nameTags)
            {
                mw.NameListBox.Items.Add(name);
            }

            List<Tuple<int, string, string>> splitChatLines = new List<Tuple<int, string, string>>();
            foreach (Tuple<int, string, string> changedLine in annotatedChatLines)
            {
                string splitLine = string.Empty;
                foreach (string name in nameTags)
                {
                    int indexOfName = changedLine.Item2.IndexOf(name);
                    if (indexOfName > 20)
                    {
                        splitLine = changedLine.Item2;
                        splitLine = splitLine.Insert(indexOfName, "<br>");
                        break;
                    }
                    else
                    {
                        splitLine = changedLine.Item2;
                    }
                }
                splitChatLines.Add(new Tuple<int, string, string>(changedLine.Item1, splitLine, changedLine.Item3));

            }
            annotatedChatLines = null;
            annotatedChatLines = splitChatLines;

            return annotatedChatLines;
        }

        private static List<string> FixSplitLines(string wholeTextFile)
        {
            List<string> splitText = new List<string>();
            string fixedTextFile = wholeTextFile.Replace("\r\n\r\n", "█");
            fixedTextFile = fixedTextFile.Replace("\r\n", " ");
            splitText = fixedTextFile.Split('█').ToList();
            return splitText;
        }

        public static List<Tuple<int, string, string>> CleanChatFile(List<Tuple<int, string, string>> annotatedChatLines)
        {
            int counter = 0;

            List<Tuple<int, string, string>> cleanedLines = new List<Tuple<int, string, string>>();
            List<string> validHashes = new List<string>();

            foreach (Tuple<int, string, string> lineToHash in annotatedChatLines)
            {

                validHashes.Add(lineToHash.Item3);
            }

            validHashes = validHashes.Distinct().ToList();

            foreach (Tuple<int, string, string> chatLine in annotatedChatLines.Distinct())
            {
                counter++;

                string currentLineHash = chatLine.Item3;
                foreach (Tuple<int, string, string> searchLine in annotatedChatLines)
                {

                    if (validHashes.Contains(searchLine.Item3))
                    {
                        cleanedLines.Add(searchLine);
                        validHashes.Remove(searchLine.Item3);
                    }
                }
            }
            return cleanedLines;
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder stringBuilderItem = new StringBuilder();
            foreach (byte nibble in GetHash(inputString))
            {
                stringBuilderItem.Append(nibble.ToString("X2"));
            }
            return stringBuilderItem.ToString();
        }

        public static void WriteChatFile(List<string> chatlines, string path, DateTime fileDate)
        {
            chatlines.Insert(0, "================================================================================");
            if (fileDate < (DateTime.Now.AddYears(-15)))
            {
                chatlines.Insert(0, ConfigurationManager.AppSettings["DateTagText"] + fileDate.ToLongDateString());
            }
            else
            {
                chatlines.Insert(0, ConfigurationManager.AppSettings["DateTagText"] + "Unknown");
            }
            File.WriteAllLines(path, chatlines);
        }
    }
}
