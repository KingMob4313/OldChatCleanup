using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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
        public static List<string> nameTagsWithColon = new List<string>();

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
                    nameTagsWithColon.Add(name + ":");
                }
            }

            List<Tuple<int, string, string>> splitChatLines = new List<Tuple<int, string, string>>();
            int lineCounterIncrement = 0;

            //Split lines where lines from different people end up on the same line
            foreach (Tuple<int, string, string> changedLine in annotatedChatLines)
            {
                int MaxLength = nameTags.Max(x => x.Length) + 1;
                string currentLine = changedLine.Item2;
                string splitLine = string.Empty;
                bool wasLineSplit = false;

                foreach (string name in nameTagsWithColon)
                {
                    int indexOfName = currentLine.IndexOf(name);
                    int nameCount = 0;
                    string tempSecondName = string.Empty;
                    foreach (string tempName in nameTagsWithColon)
                    {
                        MatchCollection nameMatches = Regex.Matches(currentLine, @"(" + Regex.Escape(tempName) + ")");
                        nameCount = nameMatches.Count + nameCount;
                        if (nameCount > 1)
                        {
                            tempSecondName = nameMatches[0].Value.ToString();
                            indexOfName = currentLine.IndexOf(name);
                            nameCount = 0;
                        }
                    }
                    if ((indexOfName > (MaxLength * 2) && currentLine.Length > 3))
                    {
                        //indexOfName = currentLine.IndexOf(tempSecondName);
                        string firstLine = currentLine.Substring(0, indexOfName);
                        string secondLine = currentLine.Substring(indexOfName, (currentLine.Length - indexOfName));
                        lineCounterIncrement++;

                        splitChatLines.Add(new Tuple<int, string, string>((changedLine.Item1 + (lineCounterIncrement - 2)), firstLine, GetChatLineHash(firstLine)));
                        splitChatLines.Add(new Tuple<int, string, string>((changedLine.Item1 + (lineCounterIncrement - 1)), secondLine, GetChatLineHash(secondLine)));
                        wasLineSplit = true;
                        break;
                    }
                    else
                    {
                        if (changedLine.Item2.Trim().StartsWith(name) || nameCount < 2)
                        {
                            splitChatLines.Add(new Tuple<int, string, string>((changedLine.Item1 + lineCounter), currentLine, GetChatLineHash(currentLine)));
                            break;
                            //splitLine = currentLine;
                        }
                    }

                }
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

                    //Fucking hell. Okay PM FORMAT is: 
                    //[{Sender}] -> [{Receiver}]: Message
                    //In order to fix this, I need to change name handling.
                    //1) I need to handle names without the colon
                    //2) Use the name with the colon to find first lines
                    //3) Use names with -> t find real PMs.
                    //If the line doesn't contain a PM, include it
                    //This is bugged
                    //This will get you lines that have '->' but aren't whispers 
                    //^[\w\s]+[:]{1}[\s\w\s:]+[-\>]+

                    if (line.Contains(@" -> ") && Regex.IsMatch(line, @"^[\w\s]+[:]{1}[\s\w\s:]+[-\>]+"))
                    {
                        currentChatLines.Add(new Tuple<int, string, string>(lineCounter, line, GetChatLineHash(chatLine)));
                    }
                    else if (!line.Contains(@" -> "))
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
            int subCount = chatLine.IndexOf(':');
            int endSpot = (((chatLine.Length / 6) < 6) ? chatLine.Length - 3 - subCount - 1 : ((chatLine.Length - subCount - 1) - (chatLine.Length / 7)));
            //if (line.Length < 35)
            //{
            //    subCount = line.Length / 2;
            //}
            if (subCount > 0 && (endSpot > subCount))
            {
                return GetHashString(chatLine.Substring(subCount, (endSpot)));
            }
            else
            {
                return string.Empty;
            }
        }

        private static List<string> GetNamesFromChat(List<Tuple<int, string, string>> annotatedChatLines)
        {
            foreach (Tuple<int, string, string> nameLine in annotatedChatLines)
            {
                var nameMatch = Regex.Match(nameLine.Item2, @"^([ -\)\+-~]+:)");

                foreach (Capture match in nameMatch.Groups)
                {
                    if (match.Length > 2)
                    {
                        int indexOfColon = 0;
                        indexOfColon = match.Value.Trim().IndexOf(':');
                        nameTags.Add(match.Value.Trim().Substring(0, (indexOfColon)));
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
            bool IsICQchat = wholeTextFile.StartsWith("ICQ Chat Session");
            if (!IsICQchat)
            {
                string fixedTextFile = wholeTextFile.Replace("\r\n\r\n", "█");
                fixedTextFile = fixedTextFile.Replace("\r\n", " ");
                splitText = fixedTextFile.Split('█').ToList();
            }
            else
            {
                string fixedTextFile = wholeTextFile.Replace("\r\n", "█");
                splitText = fixedTextFile.Split('█').ToList(); ;
            }
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
            chatlines.Insert(0, "<hr>" + "\r\n");

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

        public static DateTime ParseICQDate(string inputDate)
        {
            DateTime currentDate = new DateTime();

            currentDate = DateTime.ParseExact(inputDate.Substring(6, inputDate.Length - 6), "dddd, MMMM d, yyyy", CultureInfo.CreateSpecificCulture("en-US"));

            return currentDate;
        }
    }
}
