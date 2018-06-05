using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Keycorder_GUI
{
    /// <summary>
    /// Interaction logic for RecentAction.xaml
    /// </summary>
    public partial class RecentAction : UserControl
    {
        public string Key { get; set; }
        public string Behavior { get; set; }

        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                MainGrid.Background = value.Equals("") ? Brushes.Coral : Brushes.CornflowerBlue;
            }
        }

        public DateTime Time { get; set; }
        private string _timeDisplay;
        public string TimeDisplay
        {
            get { return Time.ToString("mm:ss:ff"); }
            private set => _timeDisplay = value;
        }


        public RecentAction()
        {
            InitializeComponent();

            DataContext = this;

            Time = DateTime.Now;
            Key = "A";
        }
    }
}
