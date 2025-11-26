using System;
using static PredicateAnalyzer;

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

        int count = _parser.CountQuantifiers(expression);

        if (count > 1)
        {
            throw new ArgumentException("В выражении должен быть ровно 1 квантор.");
        }

        // Если count == 0 или count == 1
        return count == 1;
    }

    public string NormalizeToNCalc(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Выражение не может быть пустым.", nameof(expression));

        return _parser.NormalizeToNCalc(expression);
    }

    public string ExtractQuantifierVariable(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Выражение не может быть пустым.", nameof(expression));

        // Вызываем метод из класса Parser
        return _parser.ExtractQuantifierVariable(expression);
    }


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