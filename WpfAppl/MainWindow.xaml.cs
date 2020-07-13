using System;
using System.Windows;
using System.Data.SQLite;
using System.Data;

namespace WpfAppli
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string cs = @"URI=file:C:\sqllite\MedicDB.db";

        public MainWindow()
        {
            InitializeComponent();

            using (var con = new SQLiteConnection(cs))
            {
                try
                {
                    con.Open();
                    string query = "SELECT numurs, nosaukums FROM Category;" +
                        "Select numurs, nosaukums, kods, cenaBPVN from Medicdata";
                    SQLiteCommand cmd = new SQLiteCommand(query, con);
                    SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    dgCategory.DataContext = ds.Tables[0];
                    dgMedic.DataContext = ds.Tables[1];

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
