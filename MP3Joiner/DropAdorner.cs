using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MP3Joiner
{
    public class DropAdorner : Adorner
    {
        private readonly bool _isAbove;
        private readonly VisualCollection _visuals;
        private readonly Line _line;

        public DropAdorner(UIElement adornedElement, bool isAbove)
            : base(adornedElement)
        {
            _isAbove = isAbove;
            _visuals = new VisualCollection(this);
            _line = new Line
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                X1 = 0,
                Y1 = 0,
                X2 = adornedElement.RenderSize.Width,
                Y2 = 0
            };

            if (!_isAbove)
            {
                _line.Y1 = adornedElement.RenderSize.Height;
                _line.Y2 = adornedElement.RenderSize.Height;
            }

            _visuals.Add(_line);
        }

        protected override int VisualChildrenCount => _visuals.Count;

        protected override Visual GetVisualChild(int index)
        {
            return _visuals[index];
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _line.X2 = AdornedElement.RenderSize.Width;
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _line.X2 = AdornedElement.RenderSize.Width;
            return finalSize;
        }
    }
}
