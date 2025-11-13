using OxyPlot;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace lab_2_verevka
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IParserManager _parserManager;
        private readonly IPlotPredicateService _plotService;

        /// <summary>
        /// Модель для OxyPlot, к которой привязан XAML.
        /// </summary>
        public PlotModel PlotModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация ПИ
            var parser = new Parser();
            var analyzer = new PredicateAnalyzer();
            var generator = new PlotGenerator();

            _parserManager = new ParserManager(parser);
            _plotService = new PlotPredicateService(analyzer, generator);

            // Устанавливаем DataContext, чтобы {Binding PlotModel} работал
            this.DataContext = this;

            // Инициализация пустой модели графика
            PlotModel = new PlotModel { Title = "График истинности (пусто)" };
            TruthPlotView.Model = PlotModel;
        }

        // --- Обработчики кнопок ---

        /// <summary>
        /// Обработчик для кнопки "Применить область".
        /// Заполняет список дискретных значений.
        /// </summary>
        private void ApplyDomainButton_Click(object sender, RoutedEventArgs e)
        {
            DomainValuesList.Items.Clear();
            ResultBox.Text = "";
            ResultBox.Background = Brushes.White;

            try
            {
                if (!double.TryParse(MinInput.Text, out double min) ||
                    !double.TryParse(MaxInput.Text, out double max) ||
                    !double.TryParse(StepInput.Text, out double step))
                {
                    throw new ArgumentException("Min, Max и Step должны быть числовыми значениями.");
                }

                if (step <= 0) throw new ArgumentException("Шаг (Step) должен быть > 0.");
                if (min >= max) throw new ArgumentException("Min должен быть меньше Max.");

                // Заполнение ListBox
                for (double x = min; x <= max; x += step)
                {
                    DomainValuesList.Items.Add($"x = {x:F4}");
                    // Ограничиваем количество, чтобы не зависнуть
                    if (DomainValuesList.Items.Count > 1000)
                    {
                        DomainValuesList.Items.Add("... (Слишком много точек)");
                        break;
                    }
                }

                if (DomainValuesList.Items.Count == 0)
                {
                    DomainValuesList.Items.Add("Область пуста (проверьте Min/Max/Step).");
                }

                ResultBox.Text = $"Область определения успешно установлена: [{min:F2}..{max:F2}] с шагом {step:F4}.";

            }
            catch (Exception ex)
            {
                ResultBox.Background = Brushes.MistyRose;
                ResultBox.Text = $"ОШИБКА ОБЛАСТИ:\n{ex.Message}";
            }
        }


        /// <summary>
        /// Главный обработчик, вызывающий ПИ для анализа и построения графика.
        /// </summary>
        private void BuildGraphButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Очистка предыдущих результатов
            ResultBox.Text = "";
            ResultBox.Background = Brushes.White;
            PlotModel = new PlotModel(); // Очистка графика
            TruthPlotView.Model = PlotModel; // Обновление OxyPlot

            try
            {
                // --- 2. Сбор и Валидация Ввода (Уровень ПИ/View) ---
                string formula = FormulaInput.Text;

                if (string.IsNullOrWhiteSpace(formula))
                {
                    throw new ArgumentException("Формула не может быть пустой.");
                }

                // Проверка домена (повторяем, так как кнопка "Применить" могла быть пропущена)
                if (!double.TryParse(MinInput.Text, out double min) ||
                    !double.TryParse(MaxInput.Text, out double max) ||
                    !double.TryParse(StepInput.Text, out double step))
                {
                    throw new ArgumentException("Min, Max и Step должны быть числами.");
                }
                if (step <= 0) throw new ArgumentException("Шаг (Step) должен быть > 0.");
                if (min >= max) throw new ArgumentException("Min должен быть меньше Max.");

                // --- 3. Вызов Сервиса Парсера (ПИ 1) ---

                // Проверка, является ли это предикатом
                if (!_parserManager.IsPredicate(formula))
                {
                    throw new ArgumentException("Введенная строка не является предикатом (нет переменных или оператора сравнения).");
                }

                // Получаем данные от парсера
                bool hasQuantifiers = _parserManager.HasQuantifiers(formula);
                string ncalcText = _parserManager.NormalizeToNCalc(formula);

                // --- 4. Создание предиката (общая часть) ---

                // Создание объекта Predicate
                Predicate predicate;
                try
                {
                    var ncalcExpr = new NCalc.Expression(ncalcText);
                    predicate = new Predicate(ncalcExpr, hasQuantifiers);
                }
                catch (Exception ncalcEx)
                {
                    // Ловим ошибку синтаксиса, которую NCalc не может разобрать
                    throw new ArgumentException($"Ошибка синтаксиса в формуле: {ncalcEx.Message}");
                }

                // --- 5. Принятие решения (Логика ПИ) ---

                if (hasQuantifiers)
                {
                    // Определяем тип квантора
                    string quantifierType = formula.Trim().ToLower().StartsWith("forall") || formula.Trim().StartsWith("∀")
                        ? "forall"
                        : "exists";

                    // Преобразуем в enum для вызова сервиса
                    var quantifierEnum = quantifierType == "forall"
                        ? PredicateAnalyzer.QuantifierEvaluationType.Universal
                        : PredicateAnalyzer.QuantifierEvaluationType.Existential;

                    // Вычисляем истинность высказывания с квантором
                    bool isTrue = _plotService.EvaluateQuantifiedStatement(predicate, quantifierEnum, min, max, step);


                    // Формируем результат
                    string quantifierName = quantifierType == "forall" ? "∀ (для всех)" : "∃ (существует)";
                    string truthResult = isTrue ? "ИСТИНА" : "ЛОЖЬ";

                    ResultBox.Background = isTrue ? Brushes.LightGreen : Brushes.LightPink;
                    ResultBox.Text = $"ВЫСКАЗЫВАНИЕ С КВАНТОРОМ:\n" +
                                   $"Формула: {formula}\n" +
                                   $"Квантор: {quantifierName}\n" +
                                   $"Домен: [{min}, {max}], шаг: {step}\n" +
                                   $"Результат: {truthResult}\n\n" +
                                   $"Высказывание {(isTrue ? "истинно" : "ложно")} на заданном домене.";
                    return;
                }

                // --- 6. Обработка обычных предикатов (ПИ 2) ---

                // Вызов анализатора
                var type = _plotService.GetPredicateType(predicate, min, max, step);
                var segments = _plotService.GetTruthSegments(predicate, min, max, step);

                // Построение графика
                PlotModel = _plotService.Generate1DPlot(segments, min, max);
                TruthPlotView.Model = PlotModel; // Обновление OxyPlot

                // Вывод результата
                ResultBox.Text = $"Анализ завершен.\n" +
                                $"Тип предиката: {type}\n" +
                                $"Область NCalc: {ncalcText}\n" +
                                $"Найдено отрезков истинности: {segments.Count}";
            }
            catch (Exception ex)
            {
                // --- 7. Обработка ЛЮБОЙ Ошибки ---
                ResultBox.Background = Brushes.MistyRose;
                ResultBox.Text = $"ОШИБКА:\n{ex.Message}";
            }
        }
        // --- Вспомогательные методы (Ваш код для кнопок символов) ---

        // Единый обработчик для всех кнопок символов
        private void InsertSymbol_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string symbol = button.Content.ToString();
                InsertTextAtCaret(FormulaInput, symbol);
            }
        }

        // Метод вставки текста в место курсора
        private void InsertTextAtCaret(TextBox textBox, string text)
        {
            int selectionStart = textBox.SelectionStart;
            textBox.Text = textBox.Text.Insert(selectionStart, text);
            textBox.SelectionStart = selectionStart + text.Length;
            textBox.Focus();
        }
    }
}