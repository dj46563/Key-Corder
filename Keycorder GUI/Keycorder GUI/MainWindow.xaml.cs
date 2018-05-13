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
using System.Windows.Threading;

namespace Keycorder_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Registrar _registrar;

        private readonly DispatcherTimer _stopwatchDispatcherTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            _registrar = new Registrar("cfg");
            Stopwatch.DataContext = _registrar;

            // Have the dispatch timer call the update method on stopwatch every millisecond
            _stopwatchDispatcherTimer.Tick += new EventHandler(StopwatchDispatcherTimer_Tick);
            _stopwatchDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            _stopwatchDispatcherTimer.IsEnabled = true;
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            _registrar.RegisterEvent(e.Key);
        }

        private void StopwatchDispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_registrar.IsRunning)
                Stopwatch.Text = _registrar.Elapsed.ToString(@"hh\:mm\:ss\:ff");
        }
    }
}
