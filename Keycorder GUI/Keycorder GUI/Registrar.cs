using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using OfficeOpenXml;

namespace Keycorder_GUI
{
    // struct that contains a key and its assocaited behavior
    public struct KeyBehavior
    {
        public Key key;
        public string behavior;
    }

    public class Registrar
    {
        // The list of key press events that we have logged in memory
        public List<KeyPressEvent> KeyPressEvents { get; set; }
        public List<KeyDurEvent> KeyDurEvents { get; set; }
        public List<KeyDurEvent> InProgressDurEvents { get; set; }

        // Properties to get stopwatch info 
        public TimeSpan Elapsed => _stopwatch.Elapsed;
        public bool IsRunning => _stopwatch.IsRunning;

        // List of keys that we are looking for, these are configured in ctor
        public readonly List<KeyBehavior> PressKeys = new List<KeyBehavior>();
        public readonly List<KeyBehavior> DurKeys = new List<KeyBehavior>();
        public readonly Key PauseKey;

        private readonly Stopwatch _stopwatch; 

        public Registrar(string configFile)
        {
            KeyPressEvents = new List<KeyPressEvent>();
            KeyDurEvents = new List<KeyDurEvent>();
            InProgressDurEvents = new List<KeyDurEvent>();

            _stopwatch = new Stopwatch();

            // Configure which keys are for what
            PauseKey = Key.Space;
        }

        // returns the behavior string associated with a key
        public string GetBehaviorOfKey(Key key)
        {
            // first, check the KeyPress list
            KeyBehavior keyEvent = PressKeys.Find(x => x.key == key);
            if (keyEvent.behavior != null)
            {
                return keyEvent.behavior;
            }

            // then check the KeyDurEvents
            keyEvent = DurKeys.Find(x => x.key == key);
            if (keyEvent.behavior != null)
            {
                return keyEvent.behavior;
            }

            // keyEvent is null so throw an exception
            throw new ArgumentException("Key does not have a behavior");
        }

        // Check if the key is a key we care about, if so handle it
        public void RegisterEvent(Key key)
        {
            TimeSpan now = _stopwatch.Elapsed;

            // Start/Stop key was pressed
            if (key == PauseKey)
            {
                if (_stopwatch.IsRunning) { _stopwatch.Stop(); }
                else { _stopwatch.Start(); }
            }
            // A one off non duration key is pressed
            else if (PressKeys.Select(x => x.key).Contains(key))
            {
                KeyDurEvents.Add(new KeyDurEvent(key, now, TimeSpan.MinValue));
                //KeyPressEvents.Add(new KeyPressEvent(key, now));
            }
            // A duration key is pressed, check if we already have that key in progress
            else if (DurKeys.Select(x => x.key).Contains(key))
            {
                var durEvent = InProgressDurEvents.Find(x => x.Key == key);
                if (durEvent != default(KeyDurEvent)) // The pressed key was already in progress
                {
                    KeyDurEvents.Add(new KeyDurEvent(key, durEvent.Start, now));
                    InProgressDurEvents.Remove(durEvent);
                }
                else
                {
                    InProgressDurEvents.Add(new KeyDurEvent(key, now, TimeSpan.MinValue));
                }
            }
        }

        public void Clear()
        {
            KeyPressEvents.Clear();
            KeyDurEvents.Clear();
            InProgressDurEvents.Clear();
            _stopwatch.Reset();
            _stopwatch.Stop();
        }

        public void Pause()
        {
            _stopwatch.Stop();
        }

