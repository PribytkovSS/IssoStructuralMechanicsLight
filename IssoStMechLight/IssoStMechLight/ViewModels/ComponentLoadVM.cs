using IssoStMechLight.Models;
using IssoStMechLight.Views;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace IssoStMechLight.ViewModels
{
    class ComponentLoadVM
    {
        public static void Draw(ComponentLoad load, ModelViewSurface surface, SKCanvas canvas)
        {
            switch (load.Type)
            {
                case ComponentTypes.ctDistributedLoad: DrawDistributed(load, surface, canvas); break;
                case ComponentTypes.ctForce: DrawConcentrated(load, surface, canvas); break;
                case ComponentTypes.ctMoment: DrawMoment(load, surface, canvas); break;
            }
        }

        private static void DrawMoment(ComponentLoad load, ModelViewSurface surface, SKCanvas canvas)
        {
            // 
        }

        private static void DrawConcentrated(ComponentLoad load, ModelViewSurface surface, SKCanvas canvas)
        {
            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = Color.PaleVioletRed.ToSKColor(),
                IsAntialias = true,
                StrokeWidth = 1
            };

            SKPath dc = GetConcentrated(load, surface.ViewHeight);            
            dc.Transform(SKMatrix.MakeRotationDegrees(-load.Direction));
            SKPoint pt = IssoConvert.IssoPoint2DToSkPoint(load.AppNodes[0].Location, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
            dc.Transform(SKMatrix.MakeTranslation(pt.X, pt.Y));
            canvas.DrawPath(dc, paint);
        }

        private static void DrawDistributed(ComponentLoad load, ModelViewSurface surface, SKCanvas canvas)
        {
            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = Color.Red.ToSKColor(),
                IsAntialias = true,
                StrokeWidth = 1
            };

            SKPath dc = GetEquallyDistributed(load, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
            canvas.DrawPath(dc, paint);
        }

        private static SKMatrix TRMatrix(ComponentLoad load, float scaleFactor, IssoPoint2D origin, float ViewHeight, out SKMatrix rmatrix)
        {
            SKPoint point = IssoConvert.IssoPoint2DToSkPoint(load.AppNodes[0].Location, scaleFactor, origin, ViewHeight);
            SKMatrix matrix = new SKMatrix();
            matrix.SetScaleTranslate(1, 1, point.X, point.Y);

            rmatrix = SKMatrix.MakeRotationDegrees(load.Direction + 90, point.X, point.Y);

            return matrix;
        }

        private static SKPath GetDistibutedArrow(bool reverse)
        {
            SKPath p = new SKPath();

            // Если не reverse - То острие стрелки расположено в точке 0, 0
            // В противном случае эти координаты - у начала стреки 
            if (!reverse)
            {
                p.MoveTo(0, 0);
                p.LineTo(- 1f / 6f, - 1f / 5f);
                p.LineTo(1f / 6f, - 1f / 5f);
                p.LineTo(0, 0);
                p.MoveTo(0, - 1f / 5f);
                p.LineTo(0, - 1);
            } else
            {
                p.MoveTo(0, -1);
                p.LineTo(-1f / 6f, -4f / 5f);
                p.LineTo(1f / 6f, -4f / 5f);
                p.LineTo(0, -1);
                p.MoveTo(0, -4f / 5f);
                p.LineTo(0, 0);
            }

            p.Transform(SKMatrix.MakeRotation((float)Math.PI / 2));

            return p;
        }

        private static SKPath GetEquallyDistributed(ComponentLoad load, float scaleFactor, IssoPoint2D origin, float ViewHeight)
        {
            SKPath p = new SKPath();
            float Angle = load.Direction * (float)Math.PI / 180f;
            float length = IssoDist.PointDst(load.AppNodes[0].Location, load.AppNodes[1].Location);
            float dx = (load.AppNodes[1].Location.X - load.AppNodes[0].Location.X) * scaleFactor;
            float dy = -(load.AppNodes[1].Location.Y - load.AppNodes[0].Location.Y) * scaleFactor;

            float ArrawHeight = ViewHeight / 30;
            float step = ViewHeight / 70;

            int stepcnt = Math.Max((int)Math.Round(length * scaleFactor / step, 0), 1);
            SKPoint start = IssoConvert.IssoPoint2DToSkPoint(load.AppNodes[0].Location, scaleFactor, origin, ViewHeight); 
            dx = dx / stepcnt;
            dy = dy / stepcnt;

            SKMatrix rotate = SKMatrix.MakeRotation(-Angle);             

            for (int i = 0; i < stepcnt + 1; i++)
            {
                SKPath arrow = GetDistibutedArrow((load.Value > 0) || (load.isOrthogonal && load.isReverse));
                                
                arrow.Transform(SKMatrix.MakeScale(ArrawHeight, ArrawHeight));
                arrow.Transform(rotate);

                p.AddPath(arrow, start.X, start.Y);

                start.X += dx;
                start.Y += dy;
            }
            start = IssoConvert.IssoPoint2DToSkPoint(load.AppNodes[0].Location, scaleFactor, origin, ViewHeight);
            SKPoint end = IssoConvert.IssoPoint2DToSkPoint(load.AppNodes[1].Location, scaleFactor, origin, ViewHeight);

            p.MoveTo(start.X + ArrawHeight * (float)Math.Cos(Angle), 
                     start.Y - ArrawHeight * (float)Math.Sin(Angle));
            p.LineTo(end.X + ArrawHeight * (float)Math.Cos(Angle), 
                     end.Y - ArrawHeight * (float)Math.Sin(Angle));

            return p;
        }

        private static SKPath GetConcentrated(ComponentLoad load, float ViewHeight)
        {
            SKPath p = new SKPath();

            float ArrawHeight = ViewHeight / 15;
            // Положительная сила направлена слева направо или снизу вверх
            // Положительная сила - тянет. Поэтому её начало - это точка, в которой сила приложена
            // Если сила отрицательная, т. е. она толкает, и приложена в начале координат
            float s = Math.Sign(load.Value);
            if (load.Value < 0)
            {
                p.MoveTo(0, 0);
                p.LineTo(-ArrawHeight / 6, -ArrawHeight / 5);
                p.LineTo(ArrawHeight / 6, -ArrawHeight / 5);
                p.LineTo(0, 0);
                p.MoveTo(0, -ArrawHeight / 5);
                p.LineTo(0, -ArrawHeight);
            } else
            {
                p.MoveTo(0, -ArrawHeight);
                p.LineTo(-ArrawHeight / 6, -4 * ArrawHeight / 5);
                p.LineTo(ArrawHeight / 6, - 4 * ArrawHeight / 5);
                p.LineTo(0, -ArrawHeight);
                p.MoveTo(0, -4 * ArrawHeight / 5);
                p.LineTo(0, 0);
            }

            p.Transform(SKMatrix.MakeRotation((float)Math.PI / 2));

            return p;
        }

        public static bool Contains(ComponentLoad load, IssoPoint2D pt, ModelViewSurface surface)
        {           
            switch (load.CompType)
            {
                case ComponentTypes.ctDistributedLoad:
                    {
                        float ArrawHeight = surface.ViewHeight / 30;
                        SKPath b = new SKPath();
                        b.MoveTo(load.AppNodes[0].Location.X, load.AppNodes[0].Location.Y);
                        b.LineTo(load.AppNodes[0].Location.X, load.AppNodes[0].Location.Y + ArrawHeight);
                        b.LineTo(load.AppNodes[1].Location.X, load.AppNodes[1].Location.Y + ArrawHeight);
                        b.LineTo(load.AppNodes[1].Location.X, load.AppNodes[1].Location.Y);
                        b.Close();
                        return b.Contains(pt.X, pt.Y);                       
                    }
                case ComponentTypes.ctForce:
                    {
                        float ArrawHeight = surface.ViewHeight / 15;
                        SKPath b = new SKPath();
                        b.MoveTo(load.AppNodes[0].Location.X - ArrawHeight / 6, load.AppNodes[0].Location.Y);
                        b.LineTo(load.AppNodes[0].Location.X - ArrawHeight / 6, load.AppNodes[0].Location.Y - ArrawHeight);
                        b.LineTo(load.AppNodes[0].Location.X + ArrawHeight / 6, load.AppNodes[0].Location.Y - ArrawHeight);
                        b.LineTo(load.AppNodes[0].Location.X + ArrawHeight / 6, load.AppNodes[0].Location.Y);
                        b.Close();
                        SKMatrix rotate = SKMatrix.MakeRotationDegrees(load.Direction + 90, load.AppNodes[0].Location.X, load.AppNodes[0].Location.Y); ;                        
                        b.Transform(rotate);
                        return b.Contains(pt.X, pt.Y);
                    }
                default: return false;
            }            
        }
    }
}
