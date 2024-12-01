using System.Text.RegularExpressions;

namespace WPR.Utils;

public class KvkChecker
{
    public static bool IsValidKvkNumber(int? kvkNumber)
    {
        return kvkNumber.HasValue && kvkNumber.Value.ToString().Length == 8;
    }
}