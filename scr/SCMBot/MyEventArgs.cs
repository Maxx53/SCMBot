using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCMBot
{
    public class MyEventArgs : EventArgs
    {
        public string Message { get; internal set; }
        public int Code { get; internal set; }
        public int Value { get; internal set; }

        public MyEventArgs(int code, int value, string message)
        {
            this.Code = code;
            this.Value = value;
            this.Message = message;
        }
    }
}
