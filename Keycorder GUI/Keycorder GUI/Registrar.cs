using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using OfficeOpenXml;

namespace Keycorder_GUI
{
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
        public readonly List<Key> PressKeys = new List<Key>();
        public readonly List<Key> DurKeys = new List<Key>();
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
            else if (PressKeys.Contains(key))
            {
                KeyDurEvents.Add(new KeyDurEvent(key, now, TimeSpan.MinValue));
                //KeyPressEvents.Add(new KeyPressEvent(key, now));
            }
            // A duration key is pressed, check if we already have that key in progress
            else if (DurKeys.Contains(key))
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
            using (ExcelPackage package = new ExcelPackage())
            {
                // Create workbook and headers
                var ws = package.Workbook.Worksheets.Add("Key Log");
                ws.Cells["A1"].Value = "Key";
                ws.Cells["B1"].Value = "Start";
                ws.Cells["C1"].Value = "End";
                ws.Cells["D1"].Value = "Duration";

                int row = 2;
                foreach (var keyEvent in KeyDurEvents)
                {
                    ws.Cells[row, 1].Value = keyEvent.Key;
                    ws.Cells[row, 2].Value = keyEvent.Start.ToString(@"mm\:ss\:ff");

                    if (!keyEvent.End.Equals(TimeSpan.MinValue)) // dur event
                    {
                        ws.Cells[row, 3].Value = keyEvent.End.ToString(@"mm\:ss\:ff");
                        ws.Cells[row, 4].Value = keyEvent.Duration.ToString(@"mm\:ss\:ff");
                    }
                    row++;
                }

                package.SaveAs(new FileInfo(filename));
            }
        }
    }
}