        public void OutputToSheet(string filename)
        {
            // Creating the summary lists
            var pressStats = Enumerable.Repeat(new { Key = Key.A, Count = 0 }, 0).ToList();
            var durStats = Enumerable.Repeat(new { Key = Key.A, Time = TimeSpan.Zero }, 0).ToList();

            foreach (KeyBehavior key in PressKeys)
            {
                pressStats.Add(new { Key = key.key, Count = 0 });
            }
            foreach (KeyBehavior key in DurKeys)
            {
                durStats.Add(new { Key = key.key, Time = TimeSpan.Zero });
            }

            using (ExcelPackage package = new ExcelPackage())
            {
                // Create workbook and headers
                var ws = package.Workbook.Worksheets.Add("Key Log");
                ws.Cells["A1"].Value = "Key";
                ws.Cells["B1"].Value = "Behavior";
                ws.Cells["C1"].Value = "Start";
                ws.Cells["D1"].Value = "End";
                ws.Cells["E1"].Value = "Duration";

                int row = 2;
                foreach (var keyEvent in KeyDurEvents)
                {
                    ws.Cells[row, 1].Value = keyEvent.Key;

                    // write the behavior in, if there is no associated behavior use empty string
                    try
                    {
                        ws.Cells[row, 2].Value = GetBehaviorOfKey(keyEvent.Key);
                    }
                    catch (ArgumentException e)
                    {
                        ws.Cells[row, 2].Value = "";
                    }
                    
                    ws.Cells[row, 3].Value = keyEvent.Start.ToString(@"mm\:ss\:ff");

                    if (!keyEvent.End.Equals(TimeSpan.MinValue)) // dur event
                    {
                        ws.Cells[row, 4].Value = keyEvent.End.ToString(@"mm\:ss\:ff");
                        ws.Cells[row, 5].Value = keyEvent.Duration.ToString(@"mm\:ss\:ff");

                        // add dur event to durStats
                        var stat = durStats.Find(x => x.Key == keyEvent.Key);
                        if (durStats.Select(x => x.Key).Contains(keyEvent.Key))
                        {
                            TimeSpan time = stat.Time;
                            durStats[durStats.FindIndex(x => x.Key == keyEvent.Key)] =
                                new {Key = keyEvent.Key, Time = time.Add(keyEvent.Duration)};
                        }
                        else
                        {
                            durStats.Add(new {Key = keyEvent.Key, Time = keyEvent.Duration });
                        }
                    }
                    else
                    {
                        // add press event to pressStats
                        var stat = pressStats.Find(x => x.Key == keyEvent.Key);
                        if (pressStats.Select(x => x.Key).Contains(keyEvent.Key))
                        {
                            int count = stat.Count;
                            pressStats[pressStats.FindIndex(x => x.Key == keyEvent.Key)] =
                                new {Key = keyEvent.Key, Count = count + 1};
                        }
                        else
                        {
                            pressStats.Add(new { Key = keyEvent.Key, Count = 1 });
                        }
                    }
                    row++;
                }

                // Output the summary info
                ws.Cells["G1"].Value = "Summary";

                // display press stats
                ws.Cells["G2"].Value = "Key";
                ws.Cells["H2"].Value = "Behavior";
                ws.Cells["I2"].Value = "Count";
                row = 3;
                
                foreach (var stat in pressStats)
                {
                    ws.Cells[row, 7].Value = stat.Key;
                    ws.Cells[row, 8].Value = GetBehaviorOfKey(stat.Key);
                    ws.Cells[row, 9].Value = stat.Count;
                    row++;
                }

                // display dur stats
                row += 2;
                ws.Cells[row, 7].Value = "Key";
                ws.Cells[row, 8].Value = "Behavior";
                ws.Cells[row, 9].Value = "Percentage";
                ws.Cells[row, 10].Value = "Duration (s)";
                row++;
                foreach (var stat in durStats)
                {
                    ws.Cells[row, 7].Value = stat.Key;
                    ws.Cells[row, 8].Value = GetBehaviorOfKey(stat.Key);
                    ws.Cells[row, 9].Value = Math.Round((stat.Time.TotalMilliseconds / _stopwatch.Elapsed.TotalMilliseconds * 100), 1).ToString(CultureInfo.InvariantCulture) + "%";
                    ws.Cells[row, 10].Value = Math.Round(stat.Time.TotalSeconds, 2);
                    row++;
                }

                // display total session time
                ws.Cells[1, 11].Value = "Session Time";
                ws.Cells[2, 11].Value = Math.Round(_stopwatch.Elapsed.TotalSeconds, 2);

                package.SaveAs(new FileInfo(filename));
            }
        }
    }
}
