using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace POBR {
    #region Color criterions
    public abstract class Criterion {
        public abstract bool Match(Color c);
    }

    public class HSBCriterion: Criterion {
        private double brightnessMargin;
        private double hueMargin;
        private double saturationMargin;

        private double h;
        private double s;
        private double b;

        public Color SearchColor;
        
        public HSBCriterion() {
        }

        public HSBCriterion(Color sc) {
            SearchColor = sc;
            
            Tuple<double, double, double> search = Utilities.RGBtoHSB(SearchColor.R, SearchColor.G, SearchColor.B);

            h = search.Item1;
            s = search.Item2;
            b = search.Item3;
        }

        public HSBCriterion(Color sc, double bm, double hm, double sm) : this(sc) {
            brightnessMargin = bm / 100;
            hueMargin = hm;
            saturationMargin = sm / 100;
        }

        public void SetBrightnessMargin(double m) {
            brightnessMargin = m;
        }

        public void SetHueMargin(double m) {
            hueMargin = m;
        }

        public void SetSaturationMargin(double m) {
            saturationMargin = m;
        }

        public override bool Match(Color c) {
            Tuple<double, double, double> hsb = Utilities.RGBtoHSB(c.R, c.G, c.B);

            float hue = (float)Math.Abs(hsb.Item1 - h);
            if( hue > hueMargin )
                return false;

            float brightness = (float)Math.Abs(hsb.Item3 - b);
            if( brightness > brightnessMargin )
                return false;

            float saturation = (float)Math.Abs(hsb.Item2 - s);
            if( saturation > saturationMargin )
                return false;

            return true;
        }
    }

    public class ColorCriterion : Criterion {
        public Color color;

        public ColorCriterion(Color c) {
            color = c;
        }

        public override bool Match(Color c) {
            return c.ToArgb() == color.ToArgb();
        }
    }

    public class NotBlackCriterion: Criterion {
        public NotBlackCriterion() {
        }

        public override bool Match(Color c) {
            return c.ToArgb() != Color.Black.ToArgb();
        }
    }
    #endregion

    #region Geometric criterions
    public abstract class GeometricCriterion {
        public abstract bool Match(GeometricMoments gm);
    }

    public class BlueCircleCriterion: GeometricCriterion {
        public override bool Match(GeometricMoments gm) {
            if( gm.GetM1() >= 0.50 && gm.GetM1() <= 1.1
            && gm.GetM7() >= 0.1 && gm.GetM7() <= 0.4 )
                return true;

            return false;
        }
    }

    public class BlueLCriterion: GeometricCriterion {
        public override bool Match(GeometricMoments gm) {
            if( gm.GetM1() >= 0.30 && gm.GetM1() <= 0.6
            &&  gm.GetM7() >= 0.003 && gm.GetM7() <= 0.05 )
                return true;

            return false;
        }
    }

    public class OrangeCircleCriterion: GeometricCriterion {
        public override bool Match(GeometricMoments gm) {
            if( gm.GetM1() >= 0.16 && gm.GetM1() <= 0.27
            &&  gm.GetM7() >= 0.005 && gm.GetM7() <= 0.015 )
                return true;

            return false;
        }
    }
    #endregion

    #region Spatial criterions
    public abstract class SpatialCriterion {
        public abstract bool Match(PicturePart p1, PicturePart p2);
    }

    public class InsideBlueCircleCriterion: SpatialCriterion {
        public override bool Match(PicturePart blueCircle, PicturePart part) {
            if( blueCircle.start.X < part.start.X && blueCircle.start.Y < part.start.Y
             && blueCircle.end.X > part.end.X     && blueCircle.end.Y > part.end.Y )
                return true;

            return false;
        }
    }

    public class OrangeCircleNearLCriterion: SpatialCriterion {
        public override bool Match(PicturePart blueL, PicturePart orangeCircle) {
            Point blueLCenter = blueL.GetCenter();
            Point orangeCircleCenter = orangeCircle.GetCenter();

            double distance = Math.Sqrt(Math.Pow(blueLCenter.X - orangeCircleCenter.X, 2) + Math.Pow(blueLCenter.Y - orangeCircleCenter.Y, 2));
            if( distance <= ( blueL.end.X - blueL.start.X ) )
                return true;

            return false;
        }
    }
    #endregion
}
