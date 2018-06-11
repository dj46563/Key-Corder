using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Keycorder_GUI
{
    public class KeyPressEvent
    {
        public Key Key { get; set; }
        public TimeSpan Start { get; set; }
        public string Behavior { get; set; }

        public KeyPressEvent(Key key, TimeSpan start)
        {
            Key = key;
            Start = start;
        }
    }
}
