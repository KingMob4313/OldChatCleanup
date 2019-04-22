using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static List<string> NameListForForm;
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

            

            annotatedChatLines = ChatFile.ReadChatfile(OFD.FileName, this);
            List<string> chatLine = new List<string>();

            foreach (Tuple<int, string, string> line in annotatedChatLines)
            {

                if (line.Item2.Contains("<br>"))
                {
                    Console.Beep();
                }
                string recheckedLine = line.Item2.Replace("<br>", "\r\n\r\n");
                chatLine.Add(recheckedLine + "\r\n");
            }

            string savePathAndFile = Path.GetDirectoryName(OFD.FileName) + "\\cleaned\\" + OFD.SafeFileName;
            ChatContentTextBox.Text = string.Join("", chatLine);

            //List<string> HTMLLines = AddHTMLTags(chatLine);
            ChatFile.WriteChatFile(chatLine, savePathAndFile, fileModDate);
        }

        private List<string> AddHTMLTags(List<string> chatLines)
        {
            List<string> nameTags = new List<string>();
            List<string> newHTMLLines = new List<string>();

            foreach (string singleLine in chatLines)
            {
                var nameMatch = Regex.Match(singleLine, @"^([A-Za-z\s]+:\s)");
                foreach (Capture match in nameMatch.Groups)
                {
                    if (match.Length > 2)
                    {
                        nameTags.Add(match.Value.Trim());
                    }
                }
            }
            nameTags = nameTags.Distinct().ToList();

            foreach (string htmlLines in chatLines)
            {
                string changedHTMLLine = string.Empty;
                foreach (string name in nameTags)
                {
                    int startIndex = 0;
                    startIndex = htmlLines.IndexOf(name);
                    string boldTag = "<span style=\"font-weight: bold;\">";
                    if (startIndex > -1)
                    {
                        changedHTMLLine = htmlLines.Insert(startIndex, boldTag);
                        changedHTMLLine = changedHTMLLine.Insert((startIndex + boldTag.Length + name.Length), "</span>");
                    }


                }
                newHTMLLines.Add(changedHTMLLine);
            }
            return newHTMLLines;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
