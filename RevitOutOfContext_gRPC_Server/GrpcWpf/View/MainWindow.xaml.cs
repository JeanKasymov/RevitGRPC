using AduSkin.Controls.Metro;
using GrpcWpf.VM;
namespace GrpcWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel();        
        }        
    }
}