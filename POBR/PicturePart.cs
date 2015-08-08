using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace POBR {
    public class Pixel {
        public int X;
        public int Y;
        public Color Color;

        public Pixel(int x, int y, Color c) {
            X = x;
            Y = y;
            Color = c;
        }
    }

    public class PicturePart {
        private static int minPartSize = 150;

        public Bitmap part;
        public Point start;
        public Point end;

        public List<Point> pixels;

        public PicturePart(Bitmap picture, Point s, Point e) {
            part = picture;
            start = s;
            end = e;

            pixels = new List<Point>();
        }

        public static List<PicturePart> GetParts(Bitmap image, Criterion criterion) {
            bool[,] imageArray = ConvertToBool(image, criterion);
            return GetParts(image, imageArray);
        }

        private static List<PicturePart> GetParts(Bitmap image, bool[,] imageArray) {
            List<PicturePart> parts = new List<PicturePart>();
            for (int x = 2; x < imageArray.GetLength(0) - 4; x++) {
                for (int y = 2; y < imageArray.GetLength(1) - 4; y++) {
                    if (imageArray[x, y]) {
                        Stack<Point> pending = new Stack<Point>();
                        pending.Push(new Point(x, y));
                        PicturePart pp = new PicturePart(image, new Point(x, y), new Point(x, y));

                        int count = 0;
                        while (pending.Count > 0) {
                            count++;
                            Point p = (Point) pending.Pop();
                            if (imageArray[p.X, p.Y]) {
                                imageArray[p.X, p.Y] = false;

                                pp.AddPoint(p.X, p.Y);

                                if (p.X > 0 && imageArray[p.X - 1, p.Y])
                                        pending.Push(new Point(p.X - 1, p.Y));

                                if (p.X + 1 < imageArray.GetLength(0) && imageArray[p.X + 1, p.Y])
                                        pending.Push(new Point(p.X + 1, p.Y));

                                if (p.Y > 0 && imageArray[p.X, p.Y - 1])
                                        pending.Push(new Point(p.X, p.Y - 1));

                                if (p.Y + 1 < imageArray.GetLength(1) && imageArray[p.X, p.Y + 1])
                                        pending.Push(new Point(p.X, p.Y + 1));

                                // check cross
                                //if( p.X > 0 && p.Y > 0 && imageArray[p.X - 1, p.Y - 1] )
                                //    pending.Push(new Point(p.X - 1, p.Y - 1));

                                //if( p.X + 1 < imageArray.GetLength(0) && p.Y > 0 && imageArray[p.X + 1, p.Y - 1] )
                                //    pending.Push(new Point(p.X + 1, p.Y));

                                //if( p.Y > 0 && p.X > 0 && imageArray[p.X - 1, p.Y - 1] )
                                //    pending.Push(new Point(p.X - 1, p.Y - 1));

                                //if( p.Y + 1 < imageArray.GetLength(1) && p.X > 0 && imageArray[p.X - 1, p.Y + 1] )
                                //    pending.Push(new Point(p.X - 1, p.Y + 1));
                            }
                        }

                        if(count > minPartSize) {
                            parts.Add(pp);
                        }
                    }
                }
            }
            
            return parts;
        }

        private static bool[,] ConvertToBool(Bitmap image, Criterion criterion) {
            bool[,] imageArray = new bool[image.Width, image.Height];
            for( int x = 0; x < image.Width; x++ ) {
                for (int y = 0; y < image.Height; y++) {
                    if(criterion.Match(image.GetPixel(x, y)))
                        imageArray[x, y] = true;
                    else
                        imageArray[x, y] = false;
                }
            }

            return imageArray;
        }

        protected void AddPoint(int x, int y) {
            pixels.Add(new Point(x, y));

            start.X = Math.Min(start.X, x);
            start.Y = Math.Min(start.Y, y);

            end.X = Math.Max(end.X, x);
            end.Y = Math.Max(end.Y, y);
        }

        public Point GetCenter() {
            return new Point(
                (int) (start.X + (end.X - start.X) / 2),
                (int) (start.Y + (end.Y - start.Y) / 2)
            );
        }
    }
}
