using HG.Coprorate.Firebrand.CustomLogs;
using HG.Coprorate.Firebrand.Models;
using HG.Corporate.Core;
using HG.Corporate.Core.Search.SearchServices;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using HG.Corporate.Core.Helpers;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class BookSitecoreItemCreationHelper
    {
        private string bookItemTemplateId = SiteSettings.AppSettingsCollection["BookBranchTemplateId"];
        private string booksBucketItemId = SiteSettings.AppSettingsCollection["BookBucketItemId"];
        private string HGBCategoriesFolderItemId = SiteSettings.AppSettingsCollection["HGBCategoriesFolderItemId"];
        private string HGTCategoriesFolderItemId = SiteSettings.AppSettingsCollection["HGTCategoriesFolderItemId"];
        private string HGECategoriesFolderItemId = SiteSettings.AppSettingsCollection["HGECategoriesFolderItemId"];
        private string BICCategoryBucketItemId = SiteSettings.AppSettingsCollection["BICCategoryBucketItemId"];
        private string FormatItemTemplateId = SiteSettings.AppSettingsCollection["FormatItemTemplateId"];
        private string FormatListFolderItemId = SiteSettings.AppSettingsCollection["FormatListFolderItemId"];
        private string sitecoreMasterDatabaseName = "master";

        private ChildList _FormatListItems { get; set; }

        private ChildList FormatListItems
        {
            get
            {
                if (_FormatListItems == null)
                {
                    _FormatListItems = masterDb.GetItem(FormatListFolderItemId).Children;
                }
                return _FormatListItems;
            }
        }

        private ChildList _BICCategoriesItems { get; set; }

        private ChildList BICCategoriesItems
        {
            get
            {
                if (_BICCategoriesItems == null)
                {
                    _BICCategoriesItems = masterDb.GetItem(BICCategoryBucketItemId).Children;
                }
                return _BICCategoriesItems;
            }
        }

        private string TitleFieldName = "Title";
        private string SubTitleFieldName = "Subtitle";
        private string ISBNFieldName = "ISBN";
        private string PublisherFieldName = "Publisher";
        private string MediaFileFieldName = "MediaFile";
        private string ImprintFieldName = "Imprint";
        private string AuthorFieldName = "Authors";
        private string ContributorSequenceFieldName = "ContributorSequence";
        private string IllustratorsFieldName = "Illustrators";
        private string FormatFieldName = "Format";
        private string SeriesFieldName = "Series";
        private string SeriesNameFieldName = "SeriesName";
        private string SeriesNumberFieldName = "Series Number";
        private string PageNumberFieldName = "PageNumber";
        private string MinAgeFieldName = "MinAge";
        private string MaxAgeFieldName = "MaxAge";
        private string DescriptionFieldName = "Description";
        private string PublishStatusFieldName = "PublishStatus";
        private string HeightFieldName = "Height";
        private string WidthFieldName = "Width";
        private string PriceFieldName = "Price";
        private string WeightFieldName = "Weight";
        private string PubDateFieldName = "PubDate";
        private string PublisherGroupField = "PublisherGroup";
        private string CategoryFieldName = "Category";
        private string BICCategoriesFieldName = "BICCategories";
        private string MainBICCategoryFieldName = "MainBICCategory";

        private List<string> ExistingBooks = new List<string>();
        private List<string> ExistingAuthors = new List<string>();
        private List<string> ExistingPublishers = new List<string>();
        private List<string> ExistingSeries = new List<string>();

        private Database masterDb
        {
            get
            {
                return Sitecore.Configuration.Factory.GetDatabase(sitecoreMasterDatabaseName);
            }
        }

        private AuthorSitecoreItemCreationHelper _authorSitecoreItemCreationHelper;
        private PublisherSitecoreItemHelper _publisherSitecoreItemHelper;
        private SeriesSitecoreItemCreationHelper _seriesSitecoreItemCreationHelper;

        public BookSitecoreItemCreationHelper()
        {
            _authorSitecoreItemCreationHelper = new AuthorSitecoreItemCreationHelper();
            _publisherSitecoreItemHelper = new PublisherSitecoreItemHelper();
            _seriesSitecoreItemCreationHelper = new SeriesSitecoreItemCreationHelper();
        }

        /// <summary>
        /// Import books' data into sitecore and create new items
        /// </summary>
        /// <param name="books">Books obejcts generated from XML</param>
        public void ImportBookItemsToSitecore(List<Book> books)
        {
            ExistingBooks = OnixBooksSearchService.GetAllBooksISBN();
            ExistingAuthors = OnixAuthorsSearchService.GetAllAuthorsNames();
            ExistingPublishers = OnixPublishersSearchService.GetAllPublishersNames();
            ExistingSeries = OnixSeriesSearchService.GetAllSeriesNames();
            foreach (var book in books)
            {
                try
                {
                    string bookName = ItemNameHelper.RemoveSpecialCharacters(book.Title.TitleText);
                    if (!ExistingBooks.Contains(book.ISBN))
                    {
                        ExistingBooks.Add(book.ISBN);
                        this.CreateBookItemInSitecore(book, bookName);
                    }
                    else
                    {
                        var bookSearchItem = OnixBooksSearchService.GetBookByIsbn(book.ISBN);
                        if (bookSearchItem != null)
                        {
                            Item bookItem = bookSearchItem.GetItemFromMasterDb();
                            if (bookItem != null)
                            {
                                BooksImportLog.Info(string.Format("Updating existing book [{0}] with item ID: {1}", book.Title.TitleText, bookItem != null ? bookItem.ID.ToString() : ""));
                                this.ExtractDataToBookItem(book, bookItem);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    BooksImportLog.Error("There is an error when trying to import book " + book.Title.TitleText + " in the loop", ex);
                }
            }
        }

        /// <summary>
        /// Create a sigle book item in sitecore
        /// </summary>
        /// <param name="book">Book object generated from XML</param>
        private void CreateBookItemInSitecore(Book book, string bookName)
        {
            try
            {
                using (new SecurityDisabler())
                {
                    Item bookItem = SitecoreCreationHelper.CreateBranchItem(bookName, masterDb, bookItemTemplateId, booksBucketItemId);
                    BooksImportLog.Info(string.Format("Created new book [{0}] with item ID: {1}", book.Title.TitleText, bookItem != null ? bookItem.ID.ToString() : ""));

                    this.ExtractDataToBookItem(book, bookItem);
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to import book (new) " + book.Title.TitleText, ex);
            }
        }

        private void ExtractDataToBookItem(Book book, Item bookItem)
        {
            string publisher = string.Empty;
            string series = string.Empty;
            Item publisherOwner = null;
            if (book.Publisher != null && book.Publisher.GroupItem != null)
            {
                publisherOwner = book.Publisher.GroupItem;
            }

            if (book.Authors != null && book.Authors.Count > 0)
            {
                this._authorSitecoreItemCreationHelper.ImportAuthorsItemsToSitecore(ContributorType.Author, ref book, ref ExistingAuthors, publisherOwner);
            }
            if (book.Illustrators != null && book.Illustrators.Count > 0)
            {
                this._authorSitecoreItemCreationHelper.ImportAuthorsItemsToSitecore(ContributorType.Illustrator, ref book, ref ExistingAuthors, publisherOwner);
            }
            if (book.Publisher != null)
            {
                publisher = _publisherSitecoreItemHelper.ImportPublishersItemsToSitecore(book.Publisher, ref ExistingPublishers);
            }
            if (book.Series != null)
            {
                series = _seriesSitecoreItemCreationHelper.ImportSeriesItemsToSitecore(book.Series, ref ExistingSeries, publisherOwner);
            }

            this.populateBookItemData(book, bookItem, publisher, series);

            //publish book item
            SitecorePublishHelper sp = new SitecorePublishHelper();
            sp.PublishItem(bookItem, false);
        }

        /// <summary>
        /// populate book data with the book object into sitecore
        /// </summary>
        /// <param name="book">Book object generated from XML</param>
        /// <param name="bookItem">Sitecore book item</param>
        private void populateBookItemData(Book book, Item bookItem, string publisher, string series)
        {
            bookItem.Editing.BeginEdit();
            try
            {
                if (bookItem.Fields["HashCode"] != null)
                    bookItem["HashCode"] = book.HashCode;

                if (book.Title != null)
                {
                    bookItem.Fields[TitleFieldName].Value = book.Title.TitleText;
                    bookItem.Fields[SubTitleFieldName].Value = book.Title.SubTitle;
                }
                bookItem.Fields[ISBNFieldName].Value = book.ISBN;
                if (!string.IsNullOrWhiteSpace(publisher))
                {
                    MultilistField pId = (MultilistField)bookItem.Fields[PublisherFieldName];
                    pId.Value = string.Join("|", publisher);
                    if (book.Publisher.GroupItem != null)
                    {
                        bookItem.Fields[PublisherGroupField].Value = book.Publisher.GroupItem.ID.ToString();
                    }
                }

                if (!string.IsNullOrWhiteSpace(series))
                {
                    bookItem.Fields[SeriesNameFieldName].Value = book.Series.Name;
                    MultilistField sId = (MultilistField)bookItem.Fields[SeriesFieldName];
                    sId.Value = string.Join("|", series);
                }
                if (!string.IsNullOrWhiteSpace(book.Format))
                {
                    Item formatSitecoreItem = FormatListItems.Where(f => f.DisplayName == book.Format).FirstOrDefault();
                    if (formatSitecoreItem != null)
                    {
                        bookItem.Fields[FormatFieldName].Value = formatSitecoreItem.ID.ToString();
                    }
                }
                bookItem.Fields[PageNumberFieldName].Value = book.PageNumber;
                bookItem.Fields[MinAgeFieldName].Value = book.MinAge;
                bookItem.Fields[MaxAgeFieldName].Value = book.MaxAge;
                bookItem.Fields[SeriesNumberFieldName].Value = book.SeriesNumeber;
                bookItem.Fields[DescriptionFieldName].Value = HtmlHelper.RemoveHtmlTags(book.Description, new List<string> { "div", "span" });
                bookItem.Fields[PublishStatusFieldName].Value = book.PublishStatus.ToString();
                if (book.Dimensions != null)
                {
                    bookItem.Fields[HeightFieldName].Value = book.Dimensions.Height;
                    bookItem.Fields[WidthFieldName].Value = book.Dimensions.Width;
                }
                bookItem.Fields[PriceFieldName].Value = book.Price;
                bookItem.Fields[WeightFieldName].Value = book.Weight;
                ((DateField)bookItem.Fields[PubDateFieldName]).Value = DateUtil.ToIsoDate(book.PubDate);

                bookItem.Fields[MediaFileFieldName].Value = book.MediaFileLink;
                bookItem.Fields[ImprintFieldName].Value = book.ImprintName;

                List<string> authorsList = new List<string>();
                List<string> illustratorsList = new List<string>();
                string contributorWithSequence = string.Empty;
                if (book.Authors != null && book.Authors.Count > 0)
                {
                    foreach (var author in book.Authors)
                    {
                        authorsList.Add(author.SitecoreID);
                        contributorWithSequence += string.Format("{0}(Author-{1})", author.SitecoreID, author.Sequence);
                    }
                    if (authorsList.Count > 0)
                    {
                        MultilistField ald = (MultilistField)bookItem.Fields[AuthorFieldName];
                        ald.Value = string.Join("|", authorsList);
                    }
                }

                if (book.Illustrators != null && book.Illustrators.Count > 0)
                {
                    foreach (var illustrator in book.Illustrators)
                    {
                        illustratorsList.Add(illustrator.SitecoreID);
                        contributorWithSequence += string.Format("{0}(Illustrator-{1})", illustrator.SitecoreID, illustrator.Sequence);
                    }

                    if (illustratorsList.Count > 0)
                    {
                        MultilistField ild = (MultilistField)bookItem.Fields[IllustratorsFieldName];
                        ild.Value = string.Join("|", illustratorsList);
                    }
                }
                bookItem.Fields[ContributorSequenceFieldName].Value = contributorWithSequence;
                if (book.Category != null)
                {
                    bookItem.Fields[MainBICCategoryFieldName].Value = book.Category.MainBICCategoryName;
                }
                bookItem.Fields[BICCategoriesFieldName].Value = string.Join("|", book.BICCategories);

                string mainHGCategory = string.Empty;
                List<string> hGCategories = new List<string>();
                List<string> HGCategories = this.GenerateHGCategories(book, ref mainHGCategory, ref hGCategories);
                if (HGCategories != null && HGCategories.Count > 0)
                {
                    MultilistField ild = (MultilistField)bookItem.Fields[CategoryFieldName];
                    ild.Value = string.Join("|", HGCategories);
                }

                List<string> authors = new List<string>();
                if (book.Authors != null && book.Authors.Count > 0)
                {
                    foreach (var author in book.Authors)
                    {
                        if (!string.IsNullOrWhiteSpace(author.PersonName))
                        {
                            authors.Add(author.PersonName);
                        }
                        else if (!string.IsNullOrWhiteSpace(author.CorporateName))
                        {
                            authors.Add(author.CorporateName);
                        }
                    }
                }
                string mainAuthorName = string.Empty;
                if (book.Authors != null && book.Authors.Count >= 1)
                {
                    var mainAuthor = book.Authors.SingleOrDefault(a => a.Sequence == "1");
                    mainAuthorName = mainAuthor == null ? "" : mainAuthor.PersonName;
                    if (mainAuthor != null)
                    {
                        mainAuthorName = mainAuthor.PersonName;
                    }
                    else if (book.Authors.FirstOrDefault().CorporateName != null && !string.IsNullOrWhiteSpace(book.Authors.FirstOrDefault().CorporateName))
                    {
                        mainAuthorName = book.Authors.FirstOrDefault().CorporateName;
                    }

                }
                bookItem.Fields["MetaTitle"].Value = string.Format("{0} by {1} | {2}",
                    book.Title.TitleText,
                    mainAuthorName,
                    "Hardie Grant Publishing");

                bookItem.Fields["MetaDescription"].Value = string.Format("{0}{1} by {2}{3}{4}. Hardie Grant Publishing.",
                    book.Title.TitleText,
                    string.IsNullOrWhiteSpace(book.Title.SubTitle) ? "" : ": " + book.Title.SubTitle,
                    authors.Count() > 0 ? string.Join(", ", authors) + ". " : "",
                    book.Publisher == null ? "" :
                    book.Publisher.GroupItem == null ? "" : book.Publisher.GroupItem["Name"],
                    mainHGCategory
                    );
                bookItem.Fields["MetaKeywords"].Value = string.Format("{0},{1}{2}{3}{4}{5}, Hardie Grant Publishing",
                    book.Title.TitleText,
                    string.IsNullOrWhiteSpace(book.Title.SubTitle) ? "" : book.Title.SubTitle + ", ",
                    authors.Count() > 0 ? string.Join(", ", authors) + ", " : "",
                    string.IsNullOrWhiteSpace(mainHGCategory) ? "" : mainHGCategory + ",",
                    hGCategories.Count() > 0 ? string.Join(", ", hGCategories) + ", " : "",
                    book.Publisher == null ? "" :
                    book.Publisher.GroupItem == null ? "" : book.Publisher.GroupItem["Name"]);
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to populate data for book " + book.Title.TitleText + "into item" + bookItem.ID.ToString(), ex);
            }
            finally
            {
                bookItem.Editing.EndEdit();

                BooksImportLog.Info(string.Format("Populated data for book [{0}] with item ID: {1}", book.Title.TitleText, bookItem != null ? bookItem.ID.ToString() : ""));
            }
        }

        private List<string> GenerateHGCategories(Book book, ref string mainHGCategroy, ref List<string> hGCategories)
        {
            if (book.Publisher.GroupItem != null)
            {

                List<string> hgCategoriesIds = new List<string>();
                ChildList CategoriesItems = null;
                switch (book.Publisher.GroupItem["Name"])
                {
                    case "Hardie Grant Books":
                        CategoriesItems = masterDb.GetItem(HGBCategoriesFolderItemId).Children;
                        break;

                    case "Hardie Grant Travel":
                        CategoriesItems = masterDb.GetItem(HGTCategoriesFolderItemId).Children;
                        break;

                    case "Hardie Grant Egmont":
                        CategoriesItems = masterDb.GetItem(HGECategoriesFolderItemId).Children;
                        break;
                }
                if (book.Publisher.GroupItem["Name"] == "Hardie Grant Books" || book.Publisher.GroupItem["Name"] == "Hardie Grant Travel")
                {
                    foreach (string BICCategory in book.BICCategories)
                    {
                        string hgCategory = this.GetHGItemIDByBICCategory(BICCategory, hgCategoriesIds, CategoriesItems);
                        if (!string.IsNullOrWhiteSpace(hgCategory))
                            hGCategories.Add(hgCategory);
                    }
                    if (book.Category != null)
                    {
                        if (!string.IsNullOrWhiteSpace(book.Category.MainBICCategoryName))
                        {
                            mainHGCategroy = this.GetHGItemIDByBICCategory(book.Category.MainBICCategoryName, hgCategoriesIds, CategoriesItems);
                            if (!string.IsNullOrWhiteSpace(mainHGCategroy))
                                hGCategories.Add(mainHGCategroy);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(book.MinAge) || !string.IsNullOrWhiteSpace(book.MaxAge))
                    {
                        int number;
                        int AudienceMinAge = -1;
                        int AudienceMaxAge = 9999;
                        if (!string.IsNullOrWhiteSpace(book.MinAge) && Int32.TryParse(book.MinAge, out number))
                        {
                            AudienceMinAge = System.Convert.ToInt32(book.MinAge);
                        }
                        if (!string.IsNullOrWhiteSpace(book.MaxAge) && Int32.TryParse(book.MaxAge, out number))
                        {
                            AudienceMaxAge = System.Convert.ToInt32(book.MaxAge);
                        }
                        if (AudienceMinAge == -1 && AudienceMaxAge != 9999)
                        {
                            AudienceMinAge = AudienceMaxAge;
                        }
                        if (AudienceMinAge != -1 && AudienceMaxAge == 9999)
                        {
                            AudienceMaxAge = AudienceMinAge;
                        }
                        foreach (Item item in CategoriesItems)
                        {
                            if ((System.Convert.ToInt32(item.Fields["MinAge"].Value) <= AudienceMinAge && System.Convert.ToInt32(item.Fields["MaxAge"].Value) >= AudienceMaxAge)
                                ||
                                (System.Convert.ToInt32(item.Fields["MinAge"].Value) <= AudienceMaxAge && System.Convert.ToInt32(item.Fields["MaxAge"].Value) >= AudienceMaxAge))
                            {
                                string hgCategory = item["Name"];
                                if (!string.IsNullOrWhiteSpace(hgCategory))
                                {
                                    hGCategories.Add(hgCategory);
                                }
                                hgCategoriesIds.Add(item.ID.ToString());
                            }
                        }
                    }
                }


                return hgCategoriesIds.Distinct().ToList();
            }
            return null;
        }

        private string GetHGItemIDByBICCategory(string BICCateName, List<string> hgCategoriesIds, ChildList CategoriesItems)
        {
            string cateName = string.Empty;
            var BICCategorySearchItem = OnixBicCategoriesSearchService.GetBICCategoryItemByCode(BICCateName);
            if (BICCategorySearchItem != null)
            {
                Item BICCategoryItem = masterDb.GetItem(BICCategorySearchItem.ItemId);
                if (BICCategoryItem != null)
                {
                    Item item = CategoriesItems.Where(i => i.Fields["Mapped BIC Categories"].Value.Contains(BICCategoryItem.ID.ToString())).FirstOrDefault();
                    if (item != null)
                    {
                        cateName = item["Name"];
                        hgCategoriesIds.Add(item.ID.ToString());
                    }
                }
            }
            return cateName;
        }
    }
}