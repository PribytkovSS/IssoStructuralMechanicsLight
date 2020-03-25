using IssoStMechLight.Models;
using IssoStMechLight.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace IssoStMechLight.Views
{
    public class BindingEditor : ContentView
    {
        Entry DimValue;
        public ModelViewSurface modelSurface;
        private ImageButton ButtonSupClose, ButtonDelete;

        public BindingEditor()
        {
            DimValue = new Entry()
            {
                Placeholder = StringResources.BindingDimValuePlaceholder,
                Keyboard = Keyboard.Numeric,
                ReturnType = ReturnType.Done,
                WidthRequest = 100
            };

            ButtonSupClose = new ImageButton
            {
                Source = ImageSource.FromStream(() => new MemoryStream(StringResources.IconClose)),
            };

            ButtonSupClose.Pressed += ButtonSupClose_Clicked;
            ButtonSupClose.BackgroundColor = Color.CornflowerBlue;

            ButtonDelete = new ImageButton
            {
                Source = ImageSource.FromStream(() => new MemoryStream(StringResources.delete88)),
            };

            ButtonDelete.Pressed += ButtonDelete_Pressed;
            ButtonDelete.BackgroundColor = Color.LightPink;

            DimValue.Completed += DimValue_Completed;

            Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HeightRequest = 40,
                WidthRequest = 400,
                BackgroundColor = Color.White,
                Children = { DimValue, ButtonSupClose, ButtonDelete}
            };
        }

        private void ButtonDelete_Pressed(object sender, EventArgs e)
        {
            ComponentBasic c = modelSurface.GetSelectedComponent();
            if (c?.CompType == ComponentTypes.ctBinding)
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

        private void DimValue_Completed(object sender, EventArgs e)
        {
            string value = ((Entry)sender).Text.Trim();
            float dim;
            if (float.TryParse(value, out dim)) SetBindingValue(dim);
        }

        private void SetBindingValue(float dim)
        {
            ComponentBasic c = modelSurface.GetSelectedComponent();
            if (c?.CompType == ComponentTypes.ctBinding)
            {
                IssoBinding b = (IssoBinding)c;
                bool res = false;
                float val = b.Value;
                b.Value = dim;
                switch (b.Type)
                {
                    case IssoBindingType.Horizontal: res = modelSurface.Model.PreprocessBindingChange(b, (Math.Sign(b.Target.Location.X - b.Source.Location.X)) * dim, float.MaxValue); break;
                    case IssoBindingType.Vertical: res = modelSurface.Model.PreprocessBindingChange(b, float.MaxValue, (Math.Sign(b.Target.Location.Y - b.Source.Location.Y)) * dim); break;
                }
                if (!res) b.Value = val;
                else EditBinding();
                modelSurface.OnSelectedComponentChanged();
            }
        }

        public void EditBinding()
        {
            IsVisible = true;
            ComponentBasic c = modelSurface.GetSelectedComponent();
            if (c?.CompType == ComponentTypes.ctBinding)
            {
                string dim = ((IssoBinding)c).Value.ToString("G0");
                DimValue.Text = dim;
            }
        }
    }
}