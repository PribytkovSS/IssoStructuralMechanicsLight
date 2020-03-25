using IssoStMechLight.Models;
using IssoStMechLight.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;

namespace IssoStMechLight.ViewModels
{
    public static class ComponentNodeVM
    {
        public static void DrawNode(ComponentNode node, ModelViewSurface surface, SKCanvas canvas)
        {
            SKPath p1 = Rigid();
            SKPath p = new SKPath();
            /* switch (node.Type)
             {
                 case NodeType.Hinge: p = Hinge(); break;
                 case NodeType.Rigid: p = Rigid(); break;
                 case NodeType.Pin2d: DrawPinSupport(node, surface, canvas, false); return;
                 case NodeType.Roller2d: DrawRollerSupport(node, surface, canvas, false); return;
                 default: p = Point(); break;
             } */
            if (node.DisallowedDisplacements.Contains(NodeDisplacement.X)) AddX(p);
            if (node.DisallowedDisplacements.Contains(NodeDisplacement.Y)) AddY(p);
            if (node.DisallowedDisplacements.Contains(NodeDisplacement.Rotation)) AddR(p);
            
            float scale = surface.ViewHeight / 80;

            SKPoint location = IssoConvert.IssoPoint2DToSkPoint(node.Location, surface.ScaleFactor, surface.Origin, surface.ViewHeight);

            SKMatrix matrix = new SKMatrix();
            matrix.SetScaleTranslate(scale, scale, location.X, location.Y);
            p.Transform(matrix);
            p1.Transform(matrix);

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = Color.Aquamarine.ToSKColor(),
                IsAntialias = true,
                StrokeWidth = 1
            }; 
            // Если компонент выбран, рисуем его пожирнее
            if (node.CompState == ComponentState.csEdited) paint.StrokeWidth += 1;
            canvas.DrawPath(p1, paint);
            paint.Style = SKPaintStyle.Stroke;
            canvas.DrawPath(p, paint);
        }

        private static void AddR(SKPath p)
        {
            float x = -2, y = -2.85f;
            p.MoveTo(-x, -y);
            p.LineTo(x, y);
            double l = Math.Sqrt(Math.Pow(-x - x, 2) + Math.Pow(-y - y, 2));
            double a = Math.Atan2(y, x);
            double dx = l / 5 * Math.Cos(a);
            double dy = l / 5 * Math.Sin(a);
            for (int i = -1; i < 5; i++)
            {
                p.MoveTo(-x + (float)dx * i + x, -y + (float)dy * i);
                p.LineTo(-x + (float)dx * (i + 1), -y + (float)dy * (i + 1));
            }
        }

        private static void AddX(SKPath p)
        {
            SKPath XPath = new SKPath();
            AddY(XPath);
            SKMatrix rmatrix = SKMatrix.MakeRotationDegrees(-90, 0, 0);
            XPath.Transform(rmatrix);
            p.AddPath(XPath);
        }

        private static void AddY(SKPath p)
        {
            for (int i = 0; i < 5; i++)
            {                
                p.MoveTo(-0.7f, .7f * (i+1));
                p.LineTo(0, .7f * i);
                p.LineTo(1f, .7f * (i + 1));
                p.MoveTo(0, .7f * i);
                p.LineTo(0, .7f * (i + 1));
            }
        }

        private static SKPath Hinge()
        {            
            // Изображение шарнира
            SKPath pin = new SKPath();
            // Шарнир вверху
            pin.AddCircle(0, 0, 1);
                       
            return pin;
        }

        private static SKPath Rigid()
        {
            // Изображение шарнира
            SKPath pin = new SKPath();
            // Шарнир вверху
            pin.AddRect(new SKRect(-0.5f, -0.5f, 0.5f, 0.5f));

            return pin;
        }

        private static SKPath Point()
        {
            // Изображение шарнира
            SKPath pin = new SKPath();
            // Шарнир вверху
            pin.MoveTo(0, 0); pin.LineTo(0.1f, 0);

            return pin;
        }

        public static bool Contains(ComponentNode node, IssoPoint2D pt, ModelViewSurface surface)
        {
            SKPath pin;
            // Проверяем, находится ли указанная точка внутри области отображения узла
            //if ((node.Type == NodeType.Rigid) || (node.Type == NodeType.Hinge))
                 pin = Rigid();
           // else pin = SupportBounds();

            SKMatrix rotate;
            SKPoint skp = IssoConvert.IssoPoint2DToSkPoint(pt, surface.ScaleFactor, surface.Origin, surface.ViewHeight);
            pin.Transform(TRMatrix(node, surface, out rotate));
            pin.Transform(rotate);
            return pin.Contains(skp.X, skp.Y);
        }

