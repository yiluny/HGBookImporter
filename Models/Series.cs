using HG.Corporate.Core.Helpers;

namespace HG.Coprorate.Firebrand.Models
{
    public class Series
    {
        public string DisplayName
        {
            get { return ItemNameHelper.RemoveSpecialCharacters(this.Name); }
        }

        public string Name { get; set; }
    }
}