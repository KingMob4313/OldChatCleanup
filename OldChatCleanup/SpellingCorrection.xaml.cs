using System.Windows;

namespace OldChatCleanup
{
    /// <summary>
    /// Interaction logic for SpellingCorrection.xaml
    /// </summary>
    public partial class SpellingCorrection : Window
    {
        public SpellingCorrection()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            DialogResult = true;

        }
    }
}
