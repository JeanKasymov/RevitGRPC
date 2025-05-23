using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using RevitAddinOutOfContext_gRPC_Client.Service;
using RevitAddinOutOfContext_gRPC_Client.View;
using RevitAddinOutOfContext_gRPC_Client.VM;
using RevitOutOfContext_gRPC_ProtosF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace RevitAddinOutOfContext_gRPC_Client
{
    [Transaction(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class RevitGrpcAddinApp : IExternalApplication
    {
        public static FamilyDbControl familyDbControl;
        public static UIControlledApplication _uiControlApplication;
        public string _userName;
        public string _versionName;
        string _versionNum;
        static bool _canRunRequest = true;
        Health.HealthClient _clientHealth;
        public static Greeter.GreeterClient _client;
        public static Queue<DBCommand> ActionQueue { get; set; } = new Queue<DBCommand>();
        FamiliDbVM familyDbVm = new FamiliDbVM(ActionQueue);
        List<ElementId> _added_element_ids = new List<ElementId>();
        public Result OnStartup(UIControlledApplication uiControlApplication)
        {
            try
            {

                _uiControlApplication = uiControlApplication;
                _uiControlApplication.ControlledApplication.ApplicationInitialized += OnInitialized;
                _uiControlApplication.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened;
                _userName = Environment.UserName;
                _versionName = _uiControlApplication.ControlledApplication.VersionName;
                _versionNum = _uiControlApplication.ControlledApplication.VersionNumber;

                //uiControlApplication.ViewActivated += OnViewActivated;
                familyDbControl = new FamilyDbControl(familyDbVm);
                familyDbControl.DataContext = familyDbVm;
                _uiControlApplication.RegisterDockablePane(
                        FamilyDbControl.PaneId,
                        FamilyDbControl.PaneName,
                        familyDbControl);
                if (!DockablePane.PaneIsRegistered(FamilyDbControl.PaneId))
                {
                    _uiControlApplication.RegisterDockablePane(
                        FamilyDbControl.PaneId,
                        FamilyDbControl.PaneName,
                        familyDbControl);
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("fail", ex.Message);
                return Result.Cancelled;
            }
        }
        private void ControlledApplication_DocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            if (familyDbVm != null)
            {
                familyDbVm.Document = e.Document;
            }
        }
        public async void OnInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            try
            {
                UIApplication uiapp = sender as UIApplication;
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (messege, cert, chain, sslPolicyErrors) => { return true; };
                var handler = new GrpcWebHandler(httpClientHandler);
                var options = new GrpcChannelOptions
                {
                    HttpHandler = handler,
                };
                options.UnsafeUseInsecureChannelCallCredentials = true;
                var channel = GrpcChannel.ForAddress("http://localhost:5064", options);
                //var channel = new NamedPipeChannel(".", "MY_PIPE_NAME");a
                _clientHealth = new Health.HealthClient(channel);
                _client = new Greeter.GreeterClient(channel);
                RequestToServer("DllPathRequest");
                _uiControlApplication.Idling += OnIdling;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.Message);
            }
        }
        public static void RequestToServer(string name, string procesId = "", string text = "")
        {
            try
            {
                CommandReply commandReply = new CommandReply();
                new Thread(async () =>
                {
                    _canRunRequest = false;
                    System.Threading.Thread.Sleep(1000);

                    var procsId = Process.GetCurrentProcess().Id;
                    var dllPathRequest = new HelloRequest
                    {
                        Name = name,
                        ProcesId = procesId,
                        Text = text
                    };
                    try
                    {
                        commandReply = await _client.SayHelloAsync(dllPathRequest);
                    }
                    catch (Grpc.Core.RpcException)
                    {
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Exception", ex.Message);
                    }
                    if (!string.IsNullOrEmpty(commandReply.Command))
                    {                        
                        RevitPluginService.AddDllPath(commandReply.Command);
                    }
                    _canRunRequest = true;
                }).Start();
            }
            catch (Exception ex)
            {
                _canRunRequest = true;
                TaskDialog.Show("Exception", ex.Message);
            }
        }
        public Result OnShutdown(UIControlledApplication uiControlApplication)
        {
            uiControlApplication.Idling -= OnIdling;
            return Result.Succeeded;
        }
        private void OnIdling(object sender, IdlingEventArgs e)
        {
            try
            {
                UIApplication uiapp = sender as UIApplication;
                if (_canRunRequest)
                {
                    RequestToServer("DllPathRequest");
                    var dllPath = RevitPluginService.GetDllPathName(); 
                    if (dllPath != "")
                    {
                        new RevitEvent().Run(app =>
                        {
                            var res = RevitPluginService.DoCommand(uiapp);
                            if (dllPath.Contains("ENS.dll"))
                            {
                                RequestToServer("DllResult", dllPath, "Done");
                            }
                        });
                    }
                    if (ActionQueue.Count > 0)
                    {
                        ActionQueue.Dequeue().DoDBCommand(uiapp, familyDbVm);
                        
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.Message);
            }
        }
    }
}