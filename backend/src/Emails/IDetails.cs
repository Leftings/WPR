namespace WPR.Email;

public interface IDetails{
    public Task SetDetailsAsync(object reference);
    public Task<Dictionary<string, object>> GetDetailsAsync(); 
}