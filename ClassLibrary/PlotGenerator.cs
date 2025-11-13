using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Collections.Generic;

public class PlotGenerator
{
    /// <summary>
    /// Создает 1D-график OxyPlot на основе отрезков истинности.
    /// </summary>
    public PlotModel Create1DPlot(List<TruthSegment> segments, double min, double max)
    {
        var plotModel = new PlotModel { Title = "Область Истинности" };

        // 1. Ось X (Значения)
        plotModel.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Minimum = min,
            Maximum = max,
            Title = "X"
        });

        // 2. Ось Y (просто для визуализации, скрываем ее)
        plotModel.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            IsAxisVisible = false, // Скрываем ось Y
            Minimum = -1,
            Maximum = 1
        });

        // 3. Серия данных (отрезки)
        // Мы будем использовать 'IntervalBarSeries' или 'AreaSeries' для отрезков.
        // Самый простой способ - использовать 'ScatterSeries' для точек.
        // Но для "отрезков" лучше 'LineSeries' или 'AreaSeries'.

        var series = new AreaSeries
        {
            Color = OxyColors.Green,
            Fill = OxyColors.LightGreen,
            StrokeThickness = 2
        };

        if (segments.Count == 0)
        {
            // Если область пуста, рисуем "0" на всем протяжении
            series.Points.Add(new DataPoint(min, 0));
            series.Points.Add(new DataPoint(max, 0));
        }
        else
        {
            // Рисуем отрезки на высоте Y=1, с разрывами (Y=0)
            double lastEnd = min;
            foreach (var seg in segments)
            {
                // Разрыв
                if (seg.Start > lastEnd)
                {
                    series.Points.Add(new DataPoint(lastEnd, 0));
                    series.Points.Add(new DataPoint(seg.Start, 0));
                }

                // Отрезок
                series.Points.Add(new DataPoint(seg.Start, 1));
                series.Points.Add(new DataPoint(seg.End, 1));
                lastEnd = seg.End;
            }
            // Завершаем график
            if (lastEnd < max)
            {
                series.Points.Add(new DataPoint(lastEnd, 0));
                series.Points.Add(new DataPoint(max, 0));
            }
        }

        plotModel.Series.Add(series);
        return plotModel;
    }
}