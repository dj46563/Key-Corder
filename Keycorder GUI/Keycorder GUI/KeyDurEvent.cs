using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Keycorder_GUI
{
    public class KeyDurEvent : KeyPressEvent
    {
        public TimeSpan End { get; set; }

        public TimeSpan Duration => End.Subtract(Start);

        public KeyDurEvent(Key key, TimeSpan start, TimeSpan end) : base(key, start)
        {
            End = end;
        }
    }
}
