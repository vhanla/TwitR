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

        private DispatcherTimer timer = new DispatcherTimer();
        

        public Home()
        {
            this.InitializeComponent();

            timer.Tick += getRoutes;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 250);
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

        private void goRoute(string routePath)
        {
            webView.InvokeScriptAsync("eval",
                new[] { "document.querySelector('a[href=\"" + routePath + "\"]').click()" });
        }
        private void getRoute(string routeName, string routePath)
        {
            string query = "NotifyApp.getRoute('" + routeName + "',document.querySelector('a[href=\"" + routePath + "\"]') != undefined)";
            webView.InvokeScriptAsync("eval", new[] { query });   
        }
        private void getMode()
        {
            //string query = "if(document.cookie.match('(^|;)\\s*night_mode\\s*=\\s*([^;]+)')){NotifyApp.getMode(document.cookie.match('(^|;)\\s*night_mode\\s*=\\s*([^;]+)').pop())}";
            //string query = "var dkmd = document.cookie.match('(^|;)\\s*night_mode\\s*=\\s*([^;]+)').pop();NotifyApp.getMode(dkmd)";
            //webView.InvokeScriptAsync("eval", new[] { query });
            var htt = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            var cm = htt.CookieManager;
            var cc = cm.GetCookies(new Uri("https://mobile.twitter.com"));
            foreach(var c in cc)
            {
                if(c.Name == "night_mode")
                {
                    if (c.Value == "1")
                    {
                        if(((Frame)Window.Current.Content).RequestedTheme != ElementTheme.Dark)
                        {
                            ((Frame)Window.Current.Content).RequestedTheme = ElementTheme.Dark;
                        }
                    }
                    else
                    {
                        if(((Frame)Window.Current.Content).RequestedTheme == ElementTheme.Dark)
                        {
                            ((Frame)Window.Current.Content).RequestedTheme = ElementTheme.Light;
                        }
                    }    
                }
                
            }
        }

        private void getRoutes(object sender, object e)
        {
            // find out if routes are clickable
            getRoute("home", "/home");
            getRoute("explore", "/explore");
            getRoute("profile", "/" + webComponent.getUsername());
            getRoute("notifications", "/notifications");
            getRoute("messages", "/message");
            getRoute("compose", "/compose/tweet");
            getRoute("settings", "/settins");
            getMode();
            
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
                            // inject Ctrl+Enter event listener script
                            string tweet = "(function(){var script = document.getElementById('tweetscript');if(!script){script = document.createElement('SCRIPT');}script.type='text/javascript';script.id='tweetscript';script.innerText = 'if(document.querySelector(\"[data - testid = tweet - textarea]\")){document.querySelector(\"[data - testid = tweet - textarea]\").addEventListener(\"keydown\", function(e){if(e.ctrlKey && e.keyCode==13){document.querySelector(\"[data - testid = tweet - button]\").click()}}, false)}';document.getElementsByTagName('HEAD')[0].appendChild(script);})()";
                            this.webView.InvokeScriptAsync("eval", new string[] { tweet });
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
        //https://icons8.com/articles/unofficial-style-guide-to-windows10icons/
            timer.Stop();
            switch (item.Tag)
            {
                case "home":
                    if (webComponent.getLocale() != "es")
                    {
                        //ApplicationLanguages.PrimaryLanguageOverride = webComponent.getLocale();
                        //Frame.Navigate(this.GetType());
                        this.iHome.Content = "Inicio";
                    }
                    if (webComponent.getHome)
                    {
                        goRoute("/home");
                    }
                    else
                    {
                        this.webView.Navigate(new Uri("https://mobile.twitter.com/"));
                    }
                    break;
                case "explore":
                    if (webComponent.getExplore)
                    {
                        goRoute("/explore");
                    }
                    else
                    {
                        this.webView.Navigate(new Uri("https://mobile.twitter.com/explore"));
                    }
                    break;
                case "profile":
                    if (webComponent.getProfile)
                    {
                        goRoute("/" + webComponent.getUsername());
                    }
                    else
                    {
                        this.webView.Navigate(new Uri("https://mobile.twitter.com/" + webComponent.getUsername()));
                    }
                    break;
                case "notifications":
                    if (webComponent.getNotifications)
                    {
                        goRoute("/notifications");
                    }
                    else
                    {
                        this.webView.Navigate(new Uri("https://mobile.twitter.com/notifications"));
                    }
                    break;
                case "messages":
                    if (webComponent.getMessages)
                    {
                        goRoute("/messages");
                    }
                    else
                    {
                        this.webView.Navigate(new Uri("https://mobile.twitter.com/messages"));
                    }
                    break;
                case "lists":
                    {
                        this.webView.Navigate(new Uri("https://mobile.twitter.com/" + webComponent.getUsername() + "/lists"));
                    }
                    break;
                case "tweet":
                    if (webComponent.getCompose)
                    {
                        goRoute("/compose/tweet");
                    }
                    else
                    {
                        this.webView.Navigate(new Uri("https://mobile.twitter.com/compose/tweet"));
                    }
                    break;
            }
        }

        

        private void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.IsSuccess == true)
            {
                //var uiSettings = new Windows.UI.ViewManagement.UISettings();
                //var rgba = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Accent);
                //string colr = rgba.R.ToString();
                //string colg = rgba.G.ToString();
                //string colb = rgba.B.ToString();

                ////string fn = "javascript:(function(){var style = document.getElementById('customcss');if(!style){style = document.createElement('STYLE');}style.type='text/css';style.id='customcss';style.innerText = '#react-root div header div{background-color:#000!important}[role=banner] [role=button] span{color:#fff}[role=navigation]{display:none!important}[role=banner] form{display:none!important}#react-root div header nav{background-color:#000!important}main div [role=region]{background-color:#d5d1d1!important;color:#fff!important}main>div>div{background-color:#000!important}main div [role=article]{background-color:#272525!important}main div [role=article] [data-testid=tweet] span{color:#b1b1b1!important}[data-testid=sidebarColumn]{background-color:#000!important}[data-testid=sidebarColumn] div{background-color:#363636!important;color:#eee!important}';document.getElementsByTagName('HEAD')[0].appendChild(style);})()";
                //string loading = "div[aria-label^=\"Load\"] svg{color: rgb(" + colr + "," + colg + "," + colb + ")!important}";
                //string csstyle = "[role=banner] [role=navigation]{display:none!important}[role=banner] form{display:none!important}[role=banner] [role=button] svg{display:none!important}body{-ms-user-select: none!important}div[aria-label^=Load]{display: none!important}";
                //string buttons = "/*Rounded class*/.rn-qb5c1y{border-bottom-left-radius: 0;}.rn-sqtsar{border-bottom-right-radius: 0;}.rn-waaub4{border-top-right-radius: 0;}.rn-1bxrh7q{border-top-left-radius: 0;}";
                //string postmedia = ".rn-v31qbu{border-bottom-left-radius: 0;}.rn-1mm01xx{border-bottom-right-radius: 0;}.rn-3mf89h{border-top-right-radius: 0;}.rn-12oo7hk{border-top-left-radius: 0;}";
                //string lightbox = ".rn-17ru5vd{border-bottom-left-radius: 0;}.rn-21wlf0{border-bottom-right-radius: 0;}.rn-1a1bgtt{border-top-right-radius: 0;}.rn-1nul30p{border-top-left-radius: 0;}";
                //string buttoncol = ".rn-8184n4 {color: rgb(" + colr + "," + colg + "," + colb + ")}.rn-162msjx {border-left-color: rgb(" + colr + "," + colg + "," + colb + ")}.rn-11kq4yi {border-bottom-color: rgb(" + colr + "," + colg + "," + colb + ")}.rn-biwm0c {border-right-color: rgb(" + colr + "," + colg + "," + colb + ")}.rn-1vhfjxp {border-top-color: rgb(" + colr + "," + colg + "," + colb + ")}";
                //string navcolors = ".Pdo9ySC3::after {background-color: rgb(" + colr + "," + colg + "," + colb + ")}.Pdo9ySC3{color: #25974f!important}._3XRskxP3:hover, .Pdo9ySC3 {color: rgb(" + colr + "," + colg + "," + colb + ")}";
                //string bgcolor = ".rn-10pympv {background-color: rgb(" + colr + "," + colr + "," + colb + ")}";
                //string linkcolor = ".rn-wjcbme {color: rgb(" + colr + "," + colg + "," + colb + ")}._3fUfiuOH {color: rgb(" + colr + "," + colg + "," + colb + ")}";
                //string navcolors2 = ".COfxu8s-._2t1-zb7t{border-bottom-color: rgb(" + colr + "," + colg + "," + colb + "); color: rgb(" + colr + "," + colg + "," + colb + ");}.COfxu8s-._2t1-zb7t:hover{border-bottom-color: rgb(" + colr + "," + colg + "," + colb + "); color: rgb(" + colr + "," + colg + "," + colb + ");}.COfxu8s-:hover {color: rgb(" + colr + "," + colg + "," + colb + ")}";
                ////string likecolor = ".rn-c4vgha {color: rgb(" + colr + "," + colg + "," + colb + ")!important}";

                //string fn = "javascript:(function(){var style = document.getElementById('customcss');if(!style){style = document.createElement('STYLE');}style.type='text/css';style.id='customcss';style.innerText = '"
                //    + loading
                //    + csstyle
                //    + buttons
                //    + postmedia
                //    + lightbox
                //    + buttoncol
                //    + navcolors
                //    + bgcolor
                //    + linkcolor
                //    + navcolors2
                //    + "';document.getElementsByTagName('HEAD')[0].appendChild(style);})()";
                //this.webView.InvokeScriptAsync("eval", new string[] { fn });


                try
                {
                    // inject event handler to arbitrary page once the DOM is loaded
                    // in this case add event handler to click on the main element
                    webView.InvokeScriptAsync("eval",
                        new[] { "NotifyApp.getString(document.body.getElementsByTagName('script')[0].text);" });

                    timer.Start(); 
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

        //private void NavigateWithHeader(Uri uri)
        //{
        //    //http://stackoverflow.com/questions/39490430/ddg#39529757
        //    var rq = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, uri);
        //    rq.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; ServiceUI 13) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134");
        //    webView.NavigateWithHttpRequestMessage(rq);

        //    webView.NavigationStarting += webView_NavigationStarting;
        //}
        private void webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            //webView.NavigationStarting -= webView_NavigationStarting;
            //args.Cancel = true;
            //NavigateWithHeader(args.Uri);
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

                var uiSettings = new Windows.UI.ViewManagement.UISettings();
                var rgba = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Accent);
                string colr = rgba.R.ToString();
                string colg = rgba.G.ToString();
                string colb = rgba.B.ToString();
                
                //string fn = "javascript:(function(){var style = document.getElementById('customcss');if(!style){style = document.createElement('STYLE');}style.type='text/css';style.id='customcss';style.innerText = '#react-root div header div{background-color:#000!important}[role=banner] [role=button] span{color:#fff}[role=navigation]{display:none!important}[role=banner] form{display:none!important}#react-root div header nav{background-color:#000!important}main div [role=region]{background-color:#d5d1d1!important;color:#fff!important}main>div>div{background-color:#000!important}main div [role=article]{background-color:#272525!important}main div [role=article] [data-testid=tweet] span{color:#b1b1b1!important}[data-testid=sidebarColumn]{background-color:#000!important}[data-testid=sidebarColumn] div{background-color:#363636!important;color:#eee!important}';document.getElementsByTagName('HEAD')[0].appendChild(style);})()";
                string loading = "div[aria-label^=\"Load\"] svg{color: "+ colr + "," + colg + "," + colb +")!important}";
                string csstyle = "[role=banner] [role=navigation]{display:none!important}[role=banner] form{display:none!important}[role=banner] [role=button] svg{display:none!important}body{-ms-user-select: none!important}div[aria-label^=Load]{display: none!important}";
                string buttons = "/*Rounded class*/.rn-qb5c1y{border-bottom-left-radius: 0;}.rn-sqtsar{border-bottom-right-radius: 0;}.rn-waaub4{border-top-right-radius: 0;}.rn-1bxrh7q{border-top-left-radius: 0;}";
                string postmedia = ".rn-v31qbu{border-bottom-left-radius: 0;}.rn-1mm01xx{border-bottom-right-radius: 0;}.rn-3mf89h{border-top-right-radius: 0;}.rn-12oo7hk{border-top-left-radius: 0;}";
                string lightbox = ".rn-17ru5vd{border-bottom-left-radius: 0;}.rn-21wlf0{border-bottom-right-radius: 0;}.rn-1a1bgtt{border-top-right-radius: 0;}.rn-1nul30p{border-top-left-radius: 0;}";
                string buttoncol = ".rn-8184n4 {color: rgb(" + colr + "," + colg + "," + colb + ")}.rn-162msjx {border-left-color: rgb(" + colr + "," + colg + "," + colb + ")}.rn-11kq4yi {border-bottom-color: rgb(" + colr + "," + colg + "," + colb + ")}.rn-biwm0c {border-right-color: rgb(" + colr + "," + colg + "," + colb + ")}.rn-1vhfjxp {border-top-color: rgb(" + colr + "," + colg + "," + colb + ")}";
                string navcolors = ".Pdo9ySC3::after {background-color: rgb(" + colr + "," + colg + "," + colb + ")}.Pdo9ySC3{color: #25974f!important}._3XRskxP3:hover, .Pdo9ySC3 {color: rgb(" + colr + "," + colg + "," + colb + ")}";
                string bgcolor = ".rn-10pympv {background-color: rgb(" + colr + "," + colr + "," + colb + ")}";
                string linkcolor = ".rn-wjcbme {color: rgb(" + colr + "," + colg + "," + colb + ")}._3fUfiuOH {color: rgb(" + colr + "," + colg + "," + colb + ")}";
                string navcolors2 = ".COfxu8s-._2t1-zb7t{border-bottom-color: rgb(" + colr + "," + colg + "," + colb + "); color: rgb(" + colr + "," + colg + "," + colb + ");}.COfxu8s-._2t1-zb7t:hover{border-bottom-color: rgb(" + colr + "," + colg + "," + colb + "); color: rgb(" + colr + "," + colg + "," + colb + ");}.COfxu8s-:hover {color: rgb(" + colr + "," + colg + "," + colb + ")}"; 
                //string likecolor = ".rn-c4vgha {color: rgb(" + colr + "," + colg + "," + colb + ")!important}";

                string fn = "javascript:(function(){var style = document.getElementById('customcss');if(!style){style = document.createElement('STYLE');}style.type='text/css';style.id='customcss';style.innerText = '" 
                    + loading
                    + csstyle 
                    + buttons
                    + postmedia
                    + lightbox
                    + buttoncol
                    + navcolors
                    + bgcolor
                    + linkcolor
                    + navcolors2
                    +"';document.getElementsByTagName('HEAD')[0].appendChild(style);})()";
                this.webView.InvokeScriptAsync("eval", new string[] { fn });



            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
       
    }
}
