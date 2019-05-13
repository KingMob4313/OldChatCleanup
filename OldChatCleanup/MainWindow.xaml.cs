using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
                        changedHTMLLine = htmlLines.Insert(startIndex, boldTag);
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

        private static string AddCharacterColors(string changedHTMLLine, string name, int startIndex, string boldTag)
        {
            if (name.StartsWith("Kai C") || name.Contains("4313") || name.StartsWith("Kai T") || name.Contains("Oni"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["KaiColor"] + "; " + "font-family: 'Lucida Console', Monaco, monospace; " + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("Yara") || name.StartsWith("Tcu"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["YaraColor"] + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("William Ja"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["WilliamColor"] + "\">");
                changedHTMLLine = changedHTMLLine.Insert(changedHTMLLine.Length - 2, "</span>");
            }
            else if (name.StartsWith("Lady Red") || name.StartsWith("LadyRedE") || name.StartsWith("Carissa T"))
            {
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"color:" + ConfigurationManager.AppSettings["LREColor"] + "\">");
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
                changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length + "</span>".Length), "<span style=\"font-family:'" + ConfigurationManager.AppSettings["RoriSucks"] + "', cursive, sans-serif;" + "\">");
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
