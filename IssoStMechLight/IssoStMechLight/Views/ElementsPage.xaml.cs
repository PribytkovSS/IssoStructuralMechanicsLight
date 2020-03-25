using IssoStMechLight.Models;
using IssoStMechLight.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IssoStMechLight.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ElementsPage : ContentPage
	{
        RodModelVM rodModelVM;        

        public ElementsPage (RodModelVM rodModelVM)
		{
			InitializeComponent ();
            this.rodModelVM = rodModelVM;
            BindingContext = this.rodModelVM;
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}