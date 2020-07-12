using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Net;
using System;

namespace ParsCatalogForms
{
    class WorkingClass
    {
        public Dictionary<int, CategoryData> CategoryDataDic = new Dictionary<int, CategoryData>(); //for Category
        public Dictionary<int, Dictionary<string, string>> PageMedicLinkDic = new Dictionary<int, Dictionary<string, string>>(); //for links to medikament row
        public Dictionary<string, Dictionary<string, MedicData>> MedikamentDataDic = new Dictionary<string, Dictionary<string, MedicData>>(); //for info about medikament
        private static string mainUrl = "https://www.eis.gov.lv/EIS/Categories/CategoryList.aspx?CategoryId=23411";


        public void GetAllDatatoDic()
        {
            HtmlWeb web = new HtmlWeb();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HtmlDocument document = web.Load(mainUrl);
            var catalogTable = document.GetElementbyId("ctl00_uxMainContent_uxLastLevelCategories").SelectNodes(".//a").ToList();
            string CategoryLinkTo = "";

            //main page catalogs kod,name,link
            foreach (var catItem in catalogTable)
            {
                CategoryLinkTo = catItem.GetAttributeValue("href", "");
                CategoryLinkTo = mainUrl.Substring(0, mainUrl.Length - CategoryLinkTo.Length) + CategoryLinkTo;
                string inner_ = catItem.InnerText.Trim();
                string CategoryKod = inner_.Substring(0, inner_.IndexOf("&nbsp;"));
                string CategoryNosaukums = inner_.Substring(inner_.IndexOf("&nbsp;") + 6, inner_.Length - (inner_.IndexOf("&nbsp;") + 6));
                int id = Int32.Parse(CategoryLinkTo.Substring(CategoryLinkTo.LastIndexOf("=") + 1, 5));
                if (!CategoryDataDic.ContainsKey(id))
                {
                    CategoryDataDic.Add(id, new CategoryData()
                    {
                        Kod = CategoryKod,
                        Nosaukums = CategoryNosaukums,
                        LinkTo = CategoryLinkTo
                    });
                }
            }

            //foreach catalogs grabb links
            if (CategoryDataDic.Count > 0)
            {
                foreach (int key in CategoryDataDic.Keys)
                {
                    document = web.Load(CategoryDataDic[key].LinkTo);
                    var medicTable = document.GetElementbyId("ctl00_uxMainContent_uxFilteredCheapestProductListControl_uxDataView").SelectNodes(".//a").ToList();
                    foreach (var item in medicTable)
                    {
                        var link_ = item.GetAttributeValue("href", "").Replace("&#39;", "'");
                        if (!PageMedicLinkDic.ContainsKey(key))
                        {
                            PageMedicLinkDic.Add(key, new Dictionary<string, string>() { { item.InnerText.Trim(), link_ } });
                        }
                        else
                            PageMedicLinkDic[key].Add(item.InnerText.Trim(), link_);
                    }
                }
            }
        }
        public class CategoryData
        {
            public string Kod { get; set; }
            public string Nosaukums { get; set; }
            public string LinkTo { get; set; }
        }
        public class MedicData
        {
            public string nosaukums { get; set; }
            public string kods { get; set; }
            public double cenaBPVN { get; set; }
        }
    }
}
