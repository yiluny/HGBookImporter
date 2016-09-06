using HG.Coprorate.Firebrand.CustomLogs;
using HG.Coprorate.Firebrand.Models;
using HG.Corporate.Core;
using HG.Corporate.Core.Search.SearchServices;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using HG.Corporate.Core.Helpers;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class AuthorSitecoreItemCreationHelper
    {
        private string authorItemTemplateId = SiteSettings.AppSettingsCollection["AuthorBranchTemplateId"];
        private string authorFolderItemId = SiteSettings.AppSettingsCollection["AuthorBucketItemId"];
        private static string sitecoreMasterDatabaseName = "master";
        private static Database masterDb { get { return Sitecore.Configuration.Factory.GetDatabase(sitecoreMasterDatabaseName); } }

        /// <summary>
        /// Import Authors' data into sitecore and create new items
        /// </summary>
        /// <param name="authos">Authors obejcts generated from XML</param>
        public void ImportAuthorsItemsToSitecore(ContributorType contributorType, ref Book book, ref List<string> ExistingAuthors, Item publisherGroupOwner)
        {
            List<Author> contributors = new List<Author>();
            contributors = contributorType == ContributorType.Author ? book.Authors : book.Illustrators;
            for (int i = 0; i < contributors.Count; i++)
            {
                if (!ExistingAuthors.Contains(contributors[i].DisplayName.Replace(" ", "-")))
                {
                    ExistingAuthors.Add(contributors[i].DisplayName.Replace(" ", "-"));
                    ID id = this.CreateAuthorItemInSitecore(contributors[i], contributorType, publisherGroupOwner);
                    if (!ID.IsNullOrEmpty(id))
                    {
                        contributors[i].SitecoreID = id.ToString();
                    }
                }
                else
                {
                    Item authorItem = masterDb.GetItem(authorFolderItemId).Axes.GetDescendants().FirstOrDefault(c => c.Name == contributors[i].DisplayName.Replace(" ", "-"));
                    BooksImportLog.Info(string.Format("Updating existing author [{0}] with item ID: {1}", contributors[i].DisplayName, authorItem != null ? authorItem.ID.ToString() : ""));

                    this.ExtractDataFromAuthorObject(contributors[i], contributorType, authorItem, publisherGroupOwner);
                    ID id = authorItem.ID;
                    contributors[i].SitecoreID = id.ToString();
                    if (!authorItem.Fields["ContributorType"].Value.Contains(contributorType.ToString()))
                    {
                        authorItem.Editing.BeginEdit();
                        try
                        {
                            authorItem.Fields["ContributorType"].Value += string.Format(",{0}", contributorType.ToString());
                        }
                        catch (Exception ex)
                        {
                            BooksImportLog.Error("There is an error when trying to populate data for author " + contributors[i].DisplayName + "into item" + authorItem.ID.ToString(), ex);
                        }
                        finally
                        {
                            authorItem.Editing.EndEdit();

                            BooksImportLog.Info(string.Format("Populated data for author [{0}] with item ID: {1}", contributors[i].DisplayName, authorItem != null ? authorItem.ID.ToString() : ""));
                        }
                    }
                }
                Thread.Sleep(100);
            }
            if (contributorType == ContributorType.Author)
            {
                book.Authors = contributors;
            }
            else
            {
                book.Illustrators = contributors;
            }
        }

        /// <summary>
        /// Create a sigle author item in sitecore
        /// </summary>
        /// <param name="author">Author object generated from XML</param>
        private ID CreateAuthorItemInSitecore(Author author, ContributorType contributorType, Item publisherGroupOwner)
        {
            try
            {
                using (new SecurityDisabler())
                {
                    Item authorItem = SitecoreCreationHelper.CreateBranchItem(author.DisplayName, masterDb, authorItemTemplateId, authorFolderItemId);
                    BooksImportLog.Info(string.Format("Created new author [{0}] with item ID: {1}", author.PersonNameInverted, authorItem != null ? authorItem.ID.ToString() : ""));

                    this.ExtractDataFromAuthorObject(author, contributorType, authorItem, publisherGroupOwner);
                    
                    return authorItem.ID;
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to import author " + author.PersonNameInverted, ex);
                return null;
            }
        }

        private void ExtractDataFromAuthorObject(Author author, ContributorType contributorType, Item authorItem, Item publisherGroupOwner)
        {
            this.populateAuthorItemData(author, authorItem, contributorType, publisherGroupOwner);

            //publish author item
            SitecorePublishHelper sp = new SitecorePublishHelper();
            sp.PublishItem(authorItem, false);
        }

        /// <summary>
        /// populate author data with the author object into sitecore
        /// </summary>
        /// <param name="author">Author object generated from XML</param>
        /// <param name="authorItem">Sitecore author item</param>
        private void populateAuthorItemData(Author author, Item authorItem, ContributorType contributorType, Item publisherGroupOwner)
        {
            authorItem.Editing.BeginEdit();
            try
            {
                authorItem.Fields["KeyNames"].Value = author.KeyNames;
                authorItem.Fields["NamesBeforeKey"].Value = author.NamesBeforeKey;
                authorItem.Fields["PersonName"].Value = author.PersonName;
                authorItem.Fields["PersonNameInverted"].Value = author.PersonNameInverted;
                authorItem.Fields["BiographicalNote"].Value = HtmlHelper.RemoveHtmlTags(author.BiographicalNote, new List<string>{"div", "span"});
                if (!((Sitecore.Data.Fields.CheckboxField)authorItem.Fields["DisableOverridePublishingGroupOwner"]).Checked)
                {
                    if (publisherGroupOwner!= null)
                        authorItem.Fields["PublishingGroupOwner"].Value = publisherGroupOwner.ID.ToString();
                }
                authorItem.Fields["ContributorType"].Value = contributorType.ToString();
                authorItem.Fields["CorporateName"].Value = author.CorporateName;
                authorItem.Fields["MetaTitle"].Value = string.Format("{0} | Hardie Grant Publishing", 
                    string.IsNullOrWhiteSpace(author.PersonName) ? author.CorporateName : author.PersonName);
                authorItem.Fields["MetaDescription"].Value = string.Format("{0} | {1} Hardie Grant Publishing",
                    string.IsNullOrWhiteSpace(author.PersonName) ? author.CorporateName : author.PersonName,
                    string.IsNullOrWhiteSpace(author.BiographicalNote) ? "" : getFormattedAuthorDescription(author.BiographicalNote) + "|");
                authorItem.Fields["MetaKeywords"].Value = string.Format("{0}, {1}, Hardie Grant Publishing", 
                    string.IsNullOrWhiteSpace(author.PersonName) ? author.CorporateName : author.PersonName,
                    publisherGroupOwner == null ? "" : publisherGroupOwner["Name"]);
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to populate data for author " + author.DisplayName + "into item" + authorItem.ID.ToString(), ex);
            }
            finally  
            {
                authorItem.Editing.EndEdit();

                BooksImportLog.Info(string.Format("Updated data for author [{0}] with item ID: {1}", author.DisplayName, authorItem != null ? authorItem.ID.ToString() : ""));
            }
        }

        private static string getFormattedAuthorDescription(string originalText)
        {
            int length = 80;
            string cleanText = Regex.Replace(originalText, "<.*?>", String.Empty);
            string shortText = cleanText.Substring(0, Math.Min(cleanText.Length, length));

            while (!shortText.EndsWith(" "))
            {
                length++;
                shortText = cleanText.Substring(0, Math.Min(cleanText.Length, length));
            }
            return shortText;

        }

        public static void UpdateAuthorActiveStatus()
        {
            var allAuthors = OnixAuthorsSearchService.GetAllAuthorSearchItems();
            var allBooks = OnixBooksSearchService.GetBooks();

            foreach (var Author in allAuthors)
            {
                string authorSitecoreId = Author.ItemId.ToString();
                var IfHasRelatedActiveBooks = allBooks.Where(b => ((b.Authors != null && b.Authors.Contains(authorSitecoreId)) || (b.Illustrators != null && b.Illustrators.Contains(authorSitecoreId)))
                    && (b.PublishStatus == "Forthcoming" || b.PublishStatus == "Active")).Any();
                if (IfHasRelatedActiveBooks)
                {
                    UpdateActiveStatusField(authorSitecoreId, IfHasRelatedActiveBooks);
                }
            }
        }

        private static void UpdateActiveStatusField(string sitecoreId, bool isActive)
        {
            Item authorItem = masterDb.GetItem(sitecoreId);
            authorItem.Editing.BeginEdit();
            try
            {
                ((CheckboxField)authorItem.Fields["HasActivedBooks"]).Checked = isActive;
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to set active status for author " + sitecoreId + "into item" + authorItem.ID.ToString(), ex);
            }
            finally
            {
                authorItem.Editing.EndEdit();
            }
        }
    }
}