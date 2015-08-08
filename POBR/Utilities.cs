using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POBR {
    public class Utilities {
        public static Color HSBtoRGB(float hue, float saturation, float brightness) {
            hue = hue > 1.0 ? hue / 360 : hue;
            saturation = saturation > 1.0 ? saturation / 100 : saturation;
            brightness = brightness > 1.0 ? brightness / 100 : brightness;

            int r = 0, g = 0, b = 0;
            if( saturation == 0 ) {
                r = g = b = (int)( brightness * 255.0f + 0.5f );
            }
            else {
                float h = ( hue - (float)Math.Floor(hue) ) * 6.0f;
                float f = h - (float)Math.Floor(h);
                float p = brightness * ( 1.0f - saturation );
                float q = brightness * ( 1.0f - saturation * f );
                float t = brightness * ( 1.0f - ( saturation * ( 1.0f - f ) ) );
                switch( (int)h ) {
                    case 0:
                        r = (int)( brightness * 255.0f + 0.5f );
                        g = (int)( t * 255.0f + 0.5f );
                        b = (int)( p * 255.0f + 0.5f );
                        break;
                    case 1:
                        r = (int)( q * 255.0f + 0.5f );
                        g = (int)( brightness * 255.0f + 0.5f );
                        b = (int)( p * 255.0f + 0.5f );
                        break;
                    case 2:
                        r = (int)( p * 255.0f + 0.5f );
                        g = (int)( brightness * 255.0f + 0.5f );
                        b = (int)( t * 255.0f + 0.5f );
                        break;
                    case 3:
                        r = (int)( p * 255.0f + 0.5f );
                        g = (int)( q * 255.0f + 0.5f );
                        b = (int)( brightness * 255.0f + 0.5f );
                        break;
                    case 4:
                        r = (int)( t * 255.0f + 0.5f );
                        g = (int)( p * 255.0f + 0.5f );
                        b = (int)( brightness * 255.0f + 0.5f );
                        break;
                    case 5:
                        r = (int)( brightness * 255.0f + 0.5f );
                        g = (int)( p * 255.0f + 0.5f );
                        b = (int)( q * 255.0f + 0.5f );
                        break;
                }
            }
            return Color.FromArgb(Convert.ToByte(255), Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }

        public static Tuple<double, double, double> RGBtoHSB(int red, int green, int blue) {
            // normalize red, green and blue values
            double r = ( (double)red / 255.0 );
            double g = ( (double)green / 255.0 );
            double b = ( (double)blue / 255.0 );

            // conversion start
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            double h = 0.0;
            if( max == r && g >= b ) {
                h = 60 * ( g - b ) / ( max - min );
            }
            else if( max == r && g < b ) {
                h = 60 * ( g - b ) / ( max - min ) + 360;
            }
            else if( max == g ) {
                h = 60 * ( b - r ) / ( max - min ) + 120;
            }
            else if( max == b ) {
                h = 60 * ( r - g ) / ( max - min ) + 240;
            }

            double s = ( max == 0 ) ? 0.0 : ( 1.0 - ( min / max ) );

            return new Tuple<double, double, double>(h, s, (double)max);
        }
    }
}
