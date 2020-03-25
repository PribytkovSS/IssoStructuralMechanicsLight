using System;
using System.Collections.Generic;
using System.Text;
using StarMathLib;

namespace IssoStMechLight.Models
{
    public class ElementBeam
    {
        // Это балочный элемент, у которого с обоих концов - жёсткая заделка
        // Свойства элемента 

        // Компонент модели, ставший "исходником" для элемента
        public readonly ComponentLinear Linear;
        public IssoCrossSection Section { get { if ((Linear != null) && (Linear.Section != null)) return Linear.Section; else return null; } }

        public double EModulus
        {
            get { if (Section != null) return Section.MaterialElasticity; else return 1; }
        }

        public double MInertia
        {
            get { if (Section != null) return Section.SectionInertia; else return 1; }
        }

        public double CSArea
        {
            get { if (Section != null) return Section.SectionArea; else return 1; }
        }

        public readonly double Length;
        public readonly double Angle;
        public double AngleD {  get { return Angle * 180 / Math.PI; } }
        public readonly double[] point1 = new double[2];
        public readonly double[] point2 = new double[2];

        // Реакции связей в узлах 1 и 2 от распределённой нагрузки
        // Вычисляются вызовом CalcReactions 
        private double rx1, rx2, ry1, ry2, m1, m2, cosa, sina, cosL, sinL;
        // компоненты распределённой нагрузки на элемент в местной системе координат
        private double qx, qy;

        public double Rx1 { get { return rx1; } }
        public double Ry1 { get { return ry1; } }
        public double Rx2 { get { return rx2; } }
        public double Ry2 { get { return ry2; } }
        public double M1 { get { return m1; } }
        public double M2 { get { return m2; } }

        // То же самое, но в глобальной системе координат
        private double[] GlobalReactions;
        public double Rx1g { get { return GlobalReactions[0]; } }
        public double Ry1g { get { return GlobalReactions[1]; } }
        public double Rx2g { get { return GlobalReactions[3]; } }
        public double Ry2g { get { return GlobalReactions[4]; } }
        public double M1g { get { return GlobalReactions[2]; } }
        public double M2g { get { return GlobalReactions[5]; } }

        // Шарнир в начале и конце элемента
        public readonly bool HingeStart;
        public readonly bool HingeEnd;

        // Перемещения узлов элемента
        public double[] Displacements = new double[6];
        // Узловые силы
        public double[] NodalForces = new double[6];
        // Реакции связей в узлах
        public double[] Reactions = new double[6];
        // Внутренняя матрица жёсткости
        public double[,] Matrix = new double[6, 6];
        // Матрица направляющих косинусов
        public double[,] Cosines = new double[6, 6];
        public double[,] Cosines3x3 = new double[3, 3];
        // Внешняя матрица жёсткости    
        public double[,] ExtMatrix = new double[6, 6];
             
        public readonly RodModel Model;
        public ComponentLoad DistLoad;
        

        // Индексы узлов в глобальной матрице
        public int Node1Index
        {
            get
            {
                if (Model != null) return Model.getNodeIndex(Linear.StartNode); else return -1;
            }
        }

        public int Node2Index
        {
            get
            {
                if (Model != null) return Model.getNodeIndex(Linear.EndNode); else return -1;
            }
        }
        
        public ElementBeam(ComponentLinear l, RodModel model)
        {
            Linear = l;
            Model = model;
            HingeStart = Linear.HingeStart;
            HingeEnd = Linear.HingeEnd;

            point1[0] = Linear.Start.X;
            point1[1] = Linear.Start.Y;

            point2[0] = Linear.End.X;
            point2[1] = Linear.End.Y;
            Length = Math.Sqrt(Math.Pow((point1[0] - point2[0]), 2) + Math.Pow((point1[1] - point2[1]), 2));
            Angle = Math.Atan2(point2[1] - point1[1], point2[0] - point1[0]);
            
            InitCosines();
            InitRMatrix();
            InitExtMatrix();
        }

