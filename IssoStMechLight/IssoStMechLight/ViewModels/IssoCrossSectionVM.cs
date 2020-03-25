using IssoStMechLight.Models;
using IssoStMechLight.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace IssoStMechLight.Views
{
    class IssoCrossSectionVM
    {
        public static void Draw(IssoCrossSection section, SKCanvas canvas, float ScaleFactor, IssoPoint2D Origin)
        {
            SKPath secPath = new SKPath();
            SKMatrix scale = SKMatrix.MakeScale(ScaleFactor, ScaleFactor);            
            SKMatrix translate = SKMatrix.MakeTranslation(- Origin.X * ScaleFactor, - Origin.Y * ScaleFactor);

            secPath.AddPath(section.SectionContour);
            secPath.Transform(scale);
            secPath.Transform(translate);

            canvas.DrawPath(secPath, new SKPaint()
                {
                    Style = SKPaintStyle.StrokeAndFill,
                    Color = Color.DarkBlue.ToSKColor(),
                    IsAntialias = true,
                    StrokeWidth = 3
                } 
            );

            canvas.DrawPath(secPath, new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.LightSkyBlue.ToSKColor(),
                IsAntialias = true,
                StrokeWidth = 3
            }
            );
        }

    }
}