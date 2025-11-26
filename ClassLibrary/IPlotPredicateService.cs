using System.Collections.Generic;
using OxyPlot;

/// <summary>
/// Программный интерфейс для работы с генерацией графиков и анализом предикатов.
/// </summary>
public interface IPlotPredicateService
{
    /// <summary>
    /// Вычисляет область истинности предиката для заданного диапазона и шага.
    /// </summary>
    List<TruthSegment> GetTruthSegments(Predicate predicate, double min, double max, double step);

    /// <summary>
    /// Определяет тип предиката: AlwaysTrue, AlwaysFalse, Satisfiable для заданного диапазона.
    /// </summary>
    PredicateAnalyzer.PredicateType GetPredicateType(Predicate predicate, double min, double max, double step);

    /// <summary>
    /// Создает график OxyPlot на основе отрезков истинности.
    /// </summary>
    PlotModel Generate1DPlot(List<TruthSegment> segments, double min, double max);

    /// <summary>
    /// Вычисляет истинность высказывания с квантором на дискретном домене.
    /// </summary>
    bool EvaluateQuantifiedStatement(Predicate predicate, PredicateAnalyzer.QuantifierEvaluationType type, double min, double max, double step);
}
