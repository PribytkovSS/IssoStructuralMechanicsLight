using IssoStMechLight.Models;
using IssoStMechLight.Resources;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IssoStMechLight.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        
        List<IssoMenuItem> menuItems;

        public static Dictionary<int, NavigationPage> InitMenuPages()
        {
            Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();
            ModelPage modelPage = new ModelPage();
            MenuPages.Add(0, new NavigationPage(modelPage));
            MenuPages.Add(1, new NavigationPage(new ModelSavePage(modelPage.rodModelVM)));
            MenuPages.Add(3, new NavigationPage(new NodesPage(modelPage.rodModelVM)));
            MenuPages.Add(4, new NavigationPage(new ElementsPage(modelPage.rodModelVM)));
            MenuPages.Add(6, new NavigationPage(new CrossSectionsPage(modelPage.rodModelVM)));
            MenuPages.Add(7, new NavigationPage(new ResultsPage(modelPage.rodModelVM)));
            MenuPages.Add(8, new NavigationPage(new SetupPage()));
            return MenuPages;
        }

        public MenuPage()
        {
            InitializeComponent();

            menuItems = new List<IssoMenuItem>
            {
                new IssoMenuItem {Id = 0, Title = StringResources.MenuItemModel },
                new IssoMenuItem {Id = 1, Title = StringResources.MenuItemSave },
                new IssoMenuItem {Id = 2, Title = StringResources.MenuItemLoad },
                new IssoMenuItem {Id = 3, Title = StringResources.MenuItemNodes },
                new IssoMenuItem {Id = 4, Title = StringResources.MenuItemElements },
                new IssoMenuItem {Id = 5, Title = StringResources.MenuItemLoads },
                new IssoMenuItem {Id = 6, Title = StringResources.MenuItemCrossSections },
                //new IssoMenuItem {Id = 7, Title = StringResources.MenuItemMaterials },
                new IssoMenuItem {Id = 7, Title = StringResources.MenuItemResults },
                new IssoMenuItem {Id = 8, Title = StringResources.MenuItemSetup }
            };

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.SelectedItem = menuItems[0];
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                var id = ((IssoMenuItem)e.SelectedItem).Id;
                await RootPage.NavigateFromMenu(id);
            };
        }
    }
}