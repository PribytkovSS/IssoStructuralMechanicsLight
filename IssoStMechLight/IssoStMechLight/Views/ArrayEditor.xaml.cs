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
	public partial class ArrayEditor : ContentView
	{
        public EventHandler OnArrayOK;
        public EventHandler OnArrayCancel;

        public ArrayEditor ()
		{
			InitializeComponent ();
		}

        private void CreateArrayBtn_Pressed(object sender, EventArgs e)
        {
            OnArrayOK?.Invoke(sender, e);
        }

        private void CancelBtn_Pressed(object sender, EventArgs e)
        {
            OnArrayCancel?.Invoke(sender, e);
        }
    }
}