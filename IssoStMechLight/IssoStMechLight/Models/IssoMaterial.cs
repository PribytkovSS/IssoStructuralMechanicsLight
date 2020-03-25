using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace IssoStMechLight.Models
{
    public class IssoMaterial : BindableObject
    {
        public static readonly BindableProperty MaterialNameProperty =
           BindableProperty.Create("MaterialName", typeof(string), typeof(IssoCrossSection), default(string), BindingMode.TwoWay);

        public static readonly BindableProperty MaterialElasticityProperty =
           BindableProperty.Create("MaterialElasticity", typeof(double), typeof(IssoCrossSection), default(double), BindingMode.TwoWay);


        public string MaterialName
        {
            get { return (string)GetValue(MaterialNameProperty); }
            set { SetValue(MaterialNameProperty, value); }
        }

        public double MaterialElasticity
        {
            get { return (double)GetValue(MaterialElasticityProperty); }
            set { SetValue(MaterialElasticityProperty, value); }
        }

        public IssoMaterial(string name, double eModulus)
        {
            MaterialName = name;
            MaterialElasticity = eModulus;
        }

        public override string ToString()
        {
            return String.Format("{0}, E={1} Па", MaterialName, MaterialElasticity.ToString("G2"));
        }
    }
}
