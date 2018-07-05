using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTriangleRender.Model
{
    class TriangleRenderer
    {
        private static ushort[] ConvertDoubleToUshort(double[,] imageDouble)
        {
            int imageHeight = imageDouble.GetLength(0);
            int imageWidth = imageDouble.GetLength(1);
            double imageDoubleMin = double.MaxValue;
            double imageDoubleMax = double.MinValue;
            for (int h = 0; h < imageHeight; h++)
                for (int w = 0; w < imageWidth; w++)
                {
                    if (imageDoubleMin > imageDouble[h, w]) imageDoubleMin = imageDouble[h, w];
                    if (imageDoubleMax < imageDouble[h, w]) imageDoubleMax = imageDouble[h, w];
                }

            ushort[] imageUshort = new ushort[imageHeight * imageWidth];
            for (int h = 0; h < imageHeight; h++)
                for (int w = 0; w < imageWidth; w++)
                    imageUshort[h * imageWidth + w] = (ushort)((imageDouble[h, w] - imageDoubleMin) *
                        ushort.MaxValue / (imageDoubleMax - imageDoubleMin));
            return imageUshort;
        }

        private double[,] GetMaskBinary()
        {
            var maskList = new List<double[]>();
            var minLen = int.MaxValue;
            var lines = File.ReadAllLines(@"C:\Users\abaku\Pictures\Tilt_MFL\mask.txt");
            foreach (var line in lines)
            {
                var value = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var len = value.GetLength(0);
                if (minLen > len) minLen = len;
                var bin = new double[len];
                for (int i = 0; i < len; i++)
                    bin[i] = double.Parse(value[i]);
                maskList.Add(bin);
            }
            var maskBinary = new double[minLen, maskList.Count];
            for (int i = 0; i < minLen; i++)
                for (int j = 0; j < maskList.Count; j++)
                    maskBinary[i, j] = maskList[i][j];
            return maskBinary;
        }


        public static double[,] DrawTriangle(double[,] image, double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double yMax = image.GetLength(0);
            double xMax = image.GetLength(1);

            //image = DrawPoint(image, x1, y1);
            //image = DrawPoint(image, x2, y2);
            image = DrawPoint(image, x3, y3);
            // x direction is along image width, lower x is on the left.
            // proceed only for ordered vertices
            // y1 on top y3 on bottom. y directrion is along image heiht, lower y is on the top.
            if (y1 <= y2 && y2 <= y3)
            {
                // find intersection line
                double x4 = x1 + ((y2 - y1) / (y3 - y1)) * (x3 - x1);
                //image = DrawPoint(image, x4, y2);

                // draw top flat triangle
                int l = (int)(y1 + -y1 % 1);
                image = DrawLineBot(image, l, x2, y2, x1, y1, x4);
                image = DrawLineBot(image, l + 1, x2, y2, x1, y1, x4);
                image = DrawLineBot(image, l + 2, x2, y2, x1, y1, x4);
                image = DrawLineBot(image, l + 3, x2, y2, x1, y1, x4);
                //image = DrawPoint(image, x1i, y1b);
                // draw bottom line
                for (int y = (int)Math.Ceiling(y1); y <= y2; y++)
                {
                    //for (int x = (int)Math.Floor(x1); x < x4; x++)
                    //    image[x, y] = 1;
                }
                // draw top flat triangle
                for (int y = (int)Math.Floor(y3); y > y2; y--)
                {
                    //for (int x = (int)Math.Floor(x1); x < x4; x++)
                    //    image[x, y] = 1;
                }
            }
            return image;
        }

        private static double[,] DrawLineBot(double[,] image, int l, double x1, double y1, double x2, double y2,
            double x3)
        {
            double yMax = image.GetLength(0);
            double xMax = image.GetLength(1);

            double i1x = LineIntersectionX(x1, y1, x2, y2, 0, l + 1, xMax, l + 1);
            double i2x = LineIntersectionX(x1, y1, x2, y2, 0, l, xMax, l);
            double i3x = LineIntersectionX(x2, y2, x3, y1, 0, l, xMax, l);
            double i4x = LineIntersectionX(x2, y2, x3, y1, 0, l + 1, xMax, l + 1);
            double k1 = (y1 - y2) / (x2 - x1);
            double k2 = (y2 - y1) / (x3 - x2);
            int s1x = (int)(i1x - i1x % 1);
            int s2x = (int)(i2x - i2x % 1);
            int s3x = (int)(i3x - i3x % 1);
            int s4x = (int)(i4x - i4x % 1);
            double y1s = k1 * (1 - i2x % 1);
            double y2s = k2 * (1 - i3x % 1);
            if (l > 0 && l < yMax && s1x > 0 && s1x < xMax)
                image[l, s1x] = y1s / 2;
            for (int x = s1x + 1; x < s2x; x++)
            {
                double intencity = y1s + k1 * (x - s1x - 1) + k1 / 2;
                if (intencity > 0)
                    if (l > 0 && l < yMax && x > 0 && x < xMax)
                        if (intencity > 1)
                            image[l, x] = 1;
                        else
                            image[l, x] = intencity;
            }
            //TODO make connection between edges if no gap ( s2x = s3 )
            if (s2x + 1 > s3x)
            {
                double intencity =
                    (1 - k1 * (i2x % 1) / 2) * (x1 % 1) +
                    (1 + y2s / 2) * (1 - x1 % 1);
                if (l > 0 && l < yMax && s2x > 0 && s2x < xMax)
                    image[l, s2x] = intencity;
            }
            else
            {
                if (l > 0 && l < yMax && s2x > 0 && s2x < xMax)
                    image[l, s2x] = 1 - k1 * (i2x % 1) / 2;
                for (int x = s2x + 1; x < s3x; x++)
                {
                    if (l > 0 && l < yMax && x > 0 && x < xMax)
                        image[l, x] = 1;
                }
                if (l > 0 && l < yMax && s3x > 0 && s3x < xMax)
                    image[l, s3x] = 1 + y2s / 2;
            }
            for (int x = s3x + 1; x < s4x; x++)
            {
                double intencity = 1 + y2s + k2 * (x - s3x - 1) + k2 / 2;
                if (intencity > 0)
                    if (l > 0 && l < yMax && x > 0 && x < xMax)
                        if (intencity > 1)
                            image[l, x] = 1;
                        else
                            image[l, x] = intencity;
            }
            if (l > 0 && l < yMax && s4x > 0 && s4x < xMax)
                image[l, s4x] = -k2 * (1 - i4x % 1) / 2;

            return image;
        }

        private static double LineIntersectionX(double x1, double y1, double x2, double y2,
            double x3, double y3, double x4, double y4)
        {
            return ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) /
                ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));
        }

        private static double LineIntersectionY(double x1, double y1, double x2, double y2,
            double x3, double y3, double x4, double y4)
        {
            return ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) /
                ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));
        }


        private double[,] DrawBorders(double[,] image)
        {
            for (int h = 0; h < image.GetLength(0); h++)
            {
                image[h, 0] = 1;
                image[h, image.GetLength(1) - 1] = 1;
            }
            for (int w = 0; w < image.GetLength(1); w++)
            {
                image[0, w] = 1;
                image[image.GetLength(0) - 1, w] = 1;
            }
            return image;
        }

        private static double[,] DrawPoint(double[,] image, double x1, double y1)
        {
            double yMax = image.GetLength(0);
            double xMax = image.GetLength(1);

            double x1p = x1 % 1;
            double y1p = y1 % 1;
            double x1b = (1 - x1p);
            double y1b = (1 - y1p);
            int x1f = (int)(x1 - x1p);
            int x1c = (int)(x1 + x1b);
            int y1f = (int)(y1 - y1p);
            int y1c = (int)(y1 + y1b);
            if (y1f > 0 && y1f < yMax && x1f > 0 && x1f < xMax) image[y1f, x1f] = x1b * y1b;
            if (y1f > 0 && y1f < yMax && x1c > 0 && x1c < xMax) image[y1f, x1c] = x1p * y1b;
            if (y1c > 0 && y1c < yMax && x1f > 0 && x1f < xMax) image[y1c, x1f] = x1b * y1p;
            if (y1c > 0 && y1c < yMax && x1c > 0 && x1c < xMax) image[y1c, x1c] = x1p * y1p;
            //image[(int)Math.Round(y1), (int)Math.Round(x1)] = 1;
            return image;
        }
    }
}
