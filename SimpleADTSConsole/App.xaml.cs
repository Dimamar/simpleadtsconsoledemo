using System;
using System.Windows;

namespace SimpleADTSConsole
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ResourceDictionary ThemeDictionary
        {
            // You could probably get it via its name with some query logic as well.
            get { return Resources.MergedDictionaries[3]; }
        }

        public ResourceDictionary LangDictionary
        {
            // You could probably get it via its name with some query logic as well.
            get { return Resources.MergedDictionaries[4]; }
        }

        public void ChangeTheme(Uri uri)
        {
            ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
        }

        public void ChangeLang(string lng)
        {
            var path = "lng/LngRu{0}.xaml";
            var lngPostfix = "";
            if (!string.IsNullOrEmpty(lng))
                lngPostfix = "." + lng;
            path = string.Format(path, lngPostfix);
            LangDictionary.MergedDictionaries.Clear();
            LangDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(path, UriKind.Relative) });
        }
    }
}
