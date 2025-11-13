using System;
using System.Linq.Expressions;
using Xunit;
using System.Linq;
using NCalc;
using System.Collections.Generic;
using NUnit.Framework;
namespace TestProject2
{
    public class UnitTest1
    {


public class PredicateLogicTests
    {
        // TODO: Я ХЗ 
        //[Fact(DisplayName = "NormalizeToNCalc: корректная нормализация логических и математических символов")]
        //public void NormalizeToNCalc_ShouldNormalizeCorrectly()
        //{
        //    var parser = new Parser();

        //    string input = "¬(x ≥ 5 ∧ y ≤ 10) → z = 3";
        //    string expected = "not (x >= 5 and y <= 10) => z == 3";

        //    string actual = parser.NormalizeToNCalc(input);

        //    Xunit.Assert.Equal(expected, actual);
        //}

        [Fact(DisplayName = "IsPredicate: корректно определяет предикат (положительный случай)")]
        public void IsPredicate_ShouldDetectPredicate_Positive()
        {
            var parser = new Parser();
            string expression = "x > 5";

            bool result = parser.IsPredicate(expression);

            Xunit.Assert.True(result);
        }

//TODO: СДЕЛАТЬ ЧЕРЕЗ ФЛАГ
        //[Fact(DisplayName = "IsPredicate: корректно определяет не-предикат (отрицательный случай)")]
        //public void IsPredicate_ShouldDetectPredicate_Negative()
        //{
        //    var parser = new Parser();
        //    string expression = "∀x (x > 0)";

        //    bool result = parser.IsPredicate(expression);

        //    Xunit.Assert.False(result); // Содержит квантор, а не просто сравнение переменных
        //}

        [Fact(DisplayName = "PredicateAnalyzer: корректно определяет область истинности простого предиката (x > 0)")]
        public void CalculateTruthSet_ShouldReturnCorrectSegments_ForSimplePredicate()
        {
            var analyzer = new PredicateAnalyzer();
            var expr = new NCalc.Expression("x > 0");
            var predicate = new Predicate(expr, false);

            List<TruthSegment> result = analyzer.CalculateTruthSet(predicate, -2, 2, 1);

            Xunit.Assert.Single(result);
            Xunit.Assert.Equal(1, result[0].Start);
            Xunit.Assert.Equal(2, result[0].End);
        }



            [Fact(DisplayName = "PredicateAnalyzer: корректно определяет область истинности выражения с квантором ∃x (x > 0)")]
        public void CalculateTruthSet_ForExistsQuantifier_ShouldReturnNonEmpty()
        {
            var analyzer = new PredicateAnalyzer();
            var expr = new NCalc.Expression("x > 0");

                var predicate = new Predicate(expr, true);

            List<TruthSegment> result = analyzer.CalculateTruthSet(predicate, -2, 2, 1);

            // Для ∃x(x>0) область истинности должна быть непустой
            Xunit.Assert.NotEmpty(result);
            Xunit.Assert.True(result[0].Start > 0);
        }

        [Fact(DisplayName = "PredicateAnalyzer: определяет тип предиката как Satisfiable")]
        public void DeterminePredicateType_ShouldReturnSatisfiable()
        {
            var analyzer = new PredicateAnalyzer();
            var expr = new NCalc.Expression("x > 0");

                var predicate = new Predicate(expr, false);

            var result = analyzer.DeterminePredicateType(predicate, -2, 2, 1);

            Xunit.Assert.Equal(PredicateAnalyzer.PredicateType.Satisfiable, result);
        }


            [Fact(DisplayName = "PredicateAnalyzer: для ∃x(x>0) возвращает AlwaysTrue")]
        public void DeterminePredicateType_ForExistsQuantifier_ShouldReturnAlwaysTrue()
        {
            var analyzer = new PredicateAnalyzer();
            var expr = new NCalc.Expression("x > 0");
                var predicate = new Predicate(expr, true);

            var result = analyzer.DeterminePredicateType(predicate, 1, 2, 1);

            Xunit.Assert.Equal(PredicateAnalyzer.PredicateType.AlwaysTrue, result);
        }
    }

}
}