using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Comdat.DOZP.Core
{
    public class CroppingAdorner : Adorner
    {
        #region PuncturedRect class

        private class PuncturedRect : Shape
        {
            #region Dependency properties

            public static readonly DependencyProperty RectInteriorProperty =
                DependencyProperty.Register(
                    "RectInterior",
                    typeof(Rect),
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(
                        new Rect(0, 0, 0, 0),
                        FrameworkPropertyMetadataOptions.AffectsRender,
                        null,
                        new CoerceValueCallback(CoerceRectInterior),
                        false
                    ),
                    null
                );

            private static object CoerceRectInterior(DependencyObject d, object value)
            {
                PuncturedRect pr = (PuncturedRect)d;
                Rect rcExterior = pr.RectExterior;
                Rect rcProposed = (Rect)value;
                double left = Math.Max(rcProposed.Left, rcExterior.Left);
                double top = Math.Max(rcProposed.Top, rcExterior.Top);
                double width = Math.Min(rcProposed.Right, rcExterior.Right) - left;
                double height = Math.Min(rcProposed.Bottom, rcExterior.Bottom) - top;
                rcProposed = new Rect(left, top, width, height);
                return rcProposed;
            }

            public Rect RectInterior
            {
                get { return (Rect)GetValue(RectInteriorProperty); }
                set { SetValue(RectInteriorProperty, value); }
            }

            public static readonly DependencyProperty RectExteriorProperty =
                DependencyProperty.Register(
                    "RectExterior",
                    typeof(Rect),
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(
                        new Rect(0, 0, double.MaxValue, double.MaxValue),
                        FrameworkPropertyMetadataOptions.AffectsMeasure |
                        FrameworkPropertyMetadataOptions.AffectsArrange |
                        FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                        FrameworkPropertyMetadataOptions.AffectsParentArrange |
                        FrameworkPropertyMetadataOptions.AffectsRender,
                        null,
                        null,
                        false
                    ),
                    null
                );

            public Rect RectExterior
            {
                get { return (Rect)GetValue(RectExteriorProperty); }
                set { SetValue(RectExteriorProperty, value); }
            }

            #endregion

            #region Constructors

            public PuncturedRect()
                : this(new Rect(0, 0, double.MaxValue, double.MaxValue), new Rect())
            {
            }

            public PuncturedRect(Rect rectExterior, Rect rectInterior)
            {
                RectInterior = rectInterior;
                RectExterior = rectExterior;
            }

            #endregion

            #region Geometry

            protected override Geometry DefiningGeometry
            {
                get
                {
                    PathGeometry pthgExt = new PathGeometry();
                    PathFigure pthfExt = new PathFigure();
                    pthfExt.StartPoint = RectExterior.TopLeft;
                    pthfExt.Segments.Add(new LineSegment(RectExterior.TopRight, false));
                    pthfExt.Segments.Add(new LineSegment(RectExterior.BottomRight, false));
                    pthfExt.Segments.Add(new LineSegment(RectExterior.BottomLeft, false));
                    pthfExt.Segments.Add(new LineSegment(RectExterior.TopLeft, false));
                    pthgExt.Figures.Add(pthfExt);

                    Rect rectIntSect = Rect.Intersect(RectExterior, RectInterior);
                    PathGeometry pthgInt = new PathGeometry();
                    PathFigure pthfInt = new PathFigure();
                    pthfInt.StartPoint = rectIntSect.TopLeft;
                    pthfInt.Segments.Add(new LineSegment(rectIntSect.TopRight, false));
                    pthfInt.Segments.Add(new LineSegment(rectIntSect.BottomRight, false));
                    pthfInt.Segments.Add(new LineSegment(rectIntSect.BottomLeft, false));
                    pthfInt.Segments.Add(new LineSegment(rectIntSect.TopLeft, false));
                    pthgInt.Figures.Add(pthfInt);

                    CombinedGeometry cmbg = new CombinedGeometry(GeometryCombineMode.Exclude, pthgExt, pthgInt);
                    return cmbg;
                }
            }

            #endregion
        }

        #endregion

        #region CropThumb class

        private class CropThumb : Thumb
        {
            #region Private variables

            int _cpx;

            #endregion

            #region Constructor

            internal CropThumb(int cpx)
                : base()
            {
                _cpx = cpx;
            }

            #endregion

            #region Overrides

            protected override Visual GetVisualChild(int index)
            {
                return null;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                drawingContext.DrawRoundedRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(new Size(_cpx, _cpx)), 1, 1);
            }

            #endregion

            #region Positioning

            internal void SetPos(double x, double y)
            {
                Canvas.SetTop(this, y - _cpx / 2);
                Canvas.SetLeft(this, x - _cpx / 2);
            }

            #endregion
        }

        #endregion

        #region Private variables

        // Width of the thumbs. I know these really aren't "pixels", but px is still a good mnemonic.
        private const int THUMB_SIZE = 6;

        // PuncturedRect to hold the "Cropping" portion of the adorner
        private PuncturedRect _prCropMask;

        // Canvas to hold the thumbs so they can be moved in response to the user
        private Canvas _cnvThumbs;

        // Cropping adorner uses Thumbs for visual elements. The Thumbs have built-in mouse input handling.
        private CropThumb _crtTop, _crtLeft, _crtBottom, _crtRight;
        private CropThumb _crtTopLeft, _crtTopRight, _crtBottomLeft, _crtBottomRight;

        // To store and manage the adorner's visual children.
        private VisualCollection _vc;

        // DPI for screen
        private static double s_dpiX, s_dpiY;

        #endregion

        #region Properties

        public Rect ClippingRectangle
        {
            get
            {
                return _prCropMask.RectInterior;
            }
        }

        #endregion

        #region Routed Events

        public static readonly RoutedEvent CropChangedEvent = EventManager.RegisterRoutedEvent(
            "CropChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CroppingAdorner));

        public event RoutedEventHandler CropChanged
        {
            add
            {
                base.AddHandler(CroppingAdorner.CropChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(CroppingAdorner.CropChangedEvent, value);
            }
        }

        #endregion

        #region Dependency Properties

        static public DependencyProperty FillProperty = Shape.FillProperty.AddOwner(typeof(CroppingAdorner));

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        private static void FillPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            CroppingAdorner crp = (d as CroppingAdorner);

            if (crp != null)
            {
                crp._prCropMask.Fill = (Brush)args.NewValue;
            }
        }

        #endregion

        #region Constructor

        static CroppingAdorner()
        {
            Color col = Colors.Red;

            System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            s_dpiX = g.DpiX;
            s_dpiY = g.DpiY;
            col.A = 80;

            FillProperty.OverrideMetadata(typeof(CroppingAdorner),
                new PropertyMetadata(
                    new SolidColorBrush(col),
                    new PropertyChangedCallback(FillPropChanged)));
        }

        public CroppingAdorner(UIElement adornedElement, Rect rcInit)
            : base(adornedElement)
        {
            if (adornedElement == null) throw new ArgumentNullException("adornedElement");

            if (rcInit == null || rcInit == Rect.Empty)
            {
                rcInit = new Rect(0, 0, adornedElement.RenderSize.Width, adornedElement.RenderSize.Height);
            }

            System.Diagnostics.Debug.WriteLine(String.Format("CroppingAdorner Width={0:N0}, Height={1:N0}", rcInit.Width, rcInit.Height));

            _vc = new VisualCollection(this);
            _prCropMask = new PuncturedRect();
            _prCropMask.IsHitTestVisible = false;
            _prCropMask.RectInterior = rcInit;
            _prCropMask.Fill = Fill;
            _vc.Add(_prCropMask);
            _cnvThumbs = new Canvas();
            _cnvThumbs.HorizontalAlignment = HorizontalAlignment.Stretch;
            _cnvThumbs.VerticalAlignment = VerticalAlignment.Stretch;

            _vc.Add(_cnvThumbs);
            BuildCorner(ref _crtTop, Cursors.SizeNS);
            BuildCorner(ref _crtBottom, Cursors.SizeNS);
            BuildCorner(ref _crtLeft, Cursors.SizeWE);
            BuildCorner(ref _crtRight, Cursors.SizeWE);
            BuildCorner(ref _crtTopLeft, Cursors.SizeNWSE);
            BuildCorner(ref _crtTopRight, Cursors.SizeNESW);
            BuildCorner(ref _crtBottomLeft, Cursors.SizeNESW);
            BuildCorner(ref _crtBottomRight, Cursors.SizeNWSE);

            // Add handlers for Cropping.
            _crtBottomLeft.DragDelta += new DragDeltaEventHandler(HandleBottomLeft);
            _crtBottomRight.DragDelta += new DragDeltaEventHandler(HandleBottomRight);
            _crtTopLeft.DragDelta += new DragDeltaEventHandler(HandleTopLeft);
            _crtTopRight.DragDelta += new DragDeltaEventHandler(HandleTopRight);
            _crtTop.DragDelta += new DragDeltaEventHandler(HandleTop);
            _crtBottom.DragDelta += new DragDeltaEventHandler(HandleBottom);
            _crtRight.DragDelta += new DragDeltaEventHandler(HandleRight);
            _crtLeft.DragDelta += new DragDeltaEventHandler(HandleLeft);

            //add eventhandler to drag and drop 
            adornedElement.MouseLeftButtonDown += new MouseButtonEventHandler(HandleMouseLeftButtonDown);
            adornedElement.MouseMove += new MouseEventHandler(HandleMouseMove);
            adornedElement.MouseLeftButtonUp += new MouseButtonEventHandler(HandleMouseLeftButtonUp);

            // We have to keep the clipping interior withing the bounds of the adorned element
            // so we have to track it's size to guarantee that...
            FrameworkElement fel = (adornedElement as FrameworkElement);
            if (fel != null)
            {
                fel.SizeChanged += new SizeChangedEventHandler(AdornedElement_SizeChanged);
            }
        }

        #endregion

        #region Drag and drop handlers

        double originX;
        double originY;

        private void HandleDrag(double dx, double dy)
        {
            Rect rcInterior = _prCropMask.RectInterior;
            rcInterior = new Rect(dx, dy, rcInterior.Width, rcInterior.Height);

            _prCropMask.RectInterior = rcInterior;
            SetThumbs(_prCropMask.RectInterior);

            RaiseEvent(new RoutedEventArgs(CropChangedEvent, this));
        }

        private void HandleMouseMove(object sender, MouseEventArgs args)
        {
            Image img = (sender as Image);

            if (img != null && img.IsMouseCaptured)
            {
                double x = args.GetPosition(img).X;
                double y = args.GetPosition(img).Y;
                double _x = _prCropMask.RectInterior.X;
                double _y = _prCropMask.RectInterior.Y;
                double _width = _prCropMask.RectInterior.Width;
                double _height = _prCropMask.RectInterior.Height;

                if (((x > _x) && (x < (_x + _width))) && ((y > _y) && (y < (_y + _height))))
                {
                    _x = _x + (x - originX);
                    _y = _y + (y - originY);

                    if (_x < 0) { _x = 0; }
                    if ((_x + _width) > img.ActualWidth) { _x = img.ActualWidth - _width; }
                    if (_y < 0) { _y = 0; }
                    if ((_y + _height) > img.ActualHeight) { _y = img.ActualHeight - _height; }

                    originX = x;
                    originY = y;

                    HandleDrag(_x, _y);
                }
            }
        }

        private void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image img = (sender as Image);

            if (img != null)
            {
                img.CaptureMouse();

                originX = e.GetPosition(img).X;
                originY = e.GetPosition(img).Y;
            }
        }

        private void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = (sender as Image);

            if (img != null)
            {
                img.ReleaseMouseCapture();
            }
        }

        #endregion

        #region Thumb handlers

        // Generic handler for Cropping
        private void HandleThumb(double drcL, double drcT, double drcW, double drcH, double dx, double dy)
        {
            Rect rcInterior = _prCropMask.RectInterior;

            if (rcInterior.Width + drcW * dx < 0)
            {
                dx = -rcInterior.Width / drcW;
            }

            if (rcInterior.Height + drcH * dy < 0)
            {
                dy = -rcInterior.Height / drcH;
            }

            rcInterior = new Rect(
                rcInterior.Left + drcL * dx,
                rcInterior.Top + drcT * dy,
                rcInterior.Width + drcW * dx,
                rcInterior.Height + drcH * dy);

            _prCropMask.RectInterior = rcInterior;
            SetThumbs(_prCropMask.RectInterior);
            RaiseEvent(new RoutedEventArgs(CropChangedEvent, this));
        }

        // Handler for Cropping from the bottom-left.
        private void HandleBottomLeft(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(1, 0, -1, 1, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the bottom-right.
        private void HandleBottomRight(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(0, 0, 1, 1, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the top-right.
        private void HandleTopRight(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(0, 1, 1, -1, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the top-left.
        private void HandleTopLeft(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(1, 1, -1, -1, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the top.
        private void HandleTop(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(0, 1, 0, -1, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the left.
        private void HandleLeft(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(1, 0, -1, 0, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the right.
        private void HandleRight(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(0, 0, 1, 0, args.HorizontalChange, args.VerticalChange);
            }
        }

        // Handler for Cropping from the bottom.
        private void HandleBottom(object sender, DragDeltaEventArgs args)
        {
            if (sender is CropThumb)
            {
                HandleThumb(0, 0, 0, 1, args.HorizontalChange, args.VerticalChange);
            }
        }

        #endregion

        #region Other handlers

        private void AdornedElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement fel = sender as FrameworkElement;
            Rect rcInterior = _prCropMask.RectInterior;
            bool fFixupRequired = false;
            double
                intLeft = rcInterior.Left,
                intTop = rcInterior.Top,
                intWidth = rcInterior.Width,
                intHeight = rcInterior.Height;

            if (rcInterior.Left > fel.RenderSize.Width)
            {
                intLeft = fel.RenderSize.Width;
                intWidth = 0;
                fFixupRequired = true;
            }

            if (rcInterior.Top > fel.RenderSize.Height)
            {
                intTop = fel.RenderSize.Height;
                intHeight = 0;
                fFixupRequired = true;
            }

            if (rcInterior.Right > fel.RenderSize.Width)
            {
                intWidth = Math.Max(0, fel.RenderSize.Width - intLeft);
                fFixupRequired = true;
            }

            if (rcInterior.Bottom > fel.RenderSize.Height)
            {
                intHeight = Math.Max(0, fel.RenderSize.Height - intTop);
                fFixupRequired = true;
            }

            if (fFixupRequired)
            {
                _prCropMask.RectInterior = new Rect(intLeft, intTop, intWidth, intHeight);
            }
        }

        #endregion

        #region Arranging/positioning

        private void SetThumbs(Rect rc)
        {
            _crtBottomRight.SetPos(rc.Right, rc.Bottom);
            _crtTopLeft.SetPos(rc.Left, rc.Top);
            _crtTopRight.SetPos(rc.Right, rc.Top);
            _crtBottomLeft.SetPos(rc.Left, rc.Bottom);
            _crtTop.SetPos(rc.Left + rc.Width / 2, rc.Top);
            _crtBottom.SetPos(rc.Left + rc.Width / 2, rc.Bottom);
            _crtLeft.SetPos(rc.Left, rc.Top + rc.Height / 2);
            _crtRight.SetPos(rc.Right, rc.Top + rc.Height / 2);
        }

        // Arrange the Adorners.
        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect rcExterior = new Rect(0, 0, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height);
            _prCropMask.RectExterior = rcExterior;
            Rect rcInterior = _prCropMask.RectInterior;
            _prCropMask.Arrange(rcExterior);

            SetThumbs(rcInterior);
            _cnvThumbs.Arrange(rcExterior);

            return finalSize;
        }

        #endregion

        #region Public interface

        public BitmapSource Crop()
        {
            Thickness margin = AdornerMargin();
            Rect rcInterior = _prCropMask.RectInterior;
            System.Drawing.Point pxFromSize = UnitsToPx(rcInterior.Width, rcInterior.Height);
            System.Drawing.Point pxFromPos = UnitsToPx(rcInterior.Left + margin.Left, rcInterior.Top + margin.Top);
            System.Drawing.Point pxWhole = UnitsToPx(AdornedElement.RenderSize.Width + margin.Left, AdornedElement.RenderSize.Height + margin.Left);
            pxFromSize.X = Math.Max(Math.Min(pxWhole.X - pxFromPos.X, pxFromSize.X), 0);
            pxFromSize.Y = Math.Max(Math.Min(pxWhole.Y - pxFromPos.Y, pxFromSize.Y), 0);
            if (pxFromSize.X == 0 || pxFromSize.Y == 0) return null;

            Int32Rect rcFrom = new Int32Rect(pxFromPos.X, pxFromPos.Y, pxFromSize.X, pxFromSize.Y);
            RenderTargetBitmap rtb = new RenderTargetBitmap(pxWhole.X, pxWhole.Y, s_dpiX, s_dpiY, PixelFormats.Default);
            rtb.Render(AdornedElement);

            return new CroppedBitmap(rtb, rcFrom);
        }

        public Rect GetCropZone(double width, double height)
        {
            Rect cropZone = Rect.Empty;

            Thickness margin = AdornerMargin();
            Rect rcInterior = _prCropMask.RectInterior;
            System.Drawing.Point pxFromSize = UnitsToPx(rcInterior.Width, rcInterior.Height);
            System.Drawing.Point pxFromPos = UnitsToPx(rcInterior.Left + margin.Left, rcInterior.Top + margin.Top);
            System.Drawing.Point pxWhole = UnitsToPx(AdornedElement.RenderSize.Width + margin.Left, AdornedElement.RenderSize.Height + margin.Left);
            pxFromSize.X = Math.Max(Math.Min(pxWhole.X - pxFromPos.X, pxFromSize.X), 0);
            pxFromSize.Y = Math.Max(Math.Min(pxWhole.Y - pxFromPos.Y, pxFromSize.Y), 0);

            if (pxFromSize.X != 0 && pxFromSize.Y != 0)
            {
                double ratioX = width / (double)pxWhole.X;
                double ratioY = height / (double)pxWhole.Y;
                cropZone = new Rect((int)(ratioX * pxFromPos.X), (int)(ratioY * pxFromPos.Y), (int)(ratioX * pxFromSize.X), (int)(ratioY * pxFromSize.Y));
            }

            return cropZone;
        }

        #endregion

        #region Helper functions

        private Thickness AdornerMargin()
        {
            Thickness thick = new Thickness(0);

            if (AdornedElement is FrameworkElement)
            {
                thick = ((FrameworkElement)AdornedElement).Margin;
            }

            return thick;
        }

        private void BuildCorner(ref CropThumb crt, Cursor cur)
        {
            if (crt != null) return;

            crt = new CropThumb(THUMB_SIZE);

            // Set some arbitrary visual characteristics.
            crt.Cursor = cur;

            _cnvThumbs.Children.Add(crt);
        }

        private System.Drawing.Point UnitsToPx(double x, double y)
        {
            return new System.Drawing.Point((int)(x * s_dpiX / 96), (int)(y * s_dpiY / 96));
        }

        #endregion

        #region Visual tree overrides

        // Override the VisualChildrenCount and GetVisualChild properties to interface with the adorner's visual collection.
        protected override int VisualChildrenCount { get { return _vc.Count; } }
        protected override Visual GetVisualChild(int index) { return _vc[index]; }

        #endregion
    }
}
