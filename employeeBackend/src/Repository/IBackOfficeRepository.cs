using Employee.Controllers.viewRentalData;

namespace Employee.Repository;

public interface IBackOfficeRepository
{
    public (bool Status, string Message, Dictionary<string, object> Data) GetDataReview(int id);
    public (bool Status, string Message, int[] Ids) GetDataReviewIds();
}