using HG.Coprorate.Firebrand.CustomLogs;
using HG.Coprorate.Firebrand.Helpers;
using HG.Coprorate.Firebrand.Models;
using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Xml;

namespace HG.Coprorate.Firebrand.Agents
{
    public class OnixChangedBookImportAgent
    {
        private static bool importRunning;
        private bool isImportFullVersion = false;
        private string FTPChangingVersionArchivesFolderName = Sitecore.Configuration.Settings.GetSetting("FTPChangingVersionArchivesFolderName");

        public void Run()
        {
            // This is pointless as this isn't a singleton - a new instance will be created by the Task runner each time.
            if (!importRunning)
            {
                Process();
            }
            else
            {
                BooksImportLog.Info("/*** Book Data Changed file Import skipped as the previous import has yet to complete : " + DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss") + " ***\\");
            }
        }

        public void Process()
        {
            importRunning = true;
            try
            {
                BookXmlTransformHelper _xmlHelper = new BookXmlTransformHelper();
                BookSitecoreItemCreationHelper _bookSitecoreHelper = new BookSitecoreItemCreationHelper();

                List<string> ftpFiles = _xmlHelper.GetListOfExistingFilesNames();
                List<string> newFtpFiles = _xmlHelper.GetNewFtpFiles(isImportFullVersion, ftpFiles);
                if (newFtpFiles != null && newFtpFiles.Count > 0)
                {
                    BooksImportLog.Info("----------------START BOOK CHANGED FILE IMPORT SERVICE---------------");
                    foreach (string newFtpFile in newFtpFiles)
                    {
                        BooksImportLog.Info("Start getting books' data.....");
                        XmlNodeList data = _xmlHelper.ReadBookDataFromXmlFeed(newFtpFile, false);
                        List<Book> books = _xmlHelper.ConvertXmlNodeListToObjects(data);
                        BooksImportLog.Info("End getting books' data.....");

                        BooksImportLog.Info("Start creating books' items in Sitecore.....");
                        _bookSitecoreHelper.ImportBookItemsToSitecore(books);
                        BooksImportLog.Info("End creating books' items in Sitecore.....");
                        //_xmlHelper.DeleteProcessFtpFiles(newFtpFile);
                        //_xmlHelper.MoveProcessedFtpFiles(newFtpFile, FTPChangingVersionArchivesFolderName);
                    }
                }

                BooksImportLog.Info("Start updating authors' active statuses.....");
                AuthorSitecoreItemCreationHelper.UpdateAuthorActiveStatus();
                BooksImportLog.Info("End updating authors' active statuses.....");
                BooksImportLog.Info("-----------------END BOOK CHANGED FILE IMPORT SERVICE---------------");
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
    }
}