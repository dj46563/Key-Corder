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

        // Used to flash the background of the window
        private int _flashCount = 0;
        private readonly DispatcherTimer _flashDispatcherTimer = new DispatcherTimer();

        // holds a list of all the keys that are being held down, keys are removed once they do keyup
        private readonly List<Key> _keysDown = new List<Key>();

        public MainWindow()
        {
            InitializeComponent();

            _registrar = new Registrar("cfg");
            Stopwatch.DataContext = _registrar;
            Stopwatch.Text = _registrar.Elapsed.ToString(@"mm\:ss\:ff");

            // Have the dispatch timer call the update method on stopwatch every millisecond
            _stopwatchDispatcherTimer.Tick += new EventHandler(StopwatchDispatcherTimer_Tick);
            _stopwatchDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            _stopwatchDispatcherTimer.Start();

            _flashDispatcherTimer.Tick += new EventHandler(FlashDispatcherTimer_Tick);
            _flashDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }

        // Toggle the screen flash 4 times between white and red before stopping itself
        private void FlashDispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_flashCount++ < 4)
            {
                MainGrid.Background = Equals(MainGrid.Background, Brushes.White) ? Brushes.Red : Brushes.White;
            }
            else
            {
                MainGrid.Background = Brushes.White;
                _flashCount = 0;
                _flashDispatcherTimer.Stop();
            }
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            // Only register keys if the stopwatch it running or if it is a stopwatch start/stop button
            if (e.Key == _registrar.PauseKey || (_registrar.IsRunning && !_keysDown.Contains(e.Key)))
            {
                _keysDown.Add(e.Key);

                // Whenever any key is pressed let the registrar know
                _registrar.RegisterEvent(e.Key);

                // Find the GUI keys with this key and flash them
                foreach (var keyboardButton in KeyboardGrid.Children.Cast<KeyboardButton>().Where(x => x.KeyEnum == e.Key))
                {
                    keyboardButton.FlashColor();
                    keyboardButton.InProgress = !keyboardButton.InProgress;
                }

                // Set the visibility of the "PAUSED" textblock backed on if the stopwatch is running or not
                PausedBlock.Visibility = !_registrar.IsRunning ? Visibility.Visible : Visibility.Hidden;
            }
            // Flash some kind of error to show that you are trying to press keys while paused
            else
            {
                _flashDispatcherTimer.Start();
            }
        }

        // Once a key is released remove it from the list so that it can be pressed again
        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            _keysDown.Remove(e.Key);
        }

        // is run every millisecond
        private void StopwatchDispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_registrar.IsRunning)
            {
                // Sets the stopwatch text
                Stopwatch.Text = _registrar.Elapsed.ToString(@"mm\:ss\:ff");

                // For all the in progress keys: update the little elapsed times they have
                foreach (var inProgressEvent in _registrar.InProgressDurEvents)
                {
                    KeyboardButton button = KeyboardGrid.Children.Cast<KeyboardButton>()
                        .First(x => x.KeyEnum == inProgressEvent.Key);
                    button.ElapsedTime = _registrar.Elapsed.Subtract(inProgressEvent.Start).ToString(@"ss\:ff");
                }
            }
        }
    }
}
