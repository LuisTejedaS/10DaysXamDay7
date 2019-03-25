using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;

namespace TenXamarinD7.Droid
{
    [Activity(Label = "TenXamarinD7", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);


            string fileName = "database.db3";
            string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            string fullPath = Path.Combine(folderPath, fileName); //Added using System.IO;

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);

            LoadApplication(new App(fullPath)); 
        }
    }
}