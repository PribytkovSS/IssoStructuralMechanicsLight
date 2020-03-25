using IssoStMechLight.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssoStMechLight.ViewModels
{
    class IssoConvert
    {
        public static SKPoint IssoPoint2DToSkPoint(IssoPoint2D p)
        {
            return new SKPoint(p.X, p.Y);
        }

        internal static SKPoint IssoPoint2DToSkPoint(IssoPoint2D point, float scaleFactor, IssoPoint2D origin, float yTrans)
        {
            return new SKPoint()
            {
                X = (point.X - origin.X) * scaleFactor,
                Y = (point.Y - origin.Y) * (-scaleFactor) + yTrans
            };
        }

        internal static IssoPoint2D SkPointToIssoPoint2D(SKPoint point, float scaleFactor, IssoPoint2D origin, float yTrans)
        {
            return new IssoPoint2D()
            {
                X = point.X / scaleFactor + origin.X,
                Y = (point.Y - yTrans) / (-scaleFactor) + origin.Y
            };
        }
    }
}
