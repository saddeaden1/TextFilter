using LanguageExt;

namespace TextReader;

public interface ITextFileReader
{
    Either<FileReaderError, string> ReadTextFile(string filename);
}