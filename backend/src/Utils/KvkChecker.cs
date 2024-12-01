using System.Text.RegularExpressions;
using WPR.Repository;

namespace WPR.Utils;

public class KvkChecker
{
    private readonly IUserRepository _userRepository;

    public KvkChecker(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<(bool isValid, string errorMessage)> IsKvkNumberValid(int? kvkNumber)
    {
        if (!kvkNumber.HasValue || kvkNumber.Value.ToString().Length != 8)
        {
            return (false, "KVK number must be 8 digits");
        }

        bool inDatabase = await _userRepository.IsKvkNumberAsync(kvkNumber.Value);
        if (!inDatabase)
        {
            return (false, "KVK number is not a valid KVK number");
        }

        return (true, null);
    }
}