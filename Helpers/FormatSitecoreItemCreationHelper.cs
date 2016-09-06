using HG.Coprorate.Firebrand.CustomLogs;
using HG.Coprorate.Firebrand.Models;
using HG.Corporate.Core;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class FormatSitecoreItemCreationHelper
    {
        private string FormatItemTemplateId = SiteSettings.AppSettingsCollection["FormatItemTemplateId"];
        private string FormatListFolderItemId = SiteSettings.AppSettingsCollection["FormatListFolderItemId"];
        private string sitecoreMasterDatabaseName = "master";

        private Database masterDb
        {
            get
            {
                return Sitecore.Configuration.Factory.GetDatabase(sitecoreMasterDatabaseName);
            }
        }

        public void ImportFormatItemsToSitecore(List<Format> FormatList)
        {
            foreach (var Format in FormatList)
            {
                this.CreateFormatItemInSitecore(Format);
                Thread.Sleep(100);
            }
        }

        private void CreateFormatItemInSitecore(Format Format)
        {
            try
            {
                using (new SecurityDisabler())
                {
                    Item FormatListFolderItem = masterDb.GetItem(FormatListFolderItemId);
                    TemplateItem FormatItemTemplate = masterDb.GetTemplate(FormatItemTemplateId);
                    Item formatItem = FormatListFolderItem.Add(Format.Code.Replace("�", "").Replace("?", "").Trim(), FormatItemTemplate);
                    Format.PopulateData(ref formatItem);
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to import Format " + Format.Code, ex);
            }
        }
    }
}