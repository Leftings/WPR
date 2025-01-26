using WPR.Repository;

namespace WPR.Utils;

public class KvkChecker
{
    private readonly IUserRepository _userRepository;

    // Constructor voor het initialiseren van de repository
    public KvkChecker(IUserRepository userRepository)
    {
        _userRepository = userRepository; // Initialiseer de repository voor de gebruiker
    }

    // Methode om te controleren of een KVK-nummer geldig is
    public async Task<(bool isValid, string errorMessage)> IsKvkNumberValid(int? kvkNumber)
    {
        // Controleer of het KVK-nummer een waarde heeft en precies 8 cijfers bevat
        if (!kvkNumber.HasValue || kvkNumber.Value.ToString().Length != 8)
        {
            return (false, "KVK number must be 8 digits"); // KVK-nummer is ongeldig als het niet 8 cijfers heeft
        }

        // Controleer of het KVK-nummer in de database voorkomt
        bool inDatabase = await _userRepository.IsKvkNumberAsync(kvkNumber.Value);
        if (!inDatabase)
        {
            return (false, "KVK number is not a valid KVK number"); // KVK-nummer is niet geldig als het niet in de database staat
        }

        return (true, null); // KVK-nummer is geldig, geen foutmelding
    }
}