using System;

/// <summary>
/// Сервис, предоставляющий доступ к методам класса Parser.
/// При ошибках выбрасывает исключения наверх, без обработки в консоли.
/// </summary>
public class ParserManager : IParserManager
{
    private readonly Parser _parser;
    public ParserManager(Parser parser) => _parser = parser;
    public bool IsPredicate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Выражение не может быть пустым.", nameof(expression));

        return _parser.IsPredicate(expression);
    }

    public bool HasQuantifiers(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Выражение не может быть пустым.", nameof(expression));

        return _parser.DetectQuantifiers(expression);
    }

    public string NormalizeToNCalc(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Выражение не может быть пустым.", nameof(expression));

        return _parser.NormalizeToNCalc(expression);
    }
}
