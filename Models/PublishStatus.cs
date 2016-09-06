namespace HG.Coprorate.Firebrand.Models
{
    public enum PublishStatus
    {
        Unknown,
        Unspecified,
        Cancelled,
        Forthcoming,
        PostponedIndefinitely,
        Active,
        NoLongerOurProduct,
        OutOfStockIndefinitely,
        OutOfPrint,
        Inactive,
        Remaindered,
        WithdrawnFromSale,
        Recalled,
        TemporarilyWithdrawnFromSale,
        PermanentlyWithdrawnFromSale
    }

    public static class PublishingStatus
    {
        public static PublishStatus GetPublishStatus(string PublishingStatus)
        {
            switch (PublishingStatus)
            {
                case "00":
                    return PublishStatus.Unspecified;
                case "01":
                    return PublishStatus.Cancelled;
                case "02":
                    return PublishStatus.Forthcoming;
                case "03":
                    return PublishStatus.PostponedIndefinitely;
                case "04":
                    return PublishStatus.Active;
                case "05":
                    return PublishStatus.NoLongerOurProduct;
                case "06":
                    return PublishStatus.OutOfStockIndefinitely;
                case "07":
                    return PublishStatus.OutOfPrint;
                case "08":
                    return PublishStatus.Inactive;
                case "09":
                    return PublishStatus.Unknown;
                case "10":
                    return PublishStatus.Remaindered;
                case "11":
                    return PublishStatus.WithdrawnFromSale;
                case "12":
                    return PublishStatus.Recalled;
                case "15":
                    return PublishStatus.Recalled;
                case "16":
                    return PublishStatus.TemporarilyWithdrawnFromSale;
                case "17":
                    return PublishStatus.PermanentlyWithdrawnFromSale;

                default:
                    return PublishStatus.Unknown;
            }
        }
    }
}