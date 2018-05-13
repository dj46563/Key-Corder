using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Keycorder_GUI
{
    public static class Helper
    {
        public static IEnumerable<T> GetChildrenOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in GetChildrenOfType<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
