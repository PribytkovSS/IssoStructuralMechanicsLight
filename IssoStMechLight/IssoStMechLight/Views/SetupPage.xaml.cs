using IssoStMechLight.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IssoStMechLight.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SetupPage : ContentPage, INotifyPropertyChanged
    {
		public SetupPage ()
		{
			InitializeComponent ();
            BindingContext = this;
        }

        public string SetupSnapToGrid
        { get { return StringResources.SetupSnapToGrid; } }
        public string SetupSnapToNodes
        { get { return StringResources.SetupSnapToNodes; } }
        public string SetupDisplayElementNumbers
        { get { return StringResources.SetupDisplayElementNumbers; } }
        public string SetupDisplayNodeNumbers
        { get { return StringResources.SetupDisplayNodeNumbers; } }
        public string SetupDisplayLoadValues
        { get { return StringResources.SetupDisplayLoadValues; } }
        public string SetupDisplayRigidity
        { get { return StringResources.SetupDisplayRigidity; } }
        public string SetupSnapTitle
        { get { return StringResources.SetupSnapTitle; } }
        public string SetupDisplayTitle
        { get { return StringResources.SetupDisplayTitle; } }
    }
}