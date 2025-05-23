using System.Windows.Media.Imaging;

namespace GrpcWpf.Models
{
    public class PluginModel(string name, BitmapSource image, string mainClassName, string mainMethodName, string fullAssemblyPath)
    {
        public string Name => name;
        public BitmapSource Image => image;
        public string MainClassName => mainClassName;
        public string MainMethodName => mainMethodName;
        public string FullAssemblyPath => fullAssemblyPath;
    }
}