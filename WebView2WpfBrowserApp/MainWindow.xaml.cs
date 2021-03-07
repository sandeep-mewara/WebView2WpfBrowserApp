using System;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace WebView2WpfBrowserApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // NavigationEvents
            webView.NavigationStarting += WebView_NavigationStarting; ;
            webView.SourceChanged += WebView_SourceChanged;
            webView.ContentLoading += WebView_ContentLoading;
            webView.NavigationCompleted += WebView_NavigationCompleted;

            // Embedded at CoreWebView2 level
            InitializeOnceCoreWebView2Intialized();
        }

        /// <summary>
        /// initialization of CoreWebView2 is asynchronous.
        /// </summary>
        async private void InitializeOnceCoreWebView2Intialized()
        {
            await webView.EnsureCoreWebView2Async(null);

            // Hook other events
            webView.CoreWebView2.FrameNavigationStarting += CoreWebView2_FrameNavigationStarting;
            webView.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged;

            // For communication host to webview & vice versa
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.addEventListener(\'message\', event => alert(\'Message from App to WebView2 on navigation!\'));");
        }

        /// <summary>
        /// Web content in a WebView2 control may post a message to the host 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // Retrieve message from Webview2
            String uri = e.TryGetWebMessageAsString();
            addressBar.Text = uri;

            // Send message to Webview2
            webView.CoreWebView2.PostWebMessageAsString(uri);
            log.Content = $"Address bar updated ({uri}) based on WebView2 message!";
        }

        /// <summary>
        /// Execute URL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri uri = new Uri(addressBar.Text);

                if (webView != null && webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Navigate(uri.OriginalString);
                }
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Please enter correct format of url!");
            }
        }

        /// <summary>
        /// Allow only HTTPS calls
        /// WebView2 starts to navigate and the navigation results in a network request. 
        /// The host may disallow the request during the event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            String uri = e.Uri;
            if (!uri.StartsWith("https://"))
            {
                e.Cancel = true;
                //MessageBox.Show("Only HTTPS allowed!");

                // Inject JavaScript code into WebView2 controls at runtime
                webView.CoreWebView2.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link please.')");
            }
        }

        /// <summary>
        /// The source of WebView2 changes to a new URL. 
        /// The event may result from a navigation action that does not cause a network request such as a fragment navigation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebView_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            //MessageBox.Show($"Navigating to a new address: {((Microsoft.Web.WebView2.Wpf.WebView2)sender).Source}");
            //addressBar.Text = ((Microsoft.Web.WebView2.Wpf.WebView2)sender).Source.OriginalString;
        }

        /// <summary>
        /// WebView2 completes loading content on the new page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
         //   MessageBox.Show("Content loading complete!");
        }

        /// <summary>
        /// WebView starts loading content for the new page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebView_ContentLoading(object sender, CoreWebView2ContentLoadingEventArgs e)
        {
            //MessageBox.Show($"Loading new webpage as per request!");
        }

        /// <summary>
        /// navigation events inside subframes in a WebView2 instance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoreWebView2_HistoryChanged(object sender, object e)
        {
            //MessageBox.Show("Webview2 History changed!");
        }

        /// <summary>
        /// Navigation events inside subframes in a WebView2 instance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoreWebView2_FrameNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            //MessageBox.Show($"Navigation events inside subframes: {e.Uri}");
        }
    }
}
