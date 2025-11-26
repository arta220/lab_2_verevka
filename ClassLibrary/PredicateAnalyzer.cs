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

    private bool EvaluateAtPoint(Expression expression, double x, string variableName)
    {
        // Используем динамическое имя переменной
        expression.Parameters[variableName] = x;

        try
        {
            object result = expression.Evaluate();

            if (result is bool b)
            {
                return b;
            }

            // Числовое значение 
            if (result is int i)
            {
                return i != 0;
            }

            if (result is double d)
            {
                return Math.Abs(d) > 0.0001;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Определяет тип предиката 
    /// </summary>
    public PredicateType DeterminePredicateType(Predicate predicate, double min, double max, double step)
    {
        string variableName = predicate.VariableName; // Извлекаем имя переменной

        bool hasFoundTrue = false;
        bool hasFoundFalse = false;

        for (double x = min; x <= max; x += step)
        {
            if (EvaluateAtPoint(predicate._NCalcExpression, x, variableName))
                hasFoundTrue = true;
            else
                hasFoundFalse = true;

            if (hasFoundTrue && hasFoundFalse)
                return PredicateType.Satisfiable;
        }

        if (hasFoundTrue && !hasFoundFalse)
            return PredicateType.AlwaysTrue;

        return PredicateType.AlwaysFalse;
    }

    /// <summary>
    /// Вычисляет область истинности в виде "отрезков" для графика
    /// </summary>
    public List<TruthSegment> CalculateTruthSet(Predicate predicate, double min, double max, double step)
    {
        string variableName = predicate.VariableName; // Извлекаем имя переменной

        var segments = new List<TruthSegment>();
        var expression = predicate._NCalcExpression;

        TruthSegment currentSegment = null;
        bool previousState = false;

        // Используем 'max + step/2' для гарантированного включения конечной точки max
        for (double x = min; x <= max + (step / 2.0); x += step)
        {

            bool currentState = EvaluateAtPoint(expression, x, variableName);

            if (currentState && !previousState)
            {
                currentSegment = new TruthSegment { Start = x };
            }
            else if (!currentState && previousState && currentSegment != null)
            {
                currentSegment.End = x - step;
                segments.Add(currentSegment);
                currentSegment = null;
            }

            previousState = currentState;
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
    /// Enum для указания типа вычисления квантора.
    /// </summary>
    public enum QuantifierEvaluationType
    {
        Universal,
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

        string variableName = predicate.VariableName; // Извлекаем имя переменной

        var expression = predicate._NCalcExpression;
        bool hasAnyPoint = false;

        //   Основной цикл 
        for (double x = min; x <= max + (step / 2.0); x += step)
        {
            hasAnyPoint = true;

            bool resultAtPoint = EvaluateAtPoint(expression, x, variableName);

            if (evaluationType == QuantifierEvaluationType.Universal)
            {
                if (!resultAtPoint)
                {
                    return false;
                }
            }
            else // Existential
            {
                if (resultAtPoint)
                {
                    return true;
                }
            }
        }

        //  Обработка пустого домена и окончательного результата 

        if (!hasAnyPoint)
        {
            return false;
        }

        return evaluationType == QuantifierEvaluationType.Universal;
    }
}