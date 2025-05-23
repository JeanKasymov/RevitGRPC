using Autodesk.Revit.DB;
using RevitAddinOutOfContext_gRPC_Client.Service;
using System;
namespace RevitAddinOutOfContext_gRPC_Client.Model
{
    public class DBFamily : ViewModelBase, ICloneable
    {
        public int Id { get; set; }
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(); }
        }
        private string fullName;
        public string FullName
        {
            get { return fullName; }
            set { fullName = value; OnPropertyChanged(); }
        }
        private Family family;
        public Family Family
        {
            get {  return family; }
            set { family = value; OnPropertyChanged(); }
        }
        private bool isLoaded;
        public bool IsLoaded
        {
            get
            {
                return isLoaded;
            }
            set
            {
                if (value)
                {
                    ButtonContent = "О";
                    ButtonToolTip = "Обновить семейство в модели";
                }
                else
                {
                    ButtonContent = "З";
                    ButtonToolTip = "Загрузить семейство в модель";
                }
                isLoaded = value;
                OnPropertyChanged();
            }
        }
        private string buttonContent;
        public string ButtonContent
        {
            get { return buttonContent; }
            set { buttonContent = value; OnPropertyChanged(); }
        }
        private string buttonToolTip;
        public string ButtonToolTip
        {
            get { return buttonToolTip; }
            set { buttonToolTip = value; OnPropertyChanged(); }
        }
        private bool isFamilyChecked;
        public bool IsFamilyChecked
        {
            get { return isFamilyChecked; }
            set { isFamilyChecked = value; OnPropertyChanged(); }
        }
        public DBFamily(int id, string fullName, string name, bool isLoaded)
        {
            Id = id;
            Name = name;
            FullName = fullName;
            IsLoaded = isLoaded;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}