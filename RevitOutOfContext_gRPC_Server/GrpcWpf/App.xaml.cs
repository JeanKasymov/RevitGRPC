using Grpc.Net.Client;
using RevitOutOfContext_gRPC_ProtosF;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace GrpcWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Process? GrpcServerConsoleProcess { get; set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                //var path = @"K:\AutodeskAddins\Revit\Plugins\Новая папка\net7.0\GrpcServerConsole.exe";
                var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Addins\GrpcServer\GrpcServerConsole.exe";
                var processInfo = new ProcessStartInfo(path);
                processInfo.WorkingDirectory = Path.GetDirectoryName(path);
                processInfo.FileName = path;
                GrpcServerConsoleProcess = Process.Start(processInfo);

            }
            catch (Exception ex)
            {
                var strBuilder = new StringBuilder();
                strBuilder.AppendLine("Файл: " + Directory.GetCurrentDirectory());
                strBuilder.AppendLine(ex.Message);
                MessageBox.Show(strBuilder.ToString(), "Сообщение об ошибке",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            GrpcServerConsoleProcess?.Kill();
        }        
    }
}