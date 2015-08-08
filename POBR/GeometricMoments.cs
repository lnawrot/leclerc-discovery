using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace POBR {
    public class GeometricMoments {
        private PicturePart picturePart;
        private Criterion criterion;
        private double[,] mm;
        private double[,] M;
        private Point center;

        public GeometricMoments(PicturePart p, Criterion c) {
            // array[3, 3] cause we need moments m20 m02 m01 m10 m11 and m00
            mm = new double[3, 3];
            M = new double[3, 3];

            picturePart = p;
            criterion = c;
            CalculateNormalMoments();
            CalculateCentralMoments();
        }

        private void CalculateNormalMoments() {
            // we need moments m20 m02 m01 m10 m11 and m00
            foreach(System.Drawing.Point p in picturePart.pixels) {
                double sum = 1;
                for( int i = 0; i < mm.GetLength(0); i++ ) {
                    double sum2 = sum;

                    for( int j = 0; j < mm.GetLength(1); j++ ) {
                        mm[i, j] += sum2;
                        sum2 *= p.Y;
                    }

                    sum *= p.X;
                }
            }

            center.X = (int)( mm[1, 0] / mm[0, 0] );
            center.Y = (int)( mm[0, 1] / mm[0, 0] );
        }

        private void CalculateCentralMoments() {
            M[0, 0] = mm[0, 0];
            M[0, 1] = 0;
            M[1, 0] = 0;
            M[1, 1] = mm[1, 1] - mm[1, 0] * mm[0, 1] / mm[0, 0];
            M[2, 0] = mm[2, 0] - mm[1, 0] * mm[1, 0] / mm[0, 0];
            M[0, 2] = mm[0, 2] - mm[0, 1] * mm[0, 1] / mm[0, 0];
            M[2, 1] = mm[2, 1] - 2 * mm[1, 1] * center.X - mm[2, 0] * center.Y + 2 * mm[0, 1] * center.X * center.X;
            M[1, 2] = mm[1, 2] - 2 * mm[1, 1] * center.Y - mm[0, 2] * center.X + 2 * mm[1, 0] * center.Y * center.Y;
            //M[3, 0] = mm[3, 0] - 3 * mm[2, 0] * center.X + 2 * mm[1, 0] * center.X * center.X;
            //M[0, 3] = mm[0, 3] - 3 * mm[0, 2] * center.Y + 2 * mm[0, 1] * center.Y * center.Y;
        }

        public double m(int i, int j) {
            return mm[i, j];
        }

        public Point GetCenter() {
            return center;
        }

        public double GetM1() {
            return ( M[2, 0] + M[0, 2] ) / ( mm[0, 0] * mm[0, 0] );
        }

        public double GetM2() {
            return ( ( M[2, 0] - M[0, 2] ) * ( M[2, 0] - M[0, 2] ) + 4 * M[1, 1] * M[1, 1] ) / ( mm[0, 0] * mm[0, 0] * mm[0, 0] * mm[0, 0] );
        }

        //public double GetM3() {
        //    return ( ( M[3, 0] - 3 * M[1, 2] ) * ( M[3, 0] - 3 * M[1, 2] ) + ( 3 * M[2, 1] - M[0, 3] ) * ( 3 * M[2, 1] - M[0, 3] ) ) / ( mm[0, 0] * mm[0, 0] * mm[0, 0] * mm[0, 0] * mm[0, 0] );
        //}

        //public double GetM4() {
        //    return ( ( M[3, 0] + M[1, 2] ) * ( M[3, 0] + M[1, 2] ) + ( M[2, 1] + M[0, 3] ) * ( M[2, 1] + M[0, 3] ) ) / ( mm[0, 0] * mm[0, 0] * mm[0, 0] * mm[0, 0] * mm[0, 0] );
        //}

        public double GetM7() {
            return ( M[2, 0] * M[0, 2] - M[1, 1] * M[1, 1] ) / ( mm[0, 0] * mm[0, 0] * mm[0, 0] * mm[0, 0] );
        }

        //public double GetM8() {
        //    return ( M[3, 0] * M[1, 2] + M[2, 1] * M[0, 3] - M[1, 2] * M[1, 2] - M[2, 1] * M[2, 1] ) / ( mm[0, 0] * mm[0, 0] * mm[0, 0] * mm[0, 0] * mm[0, 0] );
        //}
    }
}
