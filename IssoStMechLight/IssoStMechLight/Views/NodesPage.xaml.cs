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
    public partial class NodesPage : ContentPage
    {
        private RodModelVM Model;

        public NodesPage(RodModelVM rodModelVM)
        {
            InitializeComponent();
            Model = rodModelVM;
            BindingContext = Model;
        }
    }
}
