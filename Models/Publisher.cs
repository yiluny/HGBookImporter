using HG.Corporate.Core.Helpers;
using Sitecore.Data.Items;

namespace HG.Coprorate.Firebrand.Models
{
    public class Publisher
    {
        public string Name { get; set; }

        private string PublisherItemGroupPublishersFieldName = "Group publishers";
        private string PublisherIteNameFieldName = "Name";

        public Item GetGroupItem(Item HGBPublisherGroupItem, Item HGEPublisherGroupItem, Item HGTPublisherGroupItem)
        {
            if (HGBPublisherGroupItem != null)
            {
                var HGBGroupDetail = HGBPublisherGroupItem.Fields[PublisherItemGroupPublishersFieldName].Value;
                if (HGBGroupDetail.Contains(this.Name))
                {
                    return HGBPublisherGroupItem;
                }
            }

            if (HGEPublisherGroupItem != null)
            {
                var HGEGroupDetail = HGEPublisherGroupItem.Fields[PublisherItemGroupPublishersFieldName].Value;
                if (HGEGroupDetail.Contains(this.Name))
                {
                    return HGEPublisherGroupItem;
                }
            }

            if (HGTPublisherGroupItem != null)
            {
                var HGTGroupDetail = HGTPublisherGroupItem.Fields[PublisherItemGroupPublishersFieldName].Value;
                if (HGTGroupDetail.Contains(this.Name))
                {
                    return HGTPublisherGroupItem;
                }
            }
            return null;
        }

        public Item GroupItem { get; set; }

        public string DisplayName
        {
            get { return ItemNameHelper.RemoveSpecialCharacters(this.Name); }
        }
    }
}