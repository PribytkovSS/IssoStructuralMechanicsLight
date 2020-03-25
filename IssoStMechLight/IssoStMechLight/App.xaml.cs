using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IssoStMechLight.Views;
using IssoStMechLight.Models;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace IssoStMechLight
{
    public partial class App : Application
    {

        public App(IssoFileManager m)
        {
            InitializeComponent();
            
            MainPage = new MainPage(m);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
