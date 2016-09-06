using HG.Coprorate.Firebrand.CustomLogs;
using HG.Coprorate.Firebrand.Models;
using HG.Corporate.Core;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class SeriesSitecoreItemCreationHelper
    {
        private string SeriesItemTemplateId = SiteSettings.AppSettingsCollection["SeriesBranchTemplateId"];
        private string SeriesFolderItemId = SiteSettings.AppSettingsCollection["SeriesBucketItemId"];
        private string sitecoreMasterDatabaseName = "master";

        /// <summary>
        /// Import Series' data into sitecore and create new items
        /// </summary>
        /// <param name="authos">Series obejcts generated from XML</param>
        public string ImportSeriesItemsToSitecore(Series series, ref List<string> ExistingSeries, Item publisherGroupOwner)
        {
            List<string> ids = new List<string>();
            if (!ExistingSeries.Contains(series.DisplayName.Replace(" ", "-")))
            {
                ExistingSeries.Add(series.DisplayName.Replace(" ", "-"));
                ID id = this.CreateSeriesItemInSitecore(series, publisherGroupOwner);
                return id.ToString();
            }
            else
            {
                Database masterDb = Sitecore.Configuration.Factory.GetDatabase(sitecoreMasterDatabaseName);
                Item SeriesItem = masterDb.GetItem(SeriesFolderItemId).Axes.GetDescendants().Where(i => i.DisplayName == series.DisplayName).FirstOrDefault();
                ID id = this.ExtractDataFromAuhorObject(series, SeriesItem, publisherGroupOwner);
                return id.ToString();
            }
        }

        /// <summary>
        /// Create a sigle Series item in sitecore
        /// </summary>
        /// <param name="series">Series object generated from XML</param>
        private ID CreateSeriesItemInSitecore(Series series, Item publisherGroupOwner)
        {
            try
            {
                using (new SecurityDisabler())
                {
                    Database masterDb = Sitecore.Configuration.Factory.GetDatabase(sitecoreMasterDatabaseName);
                    //Item SeriesFolderItem = masterDb.GetItem(SeriesFolderItemId);
                    //TemplateItem SeriesItemTemplate = masterDb.GetTemplate(SeriesItemTemplateId);
                    //Item SeriesItem = SeriesFolderItem.Add(series.DisplayName.Trim(), SeriesItemTemplate);

                    //Item SeriesItemTemplate = masterDb.GetItem(SeriesItemTemplateId);
                    //Item SeriesFolderItem = masterDb.GetItem(SeriesFolderItemId);
                    //BranchItem SeriesBranchItem = masterDb.GetItem(SeriesItemTemplate.ID);
                    //Item SeriesItem = SeriesFolderItem.Add(series.DisplayName.Trim(), SeriesBranchItem);
                    Item SeriesItem = SitecoreCreationHelper.CreateBranchItem(series.DisplayName, masterDb, SeriesItemTemplateId, SeriesFolderItemId);
                    return ExtractDataFromAuhorObject(series, SeriesItem, publisherGroupOwner);
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to import Series " + series.DisplayName, ex);
            }
            return null;
        }

        private ID ExtractDataFromAuhorObject(Series series, Item SeriesItem, Item publisherGroupOwner)
        {
            this.populateSeriesItemData(series, SeriesItem, publisherGroupOwner);

            //publish Series item
            SitecorePublishHelper sp = new SitecorePublishHelper();
            sp.PublishItem(SeriesItem, false);

            return SeriesItem.ID;
        }

        /// <summary>
        /// populate Series data with the Series object into sitecore
        /// </summary>
        /// <param name="series">Series object generated from XML</param>
        /// <param name="SeriesItem">Sitecore Series item</param>
        private void populateSeriesItemData(Series series, Item SeriesItem, Item publisherGroupOwner)
        {
            SeriesItem.Editing.BeginEdit();
            try
            {
                SeriesItem.Fields["SeriesName"].Value = series.Name;
                SeriesItem.Fields["MetaTitle"].Value = string.Format("{0} | Hardie Grant Publishing", series.Name);
                if (string.IsNullOrWhiteSpace(SeriesItem.Fields["MetaDescription"].Value))
                {
                    SeriesItem.Fields["MetaDescription"].Value = string.Format("{0}{1} | Hardie Grant Publishing",
                        publisherGroupOwner == null ? "" : 
                        (string.IsNullOrWhiteSpace(publisherGroupOwner["Name"]) ? "" : string.Format("A {0} series: ", publisherGroupOwner["Name"])),
                        series.Name);
                }
                if (!((Sitecore.Data.Fields.CheckboxField)SeriesItem.Fields["DisableOverridePublishingGroupOwner"]).Checked)
                {
                    if (publisherGroupOwner != null)
                        SeriesItem.Fields["PublishingGroupOwner"].Value = publisherGroupOwner.ID.ToString();
                }
                SeriesItem.Fields["MetaKeywords"].Value = string.Format("{0},{1},Hardie Grant Publishing",  series.Name, publisherGroupOwner);
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to populate data for Series " + series.DisplayName + "into item" + SeriesItem.ID.ToString(), ex);
            }
            finally
            {
                SeriesItem.Editing.EndEdit();
            }
        }
    }
}