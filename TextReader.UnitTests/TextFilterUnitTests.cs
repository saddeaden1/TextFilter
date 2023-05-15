using AutoFixture;
using FluentAssertions;
using static System.String;

namespace TextReader.UnitTests;

public class TextFilterUnitTests
{
    private TextFilter _textFilter;
    private IFixture _fixture;

    [SetUp]
    public void Setup()
    {
        _textFilter = new TextFilter();
        _fixture = new Fixture();
    }

    [Test]
    [TestCase("This is a test!", new[] { "This", "is", "a", "test" })]
    [TestCase("Another, Test!", new[] { "Another", "Test" })]
    [TestCase("Test number... 1, 2, 3.", new[] { "Test","number", "1", "2", "3" })]
    [TestCase("Split; these: words!", new[] { "Split", "these", "words" })]
    [TestCase("Remove? These! Special, Characters.", new[] { "Remove", "These", "Special", "Characters" })]
    public void TestSanitizeText(string text, string[] expected)
    {
        var result = _textFilter.SanitizeText(text);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void TestFilterVowelInMiddle()
    {
        string[] words = { "cat", "dog", "eel", "fox", "stff" };
        string[] expected = { "stff" };

        var result = _textFilter.FilterVowelInMiddle(words);

        result.Should().BeEquivalentTo(expected);
    }

    /*This text allows me to randomly generate values without duplicating the filter logic 
    it is covered by the test above but this will allow me to find a issue quicker if both
    tests fail apart from just the one above */

    [Test]
    public void TestFilterVowelInMiddleNumberOfWordsReducedOrStayedTheSame()
    {
        var words = _fixture.CreateMany<string>(5).ToArray();

        var result = _textFilter.FilterVowelInMiddle(words);

        result.Length.Should().BeLessOrEqualTo(words.Length);
    }

    [Test]
    public void TestFilterShortWords()
    {
        var shortWord = Concat(_fixture.CreateMany<char>(2));
        var longWord = Concat(_fixture.CreateMany<string>(8));

        var result = _textFilter.FilterShortWords(new []{shortWord,longWord});

        result.Should().Contain(longWord);
    }

    [Test]
    public void TestFilterShortWordsReducedOrStayedTheSame()
    {
        var words = _fixture.Build<string>().CreateMany(6).ToArray();

        var result = _textFilter.FilterVowelInMiddle(words);

        result.Length.Should().BeLessOrEqualTo(words.Length);
    }

    [Test]
    public void TestFilterWordsWithT()
    {
        string[] words = { "cat", "bat", "rat", "dog", "pig" };
        string[] expected = { "dog", "pig" };

        var result = _textFilter.FilterWordsWithT(words);

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void TestFilterWordsWithTReducedOrStayedTheSame()
    {
        var words = _fixture.Build<string>().CreateMany(6).ToArray();

        var result = _textFilter.FilterWordsWithT(words);

        result.Length.Should().BeLessOrEqualTo(words.Length);
    }
}