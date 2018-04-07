using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using WebHandler;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Globalization;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace TwitR
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        // WebView native object must be inserted in the OnNavigationStarting event handler
        private WebComponent webComponent = new WebComponent();
        public Home()
        {
            this.InitializeComponent();            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Back button
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            
            base.OnNavigatedTo(e);
            string src = "https://mobile.twitter.com/";            
            this.webView.Navigate(new Uri(src));

            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //base.OnNavigatedFrom(e);
            //SOF 43189833/backrequested-is-triggering-more-than-once-in-uwp-app
            SystemNavigationManager.GetForCurrentView().BackRequested -= MainPage_BackRequested;
        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            //if (this.Frame.CanGoBack) this.Frame.GoBack();
            if (this.webView.CanGoBack)
            {                
                this.webView.GoBack();
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }
       

        private void webView_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            if (this.webView.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
            string url = "";
            try
            {
                url = args.Uri.ToString();
                // not beautiful way to highlight the navigation view icons according to URL 
                if (url == "https://mobile.twitter.com/" + webComponent.getUsername())
                {

                    this.navView.SelectedItem = navView.MenuItems.ElementAt(3);
                }
                else if (url == "https://mobile.twitter.com/" + webComponent.getUsername() + "/lists")
                {
                    this.navView.SelectedItem = navView.MenuItems.ElementAt(6);
                }
                else if (url.Contains("https://mobile.twitter.com/settings"))
                {
                    this.navView.SelectedItem = navView.SettingsItem;
                }
                else
                {
                    switch (url)
                    {
                        case "https://mobile.twitter.com/home":
                            this.navView.SelectedItem = navView.MenuItems.ElementAt(0);
                            break;
                        case "https://mobile.twitter.com/":
                            this.navView.SelectedItem = navView.MenuItems.ElementAt(0);
                            break;
                        case "https://mobile.twitter.com/explore":
                            this.navView.SelectedItem = navView.MenuItems.ElementAt(1);
                            break;
                        case "https://mobile.twitter.com/notifications":
                            this.navView.SelectedItem = navView.MenuItems.ElementAt(4);
                            break;
                        case "https://mobile.twitter.com/messages":
                            this.navView.SelectedItem = navView.MenuItems.ElementAt(5);
                            break;
                        case "https://mobile.twitter.com/compose/tweet":
                            this.navView.SelectedItem = navView.MenuItems.ElementAt(8);
                            break;
                        default:
                            this.navView.SelectedItem = navView.MenuItems.ElementAt(2); // a separator :V
                            break;
                    }
                }
            }
            finally
            {
                this.texto.Text = url;
            }

        }

        private void navView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void navView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                this.webView.Navigate(new Uri("https://mobile.twitter.com/settings"));
            }
            else
            {
                var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
                navView_Navigate(item as NavigationViewItem);
            }
        }

        private void navView_Navigate(NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "home":
                    if (webComponent.getLocale() != "es")
                    {
                        //ApplicationLanguages.PrimaryLanguageOverride = webComponent.getLocale();
                        //Frame.Navigate(this.GetType());
                        this.iHome.Content = "Inicio";
                    }
                    this.webView.Navigate(new Uri("https://mobile.twitter.com/"));
                    break;
                case "explore":
                    this.webView.Navigate(new Uri("https://mobile.twitter.com/explore"));
                    break;
                case "profile":
                    //this.webView.Navigate(new Uri("https://mobile.twitter.com/settings/screen_name"));
                    this.webView.Navigate(new Uri("https://mobile.twitter.com/" + webComponent.getUsername()));
                    //document.querySelector("input").value
                    break;
                case "notifications":
                    this.webView.Navigate(new Uri("https://mobile.twitter.com/notifications"));
                    break;
                case "messages":
                    this.webView.Navigate(new Uri("https://mobile.twitter.com/messages"));
                    break;
                case "lists":
                    this.webView.Navigate(new Uri("https://mobile.twitter.com/" + webComponent.getUsername() + "/lists"));
                    break;
                case "tweet":
                    //https://icons8.com/articles/unofficial-style-guide-to-windows10icons/
                    this.webView.Navigate(new Uri("https://mobile.twitter.com/compose/tweet"));
                    break;
            }
        }

        private void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.IsSuccess == true)
            {
                //string fn = "javascript:(function(){var style = document.getElementById('customcss');if(!style){style = document.createElement('STYLE');}style.type='text/css';style.id='customcss';style.innerText = '#react-root div header div{background-color:#000!important}[role=banner] [role=button] span{color:#fff}[role=navigation]{display:none!important}[role=banner] form{display:none!important}#react-root div header nav{background-color:#000!important}main div [role=region]{background-color:#d5d1d1!important;color:#fff!important}main>div>div{background-color:#000!important}main div [role=article]{background-color:#272525!important}main div [role=article] [data-testid=tweet] span{color:#b1b1b1!important}[data-testid=sidebarColumn]{background-color:#000!important}[data-testid=sidebarColumn] div{background-color:#363636!important;color:#eee!important}';document.getElementsByTagName('HEAD')[0].appendChild(style);})()";
                string fn = "javascript:(function(){var style = document.getElementById('customcss');if(!style){style = document.createElement('STYLE');}style.type='text/css';style.id='customcss';style.innerText = '[role=banner] [role=navigation]{display:none!important}[role=banner] form{display:none!important}[role=banner] [role=button] svg{display:none!important}body{-ms-user-select: none!important}div[aria-label^=Load]{display: none!important}';document.getElementsByTagName('HEAD')[0].appendChild(style);})()";
                this.webView.InvokeScriptAsync("eval", new string[] { fn });


                    try
                    {
                        // inject event handler to arbitrary page once the DOM is loaded
                        // in this case add event handler to click on the main element
                        webView.InvokeScriptAsync("eval",
                            new[] { "NotifyApp.getString(document.body.getElementsByTagName('script')[0].text);" });
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
            }
        }

        private void webView_ContainsFullScreenElementChanged(WebView sender, object args)
        {
            var applicationView = ApplicationView.GetForCurrentView();

            if (sender.ContainsFullScreenElement)
            {
                //this.navView.Visibility = Visibility.Collapsed;
                this.navView.AlwaysShowHeader = false;                
                applicationView.TryEnterFullScreenMode();               
            }
            else if (applicationView.IsFullScreenMode)
            {
                this.navView.AlwaysShowHeader = true;
                applicationView.ExitFullScreenMode();
            }
        }

        private void ASB_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.QueryText != "")
            {
                string query = "https://mobile.twitter.com/search?src=typed_query&q=" + args.QueryText;
                this.webView.Navigate(new Uri(query));
            }
            else
            {
                this.webView.Navigate(new Uri("https://mobile.twitter.com/explore"));
            }
        }

        private void webView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            Debug.WriteLine("Called from scriptNotify {0}", new[] { e.Value });
        }

        private void webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            
            // Expose the native winRT object on the page's global object
            webView.AddWebAllowedObject("NotifyApp", webComponent);
        }

        private async void webView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            try
            {
                // inject event handler to arbitrary page once the DOM is loaded
                // in this case add event handler to click on the main element
                //await webView.InvokeScriptAsync("eval", new[] { "NotifyApp.setKeyCombination(243);" });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
