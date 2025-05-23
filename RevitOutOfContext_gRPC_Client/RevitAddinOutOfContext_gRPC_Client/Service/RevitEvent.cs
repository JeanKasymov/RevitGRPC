using Autodesk.Revit.UI;
using System;

namespace RevitAddinOutOfContext_gRPC_Client
{
    /// <summary>
    /// Класс для работы Revit с внешним событием 
    /// </summary>
    public class RevitEvent : IExternalEventHandler
    {
        private Action<UIApplication> _action;
        private readonly ExternalEvent _externalEvent;
        public RevitEvent()
        {
            _externalEvent = ExternalEvent.Create(this);
        }
        public void Run(Action<UIApplication> action)
        {
            _action = action;
            _externalEvent.Raise();
        }
        public void Execute(UIApplication app)
        {
            try
            {
                _action?.Invoke(app);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error IExternalEventHandler", ex.Message);
            }
        }
        public string GetName()
        {
            return nameof(RevitEvent);
        }
    }
}
