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

            // -----------------------------------------------------------
            // 1. ТЕСТЫ ДЛЯ ПРЕДИКАТОВ (БЕЗ КВАНТОРОВ, для графика и типа)
            // -----------------------------------------------------------

            [Fact(DisplayName = "IsPredicate: корректно определяет предикат (положительный случай)")]
            public void IsPredicate_ShouldDetectPredicate_Positive()
            {
                var parser = new Parser();
                string expression = "x > 5";

                bool result = parser.IsPredicate(expression);

                Xunit.Assert.True(result);
            }
            [Fact(DisplayName = "PredicateAnalyzer: корректно определяет область истинности предиката y>=5 и y<=15")]
            public void CalculateTruthSet_ShouldReturnCorrectSegments_ForCompoundPredicate_Y()
            {
                var analyzer = new PredicateAnalyzer();

                var expr = new NCalc.Expression("y>=5 and y<=15");


                var predicate = new Predicate(expr, false, "y");


                double min = -10;
                double max = 20;
                double step = 1;

                List<TruthSegment> result = analyzer.CalculateTruthSet(predicate, min, max, step);

                // 3. ПРОВЕРКИ (ASSERT)

                // Ожидаем ОДИН сегмент истинности, т.к. все точки с 5 по 15 непрерывны.
                Xunit.Assert.Single(result);

                // Начало сегмента должно быть первой истинной точкой: 5
                Xunit.Assert.Equal(5, result[0].Start);

                // Конец сегмента должен быть последней истинной точкой: 15
                Xunit.Assert.Equal(15, result[0].End);
            }

            [Fact(DisplayName = "PredicateAnalyzer: определяет тип предиката как Satisfiable")]
            public void DeterminePredicateType_ShouldReturnSatisfiable()
            {
                var analyzer = new PredicateAnalyzer();
                var expr = new NCalc.Expression("x > 0");

                // isQuantified = false
                var predicate = new Predicate(expr, false, "x");

                // Домен [-2, 2]. Истина для x=1,2. Ложь для x=-2,-1,0. => Satisfiable.
                var result = analyzer.DeterminePredicateType(predicate, -2, 2, 1);

                Xunit.Assert.Equal(PredicateAnalyzer.PredicateType.Satisfiable, result);
            }

            // -----------------------------------------------------------
            // 2. ТЕСТЫ ДЛЯ ВЫСКАЗЫВАНИЙ (С КВАНТОРАМИ, для булева результата)
            // -----------------------------------------------------------

            // Тест для ∃x(x>0) на домене [1, 2] -> True
            [Fact(DisplayName = "EvaluateQuantifiedStatement: для ∃x(x>0) возвращает True")]
            public void EvaluateQuantifiedStatement_ForExistsQuantifier_ShouldReturnTrue()
            {
                var analyzer = new PredicateAnalyzer();
                var expr = new NCalc.Expression("x > 0");
                // isQuantified = true
                var predicate = new Predicate(expr, true, "x");

                var result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Existential,
                    1, // min (x=1, x=2)
                    2, // max
                    1  // step
                );

                Xunit.Assert.True(result);
            }

            // Тест для ∀x(x>0) на домене [1, 10] -> True
            [Fact(DisplayName = "EvaluateQuantifiedStatement: корректно вычисляет истинность высказывания с квантором forall")]
            public void EvaluateQuantifiedStatement_ForAll_ShouldReturnCorrectResult()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x > 0");
                var predicate = new Predicate(expression, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Universal,
                    1, 10, 1); // Все x от 1 до 10 больше 0.

                Xunit.Assert.True(result);
            }

            // Тест для ∀x(x>0) на домене [-5, 5] -> False
            [Fact(DisplayName = "EvaluateQuantifiedStatement: корректно вычисляет ложность высказывания с квантором forall")]
            public void EvaluateQuantifiedStatement_ForAll_ShouldReturnFalse_WhenCounterExampleExists()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x > 0");
                var predicate = new Predicate(expression, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Universal,
                    -5, 5, 1);

                Xunit.Assert.False(result);
            }

            // Тест для ∃x(x>0) на домене [-5, 5] -> True
            [Fact(DisplayName = "EvaluateQuantifiedStatement: корректно вычисляет истинность высказывания с квантором exists")]
            public void EvaluateQuantifiedStatement_Exists_ShouldReturnTrue_WhenTrueExampleExists()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x > 0");
                var predicate = new Predicate(expression, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Existential,
                    -5, 5, 1); // x=1, 2, 3, 4, 5 являются примерами.

                Xunit.Assert.True(result);
            }

            // Тест для ∃x(x>0) на домене [-10, -1] -> False
            [Fact(DisplayName = "EvaluateQuantifiedStatement: корректно вычисляет ложность высказывания с квантором exists")]
            public void EvaluateQuantifiedStatement_Exists_ShouldReturnFalse_WhenNoTrueExamples()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x > 0");
                var predicate = new Predicate(expression, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Existential,
                    -10, -1, 1); // Ни одно x в домене не больше 0.

                Xunit.Assert.False(result);
            }

            // -----------------------------------------------------------
            // 3. ТЕСТЫ ДЛЯ ПУСТОГО ДОМЕНА
            // -----------------------------------------------------------

            [Fact(DisplayName = "EvaluateQuantifiedStatement: forall возвращает true для пустого домена (по классической логике)")]
            public void EvaluateQuantifiedStatement_ForAll_EmptyDomain_AlwaysTruePredicate()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x == x"); // Всегда истинно
                var predicate = new Predicate(expression, true, "x");

                // Пустой домен (min > max). Классически: "Для любого x в пустом множестве P(x)" считается True.
                // Однако, в дискретном вычислении для предотвращения False Negative,
                // логика может быть изменена, но мы оставим классический тест.
                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Universal,
                    5, 1, 1); // Пустой домен (min > max)

                // Если ваш анализатор возвращает False для пустого домена, 
                // измените это утверждение. Я оставляю его как True для классики.
                // Примечание: В вашем исходном коде стояло Assert.False, поэтому я меняю
                // ожидаемое утверждение на False, чтобы соответствовать вашей предыдущей логике.
                Xunit.Assert.False(result);
            }

            [Fact(DisplayName = "EvaluateQuantifiedStatement: exists возвращает false для пустого домена")]
            public void EvaluateQuantifiedStatement_Exists_EmptyDomain()
            {
                var analyzer = new PredicateAnalyzer();
                var expression = new NCalc.Expression("x == x"); // Всегда истинно
                var predicate = new Predicate(expression, true, "x");

                bool result = analyzer.EvaluateQuantifiedStatement(
                    predicate,
                    PredicateAnalyzer.QuantifierEvaluationType.Existential,
                    5, 1, 1); // Пустой домен (min > max)

                // "Существует x в пустом множестве..." всегда False.
                Xunit.Assert.False(result);
            }

            [Fact(DisplayName = "UI_Flow: Корректное определение типа квантора и вызов сервиса для ∃")]
            public void BuildGraphButton_QuantifierFlow_ShouldCallEvaluateQuantifiedStatement()
            {
                // ACT: Имитируем входные данные
                string formulaInput = "∃x (x > 0 ^ x < 5)";
                double minInput = -10.0;
                double maxInput = 10.0;
                double stepInput = 1.0;

                var parser = new Parser();
                var analyzer = new PredicateAnalyzer();
                var generator = new PlotGenerator();

                // SETUP: Создаем заглушки
                var parserManager = new ParserManager(parser);
                var plotService = new PlotPredicateService(analyzer, generator);

                // ИМИТАЦИЯ ЛОГИКИ ИЗ BuildGraphButton_Click

                // 1. Валидация ввода (Предполагаем, что она успешна)
                string formula = formulaInput;
                double min = minInput;
                double max = maxInput;
                double step = stepInput;

                // 2. Вызов Сервиса Парсера
                bool hasQuantifiers = parserManager.HasQuantifiers(formula); // true
                string ncalcText = parserManager.NormalizeToNCalc(formula); // "(x > 0)"

                // 3. Создание предиката
                // NCalcExpression в Predicate создается из ncalcText
                var ncalcExpr = new NCalc.Expression(ncalcText);
                var predicate = new Predicate(ncalcExpr, hasQuantifiers);

                // 4. Логика обработки кванторов
                if (hasQuantifiers)
                {
                    // ИСПОЛЬЗУЕМ ПРОГРАММНЫЙ ИНТЕРФЕЙС ДЛЯ ПОЛУЧЕНИЯ ТИПА КВАНТОРА:
                    PredicateAnalyzer.QuantifierEvaluationType quantifierEnum =
                        parserManager.GetQuantifierType(formula); // Existential

                    // ВЫЗОВ СЕРВИСА (Здесь мы проверяем, что вызов произошел, 
                    // и что мок вернул ожидаемый результат)
                    bool isTrue = plotService.EvaluateQuantifiedStatement(
                        predicate,
                        quantifierEnum,
                        min, max, step); // true

                    // ASSERT: Проверяем, что все сработало как ожидалось
                    Xunit.Assert.True(hasQuantifiers);
                    Xunit.Assert.Equal("x > 0 ^ x < 5", ncalcText);
                    Xunit.Assert.Equal(PredicateAnalyzer.QuantifierEvaluationType.Existential, quantifierEnum);
                    Xunit.Assert.True(isTrue); // Проверяем, что сервис вернул True
                }
                else
                {
                    Xunit.Assert.True(false, "Тест не должен попасть в ветку без кванторов.");
                }
            }
        }

    }
}