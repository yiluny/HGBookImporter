using Sitecore.Data.Items;
using System;
namespace HG.Coprorate.Firebrand.Models
{
    public class Format
    {
        public string Code { get; set; }

        public string Description { get; set; }

        public void PopulateData(ref Item sitecoreItem)
        {
            sitecoreItem.Editing.BeginEdit();
            try
            {
                sitecoreItem.Fields["Code"].Value = this.Code;
                sitecoreItem.Fields["Description"].Value = this.Description;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                sitecoreItem.Editing.EndEdit();
            }
        }
    }
}