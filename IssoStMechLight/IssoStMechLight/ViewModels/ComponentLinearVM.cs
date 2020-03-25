using IssoStMechLight.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using IssoStMechLight.Views;

namespace IssoStMechLight.ViewModels
{
    public static class ComponentLinearVM
    {
        public static void Draw(ComponentLinear linear, ModelViewSurface surface, SKCanvas canvas)
        {
            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.WhiteSmoke.ToSKColor(),
                IsAntialias = true,
                StrokeWidth = 2
            };

            if (linear.CompState == ComponentState.csSelected)
            {
                paint.StrokeWidth += 3;
                paint.Color = Color.CornflowerBlue.ToSKColor();
            }
              
            SKPoint pt1 = IssoConvert.IssoPoint2DToSkPoint(linear.Start, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
            SKPoint pt2 = IssoConvert.IssoPoint2DToSkPoint(linear.End, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
            canvas.DrawLine(pt1, pt2, paint);

            SKPaint fillPaint = new SKPaint() { Style = SKPaintStyle.Fill, Color = Color.Black.ToSKColor(), IsAntialias = true };
            float hr = surface.ViewHeight / 110;
            double a = Math.Atan2(pt2.Y - pt1.Y, pt2.X - pt1.X);

            if (linear.HingeStart)
            {                
                canvas.DrawCircle(pt1.X + hr * (float)Math.Cos(a), pt1.Y + hr * (float)Math.Sin(a), hr, fillPaint);
                canvas.DrawCircle(pt1.X + hr * (float)Math.Cos(a), pt1.Y + hr * (float)Math.Sin(a), hr, paint);
            }
            if (linear.HingeEnd)
            {
                canvas.DrawCircle(pt2.X + hr * (float)Math.Cos(a + Math.PI), pt2.Y + hr * (float)Math.Sin(a + Math.PI), hr, fillPaint);
                canvas.DrawCircle(pt2.X + hr * (float)Math.Cos(a + Math.PI), pt2.Y + hr * (float)Math.Sin(a + Math.PI), hr, paint);
            }
        }

        public static bool Contains(ComponentLinear linear, IssoPoint2D pt, ModelViewSurface surface)
        {
            SKPath pin = new SKPath();
            // Определим начало координат - это всегда точка, расположенная левее
            IssoPoint2D ptstart, ptend;
            if (linear.Start.X < linear.End.X)
            {
                ptstart = linear.Start;
                ptend = linear.End;
            }
            else
            {
                ptstart = linear.End;
                ptend = linear.Start;
            }
            double ang = Math.Asin((ptend.Y - ptstart.Y) / linear.Length);
            float dy = 7 / surface.scaleFactor;
            pin.MoveTo(ptstart.X, ptstart.Y - dy);
            pin.LineTo(ptstart.X, ptstart.Y + dy);
            pin.LineTo(ptstart.X + linear.Length, ptstart.Y + dy);
            pin.LineTo(ptstart.X + linear.Length, ptstart.Y - dy);
            pin.Close();
            pin.Transform(SKMatrix.MakeRotation((float)ang, ptstart.X, ptstart.Y));
            return pin.Contains(pt.X, pt.Y);
        }
    }
}
