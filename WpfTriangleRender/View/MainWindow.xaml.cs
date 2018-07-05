using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            /*double[] x = new double[200];
            for (int i = 0; i < x.Length; i++)
                x[i] = 3.1415 * i / (x.Length - 1);
            for (int i = 0; i < 5; i++)
            {
                var lg = new InteractiveDataDisplay.WPF.LineGraph();
                lines.Children.Add(lg);
                //lg.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, (byte)(i * 100), 0));
                //lg.Description = String.Format("Data series {0}", i + 1);
                lg.StrokeThickness = 20;
                //lg.Plot(x, x.Select(v => System.Math.Sin(v + i / 10.0)).ToArray());
                var pc = new System.Windows.Media.PointCollection(200);
                for (int j = 0; j < x.GetLength(0); j ++)
                    pc.Add(new Point(x[j], System.Math.Sin(x[j] + i / 10.0)));
                lg.Points = pc;
            }*/

            /*
            double phase = 0;

            const int N = 1000;
            const int M = 500;

            double[] x = new double[N + 1];
            double[] y = new double[M + 1];
            double[,] f = new double[N, M];

            // Coordinate grid is constant
            for (int i = 0; i <= N; i++)
                x[i] = -System.Math.PI + 2 * i * System.Math.PI / N;

            for (int j = 0; j <= M; j++)
                y[j] = -System.Math.PI / 2 + j * System.Math.PI / M;

            // Data array is updated
            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++)
                    f[i, j] = System.Math.Sqrt(x[i] * x[i] + y[j] * y[j]) * 
                        System.Math.Abs(System.Math.Cos(x[i] * x[i] + y[j] * y[j] + phase));
            phase += 0.1;

            long id = heatmap.Plot(f, x, y); // receive a unique operation identifier
            */

            const int N = 30;
            const int M = 18;

            double[] x = new double[N + 1];
            double[] y = new double[M + 1];
            double[,] f = new double[N, M];

            // Coordinate grid is constant
            for (int i = 0; i <= N; i++)
                x[i] = i;

            for (int j = 0; j <= M; j++)
                y[j] = j;

            var image = new double[M, N];
            image = TriangleRenderer.DrawTriangle(image, 7.0 - 0.25 + 5.0 * 2.0, 3.0 + 0.05 - 1.0 * 2.0, 2.0 - 0.25, 4.0 + 0.05, 25.75, 4.05);

            // convert coordinates. ToDo correct it in InteractiveDataDisplay
            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++)
                    f[i, j] = image[j, i];

            long id = heatmap.Plot(f, x, y); // receive a unique operation identifier
            
        }
    }
}
