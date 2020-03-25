using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IssoStMechLight.Models
{
    public class IssoCrossSectionElement
    {
        public SectionType sectionType;
        public SKPath contour = new SKPath();

        public IssoCrossSectionElement(SectionType sectionType)
        {
            this.sectionType = sectionType;
            CreateDefaultContour();            
        }

        private void CreateDefaultContour()
        {
            switch (sectionType)
            {
                case SectionType.Rectangle: CreateRectangleContour(); break;
                case SectionType.Box: CreateRectangleContour(); break;
                case SectionType.Round: CreateRectangleContour(); break;
                case SectionType.Circle: CreateRectangleContour(); break;
                case SectionType.H: CreateRectangleContour(); break;
                case SectionType.T: CreateRectangleContour(); break;
            }
        }

        private void CreateRectangleContour()
        {
            // Прямоугольное сечение с начальными размерами 
            if (contour != null) contour.Dispose();
            contour = new SKPath();
            contour.AddRect(new SKRect() { Left = 0, Right = 10, Bottom = 0, Top = 10 });
        }

        public void SetContour(SKPath contour)
        {
            this.contour = contour;
        }
    }
}