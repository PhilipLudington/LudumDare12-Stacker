using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Transforms
{
    public partial class Page : UserControl
    {
        static double y = 0;
        public Page()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Canvas.SetTop(Blue, ++y);
            Canvas.SetTop(Red, Canvas.GetTop(Red) + 1);

            TextBlockY.Text = Convert.ToString(y);
        }
    }
}