        public void CalculateForces()
        {
            int n1i = Node1Index;
            int n2i = Node2Index;
            // Заполняем вектор перемещений
            // Горизонтальное перемещение узла 1
            Displacements[0] = Model.Displacements[n1i * 3];
            // Вертикальное перемещение узла 1
            Displacements[1] = Model.Displacements[n1i * 3 + 1];
            // Поворот узла 1
            Displacements[2] = Model.Displacements[n1i * 3 + 2];
            // Горизонтальное перемещение узла 2
            Displacements[3] = Model.Displacements[n2i * 3];
            // Вертикальное перемещение узла 2
            Displacements[4] = Model.Displacements[n2i * 3 + 1];
            // Поворот узла 2
            Displacements[5] = Model.Displacements[n2i * 3 + 2];
            // Вычисляем реакции
            Reactions = StarMath.multiply(ExtMatrix, Displacements);
            Reactions = StarMath.multiply(Cosines, Reactions);
            // Переводим перемещения в местную систему координат
            Displacements = StarMath.multiply(Cosines, Displacements);            
            // Добавим узловые силы, если они есть
            // и эквивалентные реакции от распределённой нагрузки           
            NodalForces[0] = Model.Loads[n1i * 3];
            NodalForces[1] = Model.Loads[n1i * 3+1];
            NodalForces[2] = Model.Loads[n1i * 3+2];
            NodalForces[3] = Model.Loads[n2i * 3];
            NodalForces[4] = Model.Loads[n2i * 3+1];
            NodalForces[5] = Model.Loads[n2i * 3+2];
            NodalForces = StarMath.multiply(Cosines, NodalForces);
            Reactions[0] = Reactions[0] - Rx1;// - NodalForces[0];
            Reactions[1] = Reactions[1] - Ry1;//- NodalForces[1];
            Reactions[2] = Reactions[2] - M1;// - NodalForces[2];
            Reactions[3] = Reactions[3] - Rx2;// - NodalForces[3];
            Reactions[4] = Reactions[4] - Ry2;// - NodalForces[4];
            Reactions[5] = Reactions[5] - M2;// - NodalForces[5];
        }

        public double BendingMoment(float localX)
        {
            // Функция изгибающего момента:
            return Reactions[1] * localX - Reactions[2] + qy * localX * localX / 2;
        }

        public double AxialForce(float localX)
        {
            // Эффект - сжатие или растяжение - зависит от относительного положения начального узла и 
            // сечения элемента. Положительная реакция означает направление силы в сторону положительных значений координат
            // если сечение расположено правее начального узла, это означает сжатие. Если левее, то растяжение
            // Иначе говоря Reactions[0] надо умножать на знак косинуса угла наклона элемента к горизонтали
            return (-Reactions[0] - qx * localX);
        }

        public double ShearForce(float localX)
        {
            return Reactions[1] + qy * localX;
        }

        public double[] DeformedShape(float localX)
        {
            // Возвращаем координаты точки, расположенной на расстоянии localX от начала элемента 
            // вдоль его продольной оси
            // Наш балочный элемент претерпевает продольные и изгибные деформации
            // Прогиб в местной системе координат определяем двойным интегрированием эпюры моментов
            // Продольные перемещения определяем умножением продольной силы на координату и делением на жёсткость            
            double dx = (-Reactions[0] * localX - qx * localX * localX / 2);
            // Угол поворота
            double df = (Reactions[1] * Math.Pow(localX, 2) / 2 - Reactions[2] * localX + qy * Math.Pow(localX, 3) / 6);
            double dy = (Reactions[1] * Math.Pow(localX, 3) / 6 - Reactions[2] * Math.Pow(localX, 2) / 2 + qy * Math.Pow(localX, 4) / 24) + Displacements[2] * localX;
            return new double[3] 
            {
                (dx / (CSArea * EModulus) + Displacements[0]),
                (dy / (MInertia * EModulus) + Displacements[1]),
                (df / (MInertia * EModulus) + Displacements[2])
            };
        }

        public double[] DeformedShapeGlobal(float localX)
        {
            double[] ds = DeformedShape(localX);
            ds[0] = ds[0] * Math.Cos(Angle) + ds[1] * Math.Sin(Angle);
            ds[1] = -ds[0] * Math.Sin(Angle) + ds[1] * Math.Cos(Angle);
            return ds;
        }

