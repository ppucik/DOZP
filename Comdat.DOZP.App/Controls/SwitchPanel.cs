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

namespace Comdat.DOZP.App
{
    //http://www.eidias.com/blog/2013/2/19/wpf-switch-panel
    //<Controls:SwitchPanel Switch="{Binding IsEditionMode}">
    //    <Controls:SwitchPanel.ContentFalse>
    //        <TextBlock Text="Read only value" />
    //    </Controls:SwitchPanel.ContentFalse>
    //    <Controls:SwitchPanel.ContentTrue>
    //        <TextBox Text="Edit value here" />
    //    </Controls:SwitchPanel.ContentTrue>
    //</Controls:SwitchPanel>

    public class SwitchPanel : System.Windows.Controls.Panel
    {
        private ContentControl onTrueContent;
        private ContentControl onFalseContent;

        public SwitchPanel()
        {
        }

        public static readonly DependencyProperty ContentTrueProperty =
            DependencyProperty.Register("ContentTrue", typeof(UIElement), typeof(SwitchPanel), new UIPropertyMetadata(null));
        public UIElement ContentTrue
        {
            get { return (UIElement)GetValue(ContentTrueProperty); }
            set { SetValue(ContentTrueProperty, value); }
        }

        public static readonly DependencyProperty ContentFalseProperty =
            DependencyProperty.Register("ContentFalse", typeof(UIElement), typeof(SwitchPanel), new UIPropertyMetadata(null));
        public UIElement ContentFalse
        {
            get { return (UIElement)GetValue(ContentFalseProperty); }
            set { SetValue(ContentFalseProperty, value); }
        }

        public static readonly DependencyProperty SwitchProperty =
            DependencyProperty.Register("Switch", typeof(bool), typeof(SwitchPanel), new PropertyMetadata(false, OnSwitchChanged));
        public bool Switch
        {
            get { return (bool)GetValue(SwitchProperty); }
            set { SetValue(SwitchProperty, value); }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.Switch)
            {
                onTrueContent.Arrange(new Rect(finalSize));
                return onTrueContent.RenderSize;
            }
            else
            {
                onFalseContent.Arrange(new Rect(finalSize));
                return onFalseContent.RenderSize;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.Switch)
            {
                onTrueContent.Measure(availableSize);
                return onTrueContent.DesiredSize;
            }
            else
            {
                onFalseContent.Measure(availableSize);
                return onFalseContent.DesiredSize;
            }
        }

        public static void OnSwitchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SwitchPanel)d).SetContentsVisibility((bool)e.NewValue);
        }

        private void SetContentsVisibility(bool value)
        {
            if (onTrueContent != null)
                onTrueContent.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            if (onFalseContent != null)
                onFalseContent.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            onTrueContent = new ContentControl { Content = ContentTrue };
            onFalseContent = new ContentControl { Content = ContentFalse };
            Children.Add(onFalseContent);
            Children.Add(onTrueContent);
        }
    }
}
