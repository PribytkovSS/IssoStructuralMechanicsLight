using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace IssoStMechLight.Models
{
    public enum SectionType { Rectangle, Round, H, T, Box, Circle, Library, UserValues};

    public class IssoCrossSection: BindableObject
    {
        // Сечение - это набор взаимно связанных стандартных элементов
        public List<IssoCrossSectionElement> SectionElements = new List<IssoCrossSectionElement>();
        public SKPath SectionContour = new SKPath();

        public static readonly BindableProperty SectionAreaProperty =
            BindableProperty.Create("SectionArea", typeof(double), typeof(IssoCrossSection), default(double), BindingMode.TwoWay);

        public static readonly BindableProperty SectionInertiaProperty =
            BindableProperty.Create("SectionInertia", typeof(double), typeof(IssoCrossSection), default(double), BindingMode.TwoWay);

        public static readonly BindableProperty MaterialElasticityProperty =
            BindableProperty.Create("MaterialElasticity", typeof(double), typeof(IssoCrossSection), default(double), BindingMode.TwoWay);

        public static readonly BindableProperty SectionNameProperty =
             BindableProperty.Create("SectionName", typeof(string), typeof(IssoCrossSection), default(string), BindingMode.TwoWay);

        
        public string SectionName
        {
            get { return (string)GetValue(SectionNameProperty); }
            set
            {
                SetValue(SectionNameProperty, value);
            }
        }

        public override string ToString()
        {
            return SectionName;
        }

        public double SectionArea
        {
            get { return (double)GetValue(SectionAreaProperty); }
            set { SetValue(SectionAreaProperty, value); }
        }

        public double SectionInertia
        {
            get { return (double)GetValue(SectionInertiaProperty); }
            set { SetValue(SectionInertiaProperty, value); }
        }

        public double MaterialElasticity
        {
            get { return (double)GetValue(MaterialElasticityProperty); }
            set { SetValue(MaterialElasticityProperty, value); }
        }

        internal void AddElement(SectionType sectionType, int StandardId = -1)
        {
            SectionElements.Add(new IssoCrossSectionElement(sectionType));
            MakeCommonPath();
        }

        public IssoCrossSection()
        {
            SectionName = "Section";
            SectionArea = 1.0;
            SectionInertia = 1.0;
            MaterialElasticity = 1.0;
        }

        private void MakeCommonPath()
        {
            SectionContour = new SKPath();
            for (int i = 0; i < SectionElements.Count; i++) SectionContour.AddPath(SectionElements[i].contour);

        }
    }
}
