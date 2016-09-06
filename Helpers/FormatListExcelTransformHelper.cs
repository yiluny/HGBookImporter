using CsvHelper;
using CsvHelper.Configuration;
using HG.Coprorate.Firebrand.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class FormatListExcelTransformHelper
    {
        public class FormatMap : CsvClassMap<Format>
        {
            public override void CreateMap()
            {
                Map(m => m.Code).Index(0);
                Map(m => m.Description).Index(1);
            }
        }

        private string excelFilePath = "FormatList.xls";

        public List<Format> ReadExcelFile()
        {
            var FormatListFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/upload/" + excelFilePath);
            List<Format> formatList = new List<Format>();
            try
            {
                using (var csvInfo = new CsvReader(File.OpenText("FormatList.csv")))
                {
                    csvInfo.Read();
                    csvInfo.Configuration.RegisterClassMap<FormatMap>();
                    formatList = csvInfo.GetRecords<Format>().ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return formatList;
        }
    }
}
