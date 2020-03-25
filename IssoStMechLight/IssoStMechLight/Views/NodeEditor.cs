using IssoStMechLight.Models;
using IssoStMechLight.Resources;
using System;
using System.IO;

using Xamarin.Forms;

namespace IssoStMechLight.Views
{
	public class NodeEditor : ContentView
	{
        /*ImageButton ButtonSupPin;
        ImageButton ButtonSupRoller;
        ImageButton ButtonSupFixed;
        ImageButton ButtonHinge;
        ImageButton ButtonRigid;        
        Entry SupportAngle, XCoord, YCoord; */
        Switch dispX, dispY, dispR;
        ImageButton ButtonSupClose, ButtonDelete;

        public ModelViewSurface modelSurface { set; get; }

        public NodeEditor ()
		{            
            InitToolBarControls();

            dispX = new Switch() { OnColor = Color.GreenYellow, IsToggled = true, Margin = 0, VerticalOptions = LayoutOptions.CenterAndExpand };
            dispX.Toggled += DisplacementToggled;
            dispY = new Switch() { OnColor = Color.GreenYellow, IsToggled = true, Margin = 0, VerticalOptions = LayoutOptions.CenterAndExpand };
            dispY.Toggled += DisplacementToggled;
            dispR = new Switch() { OnColor = Color.GreenYellow, IsToggled = true, Margin = 0, VerticalOptions = LayoutOptions.CenterAndExpand };
            dispR.Toggled += DisplacementToggled;

            Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HeightRequest = 40,
                WidthRequest = 400,
                BackgroundColor = Color.White,            
                Children =
                {
	                //ButtonSupPin, ButtonSupRoller, ButtonSupFixed,
                    //ButtonHinge, ButtonRigid,
                    //new Label { Text = StringResources.NodeEditorAngleLabel, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center },
                    //SupportAngle,
                    //new Label { Text = StringResources.NodeEditorCoordinates, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center },
                    //XCoord, YCoord,
                    new Label { Text = "X:", VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center },
                    dispX,
                    new Label { Text = "Y:", VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center },
                    dispY,
                    new Label { Text = "Поворот:", VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center },
                    dispR,
                    ButtonSupClose,
                    ButtonDelete
                }
			};
		}

        private void DisplacementToggled(object sender, ToggledEventArgs e)
        {
            ComponentBasic c = modelSurface.GetSelectedComponent();
            if (c?.CompType == ComponentTypes.ctNode)
            {
                if (sender == dispX) SetDisplacements((ComponentNode)c, NodeDisplacement.X, dispX.IsToggled);
                if (sender == dispY) SetDisplacements((ComponentNode)c, NodeDisplacement.Y, dispY.IsToggled);
                if (sender == dispR) SetDisplacements((ComponentNode)c, NodeDisplacement.Rotation, dispR.IsToggled);
            }
            modelSurface.Invalidate();
        }

        private void SetDisplacements(ComponentNode c, NodeDisplacement D, bool isToggled)
        {
            if (isToggled)
                c.DisallowedDisplacements.Remove(D);
            else if (!c.DisallowedDisplacements.Contains(D)) c.DisallowedDisplacements.Add(D); 
        }

        public void InitToolBarControls()
        {
            /* ButtonSupPin = new ImageButton();
             ButtonSupRoller = new ImageButton();
             ButtonSupFixed = new ImageButton();
             ButtonHinge = new ImageButton();
             ButtonRigid = new ImageButton();
             ButtonSupPin.Pressed += ButtonSup_Clicked;
             ButtonSupRoller.Pressed += ButtonSup_Clicked;
             ButtonSupFixed.Pressed += ButtonSup_Clicked;
             ButtonHinge.Pressed += ButtonSup_Clicked;
             ButtonRigid.Pressed += ButtonSup_Clicked;*/
            ButtonSupClose = new ImageButton();
            ButtonSupClose.Pressed += ButtonSupClose_Clicked;

           /* ButtonSupPin.Source = ImageSource.FromStream(() => new MemoryStream(StringResources.PinSupport));
            ButtonSupRoller.Source = ImageSource.FromStream(() => new MemoryStream(StringResources.RollerSupport));
            ButtonSupFixed.Source = ImageSource.FromStream(() => new MemoryStream(StringResources.FixedSupport));
            ButtonHinge.Source = ImageSource.FromStream(() => new MemoryStream(StringResources.Hinge));
            ButtonRigid.Source = ImageSource.FromStream(() => new MemoryStream(StringResources.Rigid)); */
            ButtonSupClose.Source = ImageSource.FromStream(() => new MemoryStream(StringResources.IconClose));
           /* ToolbarButtonStates.SetStates(ButtonSupPin);
            ToolbarButtonStates.SetStates(ButtonSupRoller);
            ToolbarButtonStates.SetStates(ButtonSupFixed);
            ToolbarButtonStates.SetStates(ButtonHinge);
            ToolbarButtonStates.SetStates(ButtonRigid); */

            ButtonSupClose.BackgroundColor = Color.CornflowerBlue;

            ButtonDelete = new ImageButton
            {
                Source = ImageSource.FromStream(() => new MemoryStream(StringResources.delete88)),
            };

            ButtonDelete.Pressed += ButtonDelete_Pressed;
            ButtonDelete.BackgroundColor = Color.LightPink;
            //TODO: Когда проработаю алгоритм удаления узла, решу этот вопрос
            ButtonDelete.IsVisible = false;


            /* SupportAngle = new Entry()
             {
                 WidthRequest = 60, Placeholder = "90", Keyboard = Keyboard.Numeric,
                 ReturnType = ReturnType.Done
             };

             XCoord = new Entry()
             {
                 WidthRequest = 50,
                 Placeholder = "-",
                 Keyboard = Keyboard.Numeric,
                 ReturnType = ReturnType.Done
             };

             YCoord = new Entry()
             {
                 WidthRequest = 50,
                 Placeholder = "-",
                 Keyboard = Keyboard.Numeric,
                 ReturnType = ReturnType.Done
             };

             SupportAngle.Completed += AngleChanged;
             XCoord.Completed += CoordChanged;
             YCoord.Completed += CoordChanged; */
        }

