using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WpfTriangleRender.Model;

namespace WpfTriangleRender
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }


        public PointCollection PlotPoints
        {
            get { return (PointCollection)GetValue(PlitPointsProperty); }
            set { SetValue(PlitPointsProperty, value); }
        }

        public double[] XArray
        {
            get { return (double[])GetValue(XArrayProperty); }
            set { SetValue(XArrayProperty, value); }
        }


        public double[] YArray
        {
            get { return (double[])GetValue(YArrayProperty); }
            set { SetValue(YArrayProperty, value); }
        }

        public double[,] Data
        {
            get { return (double[,])GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        public static readonly DependencyProperty XArrayProperty =
            DependencyProperty.Register("XArray", typeof(double[]), typeof(MainWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty YArrayProperty =
            DependencyProperty.Register("YArray", typeof(double[]), typeof(MainWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(double[,]), typeof(MainWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty PlitPointsProperty =
            DependencyProperty.Register("PlotPoints", typeof(PointCollection), typeof(MainWindow), new PropertyMetadata(null));
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            const int N = 30;
            const int M = 18;

            var xArray = new double[N + 1];
            var yArray = new double[M + 1];
            var data = new double[N, M];
            for (int i = 0; i <= N; i++)
                xArray[i] = i;

            for (int j = 0; j <= M; j++)
                yArray[j] = j;

            var image = new double[M, N];
            PointCollection pc = new PointCollection() { new Point(7.0 - 0.25 + 5.0 * 2.0, 3.0 + 0.05 - 1.0 * 2.0), new Point(2.0 - 0.25, 4.0 + 0.05), new Point(25.75, 4.05), new Point(7.0 - 0.25 + 5.0 * 2.0, 3.0 + 0.05 - 1.0 * 2.0) };
            image = TriangleRenderer.DrawTriangle(image, pc[0].X, pc[0].Y, pc[1].X, pc[1].Y, pc[2].X, pc[2].Y);
            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++)
                {
                    data[i, j] = image[j, i];

                }
            PlotPoints = pc;
            XArray = xArray;
            YArray = yArray;
            Data = data;
            //heatmap.Plot(data, xArray, yArray);
        }
    }
}