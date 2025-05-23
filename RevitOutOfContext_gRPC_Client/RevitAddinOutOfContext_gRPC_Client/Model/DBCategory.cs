using RevitAddinOutOfContext_gRPC_Client.Service;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitAddinOutOfContext_gRPC_Client.Model
{
    public class DBCategory : ViewModelBase, ICloneable
    {
        public string Name { get; set; }
        private ObservableCollection<DBFamily> dBFamilies;
        public ObservableCollection<DBFamily> DBFamilies
        {
            get { return dBFamilies; }
            set { dBFamilies = value; OnPropertyChanged(); }
        }
        private DBFamily selectedDBFamily;
        public DBFamily SelectedDBFamily
        {
            get { return selectedDBFamily; }
            set { selectedDBFamily = value; OnPropertyChanged(); }
        }
        private bool isCategoryExpanded;
        public bool IsCategoryExpanded
        {
            get { return isCategoryExpanded; }
            set { isCategoryExpanded = value; OnPropertyChanged(); }
        }
        public DBCategory(string name)
        {
            Name = name;
            DBFamilies = new ObservableCollection<DBFamily>();
        }
        public DBCategory(string name, ObservableCollection<DBFamily> dBFamilies)
        {
            Name = name;
            DBFamilies = dBFamilies;
        }
        public object Clone()=>new DBCategory(Name, new ObservableCollection<DBFamily>(dBFamilies.Select(dbFam => (DBFamily)dbFam.Clone()).ToList()));
    }
}
