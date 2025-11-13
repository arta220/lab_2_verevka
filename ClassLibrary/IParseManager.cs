using System;

/// <summary>
/// Интерфейс для работы с логическими и предикатными выражениями.
/// Определяет базовые операции над строками предикатов и кванторов.
/// </summary>
public interface IParserManager
{
    /// <summary>
    /// Проверяет, является ли выражение предикатом (содержит операторы сравнения и переменные).
    /// </summary>
    bool IsPredicate(string expression);

    /// <summary>
    /// Проверяет, содержит ли выражение кванторы (∀ или ∃).
    /// </summary>
    bool HasQuantifiers(string expression);

    /// <summary>
    /// Приводит выражение к синтаксису NCalc, заменяя математические и логические символы Юникода.
    /// </summary>
    string NormalizeToNCalc(string expression);
}

