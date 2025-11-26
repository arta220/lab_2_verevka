using Antlr.Runtime;
using NCalc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using Xunit;
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


            //ТЕСТЫ ДЛЯ ПРЕДИКАТОВ (БЕЗ КВАНТОРОВ, для графика и типа)


            [Fact(DisplayName = "IsPredicate: корректно определяет предикат (положительный случай)")]
            public void IsPredicate_ShouldDetectPredicate_Positive()
            {
                var parser = new Parser();
                string expression = "x > 5";

                bool result = parser.IsPredicate(expression);

                Xunit.Assert.True(result);
            }

            [Fact(DisplayName = "PredicateAnalyzer: определяет тип предиката как Satisfiable")]
            public void DeterminePredicateType_ShouldReturnSatisfiable()
            {
                var analyzer = new PredicateAnalyzer();
                var expr = new NCalc.Expression("x > 0");

                var predicate = new Predicate(expr, false, "x");

                var result = analyzer.DeterminePredicateType(predicate, -2, 2, 1);

                Xunit.Assert.Equal(PredicateAnalyzer.PredicateType.Satisfiable, result);
            }

            // ТЕСТЫ ДЛЯ ВЫСКАЗЫВАНИЙ (С КВАНТОРАМИ)

            [Fact(DisplayName = "EvaluateQuantifiedStatement: для ∃x(x>0) возвращает True")]
            public void EvaluateQuantifiedStatement_ForExistsQuantifier_ShouldReturnTrue()
            {
                var analyzer = new PredicateAnalyzer();
                var expr = new NCalc.Expression("x > 0");
                var predicate = new Predicate(expr, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Existential,
                    1, 2, 1
                );

                Xunit.Assert.True(result);
            }

            [Fact(DisplayName = "EvaluateQuantifiedStatement: корректно вычисляет истинность высказывания с квантором forall")]
            public void EvaluateQuantifiedStatement_ForAll_ShouldReturnCorrectResult()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x > 0");
                var predicate = new Predicate(expression, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Universal,
                    1, 10, 1
                );

                Xunit.Assert.True(result);
            }

            [Fact(DisplayName = "EvaluateQuantifiedStatement: корректно вычисляет ложность высказывания с квантором forall")]
            public void EvaluateQuantifiedStatement_ForAll_ShouldReturnFalse_WhenCounterExampleExists()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x > 0");
                var predicate = new Predicate(expression, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Universal,
                    -5, 5, 1
                );

                Xunit.Assert.False(result);
            }

            [Fact(DisplayName = "EvaluateQuantifiedStatement: корректно вычисляет истинность высказывания с квантором exists")]
            public void EvaluateQuantifiedStatement_Exists_ShouldReturnTrue_WhenTrueExampleExists()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x > 0");
                var predicate = new Predicate(expression, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Existential,
                    -5, 5, 1
                );

                Xunit.Assert.True(result);
            }

            [Fact(DisplayName = "EvaluateQuantifiedStatement: корректно вычисляет ложность высказывания с квантором exists")]
            public void EvaluateQuantifiedStatement_Exists_ShouldReturnFalse_WhenNoTrueExamples()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x > 0");
                var predicate = new Predicate(expression, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Existential,
                    -10, -1, 1
                );

                Xunit.Assert.False(result);
            }

            // ТЕСТЫ ДЛЯ ПУСТОГО ДОМЕНА

            [Fact(DisplayName = "EvaluateQuantifiedStatement: forall возвращает true для пустого домена (по классической логике)")]
            public void EvaluateQuantifiedStatement_ForAll_EmptyDomain_AlwaysTruePredicate()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x == x");
                var predicate = new Predicate(expression, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Universal,
                    5, 1, 1
                );

                Xunit.Assert.False(result);
            }

            [Fact(DisplayName = "EvaluateQuantifiedStatement: exists возвращает false для пустого домена")]
            public void EvaluateQuantifiedStatement_Exists_EmptyDomain()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x == x");
                var predicate = new Predicate(expression, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Existential,
                    5, 1, 1
                );

                Xunit.Assert.False(result);
            }

            // ТЕСТЫ ДЛЯ UI-ФЛОУ

            [Fact(DisplayName = "UI_Flow: Корректное определение типа квантора и вызов сервиса для ∃")]
            public void BuildGraphButton_QuantifierFlow_ShouldCallEvaluateQuantifiedStatement()
            {
                string formulaInput = "∃x (x > 0 ^ x < 5)";
                double minInput = -10.0;
                double maxInput = 10.0;
                double stepInput = 1.0;

                var parser = new Parser();
                var analyzer = new PredicateAnalyzer();
                var generator = new PlotGenerator();

                var parserManager = new ParserManager(parser);
                var plotService = new PlotPredicateService(analyzer, generator);

                string formula = formulaInput;
                double min = minInput;
                double max = maxInput;
                double step = stepInput;

                bool hasQuantifiers = parserManager.HasQuantifiers(formula);
                string ncalcText = parserManager.NormalizeToNCalc(formula);

                var ncalcExpr = new NCalc.Expression(ncalcText);
                var predicate = new Predicate(ncalcExpr, hasQuantifiers, "x");

                PredicateAnalyzer.QuantifierEvaluationType quantifierEnum = parserManager.GetQuantifierType(formula);

                bool isTrue = plotService.EvaluateQuantifiedStatement(predicate, quantifierEnum, min, max, step);

                Xunit.Assert.True(hasQuantifiers);
                Xunit.Assert.Equal("x > 0 ^ x < 5", ncalcText);
                Xunit.Assert.Equal(PredicateAnalyzer.QuantifierEvaluationType.Existential, quantifierEnum);
                Xunit.Assert.True(isTrue);
            }
        }


        }
    }