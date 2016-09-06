using Sitecore.Data;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class SitecoreCreationHelper
    {
        public static Item CreateBranchItem(string newBranchName, Database targetDatabase,string branchTemplateId, string branchBucketId)
        {
            Item branchTemplateItem = targetDatabase.GetItem(branchTemplateId);
            Item branchBucketItem = targetDatabase.GetItem(branchBucketId);
            BranchItem branchItem = targetDatabase.GetItem(branchTemplateItem.ID);
            Item branchTargetItem = branchBucketItem.Add(newBranchName.Trim(), branchItem);
            return branchTargetItem;
        }
    }
}