        private void ButtonDelete_Pressed(object sender, EventArgs e)
        {
            ComponentBasic c = modelSurface.GetSelectedComponent();
            if (c?.CompType == ComponentTypes.ctNode)
            {
                if (modelSurface.DeleteSelectedObject())
                {
                    IsVisible = false;
                    modelSurface.IssoAction(EditorActions.None);
                }
            }
        }

        private void CoordChanged(object sender, EventArgs e)
        {
            // Изменение координат узла - оно возможно только, если у 
            // узла есть хотя бы одна степень свободы
        }

        /*private void AngleChanged(object sender, EventArgs e)
        {            
            string value = ((Entry)sender).Text.Trim();
            float angle;
            if (float.TryParse(value, out angle)) SetSupportAngle(angle);
        }

        private void SetSupportAngle(float angle)
        {
            ComponentBasic c = modelSurface.GetSelectedComponent();
            if (c?.CompType == ComponentTypes.ctNode)
            {
                ((ComponentNode)c).Angle = angle;
                EditNode();
                modelSurface.OnSelectedComponentChanged();
            }
        }*/

        /*private void SetNodeType(NodeType NType)
        {
            ComponentBasic c = modelSurface.GetSelectedComponent();
            if (c?.CompType == ComponentTypes.ctNode)
            {
                ((ComponentNode)c).Type = NType;
                EditNode();
                modelSurface.OnSelectedComponentChanged();
            }
        }*/

        /*private void ButtonSup_Clicked(object sender, EventArgs e)
        {
            if (sender == ButtonSupPin) SetNodeType(NodeType.Pin2d);
            if (sender == ButtonSupRoller) SetNodeType(NodeType.Roller2d);
            if (sender == ButtonSupFixed) SetNodeType(NodeType.Fixed2d);
            if (sender == ButtonHinge) SetNodeType(NodeType.Hinge);
            if (sender == ButtonRigid) SetNodeType(NodeType.Rigid); 
        } */

        private void ButtonSupClose_Clicked(object sender, EventArgs e)
        {
            IsVisible = false;
            modelSurface.IssoAction(EditorActions.None);
        }

        public void EditNode()
        {
            IsVisible = true;
            // Когда отображается редактор свойств опоры, 
            // считываем свойства выбранного на данный момент компонента и 
            // настраиваем начальный вид элементов редактора соответствующим образом
            ComponentBasic c = modelSurface.GetSelectedComponent();
            if (c?.CompType == ComponentTypes.ctNode)
            {
                ComponentNode node = (ComponentNode)c;
                dispX.IsToggled = !node.DisallowedDisplacements.Contains(NodeDisplacement.X);
                dispY.IsToggled = !node.DisallowedDisplacements.Contains(NodeDisplacement.Y);
                dispR.IsToggled = !node.DisallowedDisplacements.Contains(NodeDisplacement.Rotation);
                /* VisualStateManager.GoToState(ButtonSupPin, "Normal");
                 VisualStateManager.GoToState(ButtonSupRoller, "Normal");
                 VisualStateManager.GoToState(ButtonSupFixed, "Normal");
                 VisualStateManager.GoToState(ButtonHinge, "Normal");
                 VisualStateManager.GoToState(ButtonRigid, "Normal"); 
                 switch (((ComponentNode)c).Type)
                 {
                     case NodeType.Pin2d: VisualStateManager.GoToState(ButtonSupPin, "Pressed"); break;
                     case NodeType.Roller2d: VisualStateManager.GoToState(ButtonSupRoller, "Pressed"); break;
                     case NodeType.Fixed2d: VisualStateManager.GoToState(ButtonSupFixed, "Pressed"); break;
                     case NodeType.Hinge: VisualStateManager.GoToState(ButtonHinge, "Pressed"); break;
                     case NodeType.Rigid: VisualStateManager.GoToState(ButtonRigid, "Pressed"); break;
                 }
                 string Angle = ((ComponentNode)c).Angle.ToString("G0");            
                 SupportAngle.Text = Angle;
                 XCoord.Text = ((ComponentNode)c).Location.X.ToString("G2");
                 YCoord.Text = ((ComponentNode)c).Location.Y.ToString("G2"); */
            }
        }
    }
}