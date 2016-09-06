using HG.Corporate.Core.Helpers;

namespace HG.Coprorate.Firebrand.Models
{
    public class Author
    {
        public string SitecoreID { get; set; }

        public string PersonNameInverted { get; set; }

        public string KeyNames { get; set; }

        public string PersonName { get; set; }

        public string NamesBeforeKey { get; set; }

        public string BiographicalNote { get; set; }

        public string Sequence { get; set; }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(PersonName))
                {
                    return ItemNameHelper.RemoveSpecialCharacters(PersonName);
                }
                else if (!string.IsNullOrWhiteSpace(KeyNames) && !string.IsNullOrWhiteSpace(NamesBeforeKey))
                {
                    return ItemNameHelper.RemoveSpecialCharacters(string.Format("{0} {1}", NamesBeforeKey, KeyNames));
                }
                else if (!string.IsNullOrWhiteSpace(CorporateName))
                {
                    return ItemNameHelper.RemoveSpecialCharacters(CorporateName);
                }
                return "";
            }
        }

        public string CorporateName { get; set; }
    }
}