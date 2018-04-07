using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation.Metadata;

namespace WebHandler
{
    public delegate void NotifyAppHandler(int keyComb);

    [AllowForWeb]
    public sealed class WebComponent
    {
        public event NotifyAppHandler NotifyAppEvent;
        private string username = "";
        private string locale = "";

        public void setKeyCombination(int keyPress)
        {
            Debug.WriteLine("Called from webView! {0}", keyPress);
            Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(keyPress.ToString());
            dialog.ShowAsync();
        }

        public void getString(string text)
        {
            // We'll get the script containing the JSON data from mobile.twitter.com if session exists
            var sentences = new List<String>();
            int position = 0;
            int start = 0;
            do
            {
                position = text.IndexOf(';', start);
                if (position >= 0)
                {
                    sentences.Add(text.Substring(start, position - start + 1).Trim());
                    start = position + 1;

                }
            } while (position > 0);
            // normally sentences[0] contains window.__INITIAL_STATE__ = all data related to user
            // senteces[1] contains window.__META_DATA__ = {"env":"prod","isLoggedIn":true,"isRTL":false};
            if (sentences.Count > 0)
            {
                if (sentences[0].Contains("window.__INITIAL_STATE__ = "))
                {
                    string json = sentences[0].Substring(27,sentences[0].Length - 28).Trim();
                    dynamic data = JObject.Parse(json);
                    username = data.session.user.screen_name;
                    locale = data.session.language;
                    //Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(username);
                    //dialog.ShowAsync();
                }                
            }
        }

        public string getUsername()
        {
            return username;
        }

        public string getLocale()
        {
            return locale;
        }
    }
}
