using IssoStMechLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;

namespace IssoStMechLight.Models
{
    public class MaterialLib
    {
        public readonly ObservableCollection<IssoMaterial> issoMaterials;
        public string LibraryName = "Material Lib";

        private static string GetAttrValue(XmlAttribute xmlAttribute, string default_value)
        {
            if (xmlAttribute != null) return xmlAttribute.Value; else return default_value;
        }

        public static async Task<ObservableCollection<MaterialLib>> CreateMaterialsCollection(string fileName)
        {
            ObservableCollection<MaterialLib> result = new ObservableCollection<MaterialLib>();
            MainPage mp = (MainPage)Application.Current.MainPage;
            MemoryStream ms = new MemoryStream();
            switch (Device.RuntimePlatform)
            {
                case Device.UWP: await mp.FileManager.ReadFileToStream(ms, "Libraries", fileName); break;
                default: return null;
            }
            XmlDocument doc = new XmlDocument();
            ms.Seek(0, SeekOrigin.Begin);
            doc.Load(ms);
            for (int i = 0; i < doc.DocumentElement.ChildNodes.Count; i++)
            {
                XmlNode n = doc.DocumentElement.ChildNodes[i];
                if (n.Name == "x:Libraries")
                {
                    for (int j = 0; j < n.ChildNodes.Count; j++)
                    {
                        XmlNode node = n.ChildNodes[j];
                        string libName = GetAttrValue(node.Attributes["x:Name"], "libName");
                        MaterialLib lib = new MaterialLib(libName);
                        for (int k = 0; k < node.ChildNodes.Count; k++)
                        {
                            lib.AppendMaterial(GetAttrValue(node.ChildNodes[k].Attributes["x:Name"], "MaterialName"),
                                               double.Parse(GetAttrValue(node.ChildNodes[k].Attributes["x:EModulus"], "1")));   
                        }
                        result.Add(lib);
                    }
                }
            }
            return result;
        }

        public void AppendMaterial(string matName, double eModulus)
        {
            issoMaterials.Add(new IssoMaterial(matName, eModulus));
        }

        public override string ToString()
        {
            return LibraryName;
        }

        public MaterialLib(string libName)
        {
            issoMaterials = new ObservableCollection<IssoMaterial>();
            LibraryName = libName;
        }
    }
}
