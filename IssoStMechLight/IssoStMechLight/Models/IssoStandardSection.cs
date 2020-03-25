using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace IssoStMechLight.Models
{
    public class IssoStandardSection : BindableObject
    {
        public static readonly BindableProperty SectionAreaProperty =
            BindableProperty.Create("SectionArea", typeof(double), typeof(IssoStandardSection), default(double), BindingMode.TwoWay);

        public static readonly BindableProperty SectionInertiaProperty =
            BindableProperty.Create("SectionInertia", typeof(double), typeof(IssoStandardSection), default(double), BindingMode.TwoWay);

        public static readonly BindableProperty SectionNameProperty =
             BindableProperty.Create("SectionName", typeof(string), typeof(IssoStandardSection), default(string), BindingMode.TwoWay);
        
        public string SectionName
        {
            get { return (string)GetValue(SectionNameProperty); }
            set { SetValue(SectionNameProperty, value); }
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
    }
}