        public double[] DeformedShapeGlobalXY(float localX, float scale)
        {
            double[] ds = DeformedShape(localX);
            double[] gds = StarMath.multiply(StarMath.transpose(Cosines3x3), ds);
            //double dx = scale * (ds[0] * Math.Cos(Angle) + ds[1] * Math.Sin(Angle));
            //double dy = scale * (-ds[0] * Math.Sin(Angle) + ds[1] * Math.Cos(Angle));
            double cosa = Math.Cos(Angle);
            double sina = Math.Sin(Angle);
            gds[0] = localX * cosa + gds[0] * scale;
            gds[1] = localX * sina + gds[1] * scale;

            gds[0] += point1[0];
            gds[1] += point1[1];
            return gds;
        }


        private void InitExtMatrix()
        {
            //ExtMatrix = StarMath.multiply(StarMath.multiply(StarMath.transpose(Cosines), Matrix), Cosines);
            ExtMatrix = StarMath.multiply(StarMath.multiply(StarMath.transpose(Cosines), Matrix), Cosines);
        }

        private void InitCosines()
        {
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++) Cosines[i, j] = 0;

            Cosines[0, 0] = Math.Cos(Angle);
            Cosines[1, 1] = Cosines[0, 0];
            Cosines[3, 3] = Cosines[0, 0];
            Cosines[4, 4] = Cosines[0, 0];
            Cosines[2, 2] = 1; Cosines[5, 5] = Cosines[2, 2];

