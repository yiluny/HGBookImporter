using HG.Coprorate.Firebrand.CustomLogs;
using HG.Coprorate.Firebrand.Models;
using HG.Corporate.Core;
using Sitecore.Data;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using HG.Corporate.Core.Search.SearchServices;
using Sitecore.Configuration;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class BookXmlTransformHelper
    {
        private string ExtractingElmentTagName = "Product";

        private string productIdentifierNodeName = "ProductIdentifier";
        private string productIDTypeNodeName = "ProductIDType";
        private string idValueNodeName = "IDValue";
        private string ISBNRoleCode = "15";

        private string TitleNodeName = "Title";
        private string TitleTextNodeName = "TitleText";
        private string SubtitleNodeName = "Subtitle";

        private string ContributorNodeName = "Contributor";
        private string SequenceNumberNodeName = "SequenceNumber";
        private string ContributorRoleNodeName = "ContributorRole";
        private string AuthorRoleCode = "A01";
        private string IllustratorRoleCode = "A12";
        private string PersonNameNodeName = "PersonName";
        private string PersonNameInvertedNodeName = "PersonNameInverted";
        private string BiographicalNote = "BiographicalNote";
        private string NamesBeforeKey = "NamesBeforeKey";
        private string KeyNames = "KeyNames";

        private string ImprintNodeName = "Imprint";
        private string NameCodeTypeNodeName = "NameCodeType";
        private string ImprintNameNodeName = "ImprintName";
        private string ImprintRoleCode = "01";

        private string PublisherNodeName = "Publisher";
        private string PublishingRoleNodeName = "PublishingRole";
        private string PublisherNameNodeName = "PublisherName";
        private string PublishingRoleCode = "01";

        private string SerieNodeName = "Series";
        private string SerieNameNodeName = "TitleOfSeries";
        private string SerieNumberNodeName = "NumberWithinSeries";

        private string PageNumberNodeName = "NumberOfPages";

        private string AudienceRangeNodeName = "AudienceRange";
        private string AudienceRangePrecisionNodeName = "AudienceRangePrecision";
        private string AudienceRangeValueName = "AudienceRangeValue";
        private string AudienceMinAgeNodeCode = "03";
        private string AudienceMaxAgeNodeCode = "04";

        private string OtherTextNodeName = "OtherText";
        private string TextTypeCodeNodeName = "TextTypeCode";
        private string MainTextTypeCode = "01";
        private string MainTextNodeName = "Text";

        private string PubishtionDateNodeName = "PublicationDate";

        private string MeasureNodeName = "Measure";
        private string MeasureTypeCodeNodeName = "MeasureTypeCode";
        private string MeasurementName = "Measurement";
        private string HeightTypeCode = "01";
        private string WidthCode = "02";
        private string WeightTypeNode = "08";
        private string MeasureUnitCodeNodeName = "MeasureUnitCode";
        private string MeasureUnitCodeWidth = "mm";
        private string MeasureUnitCodeWeight = "gr";
        private string FormatNodeName = "ProductForm";

        private string PriceNodeName = "Price";
        private string PriceAmountNodeName = "PriceAmount";
        private string CurrencyCodeNodeName = "CurrencyCode";
        private string CurrencyCode = "AUD";

        private string BICMainSubjectNodeName = "BICMainSubject";
        private string SubjectNodeName = "Subject";
        private string SubjectSchemeIdentifierNodeName = "SubjectSchemeIdentifier";
        private string SubjectCodeNodeName = "SubjectCode";
        private string BICCategoryIdentifierCodeNodeName = "12";

        private string ftpServerPath = Sitecore.Configuration.Settings.GetSetting("OnixBookFeedFtpServerPath");
        private string userId = Sitecore.Configuration.Settings.GetSetting("OnixBookFeedFtpUserId");
        private string password = Sitecore.Configuration.Settings.GetSetting("OnixBookFeedFtpPassword");

        //private string localXmlFilePath = "HardieGrant_20160314143959_complete_onix21.xml";

        private string OnixBookCoverImageBaseUrl = SiteSettings.AppSettingsCollection["OnixBookCoverImageBaseUrl"];
        private string OnixBookCoverImageSize = SiteSettings.AppSettingsCollection["OnixBookCoverImageSize"];

        private Item _HGBPublisherGroupItem { get; set; }

        private string sitecoreMasterDatabaseName = "master";

        private Database masterDb
        {
            get
            {
                return Sitecore.Configuration.Factory.GetDatabase(sitecoreMasterDatabaseName);
            }
        }

        private Item HGBPublisherGroupItem
        {
            get
            {
                if (_HGBPublisherGroupItem == null)
                {
                    string HGBPublisherGroupID = SiteSettings.AppSettingsCollection["HGBPublisherGroupID"];
                    _HGBPublisherGroupItem = masterDb.GetItem(HGBPublisherGroupID);
                }
                return _HGBPublisherGroupItem;
            }
        }

        private Item _HGEPublisherGroupItem { get; set; }

        private Item HGEPublisherGroupItem
        {
            get
            {
                if (_HGEPublisherGroupItem == null)
                {
                    string HGEPublisherGroupID = SiteSettings.AppSettingsCollection["HGEPublisherGroupID"];
                    _HGEPublisherGroupItem = masterDb.GetItem(HGEPublisherGroupID);
                }
                return _HGEPublisherGroupItem;
            }
        }

        private Item _HGTPublisherGroupItem { get; set; }

        private Item HGTPublisherGroupItem
        {
            get
            {
                if (_HGTPublisherGroupItem == null)
                {
                    string HGTPublisherGroupID = SiteSettings.AppSettingsCollection["HGTPublisherGroupID"];
                    _HGTPublisherGroupItem = masterDb.GetItem(HGTPublisherGroupID);
                }
                return _HGTPublisherGroupItem;
            }
        }

        public BookXmlTransformHelper()
        {
        }

        /// <summary>
        /// Read Data from XML
        /// </summary>
        /// <returns>Books XML node list</returns>
        public XmlNodeList ReadBookDataFromXmlFeed(string xmlFilePath, bool loadFromLocal)
        {
            string ftpServerXmlFilePath = string.Format("{0}/{1}", ftpServerPath, xmlFilePath);
            try
            {
                BooksImportLog.Info("Start reading book data from " + ftpServerXmlFilePath);
                XmlDataDocument xmldoc = new XmlDataDocument();
                XmlNodeList xmlnode;

                if (loadFromLocal)
                    GenerateXmlDocFromLocalFile(xmlFilePath, xmldoc);
                else
                    GenerateXmlDocFromFtp(ftpServerXmlFilePath, xmldoc);

                xmlnode = xmldoc.GetElementsByTagName(ExtractingElmentTagName);

                BooksImportLog.Info("End reading book data....");
                BooksImportLog.Info("Books' nodes got from xml: " + xmlnode.Count);
                return xmlnode;
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to geting data from " + ftpServerXmlFilePath, ex);
            }
            return null;
        }

        private void GenerateXmlDocFromLocalFile(string xmlFilePath, XmlDataDocument xmldoc)
        {
            FileStream fs = new FileStream(xmlFilePath, FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);
        }

        private void GenerateXmlDocFromFtp(string ftpServerXmlFilePath, XmlDataDocument xmldoc)
        {
            var reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerXmlFilePath));
            reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            reqFTP.UseBinary = true;
            reqFTP.UsePassive = true;
            reqFTP.Credentials = new NetworkCredential(userId, password);
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            Stream ftpStream = response.GetResponseStream();
            //if (response != null &&
            // response.GetResponseStream() != null &&
            // response.GetResponseStream().CanRead)
            //{
            //    response.Close();
            //}

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            readerSettings.DtdProcessing = DtdProcessing.Ignore;
            XmlReader xmlReader = XmlReader.Create(ftpStream, readerSettings);
            xmldoc.Load(xmlReader);
        }

        public List<string> GetNewFtpFiles(bool isImportFullVersion, List<string> ftpFiles)
        {
            if (isImportFullVersion)
            {
                ftpFiles = ftpFiles.Where(f => f.Contains("complete_onix21")).ToList();
            }
            else
            {
                ftpFiles = ftpFiles.Where(f => f.Contains("onix21") && !f.Contains("complete")).ToList();
            }
            if (ftpFiles.Count > 0)
            {
                List<DateTime> FilesModifiedDateTimes = new List<DateTime>();
                foreach (var ftpFile in ftpFiles)
                {
                    Regex regex = new Regex(@"\d+");
                    Match match = regex.Match(ftpFile);
                    if (match.Success)
                    {
                        if (isImportFullVersion)
                        {
                            FilesModifiedDateTimes.Add(DateTime.ParseExact(match.Groups[0].Value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            FilesModifiedDateTimes.Add(DateTime.ParseExact(match.Groups[0].Value, "yyyyMMddHHmm", CultureInfo.InvariantCulture));
                        }
                    }
                }
                if (FilesModifiedDateTimes.Count > 0)
                {
                    List<string> results = new List<string>();
                    var LatestFileDateTime = FilesModifiedDateTimes.Max();
                    var SameDayFilesDateTime = FilesModifiedDateTimes.Where(f => f.Date == LatestFileDateTime.Date).ToList();
                    string dateTimeFormat = string.Empty;

                    if (isImportFullVersion)
                    {
                        dateTimeFormat = "yyyyMMddHHmmss";
                    }
                    else
                    {
                        dateTimeFormat = "yyyyMMddHHmm";
                    }

                    foreach (var SameDayFileDateTime in SameDayFilesDateTime)
                    {
                        var res = ftpFiles.SingleOrDefault(f => f.Contains(SameDayFileDateTime.ToString(dateTimeFormat)));
                        if (res != null)
                        {
                            results.Add(res);
                        }
                    }
                    return results;
                }
            }
            return null;
        }

        public void MoveProcessedFtpFiles(string processedFile, string newDirectory)
        {
            try
            {
                var req = (FtpWebRequest)WebRequest.Create(ftpServerPath + "/" + processedFile);
                req.Credentials = new NetworkCredential(userId, password);

                req.Method = WebRequestMethods.Ftp.Rename;
                req.RenameTo = newDirectory + "/" + processedFile;
                req.GetResponse().Close();
            }
            catch (Exception ex)
            {
                BooksImportLog.Error(string.Format("Move the ftp file {0} to {1} folder failed.", processedFile, newDirectory), ex);
            }
        }

        public void DeleteProcessFtpFiles(string processedFile)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpServerPath + "/" + processedFile);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(userId, password);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    BooksImportLog.Info(string.Format("Delete the ftp file: {0} with status {1}", processedFile, response.StatusDescription));
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error(string.Format("Delete the ftp file {0} failed.", processedFile), ex);
            }
        }

        public List<string> GetListOfExistingFilesNames()
        {
            System.Net.FtpWebRequest ftpRequest = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(ftpServerPath);
            ftpRequest.Credentials = new System.Net.NetworkCredential(userId, password);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            System.Net.FtpWebResponse response = (System.Net.FtpWebResponse)ftpRequest.GetResponse();
            System.IO.StreamReader streamReader = new System.IO.StreamReader(response.GetResponseStream());

            List<string> directories = new List<string>();

            string line = streamReader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                directories.Add(line);
                line = streamReader.ReadLine();
            }

            streamReader.Close();
            return directories;
        }

        public void RemoveDocTypeLine(Stream stream)
        {
            string line = null;
            int line_number = 0;
            int line_to_delete = 2;

            using (StreamReader reader = new StreamReader(stream))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        line_number++;

                        if (line_number == line_to_delete)
                            continue;

                        writer.WriteLine(line);
                    }
                }
            }
        }

        /// <summary>
        /// Convert books xml nodes into books objects
        /// </summary>
        /// <param name="data">Books XML node list</param>
        /// <returns>A list of books objects</returns>
        public List<Book> ConvertXmlNodeListToObjects(XmlNodeList data)
        {
            try
            {
                BooksImportLog.Info("Start converting book data... ");

                var listOfNodes = new List<XmlNode>(data.Cast<XmlNode>());
                List<Book> books = new List<Book>();
                foreach (XmlNode listOfNode in listOfNodes)
                {
                    Book book = new Book();

                    //Extract book data from nodelist
                    this.ExtractISBN(listOfNode, ref book);

                    //Only add the book if the data is new
                    bool doNotUseHash;
                    bool.TryParse(Settings.GetSetting("OverrideOnixHashCheck"), out doNotUseHash);

                    var hashCode = CryptoHelper.GenerateMd5Hash(listOfNode.InnerText);
                    if (doNotUseHash || !OnixBooksSearchService.BookHasMatchingHashCode(book.ISBN, hashCode))
                    {
                        book.HashCode = hashCode;
                        this.ExtractTitle(listOfNode, ref book);
                        this.ExtractAuthor(listOfNode, ref book);
                        this.ExtractImprint(listOfNode, ref book);
                        this.ExtractPublisher(listOfNode, ref book);
                        this.ExtractMediaFile(listOfNode, ref book);
                        this.ExtractSeries(listOfNode, ref book);
                        this.ExtractIllustrators(listOfNode, ref book);
                        this.ExtractPageNumber(listOfNode, ref book);
                        this.ExtractAgeRange(listOfNode, ref book);
                        this.ExtractDescription(listOfNode, ref book);
                        this.ExtractPublishtionDate(listOfNode, ref book);
                        this.ExtractDimensionsAndWeight(listOfNode, ref book);
                        this.ExtractPrice(listOfNode, ref book);
                        this.ExtractFormat(listOfNode, ref book);
                        this.ExtractBICMainSubject(listOfNode, ref book);
                        this.ExtractBICSubjects(listOfNode, ref book);
                        this.ExtractPublisingStatus(listOfNode, ref book);
                        books.Add(book);
                    }
                }

                BooksImportLog.Info("End converting book data... ");
                BooksImportLog.Info("Books' objects generated: " + books.Count);

                return books;
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("There is an error when trying to convert xml formatted book data to an object. book data: " + data.ToString(), ex);
            }
            return null;
        }

        private void ExtractISBN(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var ids = listOfNode.SelectNodes(productIdentifierNodeName);
                var listOfIds = new List<XmlNode>(ids.Cast<XmlNode>());
                var isbnNode = listOfIds.SingleOrDefault(i => i.SelectSingleNode(productIDTypeNodeName).InnerText == ISBNRoleCode);
                if (isbnNode != null)
                {
                    book.ISBN = isbnNode.SelectSingleNode(idValueNodeName).InnerText;
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract ISBN data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractTitle(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var titleNode = listOfNode.SelectSingleNode(TitleNodeName);
                var titleFieldNode = titleNode.SelectSingleNode(TitleTextNodeName);
                var subTitleNode = titleNode.SelectSingleNode(SubtitleNodeName);
                book.Title = new Title();
                if (titleNode != null)
                {
                    book.Title.TitleText = titleFieldNode.InnerText;
                }
                if (subTitleNode != null)
                {
                    book.Title.SubTitle = subTitleNode.InnerText;
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Title data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private string CorporateNameNodeName = "CorporateName";

        private void ExtractAuthor(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var contributors = listOfNode.SelectNodes(ContributorNodeName);
                var listOfContributors = new List<XmlNode>(contributors.Cast<XmlNode>());
                var authorsNode = listOfContributors.Where(i => i.SelectSingleNode(ContributorRoleNodeName).InnerText == AuthorRoleCode);

                if (authorsNode != null && authorsNode.Count() > 0)
                {
                    List<Author> authors = new List<Author>();
                    foreach (var authorNode in authorsNode)
                    {
                        var personNameNode = authorNode.SelectSingleNode(PersonNameNodeName);
                        var personNameInvertedNode = authorNode.SelectSingleNode(PersonNameInvertedNodeName);
                        var KeyNamesNode = authorNode.SelectSingleNode(KeyNames);
                        var NamesBeforeKeyNode = authorNode.SelectSingleNode(NamesBeforeKey);
                        var BiographicalNoteNode = authorNode.SelectSingleNode(BiographicalNote);
                        var CorporateNameNode = authorNode.SelectSingleNode(CorporateNameNodeName);
                        var SequenceNumberNode = authorNode.SelectSingleNode(SequenceNumberNodeName);
                        Author author = new Author();

                        if (CorporateNameNode != null)
                        {
                            author.CorporateName = CorporateNameNode.InnerText;
                        }
                        else
                        {
                            if (SequenceNumberNode != null)
                            {
                                author.Sequence = SequenceNumberNode.InnerText;
                            }
                            if (personNameNode != null)
                            {
                                author.PersonName = personNameNode.InnerText;
                            }
                            if (personNameInvertedNode != null)
                            {
                                author.PersonNameInverted = personNameInvertedNode.InnerText;
                            }
                            if (KeyNamesNode != null)
                            {
                                author.KeyNames = KeyNamesNode.InnerText;
                            }
                            if (NamesBeforeKeyNode != null)
                            {
                                author.NamesBeforeKey = NamesBeforeKeyNode.InnerText;
                            }
                            if (BiographicalNoteNode != null)
                            {
                                author.BiographicalNote = BiographicalNoteNode.InnerText;
                            }
                        }
                        authors.Add(author);
                    }
                    book.Authors = authors;
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Authors data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractImprint(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var Imprints = listOfNode.SelectNodes(ImprintNodeName);
                var listOfImprints = new List<XmlNode>(Imprints.Cast<XmlNode>());
                var imprintNode = listOfImprints.SingleOrDefault(i => i.SelectSingleNode(NameCodeTypeNodeName).InnerText == ImprintRoleCode);
                if (imprintNode != null)
                {
                    book.ImprintName = imprintNode.SelectSingleNode(ImprintNameNodeName).InnerText;
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Imprint data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractPublisher(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var publishers = listOfNode.SelectNodes(PublisherNodeName);
                var listOfPublishers = new List<XmlNode>(publishers.Cast<XmlNode>());
                var publisherNode = listOfPublishers.SingleOrDefault(i => i.SelectSingleNode(PublishingRoleNodeName).InnerText == PublishingRoleCode);
                if (publisherNode != null)
                {
                    book.Publisher = new Publisher()
                    {
                        Name = publisherNode.SelectSingleNode(PublisherNameNodeName).InnerText,
                    };
                    book.Publisher.GroupItem = book.Publisher.GetGroupItem(HGBPublisherGroupItem, HGEPublisherGroupItem, HGTPublisherGroupItem);
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Publisher data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractMediaFile(XmlNode listOfNode, ref Book book)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(book.ISBN))
                {
                    book.MediaFileLink = string.Format("{0}/{1}/{2}", OnixBookCoverImageBaseUrl, book.ISBN, OnixBookCoverImageSize);
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Media data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractPageNumber(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var pageNumberNode = listOfNode.SelectSingleNode(PageNumberNodeName);
                if (pageNumberNode != null)
                {
                    if (pageNumberNode != null)
                    {
                        book.PageNumber = pageNumberNode.InnerText;
                    }
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Page number data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractSeries(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var SeriesNode = listOfNode.SelectSingleNode(SerieNodeName);
                if (SeriesNode != null)
                {
                    Series series = new Series();
                    var SeriesNameNode = SeriesNode.SelectSingleNode(SerieNameNodeName);
                    if (SeriesNameNode != null)
                    {
                        series.Name = SeriesNameNode.InnerText;
                    }
                    var SeriesNumberNode = SeriesNode.SelectSingleNode(SerieNumberNodeName);
                    if (SeriesNumberNode != null)
                    {
                        book.SeriesNumeber = SeriesNumberNode.InnerText;
                    }

                    book.Series = series;
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Series data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractIllustrators(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var contributors = listOfNode.SelectNodes(ContributorNodeName);
                var listOfContributors = new List<XmlNode>(contributors.Cast<XmlNode>());
                var IllustratorsNode = listOfContributors.Where(i => i.SelectSingleNode(ContributorRoleNodeName).InnerText == IllustratorRoleCode);

                if (IllustratorsNode != null && IllustratorsNode.Count() > 0)
                {
                    List<Author> Illustrators = new List<Author>();
                    foreach (var IllustratorNode in IllustratorsNode)
                    {
                        var personNameNode = IllustratorNode.SelectSingleNode(PersonNameNodeName);
                        var personNameInvertedNode = IllustratorNode.SelectSingleNode(PersonNameInvertedNodeName);
                        var KeyNamesNode = IllustratorNode.SelectSingleNode(KeyNames);
                        var NamesBeforeKeyNode = IllustratorNode.SelectSingleNode(NamesBeforeKey);
                        var BiographicalNoteNode = IllustratorNode.SelectSingleNode(BiographicalNote);
                        var SequenceNumberNode = IllustratorNode.SelectSingleNode(SequenceNumberNodeName);

                        var CorporateNameNode = IllustratorNode.SelectSingleNode(CorporateNameNodeName);
                        Author illustrator = new Author();

                        if (CorporateNameNode != null)
                        {
                            illustrator.CorporateName = CorporateNameNode.InnerText;
                        }
                        else
                        {
                            if (SequenceNumberNode != null)
                            {
                                illustrator.Sequence = SequenceNumberNode.InnerText;
                            }
                            if (personNameNode != null)
                            {
                                illustrator.PersonName = personNameNode.InnerText;
                            }
                            if (personNameInvertedNode != null)
                            {
                                illustrator.PersonNameInverted = personNameInvertedNode.InnerText;
                            }
                            if (KeyNamesNode != null)
                            {
                                illustrator.KeyNames = KeyNamesNode.InnerText;
                            }
                            if (NamesBeforeKeyNode != null)
                            {
                                illustrator.NamesBeforeKey = NamesBeforeKeyNode.InnerText;
                            }
                            if (BiographicalNoteNode != null)
                            {
                                illustrator.BiographicalNote = BiographicalNoteNode.InnerText;
                            }
                            Illustrators.Add(illustrator);
                        }
                    }
                    book.Illustrators = Illustrators;
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Illustrators data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractAgeRange(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var AudienceRangeNode = listOfNode.SelectSingleNode(AudienceRangeNodeName);
                if (AudienceRangeNode != null)
                {
                    XmlNodeList list = AudienceRangeNode.ChildNodes;
                    string AudienceRangePrecision = string.Empty;
                    string AudienceRangeValue = string.Empty;

                    for (int j = 0; j < AudienceRangeNode.ChildNodes.Count; j++)
                    {
                        if (list[j].Name == AudienceRangePrecisionNodeName)
                        {
                            AudienceRangePrecision = list[j].InnerText;
                        }
                        if (list[j].Name == AudienceRangeValueName)
                        {
                            AudienceRangeValue = list[j].InnerText;
                            if (AudienceRangePrecision == AudienceMinAgeNodeCode)
                            {
                                book.MinAge = AudienceRangeValue;
                            }
                            else if (AudienceRangePrecision == AudienceMaxAgeNodeCode)
                            {
                                book.MaxAge = AudienceRangeValue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Series data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractPublisingStatus(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var publishingStatus = listOfNode.SelectSingleNode("PublishingStatus");
                if (publishingStatus != null)
                {
                    if (publishingStatus != null)
                    {
                        book.PublishStatus = PublishingStatus.GetPublishStatus(publishingStatus.InnerText);
                    }
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract publishing status data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractFormat(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var FormatNode = listOfNode.SelectSingleNode(FormatNodeName);
                if (FormatNode != null)
                {
                    if (FormatNode != null)
                    {
                        book.Format = FormatNode.InnerText;
                    }
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Format data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractBICMainSubject(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var BICMainSubjectNode = listOfNode.SelectSingleNode(BICMainSubjectNodeName);
                if (BICMainSubjectNode != null)
                {
                    if (BICMainSubjectNode != null)
                    {
                        book.Category = new Category()
                        {
                            MainBICCategoryName = BICMainSubjectNode.InnerText
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract BICMainSubject data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractBICSubjects(XmlNode listOfNode, ref Book book)
        {
            try
            {
                book.BICCategories = new List<string>();
                var bigCategories = listOfNode.SelectNodes(SubjectNodeName);
                var listOfBigCategories = new List<XmlNode>(bigCategories.Cast<XmlNode>());
                var BigCategoryNodes = listOfBigCategories.Where(i => i.SelectSingleNode(SubjectSchemeIdentifierNodeName).InnerText == BICCategoryIdentifierCodeNodeName);

                if (BigCategoryNodes != null && BigCategoryNodes.Count() > 0)
                {
                    foreach (var BigCategoryNode in BigCategoryNodes)
                    {
                        book.BICCategories.Add(BigCategoryNode.SelectSingleNode(SubjectCodeNodeName).InnerText);
                    }
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract BIGData failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractDescription(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var OtherTexts = listOfNode.SelectNodes(OtherTextNodeName);
                var listOfOtherTexts = new List<XmlNode>(OtherTexts.Cast<XmlNode>());
                if (listOfOtherTexts != null && listOfOtherTexts.Count > 0)
                {
                    var MainTextNode = listOfOtherTexts.SingleOrDefault(i => i.SelectSingleNode(TextTypeCodeNodeName).InnerText == MainTextTypeCode);
                    if (MainTextNode != null)
                    {
                        var mainTextNode = MainTextNode.SelectSingleNode(MainTextNodeName);
                        if (mainTextNode != null)
                        {
                            book.Description = mainTextNode.InnerText;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Description data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractPublishtionDate(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var PubishionDateNode = listOfNode.SelectSingleNode(PubishtionDateNodeName);
                if (PubishionDateNode != null)
                {
                    if (PubishionDateNode != null)
                    {
                        book.PubDate = DateTime.ParseExact(PubishionDateNode.InnerText,
                                  "yyyyMMdd",
                                   CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract publish date data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private void ExtractDimensionsAndWeight(XmlNode listOfNode, ref Book book)
        {
            try
            {
                Dimensions dimensions = new Dimensions();
                var MeasureNodes = listOfNode.SelectNodes(MeasureNodeName);
                var listOfMeasureNodes = new List<XmlNode>(MeasureNodes.Cast<XmlNode>());

                var HeightNode = listOfMeasureNodes.SingleOrDefault(i => i.SelectSingleNode(MeasureTypeCodeNodeName) != null && i.SelectSingleNode(MeasureTypeCodeNodeName).InnerText == HeightTypeCode && i.SelectSingleNode(MeasureUnitCodeNodeName) != null && i.SelectSingleNode(MeasureUnitCodeNodeName).InnerText == MeasureUnitCodeWidth);
                if (HeightNode != null)
                {
                    dimensions.Height = HeightNode.SelectSingleNode(MeasurementName).InnerText;
                }

                var WidthNode = listOfMeasureNodes.SingleOrDefault(i => i.SelectSingleNode(MeasureTypeCodeNodeName) != null && i.SelectSingleNode(MeasureTypeCodeNodeName).InnerText == WidthCode && i.SelectSingleNode(MeasureUnitCodeNodeName) != null && i.SelectSingleNode(MeasureUnitCodeNodeName).InnerText == MeasureUnitCodeWidth);
                if (WidthNode != null)
                {
                    dimensions.Width = WidthNode.SelectSingleNode(MeasurementName).InnerText;
                }
                book.Dimensions = dimensions;

                //extract weight
                var WeightNode = listOfMeasureNodes.SingleOrDefault(i => i.SelectSingleNode(MeasureTypeCodeNodeName) != null && i.SelectSingleNode(MeasureTypeCodeNodeName).InnerText == WeightTypeNode && i.SelectSingleNode(MeasureUnitCodeNodeName) != null && i.SelectSingleNode(MeasureUnitCodeNodeName).InnerText == MeasureUnitCodeWeight);
                if (WeightNode != null)
                {
                    book.Weight = WeightNode.SelectSingleNode(MeasurementName).InnerText;
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Dimensions and weight data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }

        private string SupplyDetailNodeName = "SupplyDetail";

        private void ExtractPrice(XmlNode listOfNode, ref Book book)
        {
            try
            {
                var SupplyDetailNode = listOfNode.SelectSingleNode(SupplyDetailNodeName);
                if (SupplyDetailNode != null)
                {
                    var PriceNodes = SupplyDetailNode.SelectNodes(PriceNodeName);
                    var listOfMeasureNodes = new List<XmlNode>(PriceNodes.Cast<XmlNode>());

                    var AUDPriceNode = listOfMeasureNodes.SingleOrDefault(i => i.SelectSingleNode(CurrencyCodeNodeName).InnerText == CurrencyCode);
                    if (AUDPriceNode != null)
                    {
                        book.Price = AUDPriceNode.SelectSingleNode(PriceAmountNodeName).InnerText;
                    }
                }
            }
            catch (Exception ex)
            {
                BooksImportLog.Error("Extract Price data failed. Node data: " + listOfNode.ToString(), ex);
            }
        }
    }
}