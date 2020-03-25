using IssoStMechLight.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssoStMechLight.Models
{
    public struct IssoPoint2D
    {
        public float X, Y;
    }

    class IssoRect2D
    {
        private IssoPoint2D Corner1, Corner2;

        public IssoPoint2D TopLeft
        {
            get
            {
                float x = Math.Min(Corner1.X, Corner2.X);
                float y = Math.Max(Corner1.Y, Corner2.Y);
                return new IssoPoint2D() { X = x, Y = y };
            }
        }

        public IssoPoint2D BottomRight
        {
            get
            {
                float x = Math.Max(Corner1.X, Corner2.X);
                float y = Math.Min(Corner1.Y, Corner2.Y);
                return new IssoPoint2D() { X = x, Y = y };
            }
        }

        public IssoPoint2D Origin
        {
            get
            {
                float x = Math.Min(Corner1.X, Corner2.X);
                float y = Math.Min(Corner1.Y, Corner2.Y);
                return new IssoPoint2D() { X = x, Y = y };
            }
        }

        public float Width
        {
            get
            {
                return Math.Abs(Corner1.X - Corner2.X);
            }
        }

        public float Heigth
        {
            get
            {
                return Math.Abs(Corner1.Y - Corner2.Y);
            }
        }

        public IssoRect2D(IssoPoint2D c1, IssoPoint2D c2)
        {
            Corner1 = c1;
            Corner2 = c2;
        }

        public IssoRect2D(IssoPoint2D BottomLeft, float width, float height)
        {
            Corner1 = BottomLeft;
            Corner2 = new IssoPoint2D { X = Corner1.X + width, Y = Corner1.Y + height };
        }
    }

    public class IssoQuad2D
    {
        private IssoPoint2D corner1, corner2, corner3, corner4;

        public IssoPoint2D C1 { get { return corner1; } }
        public IssoPoint2D C2 { get { return corner2; } }
        public IssoPoint2D C3 { get { return corner3; } }
        public IssoPoint2D C4 { get { return corner4; } }

        public IssoQuad2D(IssoPoint2D c1, IssoPoint2D c2, IssoPoint2D c3, IssoPoint2D c4)
        {
            // Создаём четырёхугольник по четырём точкам 
            corner1 = c1;
            corner2 = c2;
            corner1 = c1;
            corner2 = c2;
        }
    }

    public static class IssoBind
    {
        public static float OrthoAngle(float angleD)
        {
            float ds = Math.Abs(angleD);
            if ((ds >= -1.5) && (ds <= 1.5)) return 0;
            if ((ds >= 88.5) && (ds <= 91.5)) return 90;
            if ((ds >= 178.5) && (ds <= 180)) return 180;
            if ((ds <= -178.5) && (ds >= -180)) return 180;
            if ((ds <= -88.5) && (ds >= -91.5)) return -90;

            return angleD;
        }
    }

    static class IssoDist
    {
        static public IssoPoint2D MirrorPoint(IssoPoint2D point, IssoPoint2D AxisPoint1, IssoPoint2D AxisPoint2)
        {
            IssoPoint2D pt = new IssoPoint2D() { X = point.X, Y = point.Y };
            IssoPoint2D purpPt = PurpPoint(pt, AxisPoint1, AxisPoint2);

            float dst = PointDst(pt, purpPt);
            
            float ang = LineAngle(AxisPoint1, AxisPoint2);

            pt.X = pt.X + Math.Sign(purpPt.X - pt.X) * dst * 2 * Math.Abs((float)Math.Cos(Math.PI / 2 - ang));
            pt.Y = pt.Y + Math.Sign(purpPt.Y - pt.Y) * dst * 2 * Math.Abs((float)Math.Sin(Math.PI / 2 - ang));

            return pt;
        }

        static public float PointToLinear(IssoPoint2D pt, IssoPoint2D lineStart, IssoPoint2D lineEnd)
        {
            float a, b, c;
            if (lineStart.X == lineEnd.X)
            {
                b = 0; a = 1; c = -lineStart.X;
            }
            else
            if (lineStart.Y == lineEnd.Y)
            {
                b = 1; a = 0; c = lineStart.Y;
            }
            else
            {
                b = -1;
                a = (lineEnd.Y - lineStart.Y) / (lineEnd.X - lineStart.X);
                c = lineEnd.Y - lineEnd.X * a;
            }

            float d = (float)Math.Abs(((a * pt.X + b * pt.Y + c)) /
                (Math.Sqrt(a * a + b * b)));

            return d;
        }

        static public float LineAngle(IssoPoint2D lineStart, IssoPoint2D lineEnd)
        {
            float len = PointDst(lineStart, lineEnd);
            if (len > 0)
                return (float)Math.Acos((lineEnd.X - lineStart.X) / len);
            else return 0;
        }

        static public float PointToLinear(IssoPoint2D pt, ComponentLinear line)
        {
            return PointToLinear(pt, line.Start, line.End);
        }

        private static float dotProduct(IssoPoint2D p1, IssoPoint2D p2)
        {
            return (p1.X * p2.X + p1.Y * p2.Y);
        }

        public static bool isProjectedPointOnLineSegment(IssoPoint2D pt, ComponentLinear line)
        {
            // get dotproduct |e1| * |e2|
            IssoPoint2D e1 = new IssoPoint2D() { X = line.End.X - line.Start.X, Y = line.End.Y - line.Start.Y };
            float recArea = dotProduct(e1, e1);
            // dot product of |e1| * |e2|
            IssoPoint2D e2 = new IssoPoint2D() { X = pt.X - line.Start.X, Y = pt.Y - line.Start.Y };
            double val = dotProduct(e1, e2);
            return (val > 0 && val < recArea);
        }

        public static IssoPoint2D PurpPoint(IssoPoint2D pt, IssoPoint2D lineS, IssoPoint2D lineE)
        {
            // get dot product of e1, e2
            IssoPoint2D e1 = new IssoPoint2D() { X = lineE.X - lineS.X, Y = lineE.Y - lineS.Y };
            IssoPoint2D e2 = new IssoPoint2D() { X = pt.X - lineS.X, Y = pt.Y - lineS.Y };
            float valDp = dotProduct(e1, e2);
            // get length of vectors
            float lenLineE1 = (float)Math.Sqrt(e1.X * e1.X + e1.Y * e1.Y);
            float lenLineE2 = (float)Math.Sqrt(e2.X * e2.X + e2.Y * e2.Y);
            float cos = valDp / (lenLineE1 * lenLineE2);
            // length of v1P'
            float projLenOfLine = cos * lenLineE2;

            if (lenLineE1 * lenLineE2 == 0) return pt;

            IssoPoint2D p = new IssoPoint2D()
            {
                X = (lineS.X + (projLenOfLine * e1.X) / lenLineE1),
                Y = (lineS.Y + (projLenOfLine * e1.Y) / lenLineE1)
            };
            return p;
        }


        public static IssoPoint2D PurpPoint(IssoPoint2D pt, ComponentLinear line)
        {            
            return PurpPoint(pt, line.Start, line.End);
        }

        public static bool PointInRect(IssoPoint2D pt, IssoRect2D rect)
        {
            return ((pt.X >= rect.TopLeft.X) && (pt.X <= rect.BottomRight.X) &&
                    (pt.Y >= rect.BottomRight.Y) && (pt.Y <= rect.TopLeft.Y));
        }

        public static float PointDst(IssoPoint2D pt1, IssoPoint2D pt2)
        {
            return (float)Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2));
        }

        public static bool PointsOnLine(IssoPoint2D[] pts, IssoPoint2D[] line)
        {
            // Возвращает "истина" если все точки в pts лежат на отрезке, заданным двумя точками в Line
            for (int i = 0; i < pts.Length; i++)
            {
                IssoPoint2D p = pts[i];
                if (!((line[0].X <= p.X && p.X <= line[1].X) || (line[1].X <= p.X && p.X <= line[0].X)))
                {
                    // test point not in x-range
                    return false;
                }
                if (!((line[0].Y <= p.Y && p.Y <= line[1].Y) || (line[1].Y <= p.Y && p.Y <= line[0].Y)))
                {
                    // test point not in y-range
                    return false;
                }
                if (!isPointOnLineviaPDP(line[0], line[1], p)) return false;
            }
            return true;
        }

        private static bool isPointOnLineviaPDP(IssoPoint2D p1, IssoPoint2D p2, IssoPoint2D p)
        {
            return (Math.Abs(perpDotProduct(p1, p2, p)) < getEpsilon(p1, p2));
        }

        private static float perpDotProduct(IssoPoint2D p1, IssoPoint2D p2, IssoPoint2D p)
        {
            return (p1.X - p.X) * (p2.Y - p.Y) - (p1.Y - p.Y) * (p2.X - p.X);
        }

        public static float getEpsilon(IssoPoint2D p1, IssoPoint2D p2)
        {
            float dx1 = p2.X - p1.X;
            float dy1 = p2.Y - p1.Y;
            float epsilon = 0.003f * (dx1 * dx1 + dy1 * dy1);
            return epsilon;
        }        
    }
}
