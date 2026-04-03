namespace KeyCass.Modules.Speaker;

public static class Normalizer
{
    private static readonly Dictionary<string, string> ReplacerMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "voce", "você" },
        { "ate", "até" },
        { "tambem", "também" },
        { "entao", "então" },
        { "amanha", "amanhã" },
        { "sera", "será" },
        { "ja", "já" },
        { "so", "só" },
        { "soh", "só" },
        { "mae", "mãe" },
        { "maos", "mãos" },
        { "ta", "tá" },
        { "pra", "pra" },
        { "pq", "porque" },
        { "comeco", "começo" },
        { "porem", "porém" },
        { "alem", "além" },
        { "vc", "você" },
        { "pqp", "puta que paril" },
        { "tb", "também" },
    };

    private static string NormalizeWord(string word)
    {
        return ReplacerMap.GetValueOrDefault(word, word);
    }

    public static string Normalize(string text)
    {
        return string.Join(" ", text.Split(" ")
            .Select(NormalizeWord)
            .ToList());
    }
}
