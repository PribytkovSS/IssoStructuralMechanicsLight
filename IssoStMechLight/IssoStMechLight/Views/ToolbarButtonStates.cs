using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace IssoStMechLight.Views
{
    public static class ToolbarButtonStates
    {
        public static void SetStates(ImageButton button)
        {
            VisualStateGroup vsg = new VisualStateGroup();
            vsg.States.Add(new VisualState() { Name = "Pressed" });
            vsg.States[0].Setters.Add(new Setter() { Property = ImageButton.BorderColorProperty, Value = Color.CornflowerBlue });
            vsg.States.Add(new VisualState() { Name = "Normal" });
            vsg.States[1].Setters.Add(new Setter() { Property = ImageButton.BorderColorProperty, Value = Color.White });

            VisualStateManager.SetVisualStateGroups(button,
                new VisualStateGroupList() { vsg });
        }

        public static VisualStateGroupList TBS
        {
            get
            {
                return new VisualStateGroupList()
                {
                    new VisualStateGroup()
                    {
                        States = { new VisualState() { Name = "Pressed", Setters = { new Setter() { Property = ImageButton.BackgroundColorProperty, Value = Color.CornflowerBlue} }},
                                   new VisualState() { Name = "Normal", Setters = { new Setter() { Property = ImageButton.BackgroundColorProperty, Value = Color.White } }}
                                 }
                    }
                };
            }
        }
    }
}
