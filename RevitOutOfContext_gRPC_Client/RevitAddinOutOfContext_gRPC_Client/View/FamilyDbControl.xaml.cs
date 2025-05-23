using Autodesk.Revit.UI;
using RevitAddinOutOfContext_gRPC_Client.VM;
using System;
using System.Windows.Controls;

namespace RevitAddinOutOfContext_gRPC_Client.View
{
    /// <summary>
    /// Логика взаимодействия для FamilyDbControl.xaml
    /// </summary>
    public partial class FamilyDbControl : UserControl, IDockablePaneProvider
    {
        public FamilyDbControl(FamiliDbVM familiDbVM)
        {
            InitializeComponent();
            this.DataContext = familiDbVM;
        }
        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
        }
        public static DockablePaneId PaneId
        {
            get
            {
                return new DockablePaneId(new Guid("E6EF9DE9-F5F2-454B-8968-4BA2622E5CE5"));
            }
        }
        public static string PaneName
        {
            get
            {
                return "Семейства РТИ";
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (System.Windows.Controls.TextBox)sender;
            ((FamiliDbVM)this.DataContext).SearchFamily(textBox.Text);

        }
    }
}