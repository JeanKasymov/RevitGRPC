using Autodesk.Revit.DB;
using RevitAddinOutOfContext_gRPC_Client.Model;
using RevitAddinOutOfContext_gRPC_Client.Service;
using RevitOutOfContext_gRPC_ProtosF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RevitAddinOutOfContext_gRPC_Client.VM
{
    public class FamiliDbVM : ViewModelBase
    {
        private string texxt;
        public string Texxt
        {
            get { return texxt; }
            set { texxt = value; OnPropertyChanged(); }
        }
        private string buttonContent;
        public string ButtonContent
        {
            get { return buttonContent; }
            set { buttonContent = value; OnPropertyChanged(); }
        }
        private string findFamilyName;
        public string FindFamilyName
        {
            get { return findFamilyName; }
            set { findFamilyName = value; OnPropertyChanged(); }
        }
        public ObservableCollection<DBCategory> DBCategories {  get; set; }
        public List<DBCategory> BackDBCategories { get; set; }
        public Queue<DBCommand> ActionQueue { get; set; }
        public Document Document { get; set; }
        public FamiliDbVM(Queue<DBCommand> actionQueue)
        {
            DBCategories = new ObservableCollection<DBCategory>();
            BackDBCategories = new List<DBCategory>();
            Texxt = "Example";
            //RefreshFamilies();
            ActionQueue = actionQueue;
        }
        public ICommand RefreshFamiliesCommand => new RelayCommandWithoutParameter(RefreshFamilies);
        public ICommand LoadFamilyCommand => new RelayCommand<string>(LoadFamily);
        public ICommand NewFamilyInstanceCommand => new RelayCommand<string>(NewFamilyInstance);

        private async void RefreshFamilies()
        {
            try
            {
                DBCategories.Clear();
                BackDBCategories.Clear();
                var famDirPath = @"K:\ОП СПб\Шаблоны\Шаблоны для работы в ПО\Семейства Revit";
                var famDirExist = Directory.Exists(famDirPath);
                if (famDirExist)
                {
                    var subDirs = Directory.GetDirectories(famDirPath);
                    var loadedFamilies = new FilteredElementCollector(Document).OfClass(typeof(Family)).Cast<Family>().ToList();
                    var call = RevitGrpcAddinApp._client.ServerDataStream();
                    
                    foreach (var subDir in subDirs)
                    {
                        var subDirInfo = new DirectoryInfo(subDir);
                        var subDirName = subDirInfo.Name.Replace("_", "__");
                        var dBCategory = new DBCategory(subDirName);
                        var backDBCategory = new DBCategory(subDirName);
                        if (DBCategories.Any(cat => cat.Name == dBCategory.Name))
                        {
                            dBCategory = DBCategories.First(cat => cat.Name == dBCategory.Name);
                            backDBCategory = DBCategories.First(cat => cat.Name == backDBCategory.Name);
                        }
                        else
                        {
                            DBCategories.Add(dBCategory); 
                            BackDBCategories.Add(backDBCategory);
                        }
                        var currentSubDirFams = Directory.GetFiles(subDir, "*.rfa");
                        foreach (var familyFile in currentSubDirFams) 
                        {
                            var famFileInfo = new FileInfo(familyFile);
                            var famFullName = famFileInfo.FullName;
                            var famNameWithExt = famFileInfo.Name;
                            var famName = famNameWithExt.Substring(0, famNameWithExt.Length - 4);
                            var isLoaded = false;
                            if (loadedFamilies.Select(fam => fam.Name).Contains(famName))
                            {
                                isLoaded = true;
                            }
                            dBCategory.DBFamilies.Add(new DBFamily(0, famFullName, famName, isLoaded));
                            backDBCategory.DBFamilies.Add(new DBFamily(0, famFullName, famName, isLoaded));
                            await call.RequestStream.WriteAsync(new HelloRequest() { Name = "FamilyDBRequest", ProcesId = famName, Text = backDBCategory.Name });
                        }
                        var dBFilteredFamilyNames = new List<string>();
                        var dBFilteredFamilies = RevitGrpcAddinApp._client.ServerDataStream().ResponseStream;
                        while (await dBFilteredFamilies.MoveNext(new System.Threading.CancellationToken()))
                        {
                            dBFilteredFamilyNames.Add(dBFilteredFamilies.Current.Command);
                        }
                        for (int i = 0; i < dBCategory.DBFamilies.Count; i++)
                        {
                            if (!dBFilteredFamilyNames.Contains(dBCategory.DBFamilies[i].Name))
                            {
                                dBCategory.DBFamilies.Remove(dBCategory.DBFamilies[i]);
                                backDBCategory.DBFamilies.Remove(backDBCategory.DBFamilies[i]);
                            }
                        }
                    }
                }
                SearchFamily(FindFamilyName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "gRPC Client", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }
        private void LoadFamily(string dbFamFullName)
        {
            var dbCommand = new DBCommand("LoadFamily",dbFamFullName);
            ActionQueue.Enqueue(dbCommand);
        }
        private void NewFamilyInstance(string dbFamFullName)
        {
            var dbCommand = new DBCommand("NewFamilyInstance", dbFamFullName);
            ActionQueue.Enqueue(dbCommand);
        }
        public void SearchFamily(string inputName)
        {
            DBCategories.Clear();
            BackDBCategories.ForEach(cat => DBCategories.Add((DBCategory)cat.Clone()));
            var backCats = new List<DBCategory>(BackDBCategories);
            if (!string.IsNullOrEmpty(inputName))   
            {
                var dbCatsWithAnyInputNameFam = DBCategories
                    .Where(cat => cat.DBFamilies
                    .Select(dbFam => dbFam.Name.ToLower())
                    .Any(famName => famName.Contains(inputName.ToLower()))).ToList();
                DBCategories.Clear();
                foreach (var dbCatWithNameFam  in dbCatsWithAnyInputNameFam)
                {
                    dbCatWithNameFam.DBFamilies = new ObservableCollection<DBFamily>(dbCatWithNameFam.DBFamilies.Where(dbFam => dbFam.Name.ToLower().Contains(inputName.ToLower())));
                    DBCategories.Add(dbCatWithNameFam);
                    dbCatWithNameFam.IsCategoryExpanded = true;
                }
            }
        }
    }
    class DBFamilyComparer : IEqualityComparer<DBFamily>
    {
        public bool Equals(DBFamily x, DBFamily y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(DBFamily obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
