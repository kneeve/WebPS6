using System;
using System.IO;
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

namespace WebScrapeWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Scraper scraper;
        public MainWindow()
        {
            InitializeComponent();
            scraper = new Scraper();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> enrollments = scraper.GetEnrollments(SemesterComboBox.Text, YearTextbox.Text);
            DataTextbox.Text = MakeDisplayString(enrollments);
            SaveButton.IsEnabled = true;
        }
        
        private string MakeDisplayString(List<string> strings)
        {
            string res = "";
            foreach (string s in strings)
            {
                res += s + "\n";
            }
            return res;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var csv = new StringBuilder();

            foreach (string s in scraper.csEnrollments)
            {
                csv.AppendLine(s);
            }

            File.WriteAllText("../../Output/data.csv", csv.ToString());

            MessageBoxResult result = MessageBox.Show("Your data has been saved!",
                                          "Confirmation",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);
        }

        private void DescButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> descriptions = scraper.getDescriptions(ClassTextbox.Text);
            string res = MakeDisplayString(descriptions);
            DataTextbox.Text = res;
        }
    }
}
