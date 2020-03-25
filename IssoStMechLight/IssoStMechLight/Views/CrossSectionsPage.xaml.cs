using IssoStMechLight.Models;
using IssoStMechLight.Resources;
using IssoStMechLight.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IssoStMechLight.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CrossSectionsPage : ContentPage
    {
        private ImageButton AddSectionButton, DeleteSectionButton, RenameSectionButton;
        private RodModelVM rodModelVM;
        public ObservableCollection<IssoCrossSection> SectionList { get { return rodModelVM.model.CrossSections; } }
        public ObservableCollection<MaterialLib> MaterialLibs;
        
        public CrossSectionsPage (RodModelVM rodModelVM)
		{
			InitializeComponent();
            this.rodModelVM = rodModelVM;
            InitializeToolbar();            
            SectionsListView.ItemsSource = SectionList; 
        }

        private async void InitializeMaterialLib()
        {
            MaterialLibs = await MaterialLib.CreateMaterialsCollection("MaterialLibraries.xml");
        }

        private void LoadStandards(Picker standardPicker)
        {
            standardPicker.Items.Add("Прямоугольное");
            standardPicker.Items.Add("Круглое");
            standardPicker.Items.Add("Круглое полое");
            standardPicker.Items.Add("Коробчатое");
            standardPicker.Items.Add("Тавр");
            standardPicker.Items.Add("Двутавр");
            standardPicker.SelectedIndex = 0;
        }

        private SectionType NameToSectionType(string SectionTypeName)
        {
            if (SectionTypeName == "Прямоугольное") return SectionType.Rectangle;
            if (SectionTypeName == "Круглое") return SectionType.Round;
            if (SectionTypeName == "Круглое полое") return SectionType.Circle;
            if (SectionTypeName == "Коробчатое") return SectionType.Box;
            if (SectionTypeName == "Тавр") return SectionType.T;
            if (SectionTypeName == "Двутавр") return SectionType.H;
            return SectionType.Rectangle;
        }

        private void InitializeToolbar()
        {
            // Создаём кнопки панели инструментов
            AddSectionButton = new ImageButton
            {
                Source = ImageSource.FromStream(() => new MemoryStream(StringResources.AddSection)),
                HeightRequest = 40,
                BorderWidth = 2
            };

            AddSectionButton.Pressed += ButtonNew_Clicked;

            DeleteSectionButton = new ImageButton
            {
                Source = ImageSource.FromStream(() => new MemoryStream(StringResources.delete88)),
                HeightRequest = 40,
                BorderWidth = 2
            };

            DeleteSectionButton.Pressed += ButtonDelete_Clicked;

            RenameSectionButton = new ImageButton
            {
                Source = ImageSource.FromStream(() => new MemoryStream(StringResources.rename)),
                HeightRequest = 40,
                BorderWidth = 2
            };

            RenameSectionButton.Pressed += ButtonRename_Clicked;

            SectionListToolBar.Children.Add(AddSectionButton);
            SectionListToolBar.Children.Add(DeleteSectionButton);
            SectionListToolBar.Children.Add(RenameSectionButton);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            InitializeMaterialLib();
        }

        private void ButtonRename_Clicked(object sender, EventArgs e)
        {
            if (SectionsListView.SelectedItem != null)
            {
                IssoCrossSection section = (IssoCrossSection)SectionsListView.SelectedItem;
                SectionNameLabel.Text = "Новое имя для '" + section.SectionName + "': ";
                SectionNameEdit.IsVisible = true;
                SectionNameEntry.Focus();
                SectionsListView.IsEnabled = false;
            }          
        }

        private void SectionNameEntry_Completed(object sender, EventArgs e)
        {
            SectionNameEdit.IsVisible = false;
            SectionsListView.IsEnabled = true;
            IssoCrossSection section = (IssoCrossSection)SectionsListView.SelectedItem;
            section.SectionName = SectionNameEntry.Text;
        }

        private void MaterialButton_Pressed(object sender, EventArgs e)
        {
            MaterialLibView.IsVisible = true;
        }

        private void RangeButton_Pressed(object sender, EventArgs e)
        {

        }

        private void MaterialLibList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }

        private void MaterialLibItems_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }

        private void MaterialSelectButton_Pressed(object sender, EventArgs e)
        {
            MaterialLibView.IsVisible = false;
        }

        private void ButtonDelete_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ButtonNew_Clicked(object sender, EventArgs e)
        {
            SectionList.Add(new IssoCrossSection() { SectionName = "Section" + SectionList.Count.ToString() });
        }
    }
}