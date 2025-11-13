using System.Text.RegularExpressions;
using NCalc; 

public class Parser
{
    private static readonly string comparisonPattern = @"(<=|>=|==|!=|<|>)"; // Для IsPredicate
    private static readonly string variablePattern = @"[a-zA-Z]+"; // Для IsPredicate
    private static readonly string quantifierPattern = @"(∀|∃)"; // Для DetectQuantifiers

    /// <summary>
    /// Проверяет, является ли введённая строка предикатом 
    /// (наличие операторов сравнения И переменных).
    /// </summary>
    public bool IsPredicate(string expression)
    {
        // (Ваш код)
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
    /// Приводит логические и математические символы Юникода к формату NCalc.
    /// </summary>
    public string NormalizeToNCalc(string expression)
    {
        string normalized = expression;

        normalized = normalized.Replace("↔", "<=>")
                               .Replace("→", "=>"); 

        normalized = normalized.Replace("≥", ">=")
                               .Replace("≤", "<=") 
                               .Replace("≠", "!="); 


        normalized = normalized.Replace("∧", " and ")
                               .Replace("∨", " or ")
                               .Replace("¬", " not ");


        normalized = normalized.Replace("=", "==");

        // Удаление лишних пробелов 
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        return normalized;
    }

}