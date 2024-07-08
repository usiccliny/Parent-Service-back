namespace Server.Models
{
    public class DataChangedEventArgs : EventArgs
    {
        public DataChangedEventArgs(string data)
        {
            Data = data;
        }

        public string Data
        {
            get;
        }
    }
}
