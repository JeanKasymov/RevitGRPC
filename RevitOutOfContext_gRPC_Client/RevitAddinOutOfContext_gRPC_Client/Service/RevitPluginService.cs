using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace RevitAddinOutOfContext_gRPC_Client
{
    /// <summary>
    /// Класс для рефлексивной работы Revit с плагинами
    /// </summary>
    public static class RevitPluginService
    {
        public static void AddDllPath(string commandName)
        {
            commands.Clear();
            commands.Enqueue(commandName);
        }
        public static string GetDllPath()
        {
            var command = "";
            if (commands.Count > 0) { command = commands.Dequeue(); }
            return command;
        }
        public static string GetDllPathName()
        {
            var dllPathName = "";
            if (commands.Count > 0) { dllPathName = commands.Peek(); }
            return dllPathName;
        }
        internal static string DoCommand(UIApplication uiapp)
        {
            var resTask = "";
            if (commands.Count > 0 && uiapp != null)
            {
                var dllPath = GetDllPath();
                if (!string.IsNullOrEmpty(dllPath))
                {
                    resTask = ExecuteMethodFromDll(dllPath, "MainClass", "Main", new object[] { uiapp });
                }
            }
            return resTask;
        }
        private static string ExecuteMethodFromDll(string fullDllPath, string executeClassName, string executeMethodName, object[] methodParameters)
        {
            var resTask = "";
            try
            {
                Type ti = GetType(fullDllPath, executeClassName);
                MethodInfo Execute = ti.GetMethod(executeMethodName);
                resTask = Execute.Invoke(null, methodParameters) as string;
            }
            catch(TargetInvocationException targetInvokEx)
            {
                var errorMessage = targetInvokEx.InnerException.Message;
                MessageBox.Show(errorMessage, "Ошибка внутри плагина", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch(ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                MessageBox.Show(errorMessage, "Сообщение об ошибке", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return resTask;
        }
        private static Type GetType(string fullDllPath, string executeClassName)
        {
            byte[] rawAssembly = File.ReadAllBytes(fullDllPath);
            Assembly assembly = Assembly.Load(rawAssembly);
            var types = assembly?.GetTypes();
            Type ti = assembly?.GetTypes().FirstOrDefault(type => type.Name == executeClassName);
            return ti;
        }
        private static Queue<string> commands { get; set; } = new Queue<string>();
    }
}
