using Employee.Controllers.viewRentalData;

namespace Employee.Repository;

public interface IBackOfficeRepository
{
    public (bool Status, string Message, IList<Dictionary<string, object>> Data) GetDataReviews(string sort, string how);
}