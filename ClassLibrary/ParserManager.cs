using System;
using static PredicateAnalyzer;

/// <summary>
/// Сервис, предоставляющий доступ к методам класса Parser.
/// При ошибках выбрасывает исключения наверх, без обработки в консоли.
/// </summary>
public class ParserManager : IParserManager // Предполагаю, что IParserManager должен быть обновлен, если он существует.
{
    private readonly Parser _parser;
    public ParserManager(Parser parser) => _parser = parser;

    // --- Существующие методы ---

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

    // --- НОВЫЙ МЕТОД: Извлечение имени переменной ---

    /// <summary>
    /// Извлекает имя переменной, связанной квантором, или первую переменную в предикате.
    /// </summary>
    /// <param name="expression">Исходное выражение с квантором или без.</param>
    /// <returns>Имя переменной (напр., "x") или пустая строка, если не найдена.</returns>
    public string ExtractQuantifierVariable(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Выражение не может быть пустым.", nameof(expression));

        // Вызываем метод из класса Parser
        return _parser.ExtractQuantifierVariable(expression);
    }

    // --- Существующий метод GetQuantifierType ---

    public QuantifierEvaluationType GetQuantifierType(string expression)
    {
        string trimmed = expression.Trim().ToLower();

        // Проверка символов (∀) или слов (forall)
        if (trimmed.StartsWith("∀") || trimmed.StartsWith("forall"))
        {
            return QuantifierEvaluationType.Universal;
        }
        // Проверка символов (∃) или слов (exists)
        else if (trimmed.StartsWith("∃") || trimmed.StartsWith("exists"))
        {
            return QuantifierEvaluationType.Existential;
        }

        // TODO: затычка, потом мб поменять
        return QuantifierEvaluationType.Existential;
    }
}