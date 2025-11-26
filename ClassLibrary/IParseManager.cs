using System;
using static PredicateAnalyzer;

/// <summary>
/// Интерфейс для работы с логическими и предикатными выражениями.
/// Определяет базовые операции над строками предикатов и кванторов.
/// </summary>
public interface IParserManager
{
    /// <summary>
    /// Проверяет, является ли выражение предикатом 
    /// </summary>
    bool IsPredicate(string expression);

    /// <summary>
    /// Проверяет, содержит ли выражение кванторы
    /// </summary>
    bool HasQuantifiers(string expression);

    /// <summary>
    /// Приводит выражение к синтаксису NCalc, заменяя математические и логические символы Юникода.
    /// </summary>
    string NormalizeToNCalc(string expression);

    /// <summary>
    /// Определяет тип первого квантора в формуле.
    /// </summary>
    QuantifierEvaluationType GetQuantifierType(string expression);

    /// <summary>
    /// Извлекает имя переменной, связанной квантором или первую переменную в предикате.
    /// </summary>
    string ExtractQuantifierVariable(string expression);
}