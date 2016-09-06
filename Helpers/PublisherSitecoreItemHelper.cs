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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class PublisherSitecoreItemHelper
    {
        private string PublisherItemTemplateId = SiteSettings.AppSettingsCollection["PublisherItemTemplateId"];
        private string PublisherFolderItemId = SiteSettings.AppSettingsCollection["PublisherBucketItemId"];
        private string sitecoreMasterDatabaseName = "master";


        public string ImportPublishersItemsToSitecore(Publisher Publisher, ref List<string> ExistingPublishers)
        {
            string id = string.Empty;
            if (!ExistingPublishers.Contains(Publisher.DisplayName.Replace(" ", "-")))
            {
                ExistingPublishers.Add(Publisher.DisplayName.Replace(" ", "-"));
                id = this.CreatePublisherItemInSitecore(Publisher).ToString();
            }
            else
            {
                Database masterDb = Sitecore.Configuration.Factory.GetDatabase(sitecoreMasterDatabaseName);
                List<Item> PublisherItems = masterDb.GetItem(PublisherFolderItemId).Axes.GetDescendants().ToList();
                Item PublisherItem = PublisherItems.FirstOrDefault(i => i.DisplayName == Publisher.DisplayName);
                id = this.ExtractDataFromPublishersObject(Publisher, PublisherItem).ToString();
            }
            Thread.Sleep(100);
            return id;
        }

        private ID CreatePublisherItemInSitecore(Publisher Publisher)
        {
            try
            {
                using (new SecurityDisabler())
                {
                    Database masterDb = Sitecore.Configuration.Factory.GetDatabase(sitecoreMasterDatabaseName);
                    Item PublisherFolderItem = masterDb.GetItem(PublisherFolderItemId);
                    TemplateItem PublisherItemTemplate = masterDb.GetTemplate(PublisherItemTemplateId);
                    Item PublisherItem = PublisherFolderItem.Add(Publisher.DisplayName.Trim(), PublisherItemTemplate);

                    BooksImportLog.Info(string.Format("Created new publisher [{0}] with item ID: {1}", Publisher.DisplayName, PublisherItem != null ? PublisherItem.ID.ToString() : ""));

                    return ExtractDataFromPublishersObject(Publisher, PublisherItem);
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to import Publisher " + Publisher.DisplayName, ex);
            }
            return null;
        }

        private ID ExtractDataFromPublishersObject(Publisher Publisher, Item PublisherItem)
        {

            this.populatePublisherItemData(Publisher, PublisherItem);

            //publish Publisher item
            SitecorePublishHelper sp = new SitecorePublishHelper();
            sp.PublishItem(PublisherItem, false);

            return PublisherItem.ID;
        }

        /// <summary>
        /// populate Publisher data with the Publisher object into sitecore
        /// </summary>
        /// <param name="Publisher">Publisher object generated from XML</param>
        /// <param name="PublisherItem">Sitecore Publisher item</param>
        private void populatePublisherItemData(Publisher Publisher, Item PublisherItem)
        {
            PublisherItem.Editing.BeginEdit();
            try
            {
                PublisherItem.Fields["Publisher name"].Value = Publisher.Name;
                if (Publisher.GroupItem != null)
                {
                    PublisherItem.Fields["Publisher group"].Value = Publisher.GroupItem.ID.ToString();
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to populate data for Publisher " + Publisher.DisplayName + "into item" + PublisherItem.ID.ToString(), ex);
            }
            finally
            {
                PublisherItem.Editing.EndEdit();

                BooksImportLog.Info(string.Format("Updated data for publisher [{0}] with item ID: {1}", Publisher.DisplayName, PublisherItem != null ? PublisherItem.ID.ToString() : ""));
            }
        }
    }
}
