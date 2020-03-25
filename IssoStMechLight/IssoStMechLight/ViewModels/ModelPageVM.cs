using IssoStMechLight.Models;
using IssoStMechLight.Resources;
using IssoStMechLight.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace IssoStMechLight.ViewModels
{
    public class ModelPageVM: INotifyPropertyChanged
    {
        public string PageTitle
        { get
            {
                MainPage mp = (MainPage)Application.Current.MainPage;
                if ((mp != null) && (mp.FileManager.FileName() != ""))
                    return mp.FileManager.FileName();
                else
                    return StringResources.MenuItemModel;
            }
        }
        public string ButtonLinearCaption
        { get { return StringResources.ModelActionLinearComponent; }}
        public string ButtonSupportCaption
        { get { return StringResources.ModelActionSupport; }}
        public string ButtonHingeCaption
        { get { return StringResources.ModelActionHinge; } }
        public string ButtonDimCaption
        { get { return StringResources.ModelActionDim; } }
        public string ButtonDimVCaption
        { get { return StringResources.ModelActionDimV; } }
        public string ButtonSupportTypeLabel
        { get { return StringResources.ButtonSupportTypeLabel; } }
        public string ButtonForceCaption
        { get { return StringResources.ButtonForceLabel; } }
        public string ButtonDstLoadCaption
        { get { return StringResources.ButtonDstLoadLabel; } }
        public string ButtonNewCaption
        { get { return StringResources.ModelNewLabel; } }
        public string ButtonCalcCaption
        {
            get
            {
                return StringResources.ModelCalcCaption;
            }
        }

        public ImageSource SelectFrameImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.SelectFrame)); } }
        public ImageSource CopyImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.Copy)); } }
        public ImageSource ArrayImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.Array)); } }
        public ImageSource MirrorImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.Mirror)); } }

        public ImageSource NewModelImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.NewModel)); } }
        public ImageSource LinearImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.Linear)); } }
        public ImageSource HorizDimensionImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.HorizDimension)); } }
        public ImageSource VertDimensionImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.VertDimension)); } }
        public ImageSource ForceImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.Force)); } }
        public ImageSource DistLoadImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.DistLoad)); } }
        public ImageSource CalculationImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.Calculation)); } }
        public ImageSource ZoomAllImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.ZoomAll)); } }
        public ImageSource ZoomInImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.ZoomIn)); } }
        public ImageSource ZoomOutImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.ZoomOut)); } }
        public ImageSource SaveImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.Save)); } }
        public ImageSource OpentImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.Open)); } }
        public ImageSource UndoImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.Undo)); } }
        public ImageSource RedoImage { get { return ImageSource.FromStream(() => new MemoryStream(StringResources.Redo)); } }

        public VisualStateGroupList ButtonsVSM { get { return ToolbarButtonStates.TBS; } }

        public int SelectedCount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
