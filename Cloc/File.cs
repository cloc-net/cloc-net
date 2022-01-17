using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Cloc
{
    public class File
    {
        public String Language { get; set; }
        public String Path { get; set; }
        public Int32 Blank { get; set; }
        public Int32 Code { get; set; }
        public Int32 Comment { get; set; }

        public File (String language, String path, Int32 blank, Int32 code, Int32 comment)
        {
            Language = language;
            Path = path;
            Blank = blank;
            Code = code;
            Comment = comment;
        }
    }
}
