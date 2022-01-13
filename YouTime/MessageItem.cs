using System;
using System.Windows.Media;

namespace ChatClient
{	
    public class MessageItem
    {
        public string Message { get; set; }
        public SolidColorBrush BackColor { get; set; }
        public string Sender { get; set; }
        public string DateTimeMessage { get; init; }
        
        public MessageItem(string sender, string msg, SolidColorBrush color, string messageTime)
        {
            Sender = sender;
            Message = msg;
            BackColor = color;
            DateTimeMessage = messageTime;
        }
    }
}
