using System;
using System.Windows.Media;

namespace ChatClient
{
    public class MessageItem
    {
        public string Message { get; set; }
        public SolidColorBrush BackColor { get; set; }

        public MessageItem(string msg, SolidColorBrush color)
        {
            Message = msg;
            BackColor = color;
        }
    }
}
