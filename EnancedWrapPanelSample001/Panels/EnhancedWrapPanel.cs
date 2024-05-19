using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace EnancedWrapPanelSample001.Panels
{
    /// <summary>
    /// 32向 WrapPanel, 改編自 WPF WrapPanel
    /// </summary>
    public class EnhancedWrapPanel : WrapPanel
    {


        public static readonly DependencyProperty HorizontalDirectionProperty = DependencyProperty.Register(nameof(HorizontalDirection), typeof(HorizontalDirection), typeof(EnhancedWrapPanel), new FrameworkPropertyMetadata(HorizontalDirection.LeftToRight, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty VerticalDirectionProperty = DependencyProperty.Register(nameof(VerticalDirection), typeof(VerticalDirection), typeof(EnhancedWrapPanel), new FrameworkPropertyMetadata(VerticalDirection.TopToBottom, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));


        public static readonly DependencyProperty IsLineOrderByDescendingProperty = DependencyProperty.Register(nameof(IsLineOrderByDescending), typeof(bool), typeof(EnhancedWrapPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty IsItemsOrderByDescendingProperty = DependencyProperty.Register(nameof(IsItemsOrderByDescending), typeof(bool), typeof(EnhancedWrapPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));


        /// <summary>
        /// 水平生長方向
        /// </summary>
        public HorizontalDirection HorizontalDirection
        {
            get { return (HorizontalDirection)GetValue(HorizontalDirectionProperty); }
            set { SetValue(HorizontalDirectionProperty, value); }
        }

        /// <summary>
        /// 垂直生長方向
        /// </summary>
        public VerticalDirection VerticalDirection
        {
            get { return (VerticalDirection)GetValue(VerticalDirectionProperty); }
            set { SetValue(VerticalDirectionProperty, value); }
        }

        /// <summary>
        /// 一整排的排序方向
        /// H,T  = 水平, 左到右, 上到下 (itme 0 在最上排)
        /// H,B  = 水平, 左到右, 下到上 (itme 0 在最下排)
        /// V,L  = 垂直, 左到右, 上到下 (itme 0 在最左排)
        /// V,R  = 垂直, 右到左, 上到下 (itme 0 在最右排)
        /// 當這個值設定為 true 時, 會將排的順序反轉
        /// </summary>
        public bool IsLineOrderByDescending
        {
            get { return (bool)GetValue(IsLineOrderByDescendingProperty); }
            set { SetValue(IsLineOrderByDescendingProperty, value); }
        }

        /// <summary>
        /// 同一排內的項目排序
        /// H,L  = 水平, 左到右 (item 0 在最左)
        /// H,R  = 水平, 右到左 (item 0 在最右)
        /// V,T  = 垂直, 上到下 (item 0 在最上)
        /// V,B  = 垂直, 下到上 (item 0 在最下)
        /// 當這個值設定為 true 時, 會將排的順序反轉
        /// </summary>
        public bool IsItemsOrderByDescending
        {
            get { return (bool)GetValue(IsItemsOrderByDescendingProperty); }
            set { SetValue(IsItemsOrderByDescendingProperty, value); }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {

            finalSize = InternalArrang(finalSize);
            return finalSize;
        }

        private Size InternalArrang(Size finalSize)
        {
            int firstInLine = 0;
            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            double accumulatedV = 0;
            double itemU = Orientation == Orientation.Horizontal ? itemWidth : itemHeight;

            UVSize curLineSize = new UVSize(Orientation);
            UVSize uvFinalSize = new UVSize(Orientation, finalSize.Width, finalSize.Height);
            bool itemWidthSet = !WPFDoubleUtil.IsNaN(itemWidth);
            bool itemHeightSet = !WPFDoubleUtil.IsNaN(itemHeight);
            bool useItemU = Orientation == Orientation.Horizontal ? itemWidthSet : itemHeightSet;
            List<FrameworkElement> children = RetriveAllChildren();

            for (int i = 0, count = children.Count; i < count; i++)
            {
                UIElement child = children[i] as UIElement;
                FrameworkElement frameworkElement = child as FrameworkElement;
                if (child == null) continue;
                UVSize sz = new UVSize(
                    Orientation,
                    itemWidthSet ? itemWidth : child.DesiredSize.Width,
                    itemHeightSet ? itemHeight : child.DesiredSize.Height);
               // PrintSz(curLineSize, frameworkElement, sz);
                if (WPFDoubleUtil.GreaterThan(curLineSize.U + sz.U, uvFinalSize.U)) //need to switch to another line
                {                   
                    ArrangeLine(accumulatedV, curLineSize.V, firstInLine, i, useItemU, itemU);

                    accumulatedV += curLineSize.V;
                    curLineSize = sz;

                    if (WPFDoubleUtil.GreaterThan(sz.U, uvFinalSize.U)) //the element is wider then the constraint - give it a separate line                    
                    {                       
                        //switch to next line which only contain one element
                        ArrangeLine(accumulatedV, sz.V, i, ++i, useItemU, itemU);
                        accumulatedV += sz.V;
                        curLineSize = new UVSize(Orientation);
                    }
                    firstInLine = i;
                }
                else //continue to accumulate a line
                {
                    curLineSize.U += sz.U;
                    curLineSize.V = Math.Max(sz.V, curLineSize.V);
                }
            }

            //arrange the last line, if any
            if (firstInLine < children.Count)
            {
                //Debug.WriteLine($"Last Line, firstLine = {firstInLine}, children.Count = {children.Count}");
                ArrangeLine(accumulatedV, curLineSize.V, firstInLine, children.Count, useItemU, itemU);
            }
            

            return finalSize;
        }

        /// <summary>
        /// 決定要取得的 children 順序
        /// </summary>
        /// <returns></returns>
        private List<FrameworkElement> RetriveAllChildren()
        {
            List<FrameworkElement> children;
            if (!IsLineOrderByDescending)
            {
                children = InternalChildren.Cast<FrameworkElement>().ToList();
            }
            else
            {
                children = InternalChildren.Cast<FrameworkElement>().Reverse().ToList();
            }

            return children;
        }

        //private static void PrintSz(UVSize curLineSize, FrameworkElement frameworkElement, UVSize sz)
        //{
        //    Debug.WriteLine($"{frameworkElement.DataContext} SZ {nameof(sz.U)} = {sz.U}, {nameof(sz.V)} = {sz.V}");
        //    Debug.WriteLine($"{frameworkElement.DataContext} CurLineSize {nameof(curLineSize.U)} = {curLineSize.U}, {nameof(curLineSize.V)} = {curLineSize.V}");
        //}


        private void ArrangeLine(double v, double lineV, int start, int end, bool useItemU, double itemU)
        {
            double u = 0;
            bool isHorizontal = Orientation == Orientation.Horizontal;
            List<FrameworkElement> children = RetriveAllChildren();           
            // for 決定單排內的順序為 AESC 或 DESC
            if (!IsItemsOrderByDescending)
            {
                 //AESC
                for (int i = start; i < end; i++)
                {
                    UIElement child = children[i] as UIElement;
                    if (child == null) { continue; }
                    double layoutSlotU = GetLayoutSlotU(useItemU, itemU, child);
                    ArrangeChildByOHV(u, v, lineV, layoutSlotU, child);
                    u += layoutSlotU;
                    //u= ArrangeChildInLine(u, v, lineV, children, i, useItemU, itemU);
                }
            }
            else
            {
                // DESC
                for (int i = end - 1; i >= start; i--)
                {
                    UIElement child = children[i] as UIElement;
                    if (child == null) { continue; }
                    double layoutSlotU = GetLayoutSlotU(useItemU, itemU, child);
                    ArrangeChildByOHV(u, v, lineV, layoutSlotU, child);
                    u += layoutSlotU;
                    //u = ArrangeChildInLine(u, v, lineV, children, i, useItemU, itemU);
                }
            }
        }

        //private double ArrangeChildInLine(double u, double v, double lineV, List<FrameworkElement> children , int index, bool useItemU, double itemU)
        //{
        //    UIElement child = children[index] as UIElement;
        //    if (child == null) { return u; }
        //    double layoutSlotU = GetLayoutSlotU(useItemU, itemU, child);
        //    ArrangeChildByOHV(u, v, lineV, layoutSlotU, child);
        //    u += layoutSlotU;
        //    return u;
        //}


        /// <summary>
        /// 取得 LayoutSlotU
        /// </summary>
        /// <param name="useItemU"></param>
        /// <param name="itemU"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        private double GetLayoutSlotU(bool useItemU, double itemU, UIElement child)
        {
            UVSize childSize = new UVSize(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
            double layoutSlotU = useItemU ? itemU : childSize.U;
            return layoutSlotU;
        }

        /// <summary>
        /// 安排 child 的配置
        /// </summary>
        /// <param name="v"></param>
        /// <param name="lineV"></param>
        /// <param name="u"></param>
        /// <param name="child"></param>
        /// <param name="layoutSlotU"></param>
        private void ArrangeChildByOHV(double u, double v, double lineV, double layoutSlotU, UIElement child)
        {
            switch ((Orientation, HorizontalDirection, VerticalDirection))
            {
                case (Orientation.Horizontal, HorizontalDirection.LeftToRight, VerticalDirection.TopToBottom):
                    child.Arrange(new Rect(u, v, layoutSlotU, lineV));
                    break;
                case (Orientation.Horizontal, HorizontalDirection.LeftToRight, VerticalDirection.BottomToTop):
                    child.Arrange(new Rect(u, ActualHeight - v - lineV, layoutSlotU, lineV));
                    break;
                case (Orientation.Horizontal, HorizontalDirection.RightToLeft, VerticalDirection.TopToBottom):
                    child.Arrange(new Rect(ActualWidth - u - layoutSlotU, v, layoutSlotU, lineV));
                    break;
                case (Orientation.Horizontal, HorizontalDirection.RightToLeft, VerticalDirection.BottomToTop):
                    child.Arrange(new Rect(ActualWidth - u - layoutSlotU, ActualHeight - v - lineV, layoutSlotU, lineV));
                    break;
                case (Orientation.Vertical, HorizontalDirection.LeftToRight, VerticalDirection.TopToBottom):
                    child.Arrange(new Rect(v, u, lineV, layoutSlotU));
                    break;
                case (Orientation.Vertical, HorizontalDirection.LeftToRight, VerticalDirection.BottomToTop):
                    // child.Arrange(new Rect(ActualHeight - v - lineV, u, lineV, layoutSlotU));
                    child.Arrange(new Rect(v, ActualHeight - u - layoutSlotU, lineV, layoutSlotU));
                    break;
                case (Orientation.Vertical, HorizontalDirection.RightToLeft, VerticalDirection.TopToBottom):
                    child.Arrange(new Rect(ActualWidth - v - lineV, u, lineV, layoutSlotU));
                    break;
                case (Orientation.Vertical, HorizontalDirection.RightToLeft, VerticalDirection.BottomToTop):
                    child.Arrange(new Rect(ActualWidth - v - lineV, ActualHeight - u - layoutSlotU, lineV, layoutSlotU));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// this is a copy of WPF source code
        /// </summary>

        private struct UVSize
        {
            internal UVSize(Orientation orientation, double width, double height)
            {
                U = V = 0d;
                _orientation = orientation;
                Width = width;
                Height = height;
            }

            internal UVSize(Orientation orientation)
            {
                U = V = 0d;
                _orientation = orientation;
            }

            internal double U;
            internal double V;
            private Orientation _orientation;

            internal double Width
            {
                get { return _orientation == Orientation.Horizontal ? U : V; }
                set { if (_orientation == Orientation.Horizontal) U = value; else V = value; }
            }
            internal double Height
            {
                get { return _orientation == Orientation.Horizontal ? V : U; }
                set { if (_orientation == Orientation.Horizontal) V = value; else U = value; }
            }
        }
    }

    public enum HorizontalDirection
    {
        LeftToRight,
        RightToLeft,
    }

    public enum VerticalDirection
    {
        TopToBottom,
        BottomToTop,
    }
}
