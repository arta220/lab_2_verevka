using System.Text.RegularExpressions;
using NCalc;

public class Parser
{

    // TODO: допилить:
    //валидацию случая, когда  отрицание ставится перед переменной. скорее всего просто буду выкидывать ошибку с доп методом в парсере
    //метод для проверки предиката по шаблону
    private static readonly string comparisonPattern = @"(<=|>=|==|!=|<|>|≠|≤|≥)"; // Для IsPredicate
    private static readonly string variablePattern = @"[a-zA-Z]+"; // Для IsPredicate
    private static readonly string quantifierPattern = @"(∀|∃)"; // Для DetectQuantifiers
    private static readonly string validTokenAfterVariable =
        @"(?:\s|[\+\-\*/%\[\]\.\(\)]|<=|>=|==|!=|<|>|≠|≤|≥|and|or|not|&&|\|\||=>|<=>|→|↔)"; // с учетом ручного ввода через OR NOT AND
    // ПАТТЕРН ДЛЯ ПОИСКА ПЕРЕМЕННОЙ ПОСЛЕ КВАНТОРА: ^(пробелы)(КВАНТОР)(probeli)(ПЕРЕМЕННАЯ - ГРУППА 2)
    private static readonly string quantifierVariablePattern =
        @"^\s*(∀|∃|forall|exists)\s*([a-zA-Z]+)";

    /// <summary>
    /// Проверяет, следует ли сразу за первой найденной переменной 
    /// недопустимый токен (что-то, что не является оператором или допустимым символом).
    /// Например, в "x asdf < 5" после 'x' идет 'asdf'.
    /// </summary>
    private bool IsVariableFollowedByInvalidToken(string expression)
    {
       // поиск переменной
        var varMatch = Regex.Match(expression, variablePattern);

        if (!varMatch.Success)
        {
            return false;
        }

        // определениее индекса после переменной
        int endOfVariable = varMatch.Index + varMatch.Length;
        string remainingString = expression.Substring(endOfVariable).TrimStart();

        if (string.IsNullOrEmpty(remainingString))
        {
            // Переменная - последний символ в строке (например, "x") - считаем это валидным,
            // поскольку IsPredicate и так не сработает (нет сравнения).
            return false;
        }

        // Проверяем, начинается ли оставшаяся строка с допустимого знака
        // удаляя пробелы

        // негативный взгляд вперед для поиска 
        string invalidTokenPattern = $@"^(?!{validTokenAfterVariable}).";

        // если дальше идет что-то кроме лог. и сравн. операторов - false 
        return Regex.IsMatch(remainingString, invalidTokenPattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Подсчитывает общее количество кванторов (∀, ∃, forall, exists) в выражении.
    /// </summary>
    public int CountQuantifiers(string expression)
    {
        // Используем новый паттерн для поиска всех вхождений
        var matches = Regex.Matches(expression, quantifierPattern, RegexOptions.IgnoreCase);
        return matches.Count;
    }

    /// <summary>
    /// Проверяет, является ли введённая строка предикатом 
    /// (наличие операторов сравнения И переменных).
    /// </summary>
    public bool IsPredicate(string expression)
    {
        if (IsVariableFollowedByInvalidToken(expression))
        {
            return false;
        }

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
    /// Извлекает имя переменной, связанной квантором 
    /// </summary>
    /// <param name="expression">Исходное выражение.</param>
    /// <returns>Имя переменной или первая переменная в предикате .</returns>
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

        normalized = Regex.Replace(normalized, @"(∀|∃|forall|exists)\s*[a-zA-Z]\s*", "", RegexOptions.IgnoreCase).Trim();
        normalized = Regex.Replace(normalized, @"(∀|∃)\s*", "").Trim();

        normalized = normalized.Replace("≠", "!=")
                               .Replace("≥", ">=")
                               .Replace("≤", "<=")
                               .Replace("↔", "<=>")
                               .Replace("→", "=>");

        normalized = normalized.Replace("∧", " and ")
                               .Replace("∨", " or ")
                               .Replace("¬", " not ");

        normalized = normalized.Replace(":", "/");

        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        // операторы сравнения
        string comparisonOps = @"(?:<|<=|>|>=)";

        // выражение без поедания лишних скобок
        string exprPart = @"[a-zA-Z0-9\.\+\-\*\/\(\)]+";

        // --- ключевая правка: убраны внешние скобки \(? \)? ---
        string doubleComparisonPattern =
            $@"({exprPart})\s*({comparisonOps})\s*({exprPart})\s*({comparisonOps})\s*({exprPart})";

        // --- скобки вокруг всего результата полностью восстанавливают структуру ---
        string replacement = "(($1 $2 $3) and ($3 $4 $5))";

        normalized = Regex.Replace(normalized, doubleComparisonPattern, replacement, RegexOptions.IgnoreCase);

        normalized = Regex.Replace(normalized, @"(?<![<>!=])=(?![=])", "==");

        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        return normalized;
    }



}