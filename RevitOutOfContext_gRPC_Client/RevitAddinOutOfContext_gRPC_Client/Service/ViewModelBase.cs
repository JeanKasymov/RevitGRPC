using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RevitAddinOutOfContext_gRPC_Client.Service
{
    /// <summary>
    /// Базовый VM класс
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
