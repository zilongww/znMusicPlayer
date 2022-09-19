using System;
using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC
{
    public class ListBoxWrapPanel : Panel
    {
        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            Size currentLineSize = new Size();
            Size panelSize = new Size();

            foreach (UIElement element in base.InternalChildren)
            {
                element.Measure(availableSize);
                Size desiredSize = element.DesiredSize;

                if (currentLineSize.Width + desiredSize.Width > availableSize.Width)
                {
                    panelSize.Width = Math.Max(currentLineSize.Width, panelSize.Width);
                    panelSize.Height += currentLineSize.Height;
                    currentLineSize = desiredSize;

                    if (desiredSize.Width > availableSize.Width)
                    {
                        panelSize.Width = Math.Max(desiredSize.Width, panelSize.Width);
                        panelSize.Height += desiredSize.Height;
                        currentLineSize = new Size();
                    }
                }
                else
                {
                    currentLineSize.Width += desiredSize.Width;
                    currentLineSize.Height = Math.Max(desiredSize.Height, currentLineSize.Height);
                }
            }

            panelSize.Width = Math.Max(currentLineSize.Width, panelSize.Width);
            panelSize.Height += currentLineSize.Height;

            return panelSize;
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            int firstInLine = 0;
            int lineCount = 0;

            Size currentLineSize = new Size();

            double accumulatedHeight = 0;

            UIElementCollection elements = base.InternalChildren;
            double interval = 0.0;
            for (int i = 0; i < elements.Count; i++)
            {

                Size desiredSize = elements[i].DesiredSize;

                if (currentLineSize.Width + desiredSize.Width > finalSize.Width) //need to switch to another line
                {
                    interval = (finalSize.Width - currentLineSize.Width) / (i - firstInLine + 2);
                    arrangeLine(accumulatedHeight, currentLineSize.Height, firstInLine, i, interval);

                    accumulatedHeight += currentLineSize.Height;
                    currentLineSize = desiredSize;

                    if (desiredSize.Width > finalSize.Width) //the element is wider then the constraint - give it a separate line
                    {
                        arrangeLine(accumulatedHeight, desiredSize.Height, i, ++i, 0);
                        accumulatedHeight += desiredSize.Height;
                        currentLineSize = new Size();
                    }
                    firstInLine = i;
                    lineCount++;
                }
                else //continue to accumulate a line
                {
                    currentLineSize.Width += desiredSize.Width;
                    currentLineSize.Height = Math.Max(desiredSize.Height, currentLineSize.Height);
                }
            }

            if (firstInLine < elements.Count)
            {
                if (lineCount == 0)
                {
                    interval = (finalSize.Width - currentLineSize.Width) / (elements.Count - firstInLine + 1);
                }
                arrangeLine(accumulatedHeight, currentLineSize.Height, firstInLine, elements.Count, interval);
            }


            return finalSize;
        }

        private void arrangeLine(double y, double lineHeight, int start, int end, double interval)
        {
            double x = 0;
            UIElementCollection children = InternalChildren;
            for (int i = start; i < end; i++)
            {
                x += interval;
                UIElement child = children[i];
                child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineHeight));
                x += child.DesiredSize.Width;
            }
        }
    }
}