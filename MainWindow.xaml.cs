using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lab4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        delegate double MathOperation(double a);
        static double Square(double a) => a * a;
        static double SquareRoot(double a) => Math.Sqrt(a);
        static double Reciprocal(double a) => 1 / a;
        MathOperation operation;


        public MainWindow()
        {
            
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OperationBox.SelectedItem == null)
                return;

            var item = (ComboBoxItem)OperationBox.SelectedItem;
            string selected = item.Content.ToString();


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
            }
            ResultText.Text = "Selected: " + selected;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (operation == null)
            {
                ResultText.Text = "Select operation first!";
                return;
            }

            if (!double.TryParse(Txtbox1.Text, out double value))
            {
                ResultText.Text = "Invalid number!";
                return;
            }

            if (value == 0 && operation == Reciprocal)
            {
                ResultText.Text = "Cannot divide by zero!";
                return;
            }

            double result = operation(value);
            ResultText.Text = "Equals: " + result;
        }
    }
}
