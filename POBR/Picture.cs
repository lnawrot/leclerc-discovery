using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace POBR {
    public class Constants {
        public const float ORANGE_BRIGHTNESS = 65;
        public const float ORANGE_HUE = 20;
        public const float ORANGE_SATURATION = 80;

        public const float BLUE_BRIGHTNESS = 50;
        public const float BLUE_HUE = 225;
        public const float BLUE_SATURATION = 80;   

        public const float BRIGHTNESS_MARGIN = 30;
        public const float HUE_MARGIN = 25;
        public const float SATURATION_MARGIN = 20;
    }

    public class Picture: INotifyPropertyChanged {
        private Bitmap original;
        private Bitmap processed;

        private Color BLUE = Utilities.HSBtoRGB(Constants.BLUE_HUE, Constants.BLUE_SATURATION, Constants.BLUE_BRIGHTNESS);
        private Color ORANGE = Utilities.HSBtoRGB(Constants.ORANGE_HUE, Constants.ORANGE_SATURATION, Constants.ORANGE_BRIGHTNESS);

        // specific color parts
        private List<PicturePart> blueParts;
        private List<PicturePart> orangeParts;

        // possible elements of logo after checking geometric moments
        private List<PicturePart> possibleBlueCircle;
        private List<PicturePart> possibleBlueL;
        private List<PicturePart> possibleOrangeCircle;

        // found logos
        private List<Tuple<PicturePart, PicturePart, PicturePart>> logos;

        public Picture(string path) {
            using( Stream BitmapStream = System.IO.File.Open(path, System.IO.FileMode.Open) ) {
                Image img = System.Drawing.Image.FromStream(BitmapStream);
                original = new Bitmap(img);
                processed = (Bitmap)original.Clone();
            }

            Process();
        }

        public void Process() {
            Preprocess();
            SegmentationAndMoments();

            CheckSpatialCriterions();
            MarkResults();

            OnPropertyChanged("Image");
        }

        #region Image processing
        private void Preprocess() {
            // median filter
            Operation median = new MedianFilter();
            median.Perform(original, out processed);
            original = processed;

            // high pass filter
            Operation highPass = new HighPassFilter();
            highPass.Perform(original, out processed);
            original = processed;

            // check HSB colors and change matching ones with proper color
            Color replacement = Color.Black;
            ColorReplacment cr = new ColorReplacment(replacement);

            HSBCriterion blue = new HSBCriterion(BLUE, Constants.BRIGHTNESS_MARGIN, Constants.HUE_MARGIN, Constants.SATURATION_MARGIN);
            HSBCriterion orange = new HSBCriterion(ORANGE, Constants.BRIGHTNESS_MARGIN, Constants.HUE_MARGIN, Constants.SATURATION_MARGIN);

            cr.AddCriterion(blue);
            cr.AddCriterion(orange);

            cr.Perform(original, out processed);
            Bitmap orig = (Bitmap)processed.Clone();

            // dilation
            Operation dilation = new Dilation();
            dilation.Perform(orig, out processed);
            orig = processed;

            // erosion
            Operation erosion = new Erosion();
            erosion.Perform(orig, out processed);
        }

        private void SegmentationAndMoments() {
            // get blue parts
            ColorCriterion blueCC = new ColorCriterion(BLUE);
            blueParts = PicturePart.GetParts(processed, blueCC);

            // get orange parts
            ColorCriterion orangeCC = new ColorCriterion(ORANGE);
            orangeParts = PicturePart.GetParts(processed, orangeCC);

            // looog moments
            //LogMomentsValues(blueParts, blueCC);

            // check blue parts moments
            possibleBlueCircle = CheckParts(blueParts, new BlueCircleCriterion(), blueCC);
            possibleBlueL = CheckParts(blueParts, new BlueLCriterion(), blueCC);

            // check orange parts moments
            possibleOrangeCircle = CheckParts(orangeParts, new OrangeCircleCriterion(), orangeCC);
        }

        private void CheckSpatialCriterions() {
            logos = new List<Tuple<PicturePart,PicturePart,PicturePart>>();

            InsideBlueCircleCriterion insideBlueCircle = new InsideBlueCircleCriterion();
            OrangeCircleNearLCriterion orangeCircleNearL = new OrangeCircleNearLCriterion();

            foreach( PicturePart blueCircle in possibleBlueCircle )
                foreach( PicturePart blueL in possibleBlueL )
                    foreach( PicturePart orangeCircle in possibleOrangeCircle ) {
                        if( insideBlueCircle.Match(blueCircle, blueL)
                        &&  insideBlueCircle.Match(blueCircle, orangeCircle)
                        &&  orangeCircleNearL.Match(blueL, orangeCircle) ) {
                            Tuple<PicturePart, PicturePart, PicturePart> t = new Tuple<PicturePart, PicturePart, PicturePart>(blueCircle, blueL, orangeCircle);
                            logos.Add(t);
                        }
                    }
        }

        private void MarkResults() {
            if( logos.Count > 0 ) {
                foreach(Tuple<PicturePart, PicturePart, PicturePart> logo in logos ) {
                    PicturePart blueCircle = logo.Item1;
                    MakeBorder(blueCircle, Color.Cyan);
                }
            }

            processed = original;
        }
        #endregion

        #region helpers
        private void LogMomentsValues(List<PicturePart> parts, ColorCriterion cc) {
            List<PicturePart> result = new List<PicturePart>();
            foreach( PicturePart pp in parts ) {
                GeometricMoments gm = new GeometricMoments(pp, cc);
                System.Console.WriteLine(String.Format("M{0} = {1}", 1, gm.GetM1()));
                System.Console.WriteLine(String.Format("M{0} = {1}", 2, gm.GetM2()));
                //System.Console.WriteLine(String.Format("M{0} = {1}", 3, gm.GetM3()));
                //System.Console.WriteLine(String.Format("M{0} = {1}", 4, gm.GetM4()));
                System.Console.WriteLine(String.Format("M{0} = {1}\n\n", 7, gm.GetM7()));
                //System.Console.WriteLine(String.Format("M{0} = {1}\n\n", 8, gm.GetM8()));
            }
        }

        private List<PicturePart> CheckParts(List<PicturePart> parts, GeometricCriterion gc, ColorCriterion cc) {
            List<PicturePart> result = new List<PicturePart>();
            foreach( PicturePart pp in parts ) {
                if( gc.Match(new GeometricMoments(pp, cc)) )
                    result.Add(pp);
            }

            foreach( PicturePart pp in result ) {
                parts.Remove(pp);
            }

            return result;
        }

        private void MakeBorder(PicturePart pp, Color color) {
            for( int x = pp.start.X; x <= pp.end.X; x++ ) {
                original.SetPixel(x, pp.start.Y, color);
                original.SetPixel(x, pp.end.Y, color);
            }

            for( int y = pp.start.Y; y <= pp.end.Y; y++ ) {
                original.SetPixel(pp.start.X, y, color);
                original.SetPixel(pp.end.X, y, color);
            }
        }
        #endregion

        #region Image getter
        public BitmapImage Image {
            get {
                MemoryStream ms = new MemoryStream();
                processed.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                return bi;
            }
        }
        #endregion

        #region propertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            if( PropertyChanged != null )
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
