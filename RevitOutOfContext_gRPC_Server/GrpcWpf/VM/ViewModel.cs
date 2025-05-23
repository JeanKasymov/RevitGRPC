using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Net.Client;
using GrpcWpf.Models;
using Newtonsoft.Json;
using RevitOutOfContext_gRPC_ProtosF;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
namespace GrpcWpf.VM
{
    public partial class ViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<PluginModel> _plugins = new ObservableCollection<PluginModel>();
        [RelayCommand]
        private async Task SendPluginToDo(string? Name)
        { 
            try
            {
                RecordPluginOpeningDB(Name);
                var plugin = Plugins.First(plugin => plugin.Name == Name);
                var options = new GrpcChannelOptions
                {
                    HttpHandler = new HttpClientHandler()
                };
                //options.UnsafeUseInsecureChannelCallCredentials = true;
                using var channel = GrpcChannel.ForAddress("http://localhost:5002"/*, options*/);
                var client = new Greeter.GreeterClient(channel);

                var req = new HelloRequest();
                req.Name = "Command";
                req.Text = plugin.FullAssemblyPath;
                if (plugin.FullAssemblyPath.Contains("ENS.dll"))
                {
                    req.Name = "CommandWithResult";
                }
                var reply = client.SayHello(req);
                if (plugin.FullAssemblyPath.Contains("ENS.dll"))
                {
                    var ensFilePath = @"C:\ProgramData\Addins\Код из 1С\ensNames.txt";
                    if (Path.Exists(ensFilePath))
                    {
                        await GetItemId(ensFilePath, plugin.FullAssemblyPath);
                    }
                }
            }
            catch (Exception ex)  
            {
                var strBuilder = new StringBuilder();
                strBuilder.AppendLine("Файл: " + Directory.GetCurrentDirectory());
                strBuilder.AppendLine(ex.Message);
                MessageBox.Show(strBuilder.ToString(), "Сообщение об ошибке", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void RecordPluginOpeningDB(string? name)
        {
            using (Services.ApplicationContext db = new Services.ApplicationContext())
            {
                if (db.plugins.Select(plugin => plugin.Name).Contains(name))
                {
                    var plugin = db.plugins.First(plugin => plugin.Name == name);
                    plugin.OpeningsCount++;
                }
                else
                {
                    var plugin = new Plugin(name);
                    db.plugins.Add(plugin);
                }
                db.SaveChanges();
            }
        }
        [RelayCommand]
        private void GetPluginOpeningDbInfo()
        {
            using (Services.ApplicationContext db = new Services.ApplicationContext())
            {
                // получаем объекты из бд и выводим на консоль
                var plugins = db.plugins.ToList();
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Familys list:");
                foreach (var plugin in plugins)
                {
                    stringBuilder.AppendLine($"{plugin.Name} - {plugin.OpeningsCount}");
                }
                MessageBox.Show(stringBuilder.ToString());
            }
        }
        [RelayCommand]
        private void GetDbInfo()
        {
            using (Services.ApplicationContext db = new Services.ApplicationContext())
            {
                // получаем объекты из бд и выводим на консольa
                var families = db.families.ToList();
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Familys list:");

                foreach (Family family in families)
                {
                    stringBuilder.AppendLine($"{family.Id}.{family.Name} - {family.Category}");
                }
                MessageBox.Show(stringBuilder.ToString());
            }
        }
        public ViewModel()
        {
            Plugins = RefreshPlugins(@"K:\AutodeskAddins\Revit\Plugins", Plugins);
        }
        private static async Task GetItemId(string ensFilePath, string fullAssemblyPath)
        {
            try
            {
                var requestData = new Data<ItemRequest>() { Items = new List<ItemRequest>() };
                var strs = new StringBuilder();
                var Items = new List<ItemRequest>();
                using (StreamReader reader = new StreamReader(ensFilePath))
                {
                    string? line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line == "")
                        {
                            break;
                        }
                        requestData.Items.Add(new ItemRequest(line));
                        strs.Append(line);
                    }
                }
                var username = "rteApi";
                var password = "WcmevTo9$34nx.5u";
                var url = "http://sw003/rte_bsh_prod/hs/rteAPI/V1/getItemNumber";
                var itemsJson = JsonConvert.SerializeObject(requestData);
                File.WriteAllText(@"C:\ProgramData\Addins\Код из 1С\ensNames.json", itemsJson);
                var content = new StringContent(itemsJson, Encoding.UTF8, "application/json");
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                    using var response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var resultResponse = JsonConvert.DeserializeObject<Data<ItemResponse>>(result);
                    if (resultResponse != null)
                    {
                        if (resultResponse.Items.Count > 0)
                        {
                            var dirPath = @"C:\ProgramData\Addins\Код из 1С\";
                            var dir = Directory.CreateDirectory(dirPath);
                            var path = Path.Combine(dirPath, $"ensNameNumbers.txt");
                            using (StreamWriter sw = new StreamWriter(path, false))
                            {
                                var resStr = new StringBuilder();
                                foreach (var item in resultResponse.Items)
                                {
                                    resStr.AppendLine(item.ToString());
                                }
                                sw.Write(resStr.ToString());
                                MessageBox.Show($"{resStr}");
                            }
                            using var channel = GrpcChannel.ForAddress("http://localhost:5002"/*, options*/);
                            var grpcClient = new Greeter.GreeterClient(channel);
                            fullAssemblyPath = fullAssemblyPath.Replace("ENS.dll", "Set1CNumbers.dll");
                            var reply = grpcClient.SayHello(new HelloRequest { Name = "Command", Text = fullAssemblyPath });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }
        /// <summary>
        /// Обновляет список плагинов
        /// </summary>
        /// <param name="selectedPluginsPath">Выбранный новый путь к плагинам</param>
        /// <param name="plugins">Список плагинов для обновления</param>
        /// <returns></returns>
        internal static ObservableCollection<PluginModel> RefreshPlugins(string selectedPluginsPath, ObservableCollection<PluginModel> plugins)
        {
            try
            {
                plugins.Clear();
                var directory = new DirectoryInfo(selectedPluginsPath);
                if (directory.Exists)
                {
                    var directories = directory.GetDirectories();
                    foreach (var dir in directories)
                    {
                        var pluginDllName = dir.Name;
                        var mainMethodName = "Main";
                        var fileInfos = dir.GetFiles();
                        var dllFiles = fileInfos
                            .Where(file => file.Name.EndsWith($"{pluginDllName}.dll"))
                            .ToList();
                        foreach (var dllFile in dllFiles)
                        {
                            var dllAssembly = Assembly.Load(File.ReadAllBytes(dllFile.FullName));
                            BitmapSource image = null;
                            var mainClass = dllAssembly.GetType(pluginDllName + ".MainClass");
                            var mainClassName = mainClass?.Name;
                            var instance = Activator.CreateInstance(mainClass);
                            var name = mainClass.GetProperty("RussianName")?.GetValue(instance)?.ToString();
                            var pluginName = dllAssembly.GetName().Name;
                            try
                            {
                                var imageManifestResourseName = dllAssembly.GetManifestResourceNames().First(name => name.EndsWith(pluginName + ".png"));
                                var stream = dllAssembly.GetManifestResourceStream(imageManifestResourseName);
                                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                                image = decoder.Frames[0];
                            }
                            catch { }
                            var fullAssPath = dllFile.FullName;
                            var plugin = new PluginModel(name,image, mainClassName, mainMethodName, fullAssPath);
                            plugins.Add(plugin);
                        }
                    }
                }
                return plugins;
            }
            catch (Exception ex)
            {
                var strBuilder = new StringBuilder();
                strBuilder.AppendLine("Файл: " + Directory.GetCurrentDirectory());
                strBuilder.AppendLine(ex.Message);
                MessageBox.Show(strBuilder.ToString(), "Сообщение об ошибке", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return plugins;
            }
            
        }
    }
}