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
        public static List<Tuple<int, string, string>> ProcessChatFile(string fileName, MainWindow mw)
        {
            List<Tuple<int, string, string>> annotatedChatLines = new List<Tuple<int, string, string>>();
            int lineCounter = 0;
            nameTags.Clear();

            //Fix word/line wrapped lines by joining them to previous
            List<string> FileLines = FixSplitLines(File.ReadAllText(fileName));

            //Process File Lines into Tuple with Line Count, String, Hash
            annotatedChatLines = PushLinesIntoTuple(lineCounter, FileLines);

            //Get names from chat lines
            nameTags = GetNamesFromChat(annotatedChatLines);
            mw.NameListBox.Items.Clear();
            mw.NameListForForm.Clear();
            //add those lines to the control box
            foreach (string name in nameTags)
            {
                if (name.Length > 4)
                {
                    mw.NameListBox.Items.Add(name);
                    mw.NameListForForm.Add(name);
                }
            }
            
            List<Tuple<int, string, string>> splitChatLines = new List<Tuple<int, string, string>>();
            int lineCounterIncrement = 0;
            //Split lines where lines from different people end up on the same line
            foreach (Tuple<int, string, string> changedLine in annotatedChatLines)
            {
                
                string splitLine = string.Empty;
                foreach (string name in nameTags)
                {
                    int indexOfName = changedLine.Item2.IndexOf(name);
                    if (indexOfName > 20 && splitLine.Length > 3)
                    {
                        string firstLine;
                        lineCounterIncrement++;
                        firstLine = splitLine.Substring(0, indexOfName);
                        splitLine = splitLine.Substring(indexOfName);
                        splitChatLines.Add(new Tuple<int, string, string>((changedLine.Item1 + (lineCounterIncrement - 1)), firstLine, GetChatLineHash(firstLine)));
                        break;
                    }
                    else
                    {
                        splitLine = changedLine.Item2;
                    }
                }
                splitChatLines.Add(new Tuple<int, string, string>((changedLine.Item1 + lineCounterIncrement), splitLine, GetChatLineHash(splitLine)));
            }
            annotatedChatLines = null;
            
            //One more cleanup pass
            annotatedChatLines = CleanChatFile(splitChatLines);
            return annotatedChatLines;
        }

        private static List<Tuple<int, string, string>> PushLinesIntoTuple(int lineCounter, List<string> FileLines)
        {
            List<Tuple<int, string, string>> currentChatLines = new List<Tuple<int, string, string>>();
            foreach (string line in FileLines)
            {
                lineCounter++;
                string chatLine = Regex.Replace(line, @"\s+", string.Empty);

                if (line.Length > 1)
                {
                    //Doing some hash weirdness to only include the meat of the chat line
                    int subCount = 25;
                    if (line.Length < 35)
                    {
                        subCount = line.Length / 2;
                    }
                    //If the line doesn't contain a PM, include it
                    if (!line.Contains(@" -> "))
                    {
                        currentChatLines.Add(new Tuple<int, string, string>(lineCounter, line, GetChatLineHash(chatLine)));
                    }
                }
                currentChatLines = CleanChatFile(currentChatLines);
            }
            return currentChatLines;
        }

        private static string GetChatLineHash(string line)
        {
            string chatLine = Regex.Replace(line, @"\s+", string.Empty);

            //Doing some hash weirdness to only include the meat of the chat line
            int subCount = 25;
            if (line.Length < 35)
            {
                subCount = line.Length / 2;
            }
            return GetHashString(chatLine.Substring(subCount));
        }

        private static List<string> GetNamesFromChat(List<Tuple<int, string, string>> annotatedChatLines)
        {
            foreach (Tuple<int, string, string> nameLine in annotatedChatLines)
            {
                var nameMatch = Regex.Match(nameLine.Item2, @"^([ -\(\+-~]+: )");

                foreach (Capture match in nameMatch.Groups)
                {
                    if (match.Length > 2)
                    {
                        int indexOfColon = 0;
                        indexOfColon = match.Value.Trim().IndexOf(':');
                        nameTags.Add(match.Value.Trim().Substring(0, (indexOfColon + 1)));
                    }
                }
            }
            //var nameMatch = Regex.Match(line, @"^([A-Za-z\~\s]+:\s)");
            nameTags = nameTags.Distinct().ToList();

            return nameTags;
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
            List<string> CompletedChatLines = CreateChatHeader(chatlines, fileDate);
            File.WriteAllLines(path, CompletedChatLines);
        }

        private static List<string> CreateChatHeader(List<string> chatlines, DateTime fileDate)
        {
            chatlines.Insert(0, "================================================================================" + "\r\n");

            if (nameTags.Count > 0)
            {
                chatlines.Insert(0, "==Characters in this scene==");
                foreach (string character in nameTags)
                {
                    chatlines.Insert(1, character);
                }

            }
            if (fileDate < (DateTime.Now.AddYears(-15)))
            {
                chatlines.Insert(0, ConfigurationManager.AppSettings["DateTagText"] + fileDate.ToLongDateString() + "\r\n");
            }
            else
            {
                chatlines.Insert(0, ConfigurationManager.AppSettings["DateTagText"] + "Unknown" + "\r\n");
            }

            return chatlines;
        }
    }
}
