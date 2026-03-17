using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace lab4
{
    public partial class MainWindow : Window
    {
        delegate double MathOperation(double a);

        static double Square(double a) => a * a;
        static double SquareRoot(double a) => Math.Sqrt(a);
        static double Reciprocal(double a) => 1 / a;

        private MathOperation operation;

        public MainWindow()
        {
            InitializeComponent();
            ResultText.Text = "Choose an operation to begin.";
        }

        // Required: method that accepts a delegate as a parameter to process the collection
        private static List<double> ProcessCollection(IEnumerable<double> values, MathOperation op)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (op == null) throw new ArgumentNullException(nameof(op));

            var result = new List<double>();

            foreach (var v in values)
            {
                if (op == SquareRoot && v < 0)
                    throw new ArgumentOutOfRangeException(nameof(values), "Square Root is not defined for negative numbers.");

                if (op == Reciprocal && v == 0)
                    throw new DivideByZeroException("Reciprocal is not defined for zero.");

                result.Add(op(v));
            }

            return result;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OperationBox.SelectedItem == null)
                return;

            var item = (ComboBoxItem)OperationBox.SelectedItem;
            var selected = item.Content?.ToString() ?? string.Empty;

            switch (selected)
            {
                case "Square":
                    operation = Square;
                    break;
                case "Square Root":
                    operation = SquareRoot;
                    break;
                case "Reciprocal":
                    operation = Reciprocal;
                    break;
                default:
                    operation = null;
                    break;
            }

            ResultText.Text = "Selected: " + selected;
        }

        private void ProcessCollectionButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Clear();

            if (operation == null)
            {
                ResultText.Text = "Select an operation first!";
                return;
            }

            if (!TryParseDoubles(InputTextBox.Text, out var values, out var error))
            {
                ResultText.Text = error;
                return;
            }

            try
            {
                var results = ProcessCollection(values, operation);

                // show one value per line for easy validation
                OutputTextBox.Text = string.Join(Environment.NewLine, results.Select(r => r.ToString("G", CultureInfo.InvariantCulture)));
                ResultText.Text = $"Processed {results.Count} value(s).";
            }
            catch (Exception ex)
            {
                ResultText.Text = ex.Message;
            }
        }

        private static bool TryParseDoubles(string text, out List<double> values, out string error)
        {
            values = new List<double>();
            error = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                error = "Enter one or more numbers first.";
                return false;
            }

            var parts = text.Split(new[] { ',', ';', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in parts)
            {
                // InvariantCulture makes parsing consistent (dot decimal)
                if (!double.TryParse(p, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v))
                {
                    error = "Invalid number in list: " + p;
                    return false;
                }

                values.Add(v);
            }

            if (values.Count == 0)
            {
                error = "Enter one or more numbers first.";
                return false;
            }

            return true;
        }

        private void LoadSampleButton_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = "1, 2, 3\n4.5 9\n0.25";
            ResultText.Text = "Sample input loaded.";
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            InputTextBox.Clear();
            OutputTextBox.Clear();
            ResultText.Text = "Cleared.";
        }

        // Keep your old single-value handler if it’s still wired anywhere (safe to leave)
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ResultText.Text = "Use 'Process Collection' for the collection requirement.";
        }
    }
}