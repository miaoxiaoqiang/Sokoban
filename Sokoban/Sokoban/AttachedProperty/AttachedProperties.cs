using System.Windows;

namespace Sokoban.AttachedProperty
{
    public sealed class AttachedProperties : DependencyObject
    {
        static AttachedProperties()
        {

        }

        public static readonly DependencyProperty ButtonHeaderTextProperty =
        DependencyProperty.RegisterAttached("ButtonHeaderText", typeof(string), typeof(AttachedProperties), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ImagePathProperty =
        DependencyProperty.RegisterAttached("ImagePath", typeof(string), typeof(AttachedProperties), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FIconProperty = DependencyProperty.RegisterAttached(
            "FIcon", typeof(string), typeof(AttachedProperties), new FrameworkPropertyMetadata(""));

        public static readonly DependencyProperty FIconSizeProperty = DependencyProperty.RegisterAttached(
            "FIconSize", typeof(double), typeof(AttachedProperties), new FrameworkPropertyMetadata(12D));

        public static readonly DependencyProperty FIconMarginProperty = DependencyProperty.RegisterAttached(
            "FIconMargin", typeof(Thickness), typeof(AttachedProperties), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius", typeof(CornerRadius), typeof(AttachedProperties), new FrameworkPropertyMetadata(null));

        public static string GetButtonHeaderText(DependencyObject obj)
        {
            return (string)obj.GetValue(ButtonHeaderTextProperty);
        }

        public static void SetButtonHeaderText(DependencyObject obj, string value)
        {
            obj.SetValue(ButtonHeaderTextProperty, value);
        }

        public static string GetImagePath(DependencyObject obj)
        {
            return (string)obj.GetValue(ImagePathProperty);
        }

        public static void SetImagePath(DependencyObject obj, string value)
        {
            obj.SetValue(ImagePathProperty, value);
        }

        public static string GetFIcon(DependencyObject d)
        {
            return (string)d.GetValue(FIconProperty);
        }

        public static void SetFIcon(DependencyObject obj, string value)
        {
            obj.SetValue(FIconProperty, value);
        }

        public static double GetFIconSize(DependencyObject d)
        {
            return (double)d.GetValue(FIconSizeProperty);
        }

        public static void SetFIconSize(DependencyObject obj, double value)
        {
            obj.SetValue(FIconSizeProperty, value);
        }

        public static Thickness GetFIconMargin(DependencyObject d)
        {
            return (Thickness)d.GetValue(FIconMarginProperty);
        }

        public static void SetFIconMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(FIconMarginProperty, value);
        }

        public static CornerRadius GetCornerRadius(DependencyObject d)
        {
            return (CornerRadius)d.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }
    }
}
