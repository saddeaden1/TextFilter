namespace TextReader;

public class TextFilter : ITextFilter
{
    private const string Vowels = "aeiou";

    public string[] SanitizeText(string fileText)
    {
        string[] words = fileText.Split(new[] { ' ', '.', ',', ';', ':', '!', '?', '\'', '(', ')', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        return words;
    }

    public string[] FilterVowelInMiddle(string[] words)
    {
        return words.Where(word => !HasVowelInMiddle(word)).ToArray();
    }

    public string[] FilterShortWords(string[] words)
    {
        return words.Where(word => word.Length >= 3).ToArray();
    }

    public string[] FilterWordsWithT(string[] words)
    {
        return words.Where(word => !word.ToLower().Contains('t')).ToArray();
    }

    private bool HasVowelInMiddle(string word)
    {
        int middleIndex = word.Length / 2;

        if (word.Length % 2 == 0)
        {
            return Vowels.ToLower().Contains(word[middleIndex - 1]) || Vowels.Contains(word[middleIndex]);
        }
        else
        {
            return Vowels.ToLower().Contains(word[middleIndex]);
        }
    }
}