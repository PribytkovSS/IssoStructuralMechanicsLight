using IssoStMechLight.Models;
using IssoStMechLight.Resources;
using IssoStMechLight.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IssoStMechLight.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModelSavePage : ContentPage
    {
        private RodModelVM saveModel;

        public ModelSavePage(RodModelVM rodModelVM)
        {
            InitializeComponent();
            saveModel = rodModelVM;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MainPage mp = (MainPage)Application.Current.MainPage;
            FileNameEntry.Text = mp.FileManager.FileName();
            LocationEntry.Text = mp.FileManager.FileLocation();
        }

        private async void SaveButton_Pressed(object sender, EventArgs e)
        {
            MainPage mp = (MainPage)Application.Current.MainPage;

            string fname = "";
            if (sender == SaveButton) fname = mp.FileManager.FullFileName();

            switch (Device.RuntimePlatform)
            {
                case Device.UWP: await mp.FileManager.WriteStreamToFile(saveModel.Save(), fname); break;
            }

            FileNameEntry.Text = mp.FileManager.FileName();
            LocationEntry.Text = mp.FileManager.FileLocation();

            await mp.NavigateFromMenu(0);
        }

        private string getFileName()
        {
            return LocationEntry.Text + "\\" + FileNameEntry.Text;
        }
    }
}