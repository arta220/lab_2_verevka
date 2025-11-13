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
/// Класс для анализа предиката.
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
            object result = expression.Evaluate();


            if (result is bool b)
            {
                return b;
            }

            // Числовое значение 
            // Также обрабатывает 0 как False, а любое другое число как True (как в C)
            if (result is int i)
            {
                return i != 0;
            }

            if (result is double d)
            {
                // Если предикат типа 'x>0', результат должен быть bool. 
                // Но на случай, если NCalc возвращает 0.0 или 1.0, обрабатываем это как False/True.
                return Math.Abs(d) > 0.0001;
            }

            // Если выражение не является булевым (например, "x + 5"), 
            // или имеет недопустимый тип, считаем это ошибкой и возвращаем False.
            return false;
        }
        catch
        {
            // Ошибки во время вычисления (деление на ноль, синтаксическая ошибка в expression.Evaluate())
            return false;
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
        bool hasAnyPoint = false; // Флаг для отслеживания, был ли домен непустым

        // --- 1. Основной цикл ---
        for (double x = min; x <= max + (step / 2.0); x += step)
        {
            hasAnyPoint = true; // Домен не пуст!

            // ******************************************************
            // Используем исправленный EvaluateAtPoint (см. ниже)
            // ******************************************************
            bool resultAtPoint = EvaluateAtPoint(expression, x);

            if (evaluationType == QuantifierEvaluationType.Universal)
            {
                if (!resultAtPoint)
                {
                    return false; // Нашли контрпример -> Ложь
                }
            }
            else // Existential
            {
                if (resultAtPoint)
                {
                    return true; // Нашли пример -> Истина
                }
            }
        }

        // --- 2. Обработка пустого домена и окончательного результата ---

        if (!hasAnyPoint)
        {
            // Если цикл не выполнился (пустой домен)
            if (evaluationType == QuantifierEvaluationType.Universal)
            {
                return false; // Соответствует вашему тесту (Assert.False)
            }
            else // Existential
            {
                return false; // Существует в пустом множестве -> Ложь
            }
        }

        // Если дошли до этого места, значит:
        // 1. Universal: Все точки проверены, и ни одна не вернула False. -> Истина.
        // 2. Existential: Все точки проверены, и ни одна не вернула True. -> Ложь.

        return evaluationType == QuantifierEvaluationType.Universal;
    }
}
