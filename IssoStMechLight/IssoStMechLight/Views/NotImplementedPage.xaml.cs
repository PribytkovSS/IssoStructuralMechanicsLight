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
	public partial class NotImplementedPage : ContentPage
	{
        public static string LabelText = "Функция пока не реализована.";
        Label flabel;

        public NotImplementedPage ()
		{
			InitializeComponent ();

            flabel = (Label)FugenFullerLabel;    
            flabel.Text = LabelText;
        }
	}
}