using IssoStMechLight.Models;
using IssoStMechLight.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace IssoStMechLight.Views
{
    class SurfaceGrid
    {
        public float ScaledStep;
        public float gridStartX, gridStartY;
        public float gridStartXbig, gridStartYbig;
        SKBitmap gridBMP;
        SKCanvas gridBuffer;
        public bool UpdateNeeded = true;

        public IssoPoint2D Snap(IssoPoint2D pt)
        {
            IssoPoint2D res = new IssoPoint2D();

            res.X = ScaledStep * (float)Math.Truncate(pt.X / ScaledStep);
            res.Y = ScaledStep * (float)Math.Truncate(pt.Y / ScaledStep);

            if ((pt.X - res.X) > (ScaledStep / 2)) res.X += ScaledStep;
            if ((pt.Y - res.Y) > (ScaledStep / 2)) res.Y += ScaledStep;

            return res;
        }

        public void DrawGrid(ModelViewSurface surface, SKCanvas canvas)
        {
            if (UpdateNeeded)
            {
                gridBMP = new SKBitmap((int)Math.Round(surface.ViewWidth), (int)Math.Round(surface.ViewHeight));
                gridBuffer = new SKCanvas(gridBMP);
                float x = surface.Origin.X;
                float y = surface.Origin.Y;
                // Найдём ближайшее кратное 
                // Количество крупных и мелких делений сетки всегда примерно определённое.
                // Берём ширину окна просмотра и делим её на сто частей
                float stepx = surface.ViewWidth / 100;
                // Определим, скольки линейным единицам модели равен этот шаг
                // И округлим его
                ScaledStep = (float)Math.Round(stepx / surface.ScaleFactor, 1);
                // Теперь найдём кратное ScaledStep число, ближайшее к surface.Origin
                gridStartX = (float)Math.Floor(x / ScaledStep) * ScaledStep;
                gridStartY = (float)Math.Floor(y / ScaledStep) * ScaledStep;
                if (gridStartX < x) gridStartX += ScaledStep;
                if (gridStartY < y) gridStartY += ScaledStep;
                gridStartXbig = (float)Math.Floor(x / (ScaledStep * 10)) * ScaledStep * 10;
                gridStartYbig = (float)Math.Floor(y / (ScaledStep * 10)) * ScaledStep * 10;
                if (gridStartX < x) gridStartX += ScaledStep;
                if (gridStartY < y) gridStartY += ScaledStep;
                if (gridStartXbig < x) gridStartXbig += ScaledStep * 10;
                if (gridStartYbig < y) gridStartYbig += ScaledStep * 10;
                // Теперь рисуем. Для крупных и мелких делений - своя структура SKPaint
                SKPaint PaintSmall = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = Color.FromRgb(20, 20, 20).ToSKColor(),
                    IsAntialias = true,
                    StrokeWidth = 1
                };

                SKPaint PaintBig = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = PaintSmall.Color,
                    IsAntialias = true,
                    StrokeWidth = 3
                };

                SKPoint smallStart = IssoConvert.IssoPoint2DToSkPoint(new IssoPoint2D() { X = gridStartX, Y = gridStartY },
                                     surface.ScaleFactor, surface.Origin, surface.ViewHeight);
                SKPoint bigStart = IssoConvert.IssoPoint2DToSkPoint(new IssoPoint2D() { X = gridStartXbig, Y = gridStartYbig },
                                     surface.ScaleFactor, surface.Origin, surface.ViewHeight);

                for (x = smallStart.X; x < surface.ViewWidth; x += ScaledStep * surface.ScaleFactor)
                {
                    gridBuffer.DrawLine(x, 0, x, surface.ViewHeight, PaintSmall);
                }

                for (x = bigStart.X; x < surface.ViewWidth; x += ScaledStep * 10 * surface.ScaleFactor)
                {
                    gridBuffer.DrawLine(x, 0, x, surface.ViewHeight, PaintBig);
                }

                for (y = smallStart.Y; y > 0; y -= ScaledStep * surface.ScaleFactor)
                {
                    gridBuffer.DrawLine(0, y, surface.ViewWidth, y, PaintSmall);
                }

                for (y = bigStart.Y; y > 0; y -= ScaledStep * 10 * surface.ScaleFactor)
                {
                    gridBuffer.DrawLine(0, y, surface.ViewWidth, y, PaintBig);
                }

                DrawCoordinationSystem(surface, gridBuffer);
                DrawAxisText(surface, gridBuffer);

                UpdateNeeded = false;                
            }
            canvas.DrawBitmap(gridBMP, 0, 0);
        }

        public void DrawCoordinationSystem(ModelViewSurface surface, SKCanvas canvas)
        {
            float h = surface.ViewHeight / 20;
            float w = h / 5;

            SKPath arrow = new SKPath();
            arrow.MoveTo(-w, -2 * w);
            arrow.LineTo(-w, 4 * w);
            arrow.LineTo(-2*w, 4 * w);
            arrow.LineTo(0, 6 * w);
            arrow.LineTo(2*w, 4 * w);
            arrow.LineTo(w, 4 * w);
            arrow.LineTo(w, -2 * w);
            arrow.Close();

            SKPaint arrowPaint = new SKPaint()
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = Color.FromRgb(30, 30, 40).ToSKColor(),
                IsAntialias = true,
                BlendMode = SKBlendMode.Lighten,
                StrokeWidth = 1,
                TextAlign = SKTextAlign.Center,
                TextSize = 2 * w
            };

            float px = 3 * w;
            float py = surface.ViewHeight - 3 * w;
            arrow.Transform(SKMatrix.MakeTranslation(px, py));
            arrow.Transform(SKMatrix.MakeRotation((float)Math.PI, px, py));
            canvas.DrawPath(arrow, arrowPaint);
            arrow.Transform(SKMatrix.MakeRotation((float)Math.PI/2, px, py));
            canvas.DrawPath(arrow, arrowPaint);

            arrowPaint.Color = Color.White.ToSKColor();           
            canvas.DrawText("Y", px, py - 8 * w, arrowPaint);
            canvas.DrawText("X", px + 8 * w, py + w / 2, arrowPaint);
        }

        public void DrawAxisText(ModelViewSurface surface, SKCanvas canvas)
        {
            SKPaint axisPaint = new SKPaint()
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = Color.LightGray.ToSKColor(),
                IsAntialias = true,
                BlendMode = SKBlendMode.Lighten,
                StrokeWidth = 1,
                TextAlign = SKTextAlign.Center,
                TextSize = surface.ViewHeight / 75
            };

            for (float x = gridStartXbig; x < surface.ViewWidth * surface.ScaleFactor; x += ScaledStep * 10)
            {
                SKPoint pt = IssoConvert.IssoPoint2DToSkPoint(new IssoPoint2D() { X = x, Y = surface.Origin.Y },
                                 surface.ScaleFactor, surface.Origin, surface.ViewHeight);
                pt.Y -= axisPaint.TextSize * 2;
                canvas.DrawText(x.ToString("F1"), pt, axisPaint);
            }

            axisPaint.TextAlign = SKTextAlign.Left;
            for (float y = gridStartYbig + ScaledStep * 10; y < surface.ViewHeight * surface.ScaleFactor; y += ScaledStep * 10)
            {
                SKPoint pt = IssoConvert.IssoPoint2DToSkPoint(new IssoPoint2D()
                     { X = surface.Origin.X,
                       Y = y
                     }, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
                pt.X += axisPaint.TextSize;
                pt.Y += axisPaint.TextSize / 2;
                canvas.DrawText(y.ToString("F1"), pt, axisPaint);
            }
        }
    }
}
