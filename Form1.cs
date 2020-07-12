using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParsCatalogForms
{
    public partial class Form1 : Form
    {
        WorkingClass wcl = new WorkingClass();
        /// <summary>
        /// need to  change
        /// </summary>
        private static string cs = @"URI=file:C:\sqllite\MedicDB.db";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            wcl.GetAllDatatoDic();
            LoadPagesForGetInfo();
            WriteCategoryDataToDB();
            WriteMedikamentDataToDB();
            webBr.Dispose();
            Environment.Exit(-1);
        }

        /// <summary>
        /// Load each page Catalog (loop), click on each row on page and get info 
        /// ~ aptvn 2 min 30 sek.
        /// </summary>
        private async void LoadPagesForGetInfo()
        {
            if (wcl.CategoryDataDic.Count > 0 && wcl.PageMedicLinkDic.Count > 0)
            {
                foreach (int kp in wcl.CategoryDataDic.Keys)
                {
                    string strKey = wcl.CategoryDataDic[kp].Kod;
                    webBr.Navigate(wcl.CategoryDataDic[kp].LinkTo);

                    await Task.Delay(6000);
                    foreach (KeyValuePair<string, string> kvp in wcl.PageMedicLinkDic[kp])
                    {

                        webBr.Navigate(kvp.Value);
                        await Task.Delay(700);
                        HtmlElement htelement = webBr.Document.GetElementById("ctl00_uxMainContent_uxFilteredCheapestProductListControl_uxProductInfoControl_uxProductInfoPopupPanel");
                        string vv_poz_numurs = htelement.Document.GetElementById("ctl00_uxMainContent_uxFilteredCheapestProductListControl_uxProductInfoControl_uxFaNumberRow").Children[1].InnerText.Trim();
                        string nosaukums = htelement.Document.GetElementById("ctl00_uxMainContent_uxFilteredCheapestProductListControl_uxProductInfoControl_uxNameRow").Children[1].InnerText.Trim();
                        string pieg_prec_kod = htelement.Document.GetElementById("ctl00_uxMainContent_uxFilteredCheapestProductListControl_uxProductInfoControl_uxSupplierProductCodeRow").Children[1].InnerText.Trim();
                        double cena_BPVN = double.Parse(htelement.Document.GetElementById("ctl00_uxMainContent_uxFilteredCheapestProductListControl_uxProductInfoControl_uxOnePriceRow").Children[1].InnerText.Trim());
                        if (!wcl.MedikamentDataDic.ContainsKey(strKey))
                        {
                            wcl.MedikamentDataDic.Add(strKey, new Dictionary<string, WorkingClass.MedicData>()
                            { { vv_poz_numurs,
                                    new WorkingClass.MedicData()
                                    {
                                        kods = pieg_prec_kod,
                                        nosaukums = nosaukums,
                                        cenaBPVN = cena_BPVN
                                    }
                                }
                            });
                        }
                        else
                        if (!wcl.MedikamentDataDic[strKey].Keys.Contains(vv_poz_numurs))
                        {
                            wcl.MedikamentDataDic[strKey].Add(vv_poz_numurs, new WorkingClass.MedicData()
                            {
                                kods = pieg_prec_kod,
                                nosaukums = nosaukums,
                                cenaBPVN = cena_BPVN
                            });
                        }
                    }
                }
            }
        }
        /// <summary>
        ///  Write Data about medikaments
        /// </summary>
        private void WriteMedikamentDataToDB()
        {
            using (var con = new SQLiteConnection(cs))
            {
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand(con);
                cmd.CommandText = "INSERT or IGNORE INTO MedicData(CategoryID,numurs, nosaukums,kods,cenaBPVN) VALUES (@CategoryID,@numurs, @nosaukums,@kods,@cenaBPVN);";
                cmd.Parameters.Add("@numurs", DbType.String);
                cmd.Parameters.Add("@nosaukums", DbType.String);
                cmd.Parameters.Add("@kods", DbType.String);
                cmd.Parameters.Add("@CategoryID", DbType.Int32);
                cmd.Parameters.Add("@cenaBPVN", DbType.Double);
                cmd.Prepare();

                foreach (string key in wcl.MedikamentDataDic.Keys)
                {
                    int CategoryId_ = GetCategoryId(key);
                    foreach (string keystr in wcl.MedikamentDataDic[key].Keys)
                    {
                        try
                        {
                            cmd.Parameters["@numurs"].Value = keystr;
                            cmd.Parameters["@nosaukums"].Value = wcl.MedikamentDataDic[key][keystr].nosaukums;
                            cmd.Parameters["@kods"].Value = wcl.MedikamentDataDic[key][keystr].kods;
                            cmd.Parameters["@CategoryID"].Value = CategoryId_;
                            cmd.Parameters["@cenaBPVN"].Value = wcl.MedikamentDataDic[key][keystr].cenaBPVN;
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write Data about Category 
        /// </summary>
        private void WriteCategoryDataToDB()
        {
            if (wcl.CategoryDataDic.Count > 0)
            {
                using (var con = new SQLiteConnection(cs))
                {
                    con.Open();
                    SQLiteCommand cmd = new SQLiteCommand(con);
                    cmd.CommandText = "INSERT or IGNORE INTO Category(numurs, nosaukums) VALUES (@numurs, @nosaukums)";
                    cmd.Parameters.Add("@numurs", DbType.String);
                    cmd.Parameters.Add("@nosaukums", DbType.String);
                    cmd.Prepare();
                    foreach (int key in wcl.CategoryDataDic.Keys)
                    {
                        cmd.Parameters["@numurs"].Value = wcl.CategoryDataDic[key].Kod;
                        cmd.Parameters["@nosaukums"].Value = wcl.CategoryDataDic[key].Nosaukums;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        /// <summary>
        /// Get Category Id for foreignKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private int GetCategoryId(string key)
        {
            using (var con = new SQLiteConnection(cs))
            {
                try
                {
                    con.Open();
                    SQLiteCommand cmd = new SQLiteCommand("SELECT ID FROM Category WHERE numurs = @CategoryNumurs;", con);
                    // cmd.Parameters.Add("@CategoryNumurs", DbType.String).Value = key;
                    cmd.Parameters.AddWithValue("@CategoryNumurs", key);
                    // cmd.CommandText = "SELECT ID FROM Category WHERE numurs = @CategoryNumurs;";
                    Int32 CategoryID = Convert.ToInt32(cmd.ExecuteScalar());

                    return CategoryID;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
    }
}
