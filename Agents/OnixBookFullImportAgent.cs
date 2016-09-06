using HG.Coprorate.Firebrand.CustomLogs;
using HG.Coprorate.Firebrand.Helpers;
using HG.Coprorate.Firebrand.Models;
using HG.Corporate.Core;
using Sitecore.Configuration;
using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace HG.Coprorate.Firebrand.Agents
{
    public class OnixBookFullImportAgent
    {
        private static bool importRunning;
        private bool isImportFullVersion = true;
        private string xmlFilePathLocal = "HardieGrant.xml";

        private string FTPFullVersionArchivesFolderName = Sitecore.Configuration.Settings.GetSetting("FTPFullVersionArchivesFolderName");

        public void Run()
        {
            if (DateTime.Now.Day != 1 && !Sitecore.Context.User.IsAdministrator)
                return;

            // This is pointless as this isn't a singleton - a new instance will be created by the Task runner each time.
            if (!importRunning)
            {
                Process();
            }
            else
            {
                BooksImportLog.Info("/*** Book Data Full Import skipped as the previous import has yet to complete : " + DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss") + " ***\\");
            }
        }

        public void Process()
        {
            importRunning = true;
            try
            {
                BookXmlTransformHelper _xmlHelper = new BookXmlTransformHelper();
                BookSitecoreItemCreationHelper _bookSitecoreHelper = new BookSitecoreItemCreationHelper();
                List<string> newFtpFiles = new List<string>();
                bool loadFromLocal;
                bool.TryParse(Sitecore.Configuration.Settings.GetSetting("LoadOnixFeedFromLocal"), out loadFromLocal);
                if (loadFromLocal)
                {
                    string localXmlFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/upload/" + xmlFilePathLocal);
                    newFtpFiles.Add(localXmlFilePath);
                }
                else
                {
                    List<string> ftpFiles = _xmlHelper.GetListOfExistingFilesNames();
                    newFtpFiles = _xmlHelper.GetNewFtpFiles(isImportFullVersion, ftpFiles);
                }

                if (newFtpFiles != null && newFtpFiles.Count > 0)
                {
                    foreach (string newFtpFile in newFtpFiles)
                    {
                        BooksImportLog.Info("----------------START BOOK FULL IMPORT SERVICE---------------");

                        BooksImportLog.Info("Start getting books' data.....");
                        XmlNodeList data = _xmlHelper.ReadBookDataFromXmlFeed(newFtpFile, loadFromLocal);
                        List<Book> books = _xmlHelper.ConvertXmlNodeListToObjects(data);
                        BooksImportLog.Info("End getting books' data.....");

                        BooksImportLog.Info("Start creating books' items in Sitecore.....");
                        _bookSitecoreHelper.ImportBookItemsToSitecore(books);
                        BooksImportLog.Info("End creating books' items in Sitecore.....");
                        //_xmlHelper.DeleteProcessFtpFiles(newFtpFile);
                        //_xmlHelper.MoveProcessedFtpFiles(newFtpFile, FTPFullVersionArchivesFolderName);
                    }
                }
                

                

                BooksImportLog.Info("Start updating authors' active statuses.....");
                AuthorSitecoreItemCreationHelper.UpdateAuthorActiveStatus();
                BooksImportLog.Info("End updating authors' active statuses.....");
                BooksImportLog.Info("-----------------END BOOK FULL IMPORT SERVICE---------------");
            }
            catch (Exception ex)
            {
                BooksImportLog.Error(ex.Message);
            }
            importRunning = false;
        }

        private void RebuidIndex()
        {
            using (new DatabaseSwitcher(Sitecore.Data.Database.GetDatabase("master")))
            {
                Sitecore.ContentSearch.ContentSearchManager.GetIndex("corp_onix_book").Rebuild();
                Sitecore.ContentSearch.ContentSearchManager.GetIndex("corp_onix_author").Rebuild();
                Sitecore.ContentSearch.ContentSearchManager.GetIndex("corp_onix_publisher").Rebuild();
                Sitecore.ContentSearch.ContentSearchManager.GetIndex("corp_onix_series").Rebuild();
            }
        }


        private void GenerateBICCategoryList()
        {
            //To generate BIC Categories list
            BICCategorySitecoreItemHelper _bICCategorySitecoreItehelper = new BICCategorySitecoreItemHelper();
            BICCategoryExcelTransformHelper _bICCategoryExcelTransformHelper = new BICCategoryExcelTransformHelper();
            var list = _bICCategoryExcelTransformHelper.ReadExcelFile();
            _bICCategorySitecoreItehelper.ImportBICCategoriesItemsToSitecore(list);
        }


        private void GeneratFormatsList()
        {
            //To genereate Formats list
            FormatSitecoreItemCreationHelper _formatSitecoreItemCreationHelper = new FormatSitecoreItemCreationHelper();
            FormatListExcelTransformHelper _formatListExcelTransformHelper = new FormatListExcelTransformHelper();
            var list = _formatListExcelTransformHelper.ReadExcelFile();
            _formatSitecoreItemCreationHelper.ImportFormatItemsToSitecore(list);
        }
    }
}