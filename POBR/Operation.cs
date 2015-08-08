using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POBR {
    public abstract class Operation {
        public Operation() {}

        protected double CheckPixel(double pixel) {
            pixel = Math.Max(pixel, 0);
            pixel = Math.Min(255, pixel);

            return pixel;
        }

        protected int GetGray(Color c) {
            return (int)( .299 * c.R + .587 * c.G + .114 * c.B );            
        }

        public void Perform(Bitmap original, out Bitmap processed) {
            processed = (Bitmap)original.Clone();

            for( int x = 0; x < original.Width; x++ )
            for( int y = 0; y < original.Height; y++ ) {
                Process(original, ref processed, x, y);
            }
        }

        public abstract void Process(Bitmap original, ref Bitmap processed, int x, int y);
    }

    #region GrayScale
    public class GrayScale: Operation {
        public GrayScale() {}

        public override void Process(Bitmap original, ref Bitmap processed, int x, int y) {
            Color c = original.GetPixel(x, y);
            byte gray = (byte)( .299 * c.R + .587 * c.G + .114 * c.B );            
            processed.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
        }
    }
    #endregion

    #region Negation
    public class Negation : Operation {
        public Negation() { }

        public override void Process(Bitmap original, ref Bitmap processed, int x, int y) {
            Color c = original.GetPixel(x, y);

            int R = 255 - c.R;
            int G = 255 - c.G;
            int B = 255 - c.B;

            processed.SetPixel(x, y, Color.FromArgb((byte)R, (byte)G, (byte)B));
        }
    }
    #endregion

    #region BrightnessContrast
    public class BrightnessContrast: Operation {
        private int brightness;
        private float contrast; 

        public BrightnessContrast(int b, float c) {
            brightness = b;
            contrast = c;
        }

        public override void Process(Bitmap original, ref Bitmap processed, int x, int y) {
            Color c = original.GetPixel(x, y);

            int R = (int)base.CheckPixel(c.R * contrast + brightness);
            int G = (int)base.CheckPixel(c.G * contrast + brightness);
            int B = (int)base.CheckPixel(c.B * contrast + brightness);

            processed.SetPixel(x, y, Color.FromArgb((byte)R, (byte)G, (byte)B));
        }
    }
    #endregion

    #region Saturation
    public class Saturation: Operation {
        private float saturation;

        public Saturation(float s) {
            saturation = s;
        }

        public override void Process(Bitmap original, ref Bitmap processed, int x, int y) {
            Color c = original.GetPixel(x, y);
            float sat = c.GetSaturation() + saturation > 1.0f ? 1.0f : c.GetSaturation() + saturation;
            Color changed = Utilities.HSBtoRGB(c.GetHue(), sat, c.GetBrightness());

            processed.SetPixel(x, y, changed);
        }
    }
    #endregion

    #region MedianFilter
    public class MedianFilter: Operation {
        public MedianFilter() { }

        public override void Process(Bitmap original, ref Bitmap processed, int x, int y) {
            if( x > 1 && x < original.Width - 1 && y > 1 && y < original.Height - 1 ) {
                List<Color> colors = new List<Color>();
                int R, G, B;

                // add pixel mask 3x3
                colors.Add(original.GetPixel(x - 1, y - 1));
                colors.Add(original.GetPixel(x, y - 1));
                colors.Add(original.GetPixel(x + 1, y - 1));
                colors.Add(original.GetPixel(x - 1, y));
                colors.Add(original.GetPixel(x, y));
                colors.Add(original.GetPixel(x + 1, y));
                colors.Add(original.GetPixel(x - 1, y + 1));
                colors.Add(original.GetPixel(x, y + 1));
                colors.Add(original.GetPixel(x + 1, y + 1));

                // sort by RED and get median element
                colors.OrderBy(c => c.R);
                R = colors.ElementAt(4).R;

                // sort by GREEN and get median element
                colors.OrderBy(c => c.G);
                G = colors.ElementAt(4).G;

                // sort by BLUE and get median element
                colors.OrderBy(c => c.B);
                B = colors.ElementAt(4).B;

                processed.SetPixel(x, y, Color.FromArgb((byte)R, (byte)G, (byte)B));
            }
        }
    }
    #endregion

    #region HighPassFilter
    public class HighPassFilter: Operation {
        private int[] factor;
        private int sum;

        public HighPassFilter() {
            factor = new int[] {-1, -1, -1
                                -1,  10, -1
                                -1, -1, -1};

            for( int i = 0; i < factor.Length; i++ )
                sum += factor[i];
        }

        public HighPassFilter(int[] f) {
            factor = f;
            for( int i = 0; i < factor.Length; i++ )
                sum += factor[i];
        }

        public override void Process(Bitmap original, ref Bitmap processed, int x, int y) {
            if( x > 1 && x < original.Width - 1 && y > 1 && y < original.Height - 1 ) {
                Color[] colors = new Color[9];
                int R = 0,
                    G = 0,
                    B = 0;

                // add pixel mask 3x3
                colors[0] = original.GetPixel(x - 1, y - 1);
                colors[1] = original.GetPixel(x, y - 1);
                colors[2] = original.GetPixel(x + 1, y - 1);
                colors[3] = original.GetPixel(x - 1, y);
                colors[4] = original.GetPixel(x, y);
                colors[5] = original.GetPixel(x + 1, y);
                colors[6] = original.GetPixel(x - 1, y + 1);
                colors[7] = original.GetPixel(x, y + 1);
                colors[8] = original.GetPixel(x + 1, y + 1);

                for(int i = 0; i < factor.Length; i++) {
                    R += colors[i].R * factor[i];
                    G += colors[i].G * factor[i];
                    B += colors[i].B * factor[i];
                }

                if( sum != 0 ) {
                    R /= sum;
                    G /= sum;
                    B /= sum;
                }

                R = (int)base.CheckPixel(R);
                G = (int)base.CheckPixel(G);
                B = (int)base.CheckPixel(B);

                processed.SetPixel(x, y, Color.FromArgb((byte)R, (byte)G, (byte)B));
            }
        }
    }
    #endregion

    #region Tresholding
    public class Tresholding: Operation {
        private int low;
        private int high;

        public Tresholding(int l, int h) {
            low = l;
            high = h;
        }

        public override void Process(Bitmap original, ref Bitmap processed, int x, int y) {
            Color c = original.GetPixel(x, y);
            byte color;

            if( GetGray(c) >= low && GetGray(c) <= high )
                color = 255;
            else
                color = 0;

            processed.SetPixel(x, y, Color.FromArgb(color, color, color));
        }
    }
    #endregion

    #region ColorReplacment
    public class ColorReplacment: Operation {
        private List<HSBCriterion> criterions;
        private Color notMatchColor;

        public ColorReplacment(Color c) {
            notMatchColor = c;
            criterions = new List<HSBCriterion>();
        }

        public void AddCriterion(HSBCriterion c) {
            criterions.Add(c);
        }

        public override void Process(Bitmap original, ref Bitmap processed, int x, int y) {
            Color c = original.GetPixel(x, y);
            Color replacement = notMatchColor;

            foreach( HSBCriterion criterion in criterions ) {
                if( criterion.Match(c) ) {
                    replacement = criterion.SearchColor;
                    break;
                }
            }

            // if we got replacement color passed
            processed.SetPixel(x, y, replacement);
        }
    }
    #endregion

    #region Dilation
    public class Dilation: Operation {
        public Color background = Color.Black;

        public Dilation() {
        }

        public override void Process(Bitmap original, ref Bitmap processed, int x, int y) {
            if( x > 1 && x < original.Width - 1 && y > 1 && y < original.Height - 1 ) {
                if( original.GetPixel(x, y).ToArgb() == background.ToArgb() ) {
                    List<Color> colors = new List<Color>();

                    // add pixel mask 3x3
                    colors.Add(original.GetPixel(x - 1, y - 1));
                    colors.Add(original.GetPixel(x, y - 1));
                    colors.Add(original.GetPixel(x + 1, y - 1));
                    colors.Add(original.GetPixel(x - 1, y));
                    colors.Add(original.GetPixel(x + 1, y));
                    colors.Add(original.GetPixel(x - 1, y + 1));
                    colors.Add(original.GetPixel(x, y + 1));
                    colors.Add(original.GetPixel(x + 1, y + 1));

                    // get pixels which color is not black and set current pixel color to this one
                    List<Color> notBackground = colors.Where(c => !( new ColorCriterion(background).Match(c) )).ToList();
                    if( notBackground.Count() > 0 ) {
                        processed.SetPixel(x, y, notBackground.ElementAt(0));
                    }
                }
            }
        }
    }
    #endregion

    #region Erosion
    public class Erosion: Operation {
        public Color background = Color.Black;

        public Erosion() {
        }

        public override void Process(Bitmap original, ref Bitmap processed, int x, int y) {
            if( x > 1 && x < original.Width - 1 && y > 1 && y < original.Height - 1 ) {
                if( original.GetPixel(x, y).ToArgb() != background.ToArgb() ) {
                    List<Color> colors = new List<Color>();

                    // add pixel mask 3x3
                    colors.Add(original.GetPixel(x - 1, y - 1));
                    colors.Add(original.GetPixel(x, y - 1));
                    colors.Add(original.GetPixel(x + 1, y - 1));
                    colors.Add(original.GetPixel(x - 1, y));
                    colors.Add(original.GetPixel(x + 1, y));
                    colors.Add(original.GetPixel(x - 1, y + 1));
                    colors.Add(original.GetPixel(x, y + 1));
                    colors.Add(original.GetPixel(x + 1, y + 1));

                    // get pixels which color is not black and set current pixel color to this one
                    List<Color> isBackground = colors.Where(c => ( new ColorCriterion(background).Match(c) )).ToList();
                    if( isBackground.Count() > 0 ) {
                        processed.SetPixel(x, y, background);
                    }
                }
            }
        }
    }
    #endregion
}
