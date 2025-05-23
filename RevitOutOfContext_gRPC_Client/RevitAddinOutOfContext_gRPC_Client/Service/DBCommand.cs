using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAddinOutOfContext_gRPC_Client.VM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitAddinOutOfContext_gRPC_Client.Service
{
    public class DBCommand
    {
        private static List<ElementId> _added_element_ids;
        public string CommandName { get; private set; }
        public string DBFamFullName { get; private set; }
        public DBCommand(string commandName, string dBFamFullName)
        {
            CommandName = commandName;
            DBFamFullName = dBFamFullName;
        }
        public void DoDBCommand(UIApplication uiapp, FamiliDbVM familyDbVM)
        {
            try
            {
                if (!string.IsNullOrEmpty(CommandName))
                {
                    if (CommandName == "LoadFamily")
                    {
                        LoadFamily(DBFamFullName, uiapp, familyDbVM);
                    }
                    else if (CommandName == "NewFamilyInstance")
                    {
                        NewFamilyInstance(DBFamFullName, uiapp, familyDbVM);
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.Message);
            }
        }
        private static void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            _added_element_ids.AddRange(e.GetAddedElementIds());
        }
        public static void LoadFamily(string dbFamFullName, UIApplication uiapp, FamiliDbVM familiDbVM)
        {
            var dbFam = familiDbVM.DBCategories.First(dbCat => dbCat.DBFamilies.Select(fam => fam.FullName).Contains(dbFamFullName)).DBFamilies.First(fam => fam.FullName == dbFamFullName);
            if(dbFam != null)
            {
                var tr = new Transaction(uiapp.ActiveUIDocument.Document, "Загрузить семейство");
                tr.Start();
                Family family = null;
                uiapp.ActiveUIDocument.Document.LoadFamily(dbFamFullName, new FamilyLoadOption(), out family);
                if (family != null)
                {
                    dbFam.Family = family;
                    dbFam.IsLoaded = true;
                }
                tr.Commit();
            }
        }
        public static void NewFamilyInstance(string dbFamFullName,UIApplication uiapp, FamiliDbVM familyDbVM)
        {
            var doc = uiapp.ActiveUIDocument.Document;
            var dbFam = familyDbVM.DBCategories.First(dbCat => dbCat.DBFamilies.Select(fam => fam.FullName).Contains(dbFamFullName)).DBFamilies.First(fam => fam.FullName == dbFamFullName);
            if (dbFam != null)
            {
                try
                {
                    _added_element_ids = new List<ElementId>();
                    var famSym = doc.GetElement(dbFam.Family.GetFamilySymbolIds().First()) as FamilySymbol;
                    uiapp.Application.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
                    uiapp.ActiveUIDocument.PromptForFamilyInstancePlacement(famSym);
                    uiapp.Application.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
            }
        }
    }
}