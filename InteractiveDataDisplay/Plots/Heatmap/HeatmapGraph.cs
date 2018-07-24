using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace InteractiveDataDisplay.WPF
{
    /// <summary>
    /// Control to render 2d image as a heatmap. Heatmap is widely used graphical representation of 2D array
   	/// where the values of array items are represented as colors.
    /// </summary>
    [Description("Plots a heatmap graph")]
    public class HeatmapGraph : BackgroundBitmapRenderer, ITooltipProvider
    {
        private object locker = new object();

        private long dataVersion = 0;
        private double missingValue;


        public double[] XArray
        {
            get { return (double[])GetValue(XArrayProperty); }
            set { SetValue(XArrayProperty, value); }
        }
        public static readonly DependencyProperty XArrayProperty =
            DependencyProperty.Register("XArray", typeof(double[]), typeof(HeatmapGraph), new PropertyMetadata(null, (s, e) => { ((HeatmapGraph)s).InvalidateBounds(); }));


        public double[] YArray
        {
            get { return (double[])GetValue(YArrayProperty); }
            set { SetValue(YArrayProperty, value); }
        }

        public static readonly DependencyProperty YArrayProperty =
            DependencyProperty.Register("YArray", typeof(double[]), typeof(HeatmapGraph), new PropertyMetadata(null, (s, e) => { ((HeatmapGraph)s).InvalidateBounds(); }));


        public double[,] DataContainer
        {
            get { return (double[,])GetValue(DataContainerProperty); }
            set { SetValue(DataContainerProperty, value); }
        }

        public static readonly DependencyProperty DataContainerProperty =
            DependencyProperty.Register("DataContainer", typeof(double[,]), typeof(HeatmapGraph), new PropertyMetadata(null, (s, e) => {
                ((HeatmapGraph)s).dataVersion++;
                ((HeatmapGraph)s).missingValue = Double.NaN;
                ((HeatmapGraph)s).InvalidateBounds();
                ((HeatmapGraph)s).QueueRenderTask();
            }));




        protected override RenderTaskState PrepareRenderTaskState(long id, Size screenSize)
        {
            var result = base.PrepareRenderTaskState(id, screenSize);
            result.XArr = XArray;
            result.YArr = YArray;
            result.Data = DataContainer;
            return result;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="HeatmapGraph"/> class with default tooltip.
        /// </summary>
        public HeatmapGraph()
        {
            TooltipContentFunc = GetTooltipForPoint;
        }

        private static void VerifyDimensions(double[,] d, double[] x, double[] y)
        {
            double dlen0 = d.GetLength(0);
            double dlen1 = d.GetLength(1);
            double xlen = x.Length;
            double ylen = y.Length;
            if (dlen0 == xlen && dlen1 == ylen ||
               dlen0 == xlen - 1 && xlen > 1 && dlen1 == ylen - 1 && ylen > 1)
                return;
            throw new ArgumentException("Array dimensions do not match");
        }

        /// <summary>Plots rectangular heatmap.
        /// If size <paramref name="data"/> dimensions are equal to lenghtes of corresponding grid parameters
        /// <paramref name="x"/> and <paramref name="y"/> then Gradient render method is used. If <paramref name="data"/>
        /// dimension are smaller by one then Bitmap render method is used for heatmap. In all other cases exception is thrown.
        /// </summary>
        /// <param name="data">Two dimensional array of data.</param>
        /// <param name="x">Grid along x axis.</param>
        /// <param name="y">Grid along y axis.</param>
        /// <returns>ID of background operation. You can subscribe to <see cref="RenderCompletion"/>
        /// notification to be notified when this operation is completed or request is dropped.</returns>
        public long Plot(double[,] data, double[] x, double[] y)
        {
            return Plot(data, x, y, Double.NaN);
        }

        /// <summary>Plots rectangular heatmap where some data may be missing.
        /// If size <paramref name="data"/> dimensions are equal to lenghtes of corresponding grid parameters
        /// <paramref name="x"/> and <paramref name="y"/> then Gradient render method is used. If <paramref name="data"/>
        /// dimension are smaller by one then Bitmap render method is used for heatmap. In all other cases exception is thrown.
        /// </summary>
        /// <param name="data">Two dimensional array of data.</param>
        /// <param name="x">Grid along x axis.</param>
        /// <param name="y">Grid along y axis.</param>
        /// <param name="missingValue">Missing value. Data items equal to <paramref name="missingValue"/> aren't shown.</param>
        /// <returns>ID of background operation. You can subscribe to <see cref="RenderCompletion"/>
        /// notification to be notified when this operation is completed or request is dropped.</returns>
        public long Plot(double[,] data, double[] x, double[] y, double missingValue)
        {
            VerifyDimensions(data, x, y);
            lock (locker)
            {
                this.XArray = x;
                this.YArray = y;
                this.DataContainer = data;
                this.missingValue = missingValue;
                dataVersion++;
            }

            InvalidateBounds();

            return QueueRenderTask();
        }
        /// <summary>Plots rectangular heatmap where some data may be missing.
        /// If size <paramref name="data"/> dimensions are equal to lenghtes of corresponding grid parameters
        /// <paramref name="x"/> and <paramref name="y"/> then Gradient render method is used. If <paramref name="data"/>
        /// dimension are smaller by one then Bitmap render method is used for heatmap. In all other cases exception is thrown.
        /// Double, float, integer and boolean types are supported as data and grid array elements</summary>
        /// <param name="data">Two dimensional array of data.</param>
        /// <param name="x">Grid along x axis.</param>
        /// <param name="y">Grid along y axis.</param>
        /// <param name="missingValue">Missing value. Data items equal to <paramref name="missingValue"/> aren't shown.</param>
        /// <returns>ID of background operation. You can subscribe to <see cref="RenderCompletion"/>
        /// notification to be notified when this operation is completed or request is dropped.</returns>
        public long Plot<T, A>(T[,] data, A[] x, A[] y, T missingValue)
        {
            return Plot(ArrayExtensions.ToDoubleArray2D(data),
                ArrayExtensions.ToDoubleArray(x),
                ArrayExtensions.ToDoubleArray(y),
                Convert.ToDouble(missingValue, CultureInfo.InvariantCulture));
        }

        /// <summary>Plots rectangular heatmap where some data may be missing.
        /// If size <paramref name="data"/> dimensions are equal to lenghtes of corresponding grid parameters
        /// <paramref name="x"/> and <paramref name="y"/> then Gradient render method is used. If <paramref name="data"/>
        /// dimension are smaller by one then Bitmap render method is used for heatmap. In all other cases exception is thrown.
        /// Double, float, integer and boolean types are supported as data and grid array elements</summary>
        /// <param name="data">Two dimensional array of data.</param>
        /// <param name="x">Grid along x axis.</param>
        /// <param name="y">Grid along y axis.</param>
        /// <returns>ID of background operation. You can subscribe to <see cref="RenderCompletion"/>
        /// notification to be notified when this operation is completed or request is dropped.</returns>
        public long Plot<T, A>(T[,] data, A[] x, A[] y)
        {
            return Plot(ArrayExtensions.ToDoubleArray2D(data),
                ArrayExtensions.ToDoubleArray(x),
                ArrayExtensions.ToDoubleArray(y),
                Double.NaN);
        }


        /// <summary>Returns content bounds of this elements in cartesian coordinates.</summary>
        /// <returns>Rectangle with content bounds.</returns>
        protected override DataRect ComputeBounds()
        {
            if (XArray != null && YArray != null)
                return new DataRect(XArray[0], YArray[0], XArray[XArray.Length - 1], YArray[YArray.Length - 1]);
            else
                return DataRect.Empty;
        }

        /// <summary>
        /// Cached value of <see cref="Palette"/> property. Accessed both from UI and rendering thread.
        /// </summary>
        private IPalette palette = InteractiveDataDisplay.WPF.Palette.Heat;

        /// <summary>
        /// Identifies the <see cref="Palette"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PaletteProperty = DependencyProperty.Register(
            "Palette",
            typeof(Palette),
            typeof(HeatmapGraph),
            new PropertyMetadata(InteractiveDataDisplay.WPF.Palette.Heat, OnPalettePropertyChanged));

        /// <summary>Gets or sets the palette for heatmap rendering.</summary>
        [TypeConverter(typeof(StringToPaletteTypeConverter))]
        [Category("InteractiveDataDisplay")]
        [Description("Defines mapping from values to color")]
        public IPalette Palette
        {
            get { return (IPalette)GetValue(PaletteProperty); }
            set { SetValue(PaletteProperty, value); }
        }

        private bool paletteRangeUpdateRequired = true;

        private static void OnPalettePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HeatmapGraph heatmap = (HeatmapGraph)sender;
            lock (heatmap.locker)
            {
                heatmap.paletteRangeUpdateRequired = true;
                heatmap.palette = (Palette)e.NewValue;
            }
            heatmap.QueueRenderTask();
        }

        /// <summary>
        /// Identifies the <see cref="PaletteRange"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PaletteRangeProperty = DependencyProperty.Register(
            "PaletteRange",
            typeof(Range),
            typeof(HeatmapGraph),
            new PropertyMetadata(new Range(0, 1), OnPaletteRangePropertyChanged));

        /// <summary>
        /// Cached range of data values. It is accessed from UI and rendering thread.
        /// </summary>
        private Range dataRange = new Range(0, 1);

        /// <summary>Version of data for current data range. If dataVersion != dataRangeVersion then
        /// data range version should be recalculated.</summary>
        private long dataRangeVersion = -1;

        private int insidePaletteRangeSetter = 0;

        /// <summary>Gets range of data values used in palette building.</summary>
        /// <remarks>This property cannot be set from outside code. Attempt to set it from
        /// bindings result in exception.</remarks>
        [Browsable(false)]
        public Range PaletteRange
        {
            get { return (Range)GetValue(PaletteRangeProperty); }
            protected set
            {
                try
                {
                    insidePaletteRangeSetter++;
                    SetValue(PaletteRangeProperty, value);
                }
                finally
                {
                    insidePaletteRangeSetter--;
                }
            }
        }

        private static void OnPaletteRangePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var heatmap = (HeatmapGraph)sender;
            if (heatmap.insidePaletteRangeSetter <= 0)
                throw new InvalidOperationException("Palette Range property cannot be changed by binding. Use Palette property instead");
        }

        private void UpdatePaletteRange(long localDataVersion)
        {
            if (dataVersion != localDataVersion)
                return;
            paletteRangeUpdateRequired = false;
            if (palette.IsNormalized)
                PaletteRange = dataRange;
            else
                PaletteRange = palette.Range;
        }

        /// <summary>Gets range of data values for current data.</summary>
        public Range DataRange
        {
            get
            {
                if (DataContainer != null && dataVersion != dataRangeVersion)
                {
                    var r = Double.IsNaN(missingValue) ?
                        HeatmapBuilder.GetMaxMin(DataContainer) :
                        HeatmapBuilder.GetMaxMin(DataContainer, missingValue);
                    lock (locker)
                    {
                        dataRangeVersion = dataVersion;
                        dataRange = r;
                    }
                    UpdatePaletteRange(dataVersion);
                }
                return dataRange;
            }
        }

        /// <summary>
        /// Renders frame and returns it as a render result.
        /// </summary>
        /// <param name="state">Render task state for rendering frame.</param>
        /// <returns>Render result of rendered frame.</returns>
        protected override RenderResult RenderFrame(RenderTaskState state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            if (!state.Bounds.IsEmpty && !state.IsCanceled && state.Data != null)
            {
                DataRect dataRect = state.ActualPlotRect;
                DataRect output = new DataRect(0, 0, state.ScreenSize.Width, state.ScreenSize.Height);
                DataRect bounds = state.Bounds;

                if (dataRect.XMin >= bounds.XMax || dataRect.XMax <= bounds.XMin ||
                    dataRect.YMin >= bounds.YMax || dataRect.YMax <= bounds.YMin)
                    return null;

                double left = 0;
                double xmin = dataRect.XMin;
                double scale = output.Width / dataRect.Width;
                if (xmin < bounds.XMin)
                {
                    left = (bounds.XMin - dataRect.XMin) * scale;
                    xmin = bounds.XMin;
                }

                double width = output.Width - left;
                double xmax = dataRect.XMax;
                if (xmax > bounds.XMax)
                {
                    width -= (dataRect.XMax - bounds.XMax) * scale;
                    xmax = bounds.XMax;
                }

                scale = output.Height / dataRect.Height;
                double top = 0;
                double ymax = dataRect.YMax;
                if (ymax > bounds.YMax)
                {
                    top = (dataRect.YMax - bounds.YMax) * scale;
                    ymax = bounds.YMax;
                }

                double height = output.Height - top;
                double ymin = dataRect.YMin;
                if (ymin < bounds.YMin)
                {
                    height -= (bounds.YMin - dataRect.YMin) * scale;
                    ymin = bounds.YMin;
                }

                if (xmin < bounds.XMin)
                    xmin = bounds.XMin;
                if (xmax > bounds.XMax)
                    xmax = bounds.XMax;
                if (ymin < bounds.YMin)
                    ymin = bounds.YMin;
                if (ymax > bounds.YMax)
                    ymax = bounds.YMax;

                DataRect visibleData = new DataRect(xmin, ymin, xmax, ymax);

                double[,] localData;
                double[] localX, localY;
                long localDataVersion;
                IPalette localPalette;
                double localMV;
                Range localDataRange;
                bool getMaxMin = false;
                lock (locker)
                {
                    localData = state.Data;
                    localX = state.XArr;
                    localY = state.YArr;
                    localDataVersion = dataVersion;
                    localPalette = palette;
                    localMV = missingValue;
                    localDataRange = dataRange;
                    if (palette.IsNormalized && dataVersion != dataRangeVersion)
                        getMaxMin = true;
                }
                if (getMaxMin)
                {
                    localDataRange = Double.IsNaN(missingValue) ?
                        HeatmapBuilder.GetMaxMin(state.Data) :
                        HeatmapBuilder.GetMaxMin(state.Data, missingValue);
                    lock (locker)
                    {
                        if (dataVersion == localDataVersion)
                        {
                            dataRangeVersion = dataVersion;
                            dataRange = localDataRange;
                        }
                        else
                            return null;
                    }
                }
                if (paletteRangeUpdateRequired)
                    Dispatcher.BeginInvoke(new Action<long>(UpdatePaletteRange), localDataVersion);
                return new RenderResult(HeatmapBuilder.BuildHeatMap(new Rect(0, 0, width, height),
                    visibleData, localX, localY, localData, localMV, localPalette, localDataRange), visibleData, new Point(left, top), width, height);
            }
            else
                return null;
        }

        /// <summary>Gets or sets function to get tooltip object (string or UIElement)
        /// for given screen point.</summary>
        /// <remarks><see cref="GetTooltipForPoint"/> method is called by default.</remarks>
        public Func<Point, object> TooltipContentFunc
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the string that is shown in tooltip for the screen point. If there is no data for this point (or nearest points) on a screen then returns null.
        /// </summary>
        /// <param name="screenPoint">A point to show tooltip for.</param>
        /// <returns>An object.</returns>
        public object GetTooltipForPoint(Point screenPoint)
        {
            double pointData;
            Point nearest;
            if (GetNearestPointAndValue(screenPoint, out nearest, out pointData))
                return String.Format(CultureInfo.InvariantCulture, "Data: {0}; X: {1}; Y: {2}", pointData, nearest.X, nearest.Y);
            else
                return null;
        }

        /// <summary>
        /// Finds the point nearest to a specified point on a screen.
        /// </summary>
        /// <param name="screenPoint">The point to search nearest for.</param>
        /// <param name="nearest">The out parameter to handle the founded point.</param>
        /// <param name="vd">The out parameter to handle data of founded point.</param>
        /// <returns>Boolen value indicating whether the nearest point was found or not.</returns>
        public bool GetNearestPointAndValue(Point screenPoint, out Point nearest, out double vd)
        {
            nearest = new Point(Double.NaN, Double.NaN);
            vd = Double.NaN;
            if (DataContainer == null || XArray == null || YArray == null)
                return false;
            Point dataPoint = new Point(XDataTransform.PlotToData(XFromLeft(screenPoint.X)), YDataTransform.PlotToData(YFromTop(screenPoint.Y)));
            int i = ArrayExtensions.GetNearestIndex(XArray, dataPoint.X);
            if (i < 0)
                return false;
            int j = ArrayExtensions.GetNearestIndex(YArray, dataPoint.Y);
            if (j < 0)
                return false;
            if (IsBitmap)
            {
                if (i > 0 && XArray[i - 1] > dataPoint.X)
                    i--;
                if (j > 0 && YArray[j - 1] > dataPoint.Y)
                    j--;
            }
            if (i < DataContainer.GetLength(0) && j < DataContainer.GetLength(1))
            {
                vd = DataContainer[i, j];
                nearest = new Point(XArray[i], YArray[j]);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Gets the boolen value indicating whether heatmap is rendered using gradient filling. 
        /// </summary>
        public bool IsGradient
        {
            get
            {
                return (DataContainer == null || XArray == null) ? false : DataContainer.GetLength(0) == XArray.Length;
            }
        }

        /// <summary>
        /// Gets the boolen value indicating whether heatmap is rendered as a bitmap. 
        /// </summary>
        public bool IsBitmap
        {
            get
            {
                return (DataContainer == null || XArray == null) ? false : DataContainer.GetLength(0) == XArray.Length - 1;
            }
        }
    }
}

