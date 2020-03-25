using IssoStMechLight.Models;
using IssoStMechLight.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;
using System.Text;

namespace IssoStMechLight.ViewModels
{
    public static class IssoBindingVM
    {
        // Линейный размер состоит из двух размерных линий, начинающихся у узлов
        // Горизонтальной (или вертикальной) линии
        public static void DrawDimension(IssoBinding bin, ModelViewSurface surface, SKCanvas canvas)
        {
            SKPath dim = new SKPath();
            SKPoint pt1 = IssoConvert.IssoPoint2DToSkPoint(((ComponentNode)bin.Source).Location, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
            SKPoint pt2;
            SKPoint lp = IssoConvert.IssoPoint2DToSkPoint(bin.LinePlace, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
            SKPoint vLp = new SKPoint() { X = lp.X, Y = lp.Y };
            SKPoint vpt1 = new SKPoint() { X = pt1.X, Y = pt1.Y };
            
            if (bin.Target != null)
            {
                pt2 = IssoConvert.IssoPoint2DToSkPoint(((ComponentNode)bin.Target).Location, surface.ScaleFactor, surface.Origin, surface.ViewHeight);              
            } else
            {
                pt2 = new SKPoint() { X = lp.X, Y = lp.Y };
            }

            SKPoint vpt2 = new SKPoint() { X = pt2.X, Y = pt2.Y };

            // Если размер - вертикальный, то сначала повернём все точки, чтобы рисовать его также, как и горизонтальный, 
            // а затем развернём его снова
            if (bin.Type == IssoBindingType.Vertical)
            {
                SKPath ptPath = new SKPath();
                ptPath.AddPoly(new SKPoint[] { pt1, pt2, lp }, false);
                SKMatrix rmatrix = SKMatrix.MakeRotationDegrees(90, pt1.X, pt1.Y);
                ptPath.Transform(rmatrix);
                pt1 = ptPath.GetPoint(0);
                pt2 = ptPath.GetPoint(1);
                lp = ptPath.GetPoint(2);
            }
              
            float f1 = Math.Sign(lp.Y - pt1.Y);
            float f2 = Math.Sign(lp.Y - pt2.Y);

            // Одна размерная линия
            dim.MoveTo(pt1.X, pt1.Y + f1 * 5);
            dim.LineTo(pt1.X, lp.Y + f1 * 5);

            // Вторая размерная линия
            dim.MoveTo(pt2.X, pt2.Y + f2 * 5);
            dim.LineTo(pt2.X, lp.Y + f2 * 5);

            // Горизонтальная линия
            dim.MoveTo(pt1.X, lp.Y);
            dim.LineTo(pt2.X, lp.Y);

            // Одна стрелка
            float a = Math.Sign(pt2.X - pt1.X);
            dim.MoveTo(pt1.X, lp.Y);
            dim.LineTo(pt1.X + a * 5, lp.Y + 3);
            dim.MoveTo(pt1.X, lp.Y);
            dim.LineTo(pt1.X + a * 5, lp.Y - 3);

            // Вторая стрелка
            dim.MoveTo(pt2.X, lp.Y);
            dim.MoveTo(pt2.X, lp.Y);
            dim.LineTo(pt2.X - a * 5, lp.Y + 3);
            dim.MoveTo(pt2.X, lp.Y);
            dim.LineTo(pt2.X - a * 5, lp.Y - 3);

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.AliceBlue.ToSKColor(),
                IsAntialias = true,
                StrokeWidth = 1,
                TextAlign = SKTextAlign.Center,
                TextSize = 16
            };

            // Текcт размера
            string ds = (bin.Value * surface.ScaleFactor).ToString("#.00");

            if (bin.Type == IssoBindingType.Vertical)
            {
                SKMatrix rmatrix = SKMatrix.MakeRotationDegrees(-90, pt1.X, pt1.Y);
                dim.Transform(rmatrix);
                SKRect bounds = new SKRect();
                paint.MeasureText(ds, ref bounds);

                SKBitmap bmp = new SKBitmap((int)(Math.Max(bounds.Width, bounds.Height)), (int)(Math.Max(bounds.Width, bounds.Height)));
                SKCanvas k = new SKCanvas(bmp);              
                k.RotateDegrees(-90, bmp.Width / 2, bmp.Height / 2);
                k.DrawText(ds, bmp.Width / 2, bounds.Height, paint);
                canvas.DrawBitmap(bmp, vLp.X - 10 - bounds.Height, (vpt1.Y + vpt2.Y) / 2, paint);
            } else
            {
                canvas.DrawText(ds, (pt1.X + pt2.X) / 2, lp.Y - 10, paint);
            }

            canvas.DrawPath(dim, paint);
        }

        public static bool Contains(IssoBinding binding, IssoPoint2D pt, ModelViewSurface surface)
        {
            if (binding.Target == null) return false;

            switch (binding.Type)
            {
                case IssoBindingType.Horizontal: return ContainsH(binding, pt, surface);
                case IssoBindingType.Vertical: return ContainsV(binding, pt, surface);
                default: return false;
            }     
        }

        private static bool ContainsV(IssoBinding binding, IssoPoint2D pt, ModelViewSurface surface)
        {
            float bottomY = Math.Max(binding.Source.Location.Y, binding.Target.Location.Y);
            float topY = Math.Min(binding.Source.Location.Y, binding.Target.Location.Y);

            // Прямоугольник вогруг одной из размерных линий
            SKRect r = new SKRect()
            {
                Bottom = binding.Source.Location.Y + 5,
                Right = Math.Max(binding.Source.Location.X, binding.LinePlace.X) + 5,
                Top = binding.Source.Location.Y - 5,
                Left = Math.Min(binding.Source.Location.X, binding.LinePlace.X) - 5
            };

            if (r.Contains(pt.X, pt.Y)) return true;

            // Прямоугольник вокруг горизонтальной линии
            r.Top = topY;
            r.Bottom = bottomY;
            r.Left = binding.LinePlace.X - 5; 
            r.Right = binding.LinePlace.X + 5;

            if (r.Contains(pt.X, pt.Y)) return true;

            // Прямоугольник вогруг второй размерной линии
            r.Bottom = binding.Target.Location.Y + 5;
            r.Right = Math.Max(binding.Target.Location.X, binding.LinePlace.X) + 5;
            r.Top = binding.Target.Location.Y - 5;
            r.Left = Math.Min(binding.Target.Location.X, binding.LinePlace.X) - 5;

            if (r.Contains(pt.X, pt.Y)) return true;

            // Прямоугольник вогруг текста размера
            float middle = (topY + bottomY) / 2;
            string dim = binding.Value.ToString("G0");
            float tsize = dim.Length * 16;
            r.Bottom = middle + tsize / 2 + 5;
            r.Top = middle - tsize / 2 - 5;
            r.Right = binding.LinePlace.X;
            r.Left = binding.LinePlace.X - 20;

            if (r.Contains(pt.X, pt.Y)) return true;

            return false;
        }

        private static bool ContainsH(IssoBinding binding, IssoPoint2D pt, ModelViewSurface surface)
        {
            float leftX = Math.Min(binding.Source.Location.X, binding.Target.Location.X);
            float rightX = Math.Max(binding.Source.Location.X, binding.Target.Location.X);

            // Прямоугольник вогруг одной из размерных линий
            SKRect r = new SKRect()
            {
                Left = binding.Source.Location.X - 5,
                Right = binding.Source.Location.X + 5,
                Top = Math.Min(binding.Source.Location.Y, binding.LinePlace.Y),
                Bottom = Math.Max(binding.Source.Location.Y, binding.LinePlace.Y)
            };

            if (r.Contains(pt.X, pt.Y)) return true;

            // Прямоугольник вокруг горизонтальной линии
            r.Top = binding.LinePlace.Y - 5;
            r.Bottom = binding.LinePlace.Y + 5;
            r.Left = leftX - 5;
            r.Right = rightX + 5;

            if (r.Contains(pt.X, pt.Y)) return true;

            // Прямоугольник вогруг второй размерной линии
            r.Left = binding.Target.Location.X - 5;
            r.Right = binding.Target.Location.X + 5;
            r.Top = Math.Min(binding.Target.Location.Y, binding.LinePlace.Y);
            r.Bottom = Math.Max(binding.Target.Location.Y, binding.LinePlace.Y);

            if (r.Contains(pt.X, pt.Y)) return true;

            // Прямоугольник вогруг текста размера
            float middle = (leftX + rightX) / 2;
            string dim = binding.Value.ToString("G0");
            float tsize = dim.Length * 16;
            r.Left = middle - tsize / 2 - 5;
            r.Right = middle + tsize / 2 + 5;
            r.Top = binding.LinePlace.Y;
            r.Bottom = binding.LinePlace.Y + 20;

            if (r.Contains(pt.X, pt.Y)) return true;

            return false;
        }
    }
}
