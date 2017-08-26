using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Denxorz.ZoomControl
{
    /// <summary>
    /// https://github.com/andypelzer/GraphSharp/blob/master/Graph%23.Controls
    /// https://wpfextensions.codeplex.com/
    /// </summary>
    [TemplatePart(Name = PartPresenter, Type = typeof(ZoomContentPresenter))]
    public class ZoomControl : ContentControl
    {
        private const string PartPresenter = "PART_Presenter";

        public static readonly DependencyProperty AnimationLengthProperty =
            DependencyProperty.Register("AnimationLength", typeof(TimeSpan), typeof(ZoomControl),
                new UIPropertyMetadata(TimeSpan.FromMilliseconds(500)));

        public static readonly DependencyProperty MaxZoomProperty =
            DependencyProperty.Register("MaxZoom", typeof(double), typeof(ZoomControl), new UIPropertyMetadata(100.0));

        public static readonly DependencyProperty MinZoomProperty =
            DependencyProperty.Register("MinZoom", typeof(double), typeof(ZoomControl), new UIPropertyMetadata(0.01));

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(ZoomControlModes), typeof(ZoomControl),
                                        new UIPropertyMetadata(ZoomControlModes.Custom, ModePropertyChanged));

        private static void ModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;
            var mode = (ZoomControlModes)e.NewValue;
            switch (mode)
            {
                case ZoomControlModes.Fill:
                    zc.DoZoomToFill();
                    break;
                case ZoomControlModes.Original:
                    zc.DoZoomToOriginal();
                    break;
                case ZoomControlModes.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static readonly DependencyProperty ModifierModeProperty =
            DependencyProperty.Register("ModifierMode", typeof(ZoomViewModifierMode), typeof(ZoomControl),
                new UIPropertyMetadata(ZoomViewModifierMode.None));

        public static readonly DependencyProperty TranslateXProperty =
            DependencyProperty.Register("TranslateX", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(0.0, TranslateXPropertyChanged, TranslateXCoerce));

        public static readonly DependencyProperty TranslateYProperty =
            DependencyProperty.Register("TranslateY", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(0.0, TranslateYPropertyChanged, TranslateYCoerce));

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(ZoomControl),
                                        new UIPropertyMetadata(1.0, ZoomPropertyChanged));

        private Point mouseDownPosition;
        private ZoomContentPresenter presenter;

        /// <summary>Applied to the presenter.</summary>
        private ScaleTransform scaleTransform;
        private Vector startTranslate;
        private TransformGroup transformGroup;

        /// <summary>Applied to the scrollviewer.</summary>
        private TranslateTransform translateTransform;

        private int zoomAnimCount;
        private bool isZooming;

        static ZoomControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomControl), new FrameworkPropertyMetadata(typeof(ZoomControl)));
        }

        public ZoomControl()
        {
            PreviewMouseWheel += ZoomControlMouseWheel;
            PreviewMouseDown += (sender, e) => OnMouseDown(e, true);
            MouseDown += (sender, e) => OnMouseDown(e, false);
            MouseUp += ZoomControlMouseUp;
        }

        public double TranslateX
        {
            get => (double)GetValue(TranslateXProperty);
            set
            {
                BeginAnimation(TranslateXProperty, null);
                SetValue(TranslateXProperty, value);
            }
        }

        public double TranslateY
        {
            get => (double)GetValue(TranslateYProperty);
            set
            {
                BeginAnimation(TranslateYProperty, null);
                SetValue(TranslateYProperty, value);
            }
        }

        public TimeSpan AnimationLength
        {
            get => (TimeSpan)GetValue(AnimationLengthProperty);
            set => SetValue(AnimationLengthProperty, value);
        }

        public double MinZoom
        {
            get => (double)GetValue(MinZoomProperty);
            set => SetValue(MinZoomProperty, value);
        }

        public double MaxZoom
        {
            get => (double)GetValue(MaxZoomProperty);
            set => SetValue(MaxZoomProperty, value);
        }

        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set
            {
                if (value == (double)GetValue(ZoomProperty))
                    return;
                BeginAnimation(ZoomProperty, null);
                SetValue(ZoomProperty, value);
            }
        }

        private ZoomContentPresenter Presenter
        {
            get => presenter;
            set
            {
                presenter = value;
                if (presenter == null)
                    return;

                //add the ScaleTransform to the presenter
                transformGroup = new TransformGroup();
                scaleTransform = new ScaleTransform();
                translateTransform = new TranslateTransform();
                transformGroup.Children.Add(scaleTransform);
                transformGroup.Children.Add(translateTransform);
                presenter.RenderTransform = transformGroup;
                presenter.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        /// <summary>Gets or sets the active modifier mode.</summary>
        public ZoomViewModifierMode ModifierMode
        {
            get => (ZoomViewModifierMode)GetValue(ModifierModeProperty);
            set => SetValue(ModifierModeProperty, value);
        }

        /// <summary>Gets or sets the mode of the zoom control.</summary>
        public ZoomControlModes Mode
        {
            get => (ZoomControlModes)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        private static object TranslateXCoerce(DependencyObject d, object basevalue)
        {
            var zc = (ZoomControl)d;
            return zc.presenter == null ? 0.0 : (double)basevalue;
        }

        private static object TranslateYCoerce(DependencyObject d, object basevalue)
        {
            var zc = (ZoomControl)d;
            return zc.presenter == null ? 0.0 : (double)basevalue;
        }

        private void ZoomControlMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ModifierMode != ZoomViewModifierMode.Pan)
            {
                return;
            }

            ModifierMode = ZoomViewModifierMode.None;
            PreviewMouseMove -= ZoomControlPreviewMouseMove;
            ReleaseMouseCapture();
        }

        private void ZoomControlPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (ModifierMode != ZoomViewModifierMode.Pan)
            {
                return;
            }

            var translate = startTranslate + (e.GetPosition(this) - mouseDownPosition);
            TranslateX = translate.X;
            TranslateY = translate.Y;
        }

        private void OnMouseDown(MouseButtonEventArgs e, bool isPreview)
        {
            if (ModifierMode != ZoomViewModifierMode.None)
            {
                return;
            }

            switch (Keyboard.Modifiers)
            {
                case ModifierKeys.None:
                    if (!isPreview)
                        ModifierMode = ZoomViewModifierMode.Pan;
                    break;
                case ModifierKeys.Control:
                    break;
                case ModifierKeys.Shift:
                    ModifierMode = ZoomViewModifierMode.Pan;
                    break;
                case ModifierKeys.Windows:
                    break;
                default:
                    return;
            }

            if (ModifierMode == ZoomViewModifierMode.None)
            {
                return;
            }

            mouseDownPosition = e.GetPosition(this);
            startTranslate = new Vector(TranslateX, TranslateY);
            Mouse.Capture(this);
            PreviewMouseMove += ZoomControlPreviewMouseMove;
        }

        private static void TranslateXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;
            if (zc.translateTransform == null)
            {
                return;
            }

            zc.translateTransform.X = (double)e.NewValue;
            if (!zc.isZooming)
            {
                zc.Mode = ZoomControlModes.Custom;
            }
        }

        private static void TranslateYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;
            if (zc.translateTransform == null)
            {
                return;
            }

            zc.translateTransform.Y = (double)e.NewValue;
            if (!zc.isZooming)
            {
                zc.Mode = ZoomControlModes.Custom;
            }
        }

        private static void ZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;

            if (zc.scaleTransform == null)
            {
                return;
            }

            var zoom = (double)e.NewValue;
            zc.scaleTransform.ScaleX = zoom;
            zc.scaleTransform.ScaleY = zoom;
            if (!zc.isZooming)
            {
                double delta = (double)e.NewValue / (double)e.OldValue;
                zc.TranslateX *= delta;
                zc.TranslateY *= delta;
                zc.Mode = ZoomControlModes.Custom;
            }
        }

        private void ZoomControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var origoPosition = new Point(ActualWidth / 2, ActualHeight / 2);
            Point mousePosition = e.GetPosition(this);

            var deltaZoom = Math.Max(0.2, Math.Min(2.0, e.Delta / 300.0 + 1));
            DoZoom(deltaZoom, origoPosition, mousePosition, mousePosition);
        }

        private void DoZoom(double deltaZoom, Point origoPosition, Point startHandlePosition, Point targetHandlePosition)
        {
            double startZoom = Zoom;
            double currentZoom = startZoom * deltaZoom;
            currentZoom = Math.Max(MinZoom, Math.Min(MaxZoom, currentZoom));

            var startTranslate = new Vector(TranslateX, TranslateY);

            var v = (startHandlePosition - origoPosition);
            var vTarget = (targetHandlePosition - origoPosition);

            var targetPoint = (v - startTranslate) / startZoom;
            var zoomedTargetPointPos = targetPoint * currentZoom + startTranslate;
            var endTranslate = vTarget - zoomedTargetPointPos;

            double transformX = presenter == null ? 0.0 : TranslateX + endTranslate.X;
            double transformY = presenter == null ? 0.0 : TranslateY + endTranslate.Y;

            DoZoomAnimation(currentZoom, transformX, transformY);
            Mode = ZoomControlModes.Custom;
        }

        private void DoZoomAnimation(double targetZoom, double transformX, double transformY)
        {
            isZooming = true;
            var duration = new Duration(AnimationLength);
            StartAnimation(TranslateXProperty, transformX, duration);
            StartAnimation(TranslateYProperty, transformY, duration);
            StartAnimation(ZoomProperty, targetZoom, duration);
        }

        private void StartAnimation(DependencyProperty dp, double toValue, Duration duration)
        {
            if (double.IsNaN(toValue) || double.IsInfinity(toValue))
            {
                if (dp == ZoomProperty)
                {
                    isZooming = false;
                }

                return;
            }

            var animation = new DoubleAnimation(toValue, duration);
            if (dp == ZoomProperty)
            {
                zoomAnimCount++;
                animation.Completed += (s, args) =>
                {
                    zoomAnimCount--;
                    if (zoomAnimCount > 0)
                    {
                        return;
                    }

                    var zoom = Zoom;
                    BeginAnimation(ZoomProperty, null);
                    SetValue(ZoomProperty, zoom);
                    isZooming = false;
                };
            }
            BeginAnimation(dp, animation, HandoffBehavior.Compose);
        }

        private void DoZoomToOriginal()
        {
            if (presenter == null)
            {
                return;
            }

            var initialTranslate = GetInitialTranslate();
            DoZoomAnimation(1.0, initialTranslate.X, initialTranslate.Y);
        }

        private Vector GetInitialTranslate()
        {
            if (presenter == null)
            {
                return new Vector(0.0, 0.0);
            }

            var tX = -(presenter.ContentSize.Width - presenter.DesiredSize.Width) / 2.0;
            var tY = -(presenter.ContentSize.Height - presenter.DesiredSize.Height) / 2.0;
            return new Vector(tX, tY);
        }

        public void ZoomToFill()
        {
            Mode = ZoomControlModes.Fill;
        }

        private void DoZoomToFill()
        {
            if (presenter == null || Mode != ZoomControlModes.Fill)
            {
                return;
            }

            var deltaZoom = Math.Min(ActualWidth / presenter.ContentSize.Width, ActualHeight / presenter.ContentSize.Height);
            var initialTranslate = GetInitialTranslate();
            DoZoomAnimation(deltaZoom, initialTranslate.X * deltaZoom, initialTranslate.Y * deltaZoom);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Presenter = GetTemplateChild(PartPresenter) as ZoomContentPresenter;
            if (Presenter != null)
            {
                Presenter.SizeChanged += (s, a) => DoZoomToFill();
                Presenter.ContentSizeChanged += (s, a) => DoZoomToFill();
            }
            ZoomToFill();
        }
    }
}
