using System.Windows;
using System.Windows.Controls;

namespace Denxorz.ZoomControl;

public class ZoomContentPresenter : ContentPresenter
{
    public delegate void ContentSizeChangedHandler(object sender, Size newSize);

    public event ContentSizeChangedHandler? ContentSizeChanged;

    private Size contentSize;

    public Size ContentSize
    {
        get => contentSize;
        private set
        {
            if (value == contentSize)
            {
                return;
            }

            contentSize = value;
            ContentSizeChanged?.Invoke(this, contentSize);
        }
    }

    protected override Size MeasureOverride(Size constraint)
    {
        base.MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));
        const double max = 1e9;
        var x = double.IsInfinity(constraint.Width) ? max : constraint.Width;
        var y = double.IsInfinity(constraint.Height) ? max : constraint.Height;
        return new Size(x, y);
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        var child = Content as UIElement;
        if (child == null)
        {
            return arrangeBounds;
        }

        ContentSize = child.DesiredSize;
        child.Arrange(new Rect(child.DesiredSize));

        return arrangeBounds;
    }
}