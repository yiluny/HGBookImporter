using CsvHelper;
using CsvHelper.Configuration;
using HG.Corporate.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class BICCategoryExcelTransformHelper
    {
        public class BICCategoryMap : CsvClassMap<BICCategory>
        {
            public override void CreateMap()
            {
                Map(m => m.Code).Index(0);
                Map(m => m.Description).Index(1);
                Map(m => m.HGBCategory).Index(2);
                Map(m => m.HGTCategory).Index(3);
            }
        }

        private string excelFilePath = "BICCodeList.xls";

        public List<BICCategory> ReadExcelFile()
        {
            var BICCatFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/upload/" + excelFilePath);
            List<BICCategory> BICCategoryList = new List<BICCategory>();
            try
            {
                using (var csvInfo = new CsvReader(File.OpenText("BICCodeList.csv")))
                {
                    csvInfo.Read();
                    csvInfo.Configuration.RegisterClassMap<BICCategoryMap>();
                    BICCategoryList = csvInfo.GetRecords<BICCategory>().ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return BICCategoryList;
        }

    }
}