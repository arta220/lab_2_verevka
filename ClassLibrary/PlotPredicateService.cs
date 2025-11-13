using System;
using System.Collections.Generic;
using OxyPlot;

/// <summary>
/// Сервис, реализующий работу с предикатами и построением графиков.
/// Вызывает методы PredicateAnalyzer и PlotGenerator.
/// </summary>
public class PlotPredicateService : IPlotPredicateService
{
    private readonly PredicateAnalyzer _analyzer;
    private readonly PlotGenerator _plotGenerator;

    public PlotPredicateService(PredicateAnalyzer analyzer, PlotGenerator plotGenerator)
    {
        _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
        _plotGenerator = plotGenerator ?? throw new ArgumentNullException(nameof(plotGenerator));
    }

    public List<TruthSegment> GetTruthSegments(Predicate predicate, double min, double max, double step)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return _analyzer.CalculateTruthSet(predicate, min, max, step);
    }

    public PredicateAnalyzer.PredicateType GetPredicateType(Predicate predicate, double min, double max, double step)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return _analyzer.DeterminePredicateType(predicate, min, max, step);
    }

    public PlotModel Generate1DPlot(List<TruthSegment> segments, double min, double max)
    {
        if (segments == null)
            throw new ArgumentNullException(nameof(segments));

        return _plotGenerator.Create1DPlot(segments, min, max);
    }
    public bool EvaluateQuantifiedStatement(Predicate predicate, PredicateAnalyzer.QuantifierEvaluationType type, double min, double max, double step)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return _analyzer.EvaluateQuantifiedStatement(predicate, type, min, max, step);
    }
}
