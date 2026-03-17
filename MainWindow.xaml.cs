using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace lab4
{
    public partial class MainWindow : Window
    {
        delegate double MathOperation(double a);

        delegate bool Comparer<T>(T a, T b);

        static double Square(double a) => a * a;
        static double SquareRoot(double a) => Math.Sqrt(a);
        static double Reciprocal(double a) => 1 / a;

        private static void BubbleSort<T>(T[] arr, Comparer<T> compare)
        {
            for (int i = 0; i < arr.Length - 1; i++)
                for (int j = 0; j < arr.Length - i - 1; j++)
                    if (compare(arr[j], arr[j + 1]))
                    {
                        T temp = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = temp;
                    }
        }

        private bool AscendingComparer<T>(T a, T b) where T : IComparable<T>
            => a.CompareTo(b) > 0;

        private bool DescendingComparer<T>(T a, T b) where T : IComparable<T>
            => a.CompareTo(b) < 0;

        private MathOperation operation;

        // Multicast delegate for UI styling
        private delegate void UiStyleAction();
        private UiStyleAction uiStyleMulticast;

        // Defaults for reset
        private Brush defaultWindowBackground;
        private Brush defaultResultForeground;
        private double defaultResultFontSize;

        public MainWindow()
        {
            InitializeComponent();

            CaptureDefaultUi();

            // initial build (based on default checked boxes)
            BuildUiStyleMulticast();

            // slider label update
            FontSizeSlider.ValueChanged += (s, e) =>
            {
                if (FontSizeValueText != null)
                    FontSizeValueText.Text = "Selected size: " + ((int)FontSizeSlider.Value).ToString(CultureInfo.InvariantCulture);
            };

            ResultText.Text = "Ready.";
        }

        private void SortInputButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Clear();

            var ascending = (AscRadio.IsChecked == true);

            if (SortAsStringRadio.IsChecked == true)
            {
                var items = InputTextBox.Text
                    .Split(new[] { ',', ';', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToArray();

                if (items.Length == 0)
                {
                    ResultText.Text = "Enter one or more strings to sort.";
                    return;
                }

                Comparer<string> comparer = ascending
                    ? (Comparer<string>)AscendingComparer<string>
                    : (Comparer<string>)DescendingComparer<string>;

                BubbleSort(items, comparer);

                OutputTextBox.Text = string.Join(Environment.NewLine, items);
                ResultText.Text = ascending ? "Sorted strings ascending." : "Sorted strings descending.";
                return;
            }

            // else sort numbers
            if (!TryParseDoubles(InputTextBox.Text, out var values, out var error))
            {
                ResultText.Text = error;
                return;
            }

            var arr = values.ToArray();

            Comparer<double> numComparer = ascending
                ? (Comparer<double>)AscendingComparer<double>
                : (Comparer<double>)DescendingComparer<double>;

            BubbleSort(arr, numComparer);

            OutputTextBox.Text = string.Join(Environment.NewLine, arr.Select(v => v.ToString("G", CultureInfo.InvariantCulture)));
            ResultText.Text = ascending ? "Sorted numbers ascending." : "Sorted numbers descending.";
        }

        private void CaptureDefaultUi()
        {
            defaultWindowBackground = Background;
            defaultResultForeground = ResultText.Foreground;
            defaultResultFontSize = ResultText.FontSize;
        }

        private void ResetUiToDefaults()
        {
            Background = defaultWindowBackground;
            ResultText.Foreground = defaultResultForeground;
            ResultText.FontSize = defaultResultFontSize;
        }

        // Build multicast delegate based on what the user checked
        private void BuildUiStyleMulticast()
        {
            uiStyleMulticast = null;

            if (BgMethodCheckBox.IsChecked == true)
                uiStyleMulticast += ChangeBackgroundColor;

            if (FgMethodCheckBox.IsChecked == true)
                uiStyleMulticast += ChangeFontColor;

            if (SizeMethodCheckBox.IsChecked == true)
                uiStyleMulticast += ChangeLabelFontSize;
        }

        // --- Styling methods (targets required by the assignment) ---

        private void ChangeBackgroundColor()
        {
            var selected = GetSelectedComboText(BackgroundColorBox);

            switch (selected)
            {
                case "Light":
                    Background = Brushes.WhiteSmoke;
                    break;
                case "Blue":
                    Background = new SolidColorBrush(Color.FromRgb(25, 50, 120));
                    break;
                case "Green":
                    Background = new SolidColorBrush(Color.FromRgb(20, 90, 60));
                    break;
                case "Dark":
                default:
                    Background = new SolidColorBrush(Color.FromRgb(30, 30, 38));
                    break;
            }
        }

        private void ChangeFontColor()
        {
            var selected = GetSelectedComboText(FontColorBox);

            switch (selected)
            {
                case "White":
                    ResultText.Foreground = Brushes.White;
                    break;
                case "Black":
                    ResultText.Foreground = Brushes.Black;
                    break;
                case "Red":
                    ResultText.Foreground = Brushes.Red;
                    break;
                case "Orange":
                default:
                    ResultText.Foreground = Brushes.Orange;
                    break;
            }
        }

        private void ChangeLabelFontSize()
        {
            ResultText.FontSize = FontSizeSlider.Value;
        }

        private static string GetSelectedComboText(ComboBox box)
        {
            var item = box.SelectedItem as ComboBoxItem;
            return item?.Content?.ToString() ?? string.Empty;
        }

        // --- UI events for multicast demo ---

        private void BuildMulticastButton_Click(object sender, RoutedEventArgs e)
        {
            BuildUiStyleMulticast();

            var count = uiStyleMulticast?.GetInvocationList().Length ?? 0;
            ResultText.Text = "Multicast built. Methods in list: " + count.ToString(CultureInfo.InvariantCulture);
        }

        private void InvokeMulticastButton_Click(object sender, RoutedEventArgs e)
        {
            uiStyleMulticast?.Invoke();
            ResultText.Text = "Multicast invoked (all selected style methods executed).";
        }

        private void ResetUiButton_Click(object sender, RoutedEventArgs e)
        {
            ResetUiToDefaults();
            ResultText.Text = "UI reset to defaults.";
        }

        // ---------------- Existing collection processing code ----------------

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
    }
}