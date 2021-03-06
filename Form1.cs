﻿using ParsCatalogForms.Model;
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
        private static string dbPath = @"URI=file:C:\sqllite\MedicDB.db";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            wcl.GetAllDatatoDic();
            LoadPagesForGetInfo();
            WriteCategoryDataToDB();

        }

        /// <summary>
        /// Load each page Catalog (loop), click on each row on page and get info 
        /// ~ aptvn 4 min 30 sek.
        /// </summary>
        private async void LoadPagesForGetInfo()
        {
            if (wcl.CategoryDataDic.Count > 0 && wcl.PageMedicLinkDic.Count > 0)
            {
                Console.WriteLine("Reading all data");
                Console.WriteLine("Started...Waiting time ~4 min.");
                DateTime a = DateTime.Now;

                foreach (int kp in wcl.CategoryDataDic.Keys)
                {
                    string strKey = wcl.CategoryDataDic[kp].Kod;
                    webBr.Navigate(wcl.CategoryDataDic[kp].LinkTo);

                    await Task.Delay(8000);
                    foreach (KeyValuePair<string, string> kvp in wcl.PageMedicLinkDic[kp])
                    {

                        webBr.Navigate(kvp.Value);
                        await Task.Delay(900);
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

                Console.WriteLine($"Reading is done. Elapsed time {(DateTime.Now - a).TotalMinutes} min.");
            }
            if (wcl.MedikamentDataDic.Count > 0)
            {
                WriteMedikamentDataToDB();
            }
        }
        /// <summary>
        ///  Write Data about medikaments
        /// </summary>
        private void WriteMedikamentDataToDB()
        {
            if (wcl.MedikamentDataDic.Count > 0)
            {
                using (var con = new SQLiteConnection(dbPath))
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
                Console.WriteLine("Data about Medikaments writed.");
                webBr.Dispose();
                Console.WriteLine("Finish");
                Console.WriteLine("Press any key to exit");

                FillResultDatagrid();
            }
        }

        private void FillResultDatagrid()
        {

            using (var db = new context())
            {
                var list = db.MedicDatas.ToList();
                if (list.Count > 0)
                {

                    dataGridView1.DataSource = list;
                    dataGridView1.Columns[0].Visible = false;
                    dataGridView1.Columns[1].Visible = false;
                    dataGridView1.Columns[2].Visible = false;
                    dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }
        }

        /// <summary>
        /// Write Data about Category 
        /// </summary>
        private void WriteCategoryDataToDB()
        {
            if (wcl.CategoryDataDic.Count > 0)
            {
                using (var con = new SQLiteConnection(dbPath))
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
            using (var con = new SQLiteConnection(dbPath))
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
