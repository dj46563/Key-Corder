using System;
using System.Collections.Generic;
using System.IO;
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
using OfficeOpenXml;
using Microsoft.Win32;

namespace Keycorder_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Registrar _registrar;
        // keeps track of if there were any unsaved changes
        private bool _changed = false;

        private readonly DispatcherTimer _stopwatchDispatcherTimer = new DispatcherTimer();

        // Used to flash the background of the window
        private int _flashCount = 0;
        private readonly DispatcherTimer _flashDispatcherTimer = new DispatcherTimer();

        // holds a list of all the keys that are being held down, keys are removed once they do keyup
        private readonly List<Key> _keysDown = new List<Key>();


        // TEMP BAD CODE
        private List<Tuple<Key, string>> _behaviorList = new List<Tuple<Key, string>>();

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

            // Load the config file
            using (ExcelPackage xlPackage = new ExcelPackage(new FileInfo(@"C:\Work\Key-Corder\Keycorder GUI\Keycorder GUI\Config.xlsx")))
            {
                var worksheet = xlPackage.Workbook.Worksheets.First();
                // Goto the special cell that tells me how many config entries there are
                int rows = int.Parse(worksheet.Cells[2, 4].Value.ToString());
                for (int i = 0; i < rows; i++)
                {
                    // For each config entry: add the key to the registrar's lists of keys it cares about
                    string keyType = worksheet.Cells[i + 2, 2].Value.ToString();
                    string keyValue = worksheet.Cells[i + 2, 1].Value.ToString();
                    string keyBehavior;
                    try
                    {
                        keyBehavior = worksheet.Cells[i + 2, 3].Value.ToString();
                    }
                    catch (Exception)
                    {
                        keyBehavior = "";
                    }

                    Key key = (Key)Enum.Parse(typeof(Key), keyValue);

                    // Add the key and the behavior to the behavior list to later be used when creating keys
                    _behaviorList.Add(new Tuple<Key, string>(key, keyBehavior));

                    if (keyType.Equals("Once"))
                    {
                        _registrar.PressKeys.Add(key);
                    }
                    else if (keyType.Equals("Duration"))
                    {
                        _registrar.DurKeys.Add(key);
                    }
                }
            }

            // Programatically create the KeyboardButtons
            // Uses non ideal parallel arrays to handle the key's display string and the key's enum
            string[] numRow = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=" };
            Key[] numRowKeys = new Key[] { Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0, Key.OemMinus, Key.OemPlus };
            string[] topRow = new[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "[", "]" };
            Key[] topRowKeys = new Key[] { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P, Key.OemOpenBrackets, Key.OemCloseBrackets };
            string[] midRow = new[] { "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'" };
            Key[] midRowKeys = new Key[] { Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L, Key.OemSemicolon, Key.OemQuotes };
            string[] bottomRow = new[] { "Z", "X", "C", "V", "B", "N", "M", ",", ".", "/" };
            Key[] botRowKeys = new Key[] { Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M, Key.OemComma, Key.OemPeriod, Key.OemQuestion };
            RegisterRow(numRow, numRowKeys, NumRowPanel);
            RegisterRow(topRow, topRowKeys, TopRowPanel);
            RegisterRow(midRow, midRowKeys, MidRowPanel);
            RegisterRow(bottomRow, botRowKeys, BotRowPanel);
        }

        // Helper function to register each of the three rows of buttons
        private void RegisterRow(string[] row, Key[] keys, StackPanel panel)
        {
            int index = 0;
            foreach (var keyString in row)
            {
                var newKeyControl = new KeyboardButton()
                {
                    KeyType = KeyboardButton.KeyTypeEnum.Other,
                    Key = keyString,
                    Margin = new Thickness(5),
                    // A terrible ways to set the key's enum using paralell arrays :(
                    KeyEnum = keys[index],

                    // set the behavior string by looking at the behavior list, find the pair with the same key enum
                    // and assign the second value (the behavior string) to the Behavior prop
                    Behavior = _behaviorList.Find(x => x.Item1 == keys[index]).Item2
                };

                // Set the correct KeyType for the control, just affects the color of the button
                if (_registrar.PressKeys.Contains(newKeyControl.KeyEnum))
                {
                    newKeyControl.KeyType = KeyboardButton.KeyTypeEnum.OneTime;
                }
                else if (_registrar.DurKeys.Contains(newKeyControl.KeyEnum))
                {
                    newKeyControl.KeyType = KeyboardButton.KeyTypeEnum.Duration;
                }

                panel.Children.Add(newKeyControl);
                index++;
            }
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

                // set unsaved changes flag
                _changed = true;
                // Whenever any key is pressed let the registrar know
                _registrar.RegisterEvent(e.Key);

                // Find the GUI key with this key and flash it
                try
                {
                    var keyboardButton = GetKeyboardButtons().First(x => x.KeyEnum == e.Key);
                    keyboardButton.FlashColor();
                    keyboardButton.InProgress = !keyboardButton.InProgress;
                }
                // catch an exception that is thrown when we don't have a key to represent the button that was pressed
                // it really isnt a big deal and there is nothing to handle in this scenerio
                catch (InvalidOperationException)
                { }

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
                    var button = GetKeyboardButtons().First(x => x.KeyEnum == inProgressEvent.Key);
                    button.ElapsedTime = _registrar.Elapsed.Subtract(inProgressEvent.Start).ToString(@"ss\:ff");
                }
            }
        }

        // Gets all the keyboard buttons in the keyboardgrid
        private IEnumerable<KeyboardButton> GetKeyboardButtons()
        {
            return KeyboardGrid.GetChildrenOfType<KeyboardButton>();
        }

        private void ClearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Wipe everything
        private void ClearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // For all the in progress keys: remove their elapsed time counters and put them not in progress
            foreach (var inProgressEvent in _registrar.InProgressDurEvents)
            {
                var button = GetKeyboardButtons().First(x => x.KeyEnum == inProgressEvent.Key);
                button.ElapsedTime = "";
                button.InProgress = false;
            }

            // Clear all the log lists in the registrar and reset the stopwatch
            _registrar.Clear();

            // Update the stopwatch to be 0
            Stopwatch.Text = _registrar.Elapsed.ToString(@"mm\:ss\:ff");

            // Show that we are paused
            PausedBlock.Visibility = Visibility.Visible;
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Prompt for save file location
        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Pause the stopwatch to save
            _registrar.Pause();
            PausedBlock.Visibility = Visibility.Visible;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "xlsx";
            if (saveFileDialog.ShowDialog() == true)
            {
                // clear unsaved changes flag
                _changed = false;
                _registrar.OutputToSheet(saveFileDialog.FileName);
            }
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_changed)
                SaveCommand_Executed(this, null);
        }
    }
}
