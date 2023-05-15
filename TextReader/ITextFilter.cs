namespace TextReader;

public interface ITextFilter
{
    string[] SanitizeText(string fileText);
    string[] FilterVowelInMiddle(string[] words);
    string[] FilterShortWords(string[] words);
    string[] FilterWordsWithT(string[] words);
}