using FluentAssertions;

namespace TextReader.UnitTests;

[TestFixture]
public class ArgsValidatorTests
{
    private ArgsValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new ArgsValidator();
    }

    [Test]
    public void Validate_ArgsLengthIsNotTwo_ValidationError()
    {
        var result = _validator.Validate(new[] { "arg1" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Incorrect number of arguments entered");
    }

    [Test]
    public void Validate_FirstArgIsEmpty_ValidationError()
    {
        var result = _validator.Validate(new[] { "", "v" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "No file path entered please enter a file path");
    }

    [Test]
    public void Validate_SecondArgIsNotValid_ValidationError()
    {
        var result = _validator.Validate(new[] { "filePath", "invalid_option" });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "No arguments detected please enter a filtration option");
    }

    [TestCase("v")]
    [TestCase("vs")]
    [TestCase("vst")]
    [TestCase("t")]
    [TestCase("ts")]
    [TestCase("tsv")]
    [TestCase("s")]
    [TestCase("st")]
    [TestCase("stv")]
    public void Validate_ValidArgs_ValidationSuccess(string filterOptions)
    {
        var result = _validator.Validate(new[] { "filePath", filterOptions });

        result.IsValid.Should().BeTrue();
    }
}