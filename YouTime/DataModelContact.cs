using System.Windows.Media;

namespace ChatClient
{
    public class DataModelContact
    {
        public int ContactId { get; set; }
        public string Nickname { get; set; }
        public string RealName { get; set; }
        public SolidColorBrush BackColor { get; set; }
    }
}
