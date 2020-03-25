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
    public class LoadEditor : ContentView
    {
        Entry LoadValue, LoadAngle;
        ImageButton ButtonSupClose, ButtonDelete;
        Switch OrthoCheck, InvCheck;

        public ModelViewSurface modelSurface { set; get; }

        public ComponentLoad Load
        {
            get
            {
                if (modelSurface == null) return null;

                ComponentBasic b = modelSurface.GetSelectedComponent();
                if ((b?.CompType == ComponentTypes.ctDistributedLoad) || (b?.CompType == ComponentTypes.ctForce))
                {
                    return (ComponentLoad)b;
                }
                else return null;
            }
        }

        public LoadEditor()
        {
            InitToolBarControls();
            Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HeightRequest = 60,
                WidthRequest = 400,
                MinimumHeightRequest = 60,
                BackgroundColor = Color.White,
                Children =
                {
                  LoadValue,
                  new Label() { Text = StringResources.KiloNewtons, VerticalOptions = LayoutOptions.CenterAndExpand, VerticalTextAlignment = TextAlignment.Center},
                  LoadAngle,
                  new Label() { Text = StringResources.Degrees},
                  
                  new Frame()
                  {
                      BorderColor = Color.Black,
                      VerticalOptions = LayoutOptions.CenterAndExpand,
                      HorizontalOptions = LayoutOptions.Center,
                      Padding = 5,
                      MinimumWidthRequest = 200,
                      Content = new StackLayout
                      {
                          Orientation = StackOrientation.Horizontal,                          
                          Children =
                          {
                              new Label()
                              {                                  
                                  Text = "По нормали к элементу:",
                                  VerticalOptions = LayoutOptions.CenterAndExpand,
                                  VerticalTextAlignment = TextAlignment.Center,
                              },
                              OrthoCheck,
                              new Frame()
                              {
                                  BorderColor = Color.Gray,
                                  VerticalOptions = LayoutOptions.CenterAndExpand,
                                  HorizontalOptions = LayoutOptions.Center,
                                  Padding = 5,
                                  MinimumWidthRequest = 140,
                                  Content = new StackLayout
                                  {
                                      Orientation = StackOrientation.Horizontal,
                                      Children =
                                      {
                                          new Label()
                                          {
                                              Text = "К элементу:",
                                              VerticalOptions = LayoutOptions.CenterAndExpand,
                                              VerticalTextAlignment = TextAlignment.Center
                                          },
                                          InvCheck,
                                      }
                                  }
                              },
                          }
                      }
                  },                                   
                  
                  ButtonSupClose,
                  ButtonDelete
                }
            };
        }

        private void ButtonDelete_Pressed(object sender, EventArgs e)
        {
            ComponentBasic c = modelSurface.GetSelectedComponent();
            if ((c?.CompType == ComponentTypes.ctForce) || (c?.CompType == ComponentTypes.ctDistributedLoad))
            {
                if (modelSurface.DeleteSelectedObject())
                {
                    IsVisible = false;
                    modelSurface.IssoAction(EditorActions.None);
                }
            }
        }

        public void InitToolBarControls()
        {
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

            OrthoCheck = new Switch() { OnColor = Color.GreenYellow, IsToggled = false, Margin = 0, VerticalOptions = LayoutOptions.CenterAndExpand };            
            OrthoCheck.Toggled += OrthoToggled;

            InvCheck = new Switch() { OnColor = Color.GreenYellow, IsToggled = false, IsEnabled = false, Margin = 0, VerticalOptions = LayoutOptions.CenterAndExpand };
            InvCheck.Toggled += InvToggled;

            LoadAngle = new Entry()
            {
                WidthRequest = 80,
                HeightRequest = 35,
                VerticalOptions = LayoutOptions.Center,
                Placeholder = "90",
                Keyboard = Keyboard.Numeric,
                ReturnType = ReturnType.Done
            };
            LoadAngle.Completed += LoadAngle_Completed;

            LoadValue = new Entry()
            {
                Placeholder = "100",
                HeightRequest = 35,
                VerticalOptions = LayoutOptions.Center,
                Keyboard = Keyboard.Numeric,
                ReturnType = ReturnType.Done,
                WidthRequest = 80
            };
            LoadValue.Completed += LoadValue_Completed;
        }

        internal void EditLoad()
        {
            if (Load != null)
            {
                LoadValue.Text = Load.Value.ToString();
                LoadAngle.Text = Load.Direction.ToString();
                OrthoCheck.IsToggled = Load.isOrthogonal;
                InvCheck.IsToggled = !Load.isReverse;
                OrthoCheck.IsEnabled = Load.CompType == ComponentTypes.ctDistributedLoad;
                OrthoCheck.IsEnabled = Load.CompType == ComponentTypes.ctDistributedLoad;
                IsVisible = true;
            }
        }

        private void LoadValue_Completed(object sender, EventArgs e)
        {
            if (Load != null)
            {
                string value = ((Entry)sender).Text.Trim();
                if (float.TryParse(value, out float lval)) Load.Value = lval;
                modelSurface.OnSelectedComponentChanged();
            }
        }

        private void LoadAngle_Completed(object sender, EventArgs e)
        {
            if (Load != null)
            {
                string value = ((Entry)sender).Text.Trim();
                if (float.TryParse(value, out float lval)) Load.Direction = lval;
                modelSurface.OnSelectedComponentChanged();
            }
        }

        private void InvToggled(object sender, ToggledEventArgs e)
        {
            if (Load != null)
            {
                Load.isReverse = !InvCheck.IsToggled;
                modelSurface.OnSelectedComponentChanged();
            }
        }

        private void OrthoToggled(object sender, ToggledEventArgs e)
        {
            LoadAngle.IsEnabled = !OrthoCheck.IsToggled;
            InvCheck.IsEnabled = OrthoCheck.IsToggled;
            if (Load != null)
            {
                Load.isOrthogonal = OrthoCheck.IsToggled;
                modelSurface.OnSelectedComponentChanged();
            }
        }

        private void ButtonSupClose_Clicked(object sender, EventArgs e)
        {
            IsVisible = false;
            modelSurface.IssoAction(EditorActions.None);
        }
    }
}