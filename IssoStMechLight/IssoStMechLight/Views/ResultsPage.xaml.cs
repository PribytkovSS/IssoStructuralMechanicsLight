using IssoStMechLight.Models;
using IssoStMechLight.ViewModels;
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
	public partial class ResultsPage : ContentPage
	{
        private RodModelVM ModelVM;
        private Grid[] grids;

        public ResultsPage (RodModelVM rodModelVM)
		{
			InitializeComponent ();
            ModelVM = rodModelVM;
            grids = new Grid[4] { DisplacementsGrid, MomentsGrid, ShearForceGrid, AxialForceGrid };
        }

        protected override void OnAppearing()
        {
            ShowResults();
        }

        private void ShowResults()
        {
            ClearGrids();
            for (int g = 0; g < grids.Length; g++)
            {
                for (int i = 0; i < ModelVM.model.Rods.Count; i++)
                {
                    double l = ModelVM.model.Rods[i].Length;
                    // Номер элемента
                    grids[g].Children.Add(new Label
                    {
                        Text = i.ToString(),
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        BackgroundColor = Color.White
                    }, 0, 1, 2 + i, 3 + i);

                    // Номер узла в начале элемента
                    grids[g].Children.Add(new Label
                    {
                        Text = ModelVM.model.Rods[i].Node1Index.ToString(),
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        BackgroundColor = Color.White
                    }, 1, 2, 2 + i, 3 + i);

                    // Номер узла в конце элемента
                    grids[g].Children.Add(new Label
                    {
                        Text = ModelVM.model.Rods[i].Node2Index.ToString(),
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        BackgroundColor = Color.White
                    }, 2, 3, 2 + i, 3 + i);

                    for (int x = 0; x < 11; x++)
                    {
                        switch (g)
                        {
                            // Перемещения
                            case 0:
                                {
                                    double[] f = ModelVM.model.Rods[i].DeformedShape((float)(l * x) / 10);
                                    string dx = f[0].ToString("F1");
                                    string dy = f[1].ToString("F1");
                                    string r = f[2].ToString("F1");
                                    grids[g].Children.Add(new Label
                                            {
                                                Text = dx + ";" + Environment.NewLine + 
                                                       dy + ";" + Environment.NewLine + r,
                                                VerticalOptions = LayoutOptions.FillAndExpand,
                                                VerticalTextAlignment = TextAlignment.Center,
                                                HorizontalTextAlignment = TextAlignment.Center,
                                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                                BackgroundColor = Color.White
                                    }, 3 + x, 4 + x, 2 + i, 3 + i);
                                    break;
                                }
                            // Моменты
                            case 1:
                                {
                                    double m = ModelVM.model.Rods[i].BendingMoment((float)(l * x) / 10);
                                    grids[g].Children.Add(new Label
                                    {
                                        Text = m.ToString("F1"),
                                        VerticalOptions = LayoutOptions.FillAndExpand,
                                        VerticalTextAlignment = TextAlignment.Center,
                                        HorizontalTextAlignment = TextAlignment.Center,
                                        HorizontalOptions = LayoutOptions.FillAndExpand,
                                        BackgroundColor = Color.White
                                    }, 3 + x, 4 + x, 2 + i, 3 + i);
                                    break;
                                }
                            // Поперечная сила
                            case 2:
                                {
                                    double q = ModelVM.model.Rods[i].ShearForce((float)(l * x) / 10);
                                    grids[g].Children.Add(new Label
                                    {
                                       Text = q.ToString("F1"),
                                        VerticalOptions = LayoutOptions.FillAndExpand,
                                        VerticalTextAlignment = TextAlignment.Center,
                                        HorizontalTextAlignment = TextAlignment.Center,
                                        HorizontalOptions = LayoutOptions.FillAndExpand,
                                        BackgroundColor = Color.White
                                    }, 3 + x, 4 + x, 2 + i, 3 + i);
                                    break;
                                }
                            // Продольная сила
                            case 3:
                                {
                                    double n = ModelVM.model.Rods[i].AxialForce((float)(l * x) / 10);
                                    grids[g].Children.Add(new Label
                                    {
                                        Text = n.ToString("F1"),
                                        VerticalOptions = LayoutOptions.FillAndExpand,
                                        VerticalTextAlignment = TextAlignment.Center,
                                        HorizontalTextAlignment = TextAlignment.Center,
                                        HorizontalOptions = LayoutOptions.FillAndExpand,
                                        BackgroundColor = Color.White
                                    }, 3 + x, 4 + x, 2 + i, 3 + i);
                                    break;
                                }
                        }
                    }
                }
            }
        }

        private void ClearGrids()
        {
            DisplacementsGrid.Children.Clear();
            MomentsGrid.Children.Clear();
            ShearForceGrid.Children.Clear();
            AxialForceGrid.Children.Clear();

            for (int g = 0; g < grids.Length; g++)
            {
                grids[g].RowDefinitions.Clear();
                grids[g].ColumnDefinitions.Clear();
                grids[g].HorizontalOptions = LayoutOptions.FillAndExpand;

                for (int i = 0; i < ModelVM.model.Rods.Count + 2; i++)
                {
                    grids[g].RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                }

                int s = 2;
                for (int c = 0; c < 14; c++)
                {
                    if (c > 2) s = 1;
                    grids[g].ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(s, GridUnitType.Star) });
                }

                grids[g].Children.Add(new Label()
                {
                    Text = "№",
                    VerticalOptions =LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    BackgroundColor = Color.CornflowerBlue,
                    TextColor = Color.White
                }, 0, 1, 0, 2);

                grids[g].Children.Add(new Label()
                {
                    Text = "Узел начала",
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    BackgroundColor = Color.CornflowerBlue,
                    TextColor = Color.White
                }, 1, 2, 0, 2);

                grids[g].Children.Add(new Label()
                {
                    Text = "Узел конца",
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    BackgroundColor = Color.CornflowerBlue,
                    TextColor = Color.White
                }, 2, 3, 0, 2);

                grids[g].Children.Add(new Label()
                {
                    Text = "Расположение поперечного сечения",
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    BackgroundColor = Color.LightGray,
                    TextColor = Color.Black
                }, 3, 14, 0, 1);
                
                for (int x = 0; x < 11; x++)
                {
                    grids[g].Children.Add(new Label()
                    {
                        Text = ((double)x/10).ToString("F1"),
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        BackgroundColor = Color.LightGray,
                        TextColor = Color.Black
                    }, 3+x, 4+x, 1, 2);
                }
            }
        }
    }
}