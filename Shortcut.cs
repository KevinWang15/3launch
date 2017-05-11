using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace _3launch
{
    public class Shortcut
    {
        // must be upper case, 2 characters.  e.g. PS
        public String name { get; set; }

        public String fileName { get; set; }
        public String filePath { get; set; }

        public ImageSource icon { get; set; }
    }
}