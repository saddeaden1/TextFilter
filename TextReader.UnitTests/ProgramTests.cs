using Autofac;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace TextReader.UnitTests;

public class ProgramTests
{
    private Mock<ITextFilter> _textFilterMock;
    private IFixture _fixture;
    private StringWriter _consoleOutput;
    private TestEnvironmentExiter _environmentExiter;
    private Mock<ITextFileReader> _textFileReaderMock;
    private Mock<IValidator<string[]>> _validator;

    [SetUp]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        _fixture = new Fixture();

        _textFilterMock = new Mock<ITextFilter>();
        builder.Register(_ => _textFilterMock.Object).As<ITextFilter>();

        _environmentExiter = new TestEnvironmentExiter();
        builder.Register(_ => _environmentExiter).As<IEnvironmentExiter>();

        _textFileReaderMock = new Mock<ITextFileReader>();
        builder.Register(_ => _textFileReaderMock.Object).As<ITextFileReader>();

        _validator = new Mock<IValidator<string[]>>();
        builder.Register(_ => _validator.Object).As<IValidator<string[]>>();
        _validator.Setup(x => x.ValidateAsync(It.IsAny<string[]>(),It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

        _ = new Program(builder.Build());

        _consoleOutput = new StringWriter();
        Console.SetOut(_consoleOutput);
    }

    [Test]
    public async Task Main_ValidInput_ReturnsFilteredText()
    {
        //arrange
        var filePath = _fixture.Create<string>();
        var text = _fixture.Create<string>();
        var sanitizedText = _fixture.Create<string[]>();
        var filteredVowels = _fixture.Create<string[]>();
        var filteredShortWords = _fixture.Create<string[]>();
        var filteredWordsWithT = _fixture.Create<string[]>();

        _textFilterMock.Setup(x => x.SanitizeText(It.IsAny<string>())).Returns(sanitizedText);
        _textFilterMock.Setup(x => x.FilterVowelInMiddle(It.IsAny<string[]>())).Returns(filteredVowels);
        _textFilterMock.Setup(x => x.FilterShortWords(It.IsAny<string[]>())).Returns(filteredShortWords);
        _textFilterMock.Setup(x => x.FilterWordsWithT(It.IsAny<string[]>())).Returns(filteredWordsWithT);

        _textFileReaderMock.Setup(x => x.ReadTextFile(filePath)).Returns(text);

        //act
        try
        {
            await Program.Main(new[] { filePath, "vst" });
        }
        catch (UnitTestException)
        {
        }

        //assert
        _environmentExiter.ExitCode.Should().Be(0);
        _consoleOutput.ToString().Should().Contain(string.Join(" ", filteredWordsWithT));
    }

    [Test]
    public async Task Main_InvalidFilePath_FailsWithErrorMessage()
    {
        //arrange
        var invalidFilePath = _fixture.Create<string>();
        _textFileReaderMock.Setup(x => x.ReadTextFile(invalidFilePath))
            .Returns(new FileReaderError("The file does not exist."));
        var args = new [] { invalidFilePath, "vst" };

        //act
        try
        {
            await Program.Main(args);
        }
        catch (UnitTestException)
        {
        }

        //assert
        _environmentExiter.ExitCode.Should().Be(1);
        _consoleOutput.ToString().Should().Contain("The file does not exist.");
    }

    [Test]
    public async Task Main_InvalidFiltrationOption_ErrorMessageDisplayed()
    {
        //arrange
        var filePath = _fixture.Create<string>();
        var errorMessage = _fixture.Create<string>();

        _validator.Setup(x=>x.ValidateAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult
            {
                Errors = new List<ValidationFailure>
                {
                    new ()
                    {
                        ErrorMessage = errorMessage
                    }
                }
            });

        //act
        try
        {
            await Program.Main(new[] { filePath, "invalid_option" });
        }
        catch (UnitTestException)
        {
        }

        //assert
        _environmentExiter.ExitCode.Should().Be(1);
        _consoleOutput.ToString().Should().Contain(errorMessage);
    }

    [Test]
    public async Task Main_NoArguments_ErrorMessageDisplayed()
    {
        //arrange
        var args = Array.Empty<string>();

        //act
        try
        {
            await Program.Main(args);
        }
        catch (UnitTestException)
        {
        }

        //assert
        _environmentExiter.ExitCode.Should().Be(1);
        _consoleOutput.ToString().Should().Contain("No text file or filtration options inputted.");
    }

    [Test]
    public async Task Main_TextFilterThrowsException_ErrorMessageDisplayed()
    {
        //arrange
        var filePath = _fixture.Create<string>();
        var text = _fixture.Create<string>();
        var exceptionMessage = _fixture.Create<string>();
        _textFileReaderMock.Setup(x => x.ReadTextFile(filePath)).Returns(text);
        _textFilterMock.Setup(x => x.FilterVowelInMiddle(It.IsAny<string[]>()))
            .Throws(new Exception(exceptionMessage));

        //act
        try
        {
            await Program.Main(new[] { filePath, "vst" });
        }
        catch (UnitTestException)
        {
        }

        //assert
        _environmentExiter.ExitCode.Should().Be(1);
        _consoleOutput.ToString().Should().Contain(exceptionMessage);
    }
}