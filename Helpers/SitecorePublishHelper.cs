using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;
using System;
using System.Linq;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class SitecorePublishHelper
    {
        public void PublishItem(Sitecore.Data.Items.Item rootItem, bool withChildren, bool isRepublish = false)
        {
            string sourceDBName = "master";
            var sourceDB = Database.GetDatabase(sourceDBName);
            var publishingTargetsItem = sourceDB.GetItem("/sitecore/system/publishing targets");

            if (publishingTargetsItem != null)
            {
                var publishingTargets = publishingTargetsItem.GetChildren(ChildListOptions.SkipSorting).Select(i => i["Target database"]).ToList();

                foreach (string targetDBName in publishingTargets)
                {
                    ExecutePublishItem(sourceDBName, targetDBName, rootItem, withChildren, isRepublish);
                }
            }
        }

        private void ExecutePublishItem(string sourceDBName, string targetDBName, Sitecore.Data.Items.Item rootItem, bool withChildren, bool isRepubish)
        {
            try
            {
                using (new UserSwitcher(@"sitecore\admin", true))
                {
                    var sourceDB = Database.GetDatabase(sourceDBName);
                    var targetDB = Database.GetDatabase(targetDBName);

                    Sitecore.Publishing.PublishOptions publishOptions = null;
                    if (!isRepubish)
                    {
                        publishOptions =
                            new Sitecore.Publishing.PublishOptions(sourceDB,
                                                                    targetDB,
                                                                    Sitecore.Publishing.PublishMode.Smart,
                                                                    rootItem.Language,
                                                                    System.DateTime.Now);
                    }
                    else
                    {
                        publishOptions =
                      new Sitecore.Publishing.PublishOptions(sourceDB,
                                                              targetDB,
                                                              Sitecore.Publishing.PublishMode.Full,
                                                              rootItem.Language,
                                                              System.DateTime.Now);
                    }
                    Sitecore.Publishing.Publisher publisher = new Sitecore.Publishing.Publisher(publishOptions);
                    publisher.Options.RootItem = rootItem;
                    publisher.Options.Deep = withChildren;
                    publisher.Publish();
                }
            }
            catch (Exception x)
            {
                Log.Error(x.Message, this);
            }
        }
    }
}