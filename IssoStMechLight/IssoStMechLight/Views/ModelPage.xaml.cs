using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using IssoStMechLight.ViewModels;
using IssoStMechLight.Models;
using SkiaSharp.Views.Forms;
using IssoStMechLight.Resources;
using System.IO;

namespace IssoStMechLight.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ModelPage : ContentPage
	{
        //ModelViewSurface modelSurface;
        public RodModelVM rodModelVM;
        public string ModelFileName;
        public string ModelFileLocation;
        public ModelPageVM vm;
        static float ZOOM_STEP = 0.1f;

        // Выбранный элемент для отображения в редакторе 
        public IEnumerable<ComponentLinear> ModelBeams
        {
            get
            {
                //return from ComponentLinear l in rodModelVM.ModelBeams where l.CompState == ComponentState.csSelected select l;
                return new List<ComponentLinear>() { rodModelVM.FirstSelectedBeam };
            }
        }

        public ModelPage ()
		{
			InitializeComponent();
            vm = new ModelPageVM();
            BindingContext = vm;
            InitModelAndSurface();
            InitEditors();            
        }

        private void InitEditors()
        {
            nodeEditor.modelSurface = rodModelVM.surface;
            bindingEditor.modelSurface = rodModelVM.surface;
            loadEditor.modelSurface = rodModelVM.surface;
            arrayEditor.OnArrayCancel = OnArrayCancel;
            arrayEditor.OnArrayOK = OnArrayOK;
        }        

        private void ButtonRedo_Pressed(object sender, EventArgs e)
        {
            // Возвращаем отменённые изменения
            rodModelVM.Redo(); 
        }

        private void ButtonUndo_Pressed(object sender, EventArgs e)
        {
            // Отменяем изменения 
            rodModelVM.Undo();
        }

        private async void ButtonOpen_Pressed(object sender, EventArgs e)
        {
            MainPage mp = (MainPage)Application.Current.MainPage;
            MemoryStream ms = new MemoryStream();
            if (await mp.FileManager.ReadFileToStream(ms))
            {
                rodModelVM.Load(ms);
            }
        }

        private async void ButtonSave_Pressed(object sender, EventArgs e)
        {
            // Если задано имя файла, то просто молча сохраняем
            // Если имя пустое, то переключаемся в меню на страницу сохранения и 
            // даём пользователю ввести имя файла
            MainPage mp = (MainPage)Application.Current.MainPage;
            if ((ModelFileLocation != "") && (ModelFileName != ""))
            {
                await mp.FileManager.WriteStreamToFile(rodModelVM.Save(), mp.FileManager.FullFileName());
            } else
            {
                await mp.NavigateFromMenu(1);
            }
            
            Title = mp.FileManager.FileName();
        }        

        private void BtnZoomOut_Pressed(object sender, EventArgs e)
        {
            rodModelVM.surface.IssoAction(EditorActions.None);
            rodModelVM.surface.ScaleFactor *= (1f - ZOOM_STEP);            
        }

        private void BtnZoomIn_Pressed(object sender, EventArgs e)
        {
            rodModelVM.surface.IssoAction(EditorActions.None);
            rodModelVM.surface.ScaleFactor *= (1f + ZOOM_STEP);
        }

        private void BtnZoomAll_Pressed(object sender, EventArgs e)
        {
            rodModelVM.surface.IssoAction(EditorActions.None);
            rodModelVM.surface.ZoomAll();
        }

        private void InitModelAndSurface()
        {
            rodModelVM = new RodModelVM(new RodModel(), ModelSurface);            
            
            ModelSurface.EnableTouchEvents = true;
            ModelSurface.Touch += OnModelSurfaceTouch;            

            rodModelVM.surface.OnComponentSelected += ComponentSelected;
            rodModelVM.surface.OnActionNone += NoAction;
            rodModelVM.surface.OnVisualStates += CheckVisualStates;
            rodModelVM.surface.OnMirrorConfirm += MirrorConfirm;
        }

        private void MirrorConfirm(object sender, EventArgs e)
        {
            MirrorConfirmation.IsVisible = true;
        }

        private void CheckVisualStates(EditorActions obj)
        {
            switch (obj)
            {
                case EditorActions.NewLinearFirstPoint: VisualStateManager.GoToState(ButtonLinear, "Pressed"); break;
                case EditorActions.NewLinearLastPoint: VisualStateManager.GoToState(ButtonLinear, "Pressed"); break;
            }
        }

        private void SelectFrameBtn_Pressed(object sender, EventArgs e)
        {
            rodModelVM.surface.IssoAction(EditorActions.SelectFrame);
        }

        private bool CheckSelection()
        {            
            if (rodModelVM.SelectedBeamsCount == 0)
            {
                // Ничего не выбрано - показываем сообщение                
                MainPage mp = (MainPage)Application.Current.MainPage;
                mp.DisplayAlert("Предупреждение", "Сначала нужно выбрать объекты для отражения (рамкой или одиночно)", "Закрыть");
                rodModelVM.surface.CancelAction();
                return false;
            }
            return true;
        }

        private void ArrayBtn_Pressed(object sender, EventArgs e)
        {
            if (CheckSelection())
            {
                HideEditors();
                arrayEditor.BindingContext = rodModelVM;
                arrayEditor.IsVisible = true;
                rodModelVM.surface.IssoAction(EditorActions.ArrayElements);
            }
        }

        private void OnArrayOK(object sender, EventArgs e)
        {
            arrayEditor.IsVisible = false;
            rodModelVM.CreateArray();
            rodModelVM.surface.CancelAction();
            rodModelVM.DropSelection();
        }

        private void OnArrayCancel(object sender, EventArgs e)
        {
            arrayEditor.IsVisible = false;
            rodModelVM.surface.CancelAction();
        }

        private void CopyBtn_Pressed(object sender, EventArgs e)
        {
            if (CheckSelection())
            {
                HideEditors();
                CopyEditor.IsVisible = true;
                rodModelVM.surface.IssoAction(EditorActions.CopyElements);
            }
        }

        private void MirrorBtn_Pressed(object sender, EventArgs e)
        {
            if (CheckSelection())
            {
                HideEditors();
                rodModelVM.surface.IssoAction(EditorActions.MirrorElements);
            }
        }

        private void NoAction(object obj)
        {
            VisualStateManager.GoToState(ButtonLinear, "Normal");
            VisualStateManager.GoToState(ButtonDimension, "Normal");
            VisualStateManager.GoToState(ButtonDimensionV, "Normal");
            VisualStateManager.GoToState(ButtonForce, "Normal");
            VisualStateManager.GoToState(ButtonDstLoad, "Normal");
            VisualStateManager.GoToState(CopyBtn, "Normal");
            VisualStateManager.GoToState(ArrayBtn, "Normal");
            VisualStateManager.GoToState(SelectFrameBtn, "Normal");
            VisualStateManager.GoToState(MirrorBtn, "Normal");
            HideEditors();
        }

        private void ComponentSelected(object sender, EventArgs e)
        {
            HideEditors();
            switch (((ComponentBasic)sender)?.CompType)
            {
                case ComponentTypes.ctNode: nodeEditor.EditNode(); break;
                case ComponentTypes.ctBinding: bindingEditor.EditBinding(); break;
                case ComponentTypes.ctForce: loadEditor.EditLoad(); break;
                case ComponentTypes.ctDistributedLoad: loadEditor.EditLoad(); break;
                case ComponentTypes.ctLinear:
                    {
                        linearEditor.BindingContext = null;
                        linearEditor.BindingContext = rodModelVM;                        
                        linearEditor.IsVisible = true;
                        break;
                    }
            }
        }

        private void HideEditors()
        {
            nodeEditor.IsVisible = false;
            bindingEditor.IsVisible = false;
            loadEditor.IsVisible = false;
            linearEditor.IsVisible = false; 
            arrayEditor.IsVisible = false;
            CopyEditor.IsVisible = false;
        }

        private void OnModelSurfaceTouch(object sender, SKTouchEventArgs e)
        {
            rodModelVM.surface.Touch(e);
        }

        private void ButtonLinear_Clicked(object sender, EventArgs e)
        {
            rodModelVM.surface.IssoAction(EditorActions.NewLinearFirstPoint);            
        }

        private void ButtonSupport_Clicked(object sender, EventArgs e)
        {
            rodModelVM.surface.IssoAction(EditorActions.EditNode);
        }

        private void ButtonDim_Clicked(object sender, EventArgs e)
        {
            if (sender == ButtonDimension) rodModelVM.surface.IssoAction(EditorActions.NewDimensionFirstNode);
            if (sender == ButtonDimensionV) rodModelVM.surface.IssoAction(EditorActions.NewDimensionVFirstNode);
        }

        private void ButtonForce_Clicked(object sender, EventArgs e)
        {
            rodModelVM.surface.IssoAction(EditorActions.NewForce);
        }

        private void ButtonDstLoad_Clicked(object sender, EventArgs e)
        {
            rodModelVM.surface.IssoAction(EditorActions.NewDstLoad);
        }

        private void ButtonNew_Clicked(object sender, EventArgs e)
        {
            rodModelVM.model.CompsList.Clear();
            rodModelVM.surface.Invalidate();
        }

        private void ButtonCalc_Clicked(object sender, EventArgs e)
        {
            rodModelVM.model.CalculateStatic();
            rodModelVM.surface.DrawDeformedShape = true;
        }

        private void DeformedShapeSwitch_Toggled(object sender, ToggledEventArgs e)
        {

        }

        private void MomentsDiagramSwitch_Toggled(object sender, ToggledEventArgs e)
        {

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MainPage mp = (MainPage)Application.Current.MainPage;
            Title = mp.FileManager.FileName();
        }

        private void YesMirrorBtn_Pressed(object sender, EventArgs e)
        {
            rodModelVM.DoMirror();
            MirrorConfirmation.IsVisible = false;
        }

        private void NoMirrorBtn_Pressed(object sender, EventArgs e)
        {
            rodModelVM.surface.CancelAction();
            MirrorConfirmation.IsVisible = false;
        }

        private void NoCopyBtn_Pressed(object sender, EventArgs e)
        {
            HideEditors();
            rodModelVM.surface.CancelAction();
        }
    }
}