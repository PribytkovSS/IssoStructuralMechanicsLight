using IssoStMechLight.Models;
using IssoStMechLight.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace IssoStMechLight.Views
{
    class LinearEditor: ContentView
    {
        Switch HingeStart, HingeEnd;
        ImageButton ButtonSupClose, ButtonDelete; 

        public ModelViewSurface modelSurface { set; get; }

        public ComponentLinear Linear
        {
            get
            {
                if (modelSurface == null) return null;

                ComponentBasic b = modelSurface.GetSelectedComponent();
                if (b?.CompType == ComponentTypes.ctLinear)
                {
                    return (ComponentLinear)b;
                }
                else return null;
            }
        }

        public LinearEditor()
        {
            HingeStart = new Switch() { OnColor = Color.GreenYellow, IsToggled = false, Margin = 0, VerticalOptions = LayoutOptions.CenterAndExpand  };
            HingeStart.Toggled += HingeToggled;
            HingeEnd = new Switch() { OnColor = Color.GreenYellow, IsToggled = false, Margin = 0, VerticalOptions = LayoutOptions.CenterAndExpand };
            HingeEnd.Toggled += HingeToggled;

            ButtonSupClose = new ImageButton() { VerticalOptions = LayoutOptions.Center, HeightRequest = 40, WidthRequest = 40 };
            ButtonSupClose.Pressed += ButtonSupClose_Clicked;
            ButtonSupClose.Source = ImageSource.FromStream(() => new MemoryStream(StringResources.IconClose));
            ButtonSupClose.BackgroundColor = Color.CornflowerBlue;

            ButtonDelete = new ImageButton
            {
                Source = ImageSource.FromStream(() => new MemoryStream(StringResources.delete88)),
            };

            ButtonDelete.Pressed += ButtonDelete_Pressed;
            ButtonDelete.BackgroundColor = Color.LightPink;

            Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HeightRequest = 40,
                WidthRequest = 400,
                MinimumHeightRequest = 60,
                BackgroundColor = Color.White,
                Children =
                {
                    new Label() { Text = "Шарнир в начале: ", VerticalOptions = LayoutOptions.CenterAndExpand, VerticalTextAlignment = TextAlignment.Center },
                    HingeStart,
                    new Label() { Text = "Шарнир в конце: ", VerticalOptions = LayoutOptions.CenterAndExpand, VerticalTextAlignment = TextAlignment.Center },
                    HingeEnd,
                    ButtonSupClose,
                    ButtonDelete
                }
            };
        }

        private void ButtonDelete_Pressed(object sender, EventArgs e)
        {
            ComponentBasic c = modelSurface.GetSelectedComponent();
            if (c?.CompType == ComponentTypes.ctLinear) 
            {
                if (modelSurface.DeleteSelectedObject())
                {
                    IsVisible = false;
                    modelSurface.IssoAction(EditorActions.None);
                }
            }
        }

        private void ButtonSupClose_Clicked(object sender, EventArgs e)
        {
            IsVisible = false;
            modelSurface.IssoAction(EditorActions.None);
        }

        private void HingeToggled(object sender, ToggledEventArgs e)
        {
            if (Linear != null)
            {
                if (sender == HingeStart) Linear.HingeStart = HingeStart.IsToggled;
                if (sender == HingeEnd) Linear.HingeEnd = HingeEnd.IsToggled;
            }
            modelSurface.Invalidate();
        }

        internal void EditLinear()
        {
            if (Linear != null)
            {
                HingeStart.IsToggled = Linear.HingeStart;
                HingeEnd.IsToggled = Linear.HingeEnd;
                IsVisible = true;
            }
        }
    }
}
