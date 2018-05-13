using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Keycorder_GUI
{
    public class Registrar
    {
        public List<KeyPressEvent> KeyPressEvents { get; set; }
        public List<KeyDurEvent> KeyDurEvents { get; set; }
        public List<KeyDurEvent> InProgressDurEvents { get; set; }

        public TimeSpan Elapsed => _stopwatch.Elapsed;
        public bool IsRunning => _stopwatch.IsRunning;

        private readonly List<Key> _pressKeys;
        private readonly List<Key> _durKeys;
        private readonly Key _pauseKey;

        private readonly Stopwatch _stopwatch; 

        public Registrar(string configFile)
        {
            KeyPressEvents = new List<KeyPressEvent>();
            KeyDurEvents = new List<KeyDurEvent>();
            InProgressDurEvents = new List<KeyDurEvent>();

            _stopwatch = new Stopwatch();

            // Configure which keys are for what
            _pressKeys = new List<Key>() { Key.F, Key.G };
            _durKeys = new List<Key>() { Key.V, Key.B };
            _pauseKey = Key.Space;
        }

        public void RegisterEvent(Key key)
        {
            TimeSpan now = _stopwatch.Elapsed;

            if (key == _pauseKey)
            {
                if (_stopwatch.IsRunning) { _stopwatch.Stop(); }
                else { _stopwatch.Start(); }
            }
            else if (_pressKeys.Contains(key))
            {
                KeyPressEvents.Add(new KeyPressEvent(key, now));
            }
            else if (_durKeys.Contains(key))
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
    }
}
