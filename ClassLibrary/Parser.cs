using System.Text.RegularExpressions;
using NCalc;

public class Parser
{
    private static readonly string comparisonPattern = @"(<=|>=|==|!=|<|>|≠|≤|≥)"; // Для IsPredicate
    private static readonly string variablePattern = @"[a-zA-Z]+"; // Для IsPredicate
    private static readonly string quantifierPattern = @"(∀|∃)"; // Для DetectQuantifiers

    // ПАТТЕРН ДЛЯ ПОИСКА ПЕРЕМЕННОЙ ПОСЛЕ КВАНТОРА: ^(опц. пробелы)(КВАНТОР)(опц. пробелы)(ПЕРЕМЕННАЯ - ГРУППА 2)
    private static readonly string quantifierVariablePattern =
        @"^\s*(∀|∃|forall|exists)\s*([a-zA-Z]+)";

    /// <summary>
    /// Проверяет, является ли введённая строка предикатом 
    /// (наличие операторов сравнения И переменных).
    /// </summary>
    public bool IsPredicate(string expression)
    {
        return Regex.IsMatch(expression, comparisonPattern) &&
               Regex.IsMatch(expression, variablePattern);
    }

    /// <summary>
    /// Обнаруживает, есть ли в строке символы кванторов.
    /// </summary>
    public bool DetectQuantifiers(string expression)
    {
        return Regex.IsMatch(expression, quantifierPattern);
    }

    /// <summary>
    /// Извлекает имя переменной, связанной квантором (например, 'x' из '∀x (x>0)').
    /// </summary>
    /// <param name="expression">Исходное выражение.</param>
    /// <returns>Имя переменной (напр., "x") или первая переменная в предикате.</returns>
    public string ExtractQuantifierVariable(string expression)
    {
        var match = Regex.Match(expression, quantifierVariablePattern, RegexOptions.IgnoreCase);

        // Группа 2 содержит имя переменной, связанной квантором
        if (match.Success && match.Groups.Count >= 3)
        {
            return match.Groups[2].Value;
        }

        // Если квантора нет, ищем первую переменную в предикате
        var varMatch = Regex.Match(expression, variablePattern);
        if (varMatch.Success)
        {
            return varMatch.Value;
        }

        return string.Empty;
    }

    /// <summary>
    /// Приводит логические и математические символы Юникода к формату NCalc.
    /// </summary>
    public string NormalizeToNCalc(string expression)
    {
        string normalized = expression;

        // --- 1. Удаление кванторов и связанных переменных ---
        normalized = Regex.Replace(normalized, @"(∀|∃|forall|exists)\s*[a-zA-Z]\s*", "", RegexOptions.IgnoreCase).Trim();
        normalized = Regex.Replace(normalized, @"(∀|∃)\s*", "").Trim();

        // --- 2. Нормализация символов ---
        // 2.1. Сложные операторы (обрабатываем первыми!)
        normalized = normalized.Replace("≠", "!=")
                               .Replace("≥", ">=")
                               .Replace("≤", "<=")
                               .Replace("↔", "<=>")
                               .Replace("→", "=>");

        // 2.2. Логические операторы
        normalized = normalized.Replace("∧", " and ")
                               .Replace("∨", " or ")
                               .Replace("¬", " not ");

        // 2.3. Математические символы
        normalized = normalized.Replace(":", "/"); // Деление

        // 2.4. Преобразуем одиночные '=' в '=='
        normalized = Regex.Replace(normalized, @"(?<![<>!=])=(?![=])", "==");

        // --- 3. Финальная очистка ---
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        // Удаление лишних внешних скобок
        if (normalized.StartsWith("(") && normalized.EndsWith(")") && normalized.Length > 2)
        {
            normalized = normalized.Substring(1, normalized.Length - 2).Trim();
        }

        return normalized;
    }
}