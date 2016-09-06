using Glass.Mapper.Sc;
using HG.Coprorate.Firebrand.CustomLogs;
using HG.Corporate.Core;
using HG.Corporate.Core.Models;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class BICCategorySitecoreItemHelper
    {
        private string BICCategoryItemTemplateId = SiteSettings.AppSettingsCollection["BICCategoryItemTemplateId"];
        private string BICCategoryFolderItemId = SiteSettings.AppSettingsCollection["BICCategoryBucketItemId"];
        private string sitecoreMasterDatabaseName = "master";
        private string HGBCategoriesFolderItemId = SiteSettings.AppSettingsCollection["HGBCategoriesFolderItemId"];
        private string HGTCategoriesFolderItemId = SiteSettings.AppSettingsCollection["HGTCategoriesFolderItemId"];

        private Database masterDb
        {
            get
            {
                return Sitecore.Configuration.Factory.GetDatabase(sitecoreMasterDatabaseName);
            }
        }

        public void ImportBICCategoriesItemsToSitecore(List<BICCategory> BICCategories)
        {
            foreach (var BICCategory in BICCategories)
            {
                string id = this.CreateBICCategoryItemInSitecore(BICCategory);
                BICCategory.SitecoreId = id;
                Thread.Sleep(100);
            }
            this.populateBICCateData(BICCategories);
        }

        private void populateBICCateData(List<BICCategory> BICCategories)
        {
            var groupedHGBCategories = BICCategories.GroupBy(g => g.HGBCategory).ToList();
            var groupedHGTCategories = BICCategories.GroupBy(g => g.HGTCategory).ToList();
            Item HGBCategoriesFolderItem = masterDb.GetItem(HGBCategoriesFolderItemId);
            Item HGTCategoriesFolderItem = masterDb.GetItem(HGTCategoriesFolderItemId);
            foreach (var HGBCategories in groupedHGBCategories)
            {
                if (!string.IsNullOrWhiteSpace(HGBCategories.First().HGBCategory))
                {
                    this.PopulateGroupedBICCateData(HGBCategories.ToList(), HGBCategoriesFolderItem, "HGB");
                }
            }
            foreach (var HGTCategories in groupedHGTCategories)
            {
                if (!string.IsNullOrWhiteSpace(HGTCategories.First().HGTCategory))
                {
                    this.PopulateGroupedBICCateData(HGTCategories.ToList(), HGTCategoriesFolderItem, "HGT");
                }
            }
        }

        private void PopulateGroupedBICCateData(List<BICCategory> BICCategories, Item categoryFolderItem, string categoryType)
        {
            List<Item> categoryItems = null;
            List<string> IDs = new List<string>();
            if (categoryType == "HGB")
            {
                categoryItems = categoryFolderItem.Children.Where(c => BICCategories.First().HGBCategory.Contains(c["Name"])).ToList();
            }
            else
            {
                categoryItems = categoryFolderItem.Children.Where(c => BICCategories.First().HGTCategory.Contains(c["Name"])).ToList();
            }
            IDs = BICCategories.Select(b => b.SitecoreId.ToString()).ToList();
            foreach (var categoryItem in categoryItems)
            {
                categoryItem.Editing.BeginEdit();
                try
                {
                    if (IDs.Count > 0)
                    {
                        MultilistField ild = (MultilistField)categoryItem.Fields["Mapped BIC Categories"];
                        if (string.IsNullOrWhiteSpace(ild.Value))
                        {
                            ild.Value = string.Join("|", IDs);
                        }
                        else
                        {
                            ild.Value += "|" + string.Join("|", IDs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    BooksImportLog.Error("There is an error when trying to populate category data for hg categories " + BICCategories.First().HGBCategory + "into item" + categoryItem.ID.ToString(), ex);
                }
                finally
                {
                    categoryItem.Editing.EndEdit();
                }
            }
        }

        private ISitecoreService _sitecoreService
        {
            get
            {
                Database masterDb = Sitecore.Configuration.Factory.GetDatabase(sitecoreMasterDatabaseName);
                return new SitecoreService(masterDb);
            }
        }

        private string CreateBICCategoryItemInSitecore(BICCategory BICCategory)
        {
            string id = string.Empty;
            try
            {
                using (new SecurityDisabler())
                {
                    //var bicCategoryFolder = _sitecoreService.GetItem<SitecoreBaseItem>(new Guid(BICCategoryFolderItemId));
                    //_sitecoreService.Create(bicCategoryFolder, BICCategory);
                    Item BICCategoryFolderItem = masterDb.GetItem(BICCategoryFolderItemId);
                    TemplateItem BICCategoryItemTemplate = masterDb.GetTemplate(BICCategoryItemTemplateId);
                    Item BICCategoryItem = BICCategoryFolderItem.Add(BICCategory.Code.Replace("�", "").Replace("?", "").Trim(), BICCategoryItemTemplate);
                    id = BICCategoryItem.ID.ToString();
                    BICCategory.PopulateData(ref BICCategoryItem);
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to import BICCategory " + BICCategory.Code, ex);
            }
            return id;
        }

        private ID ExtractDataFromBICCategoryObject(BICCategory BICCategory, Item BICCategoryItem)
        {
            //this.populatePublisherItemData(Publisher, PublisherItem);
            //publish Publisher item
            SitecorePublishHelper sp = new SitecorePublishHelper();
            sp.PublishItem(BICCategoryItem, false);

            return BICCategoryItem.ID;
        }
    }
}