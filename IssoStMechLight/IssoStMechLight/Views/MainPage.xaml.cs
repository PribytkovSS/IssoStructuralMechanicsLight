using IssoStMechLight.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IssoStMechLight.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        Dictionary<int, NavigationPage> MenuPages = MenuPage.InitMenuPages();
        public IssoFileManager FileManager;

        public MainPage(IssoFileManager m)
        {
            InitializeComponent();
            FileManager = m;
            MasterBehavior = MasterBehavior.SplitOnLandscape;
            Detail = MenuPages[0];
        }

        public async Task NavigateFromMenu(int id)
        {
            if (!MenuPages.ContainsKey(id))
            {
                MenuPages.Add(id, new NavigationPage(new NotImplementedPage()));
            }

            var newPage = MenuPages[id];

            if (newPage != null && Detail != newPage)
            {
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);
            }
        }
    }
}