        private static SKPath SupportBounds()
        {
            SKPath pin = new SKPath();
            pin.FillType = SKPathFillType.Winding;
            pin.MoveTo(-1.6f, -1); pin.LineTo(+1.6f, -1);
            pin.LineTo(+1.6f, 5.2f); pin.LineTo(-1.6f, 5.2f);
            pin.Close();
            return pin;
        }

        private static SKPath DefaultPin()
        {
            // Изображение шарнирно-неподвижной опорной части
            SKPath pin = new SKPath();
            // Шарнир вверху
            pin.AddCircle(0, 0, 1);
            // Боковые линии треугольника
            pin.MoveTo(0, 1); pin.LineTo(-1.6f, 3.6f + 1);
            pin.MoveTo(0, 1); pin.LineTo(1.6f, 3.6f + 1);
            // Нижняя линия основания
            pin.MoveTo(-1.8f, 3.6f + 1); pin.LineTo(1.8f, 3.6f + 1);
            // Штриховка основания
            float s = 2 * 1.6f / 4;
            float x1 = -1.6f;
            float y1 = 3.6f + 1;
            for (int i = 0; i < 5; i++)
            {
                pin.MoveTo(x1, y1);
                pin.LineTo(x1 - 1.6f, y1 + 1.6f);
                x1 += s;
            }

            return pin;
        }

        private static SKPath DefaultRoller()
        {
            // Изображение шарнирно-неподвижной опорной части
            SKPath pin = new SKPath();
            // Шарнир вверху
            pin.AddCircle(0, 0, 1);
            pin.AddCircle(0, 3.6f, 1);
            // Вертикальная линия
            pin.MoveTo(0, 1); pin.LineTo(0, 3.6f - 1);
            // Нижняя линия основания
            pin.MoveTo(-1.8f, 3.6f + 1); pin.LineTo(1.8f, 3.6f + 1);
            // Штриховка основания
            float s = 2 * 1.6f / 4;
            float x1 = -1.6f;
            float y1 = 3.6f + 1;
            for (int i = 0; i < 5; i++)
            {
                pin.MoveTo(x1, y1);
                pin.LineTo(x1 - 1.6f, y1 + 1.6f);
                x1 += s;
            }

            return pin;
        }

        private static SKMatrix TRMatrix(ComponentNode c, ModelViewSurface surface, out SKMatrix rmatrix)
        {
            float scale = surface.ViewHeight / 80;

            SKPoint location = IssoConvert.IssoPoint2DToSkPoint(c.Location, surface.ScaleFactor, surface.Origin, surface.ViewHeight);

            SKMatrix matrix = new SKMatrix();
            matrix.SetScaleTranslate(scale, scale, location.X, location.Y);

            //rmatrix = SKMatrix.MakeRotationDegrees(c.Angle - 90, location.X, location.Y);
            rmatrix = SKMatrix.MakeRotationDegrees(0, location.X, location.Y);

            return matrix;
        }

        private static SKPaint GetPaint(ComponentNode c)
        {
            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.Beige.ToSKColor(),
                IsAntialias = true,
                StrokeWidth = 1
            };

            // Если компонент выбран, рисуем его пожирнее
            if (c.CompState == ComponentState.csEdited) paint.StrokeWidth += 1;

            return paint;
        }


        private static void DrawPinSupport(ComponentNode c, ModelViewSurface surface, SKCanvas canvas, bool isInt)
        {
            SKPath pin = DefaultPin();
            SKMatrix rotate;
            pin.Transform(TRMatrix(c, surface, out rotate));
            pin.Transform(rotate);
            canvas.DrawPath(pin, GetPaint(c));
        }

        private static void DrawRollerSupport(ComponentNode c, ModelViewSurface surface, SKCanvas canvas, bool isInt)
        {
            SKPath roller = DefaultRoller();
            SKMatrix rotate;
            roller.Transform(TRMatrix(c, surface, out rotate));
            roller.Transform(rotate);
            canvas.DrawPath(roller, GetPaint(c));
        }
    }
}
