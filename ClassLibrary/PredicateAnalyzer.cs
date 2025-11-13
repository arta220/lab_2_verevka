using NCalc;
using System;
using System.Collections.Generic;

/// <summary>
/// Структура для хранения отрезка на оси X.
/// </summary>
public class TruthSegment
{
    public double Start;
    public double End;
}

/// <summary>
/// "Новый класс" для анализа предиката.
/// Определяет тип и вычисляет область истинности.
/// </summary>
public class PredicateAnalyzer
{
    public enum PredicateType { AlwaysTrue, AlwaysFalse, Satisfiable }

    private bool EvaluateAtPoint(Expression expression, double x)
    {
        expression.Parameters["x"] = x;
        try
        {
            // Поскольку это Expression (не CompiledExpression), NCalc парсит его каждый раз.
            return (bool)expression.Evaluate();
        }
        catch
        {
            return false; // Ошибки (деление на ноль, синтаксис) считаем Ложью
        }
    }

    /// <summary>
    /// Определяет тип предиката 
    /// </summary>
    public PredicateType DeterminePredicateType(Predicate predicate, double min, double max, double step)
    {

        bool hasFoundTrue = false;
        bool hasFoundFalse = false;

        for (double x = min; x <= max; x += step)
        {

            if (EvaluateAtPoint(predicate._NCalcExpression, x))
                hasFoundTrue = true;
            else
                hasFoundFalse = true;

            // Оптимизация: если нашли оба, это "Выполнимый"
            if (hasFoundTrue && hasFoundFalse)
                return PredicateType.Satisfiable;
        }

        if (hasFoundTrue && !hasFoundFalse)
            return PredicateType.AlwaysTrue; // Только истина

        // (Если домен пуст или только ложь)
        return PredicateType.AlwaysFalse;
    }

    /// <summary>
    /// Вычисляет область истинности в виде "отрезков" для графика
    /// </summary>
    public List<TruthSegment> CalculateTruthSet(Predicate predicate, double min, double max, double step)
    {

        var segments = new List<TruthSegment>();
        var expression = predicate._NCalcExpression;

        TruthSegment currentSegment = null;
        bool previousState = false;

        // Корректируем начальную точку для первого шага цикла
        bool isFirstPoint = true;

        // Используем 'max + step/2' для гарантированного включения конечной точки max
        for (double x = min; x <= max + (step / 2.0); x += step)
        {
            bool currentState = EvaluateAtPoint(expression, x);

            if (currentState && !previousState)
            {
                // Начало нового отрезка (переход из Лжи в Истину)
                currentSegment = new TruthSegment { Start = x };
            }
            else if (!currentState && previousState && currentSegment != null)
            {
                // Конец отрезка (переход из Истины в Ложь)
                // Конец отрезка - это предыдущая точка (x - step).
                currentSegment.End = x - step;
                segments.Add(currentSegment);
                currentSegment = null;
            }

            previousState = currentState;
            isFirstPoint = false;
        }

        // Закрываем последний отрезок, если цикл завершился на истинном значении
        if (currentSegment != null && previousState)
        {
            currentSegment.End = max;
            segments.Add(currentSegment);
        }

        return segments;
    }
    /// <summary>
    //  Enum для указания типа вычисления квантора.
    /// </summary>
    public enum QuantifierEvaluationType
    {
        /// <summary>
        /// Универсальный (Для всех - ∀). Требует, чтобы все точки были Истинны.
        /// </summary>
        Universal,
        /// <summary>
        /// Экзистенциальный (Существует - ∃). Требует, чтобы хотя бы одна точка была Истинна.
        /// </summary>
        Existential
    }

    /// <summary>
    /// Вычисляет истинность (True/False) высказывания с квантором на заданном домене.
    /// </summary>
    /// <param name="predicate">Объект предиката (содержащий P(x)).</param>
    /// <param name="evaluationType">Тип квантора (∀ или ∃), который должен применить ПИ.</param>
    /// <param name="min">Минимум домена.</param>
    /// <param name="max">Максимум домена.</param>
    /// <param name="step">Шаг домена.</param>
    /// <returns>True или False.</returns>
    public bool EvaluateQuantifiedStatement(
        Predicate predicate,
        QuantifierEvaluationType evaluationType,
        double min,
        double max,
        double step)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var expression = predicate._NCalcExpression;

        bool foundTrue = false;
        bool foundFalse = false;

        // Обрабатываем все точки домена
        for (double x = min; x <= max + (step / 2.0); x += step) // +step/2 для учета погрешности округления
        {
            bool resultAtPoint = EvaluateAtPoint(expression, x);

            // Оптимизация: для универсального квантора - выходим при первой лжи
            if (evaluationType == QuantifierEvaluationType.Universal)
            {
                if (!resultAtPoint)
                {
                    return false; // Нашли контрпример - высказывание ложно
                }
                foundTrue = true; // Отслеживаем, что нашли хотя бы одну истину
            }
            // Для экзистенциального квантора - выходим при первой истине
            else // evaluationType == QuantifierEvaluationType.Existential
            {
                if (resultAtPoint)
                {
                    return true; // Нашли пример - высказывание истинно
                }
                foundFalse = true; // Отслеживаем, что нашли ложь
            }
        }

        // Если дошли до конца цикла:
        if (evaluationType == QuantifierEvaluationType.Universal)
        {
            // Для ∀: если дошли до конца и не нашли контрпримеров - истина
            // Но проверяем, что домен не был пустым
            return foundTrue; // Если foundTrue = false, значит домен был пуст
        }
        else // Existential
        {
            // Для ∃: если дошли до конца и не нашли истины - ложь
            return false;
        }
    }
}
