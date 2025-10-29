using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lab_2_verevka
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

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