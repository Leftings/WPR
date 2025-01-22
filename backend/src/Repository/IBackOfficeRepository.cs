namespace WPR.Repository;

public interface IBackOfficeRepository
{
    public (bool Status, string Message, Dictionary<string, object> Data) GetDataReview(int id);
    public (bool Status, string Message, int[] Ids) GetDataReviewIds();
    public (bool Status, string Message, Dictionary<string, object> Data) GetFullDataReview(int id);
}