﻿using Gecko;
using Gecko.Events;
using Gecko.Net;
using Gecko.Observers;
using Shipwreck.HlsDownloader.Properties;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Forms.Integration;

namespace Shipwreck.HlsDownloader
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private GeckoWebBrowser _Browser;
        public MainWindow()
        {
            InitializeComponent();
            Xpcom.Initialize("Firefox");
        }

        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var host = new WindowsFormsHost();
            _Browser = new GeckoWebBrowser();
            _Browser.Navigating += _Browser_Navigating;
            _Browser.Navigated += _Browser_Navigated;
            ViewModel.AddObserver();

            host.Child = _Browser;
            webViewParent.Children.Add(host);
            if (!string.IsNullOrEmpty(ViewModel?.Url))
            {
                _Browser.Navigate(ViewModel.Url);
            }
        }

        private void _Browser_Navigating(object sender, GeckoNavigatingEventArgs e)
        {
            ViewModel.OnFrameLoadStart(_Browser.Url.ToString());
        }

        private void _Browser_Navigated(object sender, GeckoNavigatedEventArgs e)
        {
            ViewModel.OnFrameLoadEnd(_Browser.Url.ToString());
        }

 

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Requests.Clear();

            var sd = Settings.Default;
            sd.CurrentUrl = ViewModel.Url;
            sd.Save();
            _Browser.Navigate(ViewModel.Url);
        }
    }
}