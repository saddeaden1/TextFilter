using Autofac;
using FluentValidation;

namespace TextReader;

public class Program
{
    private static IContainer _container;
    private static ITextFilter _textFilter;
    private static IEnvironmentExiter _exiter;
    private static ITextFileReader _textFileReader;
    private static IValidator<string[]> _validator;

    public Program(IContainer container)
    {
        _container = container;
    }

    private static void BuildContainer()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<TextFilter>().As<ITextFilter>();
        builder.RegisterType<EnvironmentExiter>().As<IEnvironmentExiter>();
        builder.RegisterType<TextFileReader>().As<ITextFileReader>();
        builder.RegisterType<ArgsValidator>().As<IValidator<string[]>>();

        _container = builder.Build();
    }

    public static async Task Main(string[] args)
    {

        if (_container == null)
        {
            BuildContainer();
        }

        //put the environment exit code in a class to allow me to inject a test version in my tests so
        //the test doesn't exit when testing this method
        _exiter = _container!.Resolve<IEnvironmentExiter>();
        _textFilter = _container!.Resolve<ITextFilter>();
        _textFileReader = _container!.Resolve<ITextFileReader>();
        _validator = _container!.Resolve<IValidator<string[]>>();

        await ValidateArgs(args);

        _textFileReader.ReadTextFile(args[0]).Match(
            fileText =>
            {
                string[] sanitizedText = _textFilter.SanitizeText(fileText);
                string[] result = sanitizedText;

                try
                {
                    if (args[1].Contains('v'))
                    {
                        result = _textFilter.FilterVowelInMiddle(sanitizedText);
                    }

                    if (args[1].Contains('s'))
                    {
                        result = _textFilter.FilterShortWords(result);
                    }

                    if (args[1].Contains('t'))
                    {
                        result = _textFilter.FilterWordsWithT(result);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"The program has failed for the following reason: {ex.Message}");
                    _exiter.Exit(1);
                }

                Console.WriteLine("The result of the text filer is:");
                Console.WriteLine(string.Join(" ", result));
                _exiter.Exit(0);

            }, error =>
            {
                Console.WriteLine($"The program has failed for the following reason: {error.Reason}");
                _exiter.Exit(1);
            });
    }

    private static async Task ValidateArgs(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No text file or filtration options inputted. Please enter the path to a text file and your filtration options");
            _exiter.Exit(1);
        }

        var validationResult = await _validator.ValidateAsync(args);

        if (!validationResult.IsValid)
        {
            Console.WriteLine(validationResult.ToString());
            _exiter.Exit(1);
        }
    }
}