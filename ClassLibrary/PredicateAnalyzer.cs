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
        // (Предполагаем, что 1D предикаты используют 'x')
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
            // ИСПРАВЛЕНО: Теперь EvaluateAtPoint принимает NCalc.Expression
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
}