            // TODO: Какая-то беда со знаками перемещений
            Cosines[0, 1] = Math.Sin(Angle); Cosines[1, 0] = -Cosines[0, 1];
            Cosines[3, 4] = Cosines[0, 1]; Cosines[4, 3] = Cosines[1, 0];

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++) Cosines3x3[i, j] = Cosines[i, j];
        }

        private void InitRMatrix()
        {
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++) Matrix[i, j] = 0;

            // В зависимости от наличия шарниров по концам элемента
            // создаём вариант матрицы жёсткости
            if (!HingeEnd && !HingeStart) BeamMatrix();
            if (HingeStart && HingeEnd) TrussMatrix();
            if (HingeStart != HingeEnd) CombinedMatrix(HingeStart);
        }

        private void CombinedMatrix(bool HingeAtFirst)
        {
            Matrix[0, 0] = EModulus * CSArea / Length;
            Matrix[0, 3] = -Matrix[0, 0];
            Matrix[3, 0] = -Matrix[0, 0];
            Matrix[3, 3] = Matrix[0, 0];

            Matrix[1, 1] = 3 * EModulus * MInertia / Math.Pow(Length, 3);
            Matrix[1, 4] = -Matrix[1, 1];
            Matrix[4, 1] = -Matrix[1, 1];
            Matrix[4, 4] = Matrix[1, 1];

            if (HingeAtFirst)
            {
                Matrix[1, 5] = 3 * EModulus * MInertia / Math.Pow(Length, 2);
                Matrix[5, 1] = Matrix[1, 5];
                Matrix[5, 4] = -Matrix[1, 5];
                Matrix[4, 5] = Matrix[4, 5];

                Matrix[5, 5] = 3 * EModulus * MInertia / Length;
                Matrix[2, 2] = 1;
            } else
            {
                Matrix[1, 2] = 3 * EModulus * MInertia / Math.Pow(Length, 2);
                Matrix[2, 1] = Matrix[1, 2];
                Matrix[2, 4] = -Matrix[1, 2];
                Matrix[4, 2] = Matrix[2, 4];

                Matrix[2, 2] = 3 * EModulus * MInertia / Length;
                Matrix[5, 5] = 1;
            }
        }

        private void TrussMatrix()
        {
            Matrix[0, 0] = EModulus * CSArea / Length;
            Matrix[0, 3] = -Matrix[0, 0];
            Matrix[3, 0] = -Matrix[0, 0];
            Matrix[3, 3] = Matrix[0, 0];
            Matrix[2, 2] = 3 * EModulus * MInertia / Length;
            Matrix[5, 5] = -3 * EModulus * MInertia / Length;
            Matrix[2, 4] = 3 * EModulus * MInertia / Math.Pow(Length, 2);
            Matrix[2, 1] = -Matrix[2, 4];
            Matrix[4, 2] = -Matrix[2, 4];
            Matrix[4, 4] = Matrix[2, 4];
        }

        private void BeamMatrix()
        {
            Matrix[0, 0] = EModulus * CSArea / Length;
            Matrix[0, 3] = -Matrix[0, 0];
            Matrix[3, 0] = -Matrix[0, 0];
            Matrix[3, 3] = Matrix[0, 0];

            Matrix[1, 1] = 12 * EModulus * MInertia / Math.Pow(Length, 3);
            Matrix[1, 4] = -Matrix[1, 1];
            Matrix[4, 1] = -Matrix[1, 1];
            Matrix[4, 4] = Matrix[1, 1];

             Matrix[1, 2] = 6 * EModulus * MInertia / Math.Pow(Length, 2);
             Matrix[1, 5] = Matrix[1, 2];
             Matrix[2, 1] = Matrix[1, 2];
             Matrix[2, 4] = -Matrix[1, 2];
             Matrix[4, 2] = -Matrix[1, 2];
             Matrix[4, 5] = -Matrix[1, 2];
             Matrix[5, 1] = Matrix[1, 2];
             Matrix[5, 4] = -Matrix[1, 2];

             Matrix[2, 2] = 4 * EModulus * MInertia / Length;
             Matrix[5, 5] = Matrix[2, 2];

             Matrix[2, 5] = 2 * EModulus * MInertia / Length;
             Matrix[5, 2] = Matrix[2, 5]; 
        }

        public void CalcEquivalentlReactions(ComponentLoad DistributedLoad)
        {
            // Разложим load на составляющие в локальной системе для 
            cosa = Math.Cos((DistributedLoad.Direction - AngleD) * Math.PI / 180);
            sina = Math.Sin((DistributedLoad.Direction - AngleD) * Math.PI / 180);
            cosL = Math.Cos(Angle);
            sinL = Math.Sin(Angle);
            qx = DistributedLoad.Value * cosa;
            qy = DistributedLoad.Value * sina;
            // Находим реакции
            // 1. Если в обоих узлах элемента нет шарниров
            // 2. Если шарнир в первом узле
            // 3. Если шарнир во втором узле
            // 4. Если шарниры в обоих узлах
            rx1 = qx * Length / 2;
            rx2 = rx1;
            if ((!HingeStart) && (!HingeEnd))
            {
                ry1 = qy * Length / 2;
                ry2 = ry1;
                //TODO: Разобраться со знаками моментов
                m1 = qy * (Length * Length) / 12;
                m2 = -m1;
            }
            if ((HingeStart) && (!HingeEnd))
            {
                ry2 = qy * 5 / 8 * Length;
                ry1 = qy * Length - ry2;
                m2 = -qy * Math.Pow(Length, 2) / 8;
                m1 = 0;
            }
            if ((!HingeStart) && (HingeEnd))
            {
                ry1 = qy * 5 / 8 * Length;
                ry2 = qy * Length - ry1;
                m1 = qy * Math.Pow(Length, 2) / 8;
                m2 = 0;
            }
            if ((HingeStart) && (HingeEnd))
            {
                ry1 = qy * Length / 2;
                ry2 = ry1;
                m1 = 0;
                m2 = 0;
            }
            CleanupReactions();
        }

        private void CleanupReactions()
        {
            sinL = Math.Round(sinL, 5);
            cosL = Math.Round(cosL, 5);
            sina = Math.Round(sina, 5);
            cosa = Math.Round(cosa, 5);
            rx1 = Math.Round(rx1, 5);
            rx2 = Math.Round(rx2, 5);
            ry1 = Math.Round(ry1, 5);
            ry2 = Math.Round(ry2, 5);
            m1 = Math.Round(m1, 5);
            m2 = Math.Round(m2, 5);
            qx = Math.Round(qx, 5);
            qy = Math.Round(qy, 5);
            double[] reactions = new double[6] { rx1, ry1, m1, rx2, ry2, m2 };
            GlobalReactions = StarMath.multiply(StarMath.transpose(Cosines), reactions);
        }
    }
}
