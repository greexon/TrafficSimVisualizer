using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;

namespace Visualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private TrackReader trackReader = new TrackReader();
        private List<KeyValuePair<double, double>> trackData;

        // compute left-down and right-up points
        Point pointA = new Point();
        Point pointB = new Point();

        // for drawing
        DrawingVisual drawingVisual = new DrawingVisual();
        DrawingContext drawingContext;
        RenderTargetBitmap bmp;
        Pen pen = new Pen(Brushes.Black, 1);
        Pen penClear = new Pen(Brushes.White, 1);
        const int DRAW_CAR_SIZE = 2;

        //syncronisation
        bool isStop = false;
        ManualResetEvent pauseEvent = new ManualResetEvent(true);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Init()
        {
            // read data
            trackData = trackReader.ReadData();
            // compute left-down and right-up points
            pointA.x = trackData.Min(x => x.Key) - Common.OFFSET;
            pointA.y = trackData.Min(x => x.Value) - Common.OFFSET;
            pointB.x = trackData.Max(x => x.Key) + Common.OFFSET;
            pointB.y = trackData.Max(x => x.Value) + Common.OFFSET;
            // init drawing
            Common.width = (int)(this.ActualWidth - imageVisualize.Margin.Left - imageVisualize.Margin.Right);
            Common.height = (int)(this.ActualHeight - imageVisualize.Margin.Bottom - imageVisualize.Margin.Top);
            bmp = new RenderTargetBitmap(Common.width, Common.height, Common.DPI, Common.DPI, PixelFormats.Pbgra32);
            imageVisualize.Source = bmp;
        }

        private void Render(int index)
        {
            drawingContext = drawingVisual.RenderOpen();
            // clear old cars
            drawingContext.DrawRectangle(Brushes.White, penClear, new Rect(0, 0, Common.width, Common.height));
            // draw cars
            for (int i = 0; i < Common.CARS_COUNT; ++i)
            {
                KeyValuePair<double, double> item = trackData[index * Common.CARS_COUNT + i];
                double xPos = (item.Key - pointA.x) * 1.0 / (pointB.x - pointA.x) * Common.width;
                double yPos = (item.Value - pointA.y) * 1.0 / (pointB.y - pointA.y) * Common.height;
                drawingContext.DrawRectangle(Brushes.Black, pen, new Rect(
                    xPos, yPos, DRAW_CAR_SIZE, DRAW_CAR_SIZE));
            }
            drawingContext.Close();
            bmp.Render(drawingVisual);
        }

        private void Draw()
        {
            int count = trackData.Count / Common.CARS_COUNT;
            for (int index = 0; index < count; ++index) {
                Dispatcher.Invoke(new Action(() => this.Render(index)));
                Thread.Sleep(Common.DELAY);
                pauseEvent.WaitOne();
                if (isStop)
                {
                    break;
                }
            }
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            isStop = false;
            Init();
            Thread drawThread = new Thread(Draw);
            drawThread.Name = "Draw";
            drawThread.Start();
        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            if (pauseEvent.WaitOne(0))
            {
                pauseEvent.Reset();
            }
            else
            {
                pauseEvent.Set();
            }
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            isStop = true;
            pauseEvent.Set();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            isStop = true;
            pauseEvent.Set();
        }
    }
}
