using System.Windows;

namespace MP3joiner
{
    /// <summary>
    /// Interaction logic for CompletionDialog.xaml
    /// </summary>
    public partial class CompletionDialog : Window
    {
        #region Public Constructors

        public CompletionDialog()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Private Methods

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion Private Methods
    }
}