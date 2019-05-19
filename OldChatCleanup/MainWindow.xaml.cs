using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Path = System.IO.Path;

namespace OldChatCleanup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> NameListForForm = new List<string>();
        List<Tuple<int, string, string>> annotatedChatLines = null;
        public bool IsICQChatFile = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            annotatedChatLines = new List<Tuple<int, string, string>>();

            OpenFileDialog OFD = new OpenFileDialog();
            OFD.ShowDialog();
            var currentFileName = OFD.FileName;
            FileNameTextBox.Text = currentFileName;

            DateTime creation = File.GetCreationTime(currentFileName);
            DateTime fileModDate = File.GetLastWriteTime(currentFileName);

            annotatedChatLines = ChatFile.ProcessChatFile(OFD.FileName, this);

            if (annotatedChatLines[0].Item2.Contains("ICQ Chat Session"))
            {
                IsICQChatFile = true;
                annotatedChatLines.RemoveRange(0, 3);
                fileModDate = ChatFile.ParseICQDate(annotatedChatLines[0].Item2);
                annotatedChatLines.RemoveRange(0, 1);
            }
            else
            {
                IsICQChatFile = false;
            }

            List<string> chatLine = new List<string>();

            foreach (Tuple<int, string, string> line in annotatedChatLines)
            {
                string recheckedLine = line.Item2.Trim();
                chatLine.Add(recheckedLine + "\r\n");
            }

            string savePathAndFile = Path.GetDirectoryName(OFD.FileName) + "\\cleaned\\" + OFD.SafeFileName;
            ChatContentTextBox.Text = string.Join("", chatLine);

            List<string> HTMLLines = AddHTMLTags(chatLine, NameListForForm);
            ChatFile.WriteChatFile(HTMLLines, savePathAndFile, fileModDate);
        }

        private List<string> AddHTMLTags(List<string> chatLines, List<string> finalNameTags)
        {
            //List<string> nameTags = new List<string>();
            List<string> newHTMLLines = new List<string>();

            foreach (string htmlLines in chatLines)
            {
                string changedHTMLLine = string.Empty;
                foreach (string name in finalNameTags)
                {
                    int startIndex = 0;
                    startIndex = htmlLines.IndexOf(name);
                    string boldTag = "<span style=\"font-weight: bold; color:#000000; \">";
                    if (startIndex > -1 && startIndex < 3)
                    {
                        string tempHtmlLines = CheckDerps(htmlLines, name);
                        changedHTMLLine = tempHtmlLines.Insert(startIndex, boldTag);
                        changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length), "</span>");
                        changedHTMLLine = AddCharacterColors(changedHTMLLine, name, startIndex, boldTag);
                    }
                }
                //If not in the namelist
                if (changedHTMLLine.Length < 3)
                {
                    changedHTMLLine = htmlLines;
                }
                changedHTMLLine = ReservedCharacterChangePass(changedHTMLLine);
                newHTMLLines.Add(changedHTMLLine);
            }
            return newHTMLLines;
        }

        private string CheckDerps(string changedHTMLLine, string name)
        {
            MatchCollection wordMatches = Regex.Matches(changedHTMLLine, @"(\w+)");
            MatchCollection nameMatches = Regex.Matches(name, @"(\w+)");
            string derpLine = string.Empty;
            int counter = 1;
            if (changedHTMLLine.ToLower().StartsWith("gunny") || changedHTMLLine.ToLower().StartsWith("rabid de"))
            {
                Random random = new Random(DateTime.Now.Minute);
                foreach (Match currentWord in wordMatches)
                {
                    int roll = random.Next(0, 9);
                    if (counter > nameMatches.Count + 1)
                    {
                        if (roll < 2)
                        {
                            derpLine += currentWord.Value + " ";
                        }
                        else if (roll == 2)
                        {
                            derpLine += "derp ";
                        }
                        else if (roll == 3)
                        {
                            derpLine += "herp ";
                        }
                        else if (roll == 4)
                        {
                            derpLine += "herpity ";
                        }
                        else if (roll == 5)
                        {
                            derpLine += "Derp ";
                        }
                        else if (roll == 6)
                        {
                            derpLine += "herpa ";
                        }
                        else if (roll == 7)
                        {
                            derpLine += "derpy ";
                        }
                        else if (roll == 8)
                        {
                            derpLine += "herpy ";
                        }
                        else
                        {
                            derpLine += "DERP! ";
                        }
                        counter++;
                    }
                    else
                    {
                        derpLine += currentWord.Value + " ";
                        counter++;
                    }
                    
                }
                changedHTMLLine = derpLine + " ";
                return changedHTMLLine + "\r\n";
            }
            else
            {
                return changedHTMLLine + "\r\n";
            }
        }

        private static string AddCharacterColors(string changedHTMLLine, string name, int startIndex, string boldTag)
        {
            if (name.StartsWith("Kai C") || name.Contains("4313") || name.StartsWith("Kai T") || name.Contains("Oni"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["KaiColor"] + "; " + "font-family: 'Lucida Console', Monaco, monospace; " + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("Yara") || name.StartsWith("Tcu") || name.StartsWith("Ajde"))
            {
                changedHTMLLine = changedHTMLLine.Replace("Ajde:", "Yara:");
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["YaraColor"] + "; " + "font-family: Verdana, Geneva, sans-serif; " + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("Guylian McA") || name.StartsWith("Guyli") || name.StartsWith("Dragonfl"))
            {
                int nameChangedIndex = 0;
                if (name.StartsWith("Dragonfl"))
                {
                    nameChangedIndex = "Guylian:".Length;
                }
                else
                {
                    nameChangedIndex = name.Length;
                }
                changedHTMLLine = changedHTMLLine.Replace("Dragonfly:", "Guylian:");
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + nameChangedIndex + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["GuylianColor"] + "; " + "'Palatino Linotype', 'Book Antiqua', Palatino, serif; " + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("William Ja"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["WilliamColor"] + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("Lady Red") || name.StartsWith("LadyRedE") || name.StartsWith("Carissa T") || name.StartsWith("PrincessV"))
            {
                int nameChangedIndex = 0;
                if (name.StartsWith("PrincessV"))
                {
                    nameChangedIndex = "Carissa Tukov:".Length;
                }
                else
                {
                    nameChangedIndex = name.Length;
                }
                changedHTMLLine = changedHTMLLine.Replace("PrincessVamp:", "Carissa Tukov:");
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + nameChangedIndex + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["LREColor"] + ";" + "font-family: Goudy Old Style, Garamond, Big Caslon, Times New Roman, serif; font-size: 1.25em;>" + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("Morgan Pow"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["MorganColor"] + "; " + "font-family: Georgia, serif;" + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("Joshua May"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["JoshColor"] + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("Hawk:"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["HawkColor"] + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("Tycho Ev"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["TychoColor"] + "; " + "font-family: Tahoma, Geneva, sans-serif;" + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("Rori Wi"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"text-transform: uppercase; font-size: 0.85em; font-family:'" + ConfigurationManager.AppSettings["RoriSucks"] + "', cursive, sans-serif;" + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.ToLower().StartsWith("gunny") || name.ToLower().StartsWith("rabid de"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"text-transform: uppercase; font-size: 0.85em; font-family:'" + ConfigurationManager.AppSettings["RoriSucks"] + "', cursive, sans-serif;" + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:#111111; font-size: 0.925em" + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            return changedHTMLLine;
        }

        private string ReservedCharacterChangePass(string changedHTMLLine)
        {
            string tempLine = changedHTMLLine.Replace('*', '✳');
            return tempLine.Replace("~", "〰️");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
