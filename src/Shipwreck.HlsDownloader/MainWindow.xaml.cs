using System.Windows;

namespace Shipwreck.HlsDownloader
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            webView.Navigate(ViewModel.Url);
        }
    }
}