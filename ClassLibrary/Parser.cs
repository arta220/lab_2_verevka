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

        // --- 1. Удаление кванторов и связанных переменных (для NCalc) ---
        // NCalc не понимает кванторы (∀, ∃) или слова (forall, exists),
        // поэтому их нужно удалить вместе с переменной, которую они связывают (например, 'x').

        // Паттерн ищет: (∀ или ∃ или forall или exists) + (необязательные пробелы) + 
        // (переменная 'x' или любая буква) + (необязательные пробелы).
        // Мы используем RegexOptions.IgnoreCase, чтобы учесть "FORALL" или "Exists".
        normalized = Regex.Replace(normalized, @"(∀|∃|forall|exists)\s*[a-zA-Z]\s*", "", RegexOptions.IgnoreCase).Trim();

        // Дополнительно удаляем символ квантора, если за ним не было переменной (например, "∀(x>0)")
        normalized = Regex.Replace(normalized, @"(∀|∃)\s*", "").Trim();

        // --- 2. Нормализация логических и математических символов ---

        normalized = normalized.Replace("↔", "<=>")
                               .Replace("→", "=>");

        normalized = normalized.Replace("≥", ">=")
                               .Replace("≤", "<=")
                               .Replace("≠", "!=");
        // Также часто используется ":" для деления, NCalc требует "/"
        normalized = normalized.Replace(":", "/");


        normalized = normalized.Replace("∧", " and ")
                               .Replace("∨", " or ")
                               .Replace("¬", " not ");


        normalized = normalized.Replace("=", "==");

        // --- 3. Финальная очистка ---

        // Удаление лишних пробелов 
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        // NCalc не любит лишние внешние скобки после удаления квантора.
        // Удаляем скобки, если они находятся в самом начале и самом конце (например, (x>0))
        if (normalized.StartsWith("(") && normalized.EndsWith(")"))
        {
            normalized = normalized.Substring(1, normalized.Length - 2).Trim();
        }


        return normalized;
    }

}