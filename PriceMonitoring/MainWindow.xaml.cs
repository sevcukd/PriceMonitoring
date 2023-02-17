using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Path = System.IO.Path;
using MessageBox = System.Windows.Forms.MessageBox;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics.Metrics;
using System.ComponentModel;
using Microsoft.VisualBasic.ApplicationServices;

namespace PriceMonitoring
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public ObservableCollection<Product> Products { get; set; }
        public string PathToFile { get; set; } = string.Empty;
        public Product curProduct { get; set; }
        public DateTime DateFromSerch { get; set; } = DateTime.Now;
        public DateTime DateFromSerchChangePrice { get; set; } = DateTime.Now;
        public CollectionView view { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Products = new ObservableCollection<Product>();
            //Products = new ObservableCollection<Product>() {
            //new Product() { Number = 1, NameProduct = "3", CreateDate = new DateTime(2023, 2, 15), Price = 20 },
            //new Product() { Number = 1, NameProduct = "3", CreateDate = new DateTime(2023, 2, 16), Price = 23 },
            //new Product() { Number = 2, NameProduct = "1", CreateDate = DateTime.Now, Price = 21 },
            //new Product() { Number = 3, NameProduct = "4", CreateDate = DateTime.Now, Price = 33 }};
            ListProduct.ItemsSource = Products;


        }

        private void ListProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PathToFileCanged(object sender, TextChangedEventArgs e)
        {
            PathToFile = PathToFileTextBox.Text;
        }

        private void OpenToFilePath(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel file (*.csv)|*.csv;*.CSV|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                try
                {
                    PathToFile = Path.GetFullPath(openFileDialog.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Помилка читання файлу");
                    return;
                }

                PathToFileTextBox.Text = PathToFile;
            }
        }


        private void GetDataButton(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(PathToFile))
                foreach (string line in System.IO.File.ReadLines(PathToFile))
                {
                    string[] dataFromLineFile = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    int.TryParse(dataFromLineFile[0], out int number);
                    DateTime createDate = DateTime.Parse(dataFromLineFile[2]);
                    double.TryParse(dataFromLineFile[3], out double price);
                    Products.Add(new Product() { Number = number, NameProduct = dataFromLineFile[1], CreateDate = createDate, Price = price });
                }
            else
            {
                MessageBox.Show("Спочатку вкажіть шлях до файлу");
                OpenToFilePath(this, e);
            }

        }

        private void SaveDataButton(object sender, RoutedEventArgs e)
        {
            List<string> StrWriteData = new List<string>();
            foreach (var item in Products)
            {
                StrWriteData.Add($"{item.Number};{item.NameProduct};{item.CreateDate};{item.Price}");
            }
            using (var dialog = new System.Windows.Forms.SaveFileDialog())
            {
                dialog.Filter = "Excel file (*.csv)|*.csv;*.CSV|All files (*.*)|*.*";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.AppendAllLines(dialog.FileName, StrWriteData);
                    PathToFile = dialog.FileName;
                    PathToFileTextBox.Text = PathToFile;
                }
            }
        }

        private void AddNewProductButton(object sender, RoutedEventArgs e)
        {
            int NewNumber = 0;
            for (int i = 0; i < Products.Count; i++)
                if (NewNumber < Products[i].Number)
                    NewNumber = Products[i].Number;


            Products.Add(new Product() { Number = NewNumber + 1, NameProduct = "", CreateDate = DateTime.Now, Price = 0 });
        }

        private void SortByNameButton(object sender, RoutedEventArgs e)
        {

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListProduct.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("NameProduct", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("CreateDate", ListSortDirection.Ascending));
        }

        private void SerchFromDateTime(object sender, RoutedEventArgs e)
        {
            view = (CollectionView)CollectionViewSource.GetDefaultView(ListProduct.ItemsSource);
            view.Filter = UserFilter;
            CollectionViewSource.GetDefaultView(ListProduct.ItemsSource).Refresh();
        }
        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(DateFromSerch.ToString("MM/dd/yyyy")))
                return true;
            else
                return ((item as Product).CreateDate.ToString("MM/dd/yyyy").IndexOf(DateFromSerch.ToString("MM/dd/yyyy"), StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void CencelSerchFromDateTime(object sender, RoutedEventArgs e)
        {
            view.Filter = null;
        }

        private void PriceChangePerMonth(object sender, RoutedEventArgs e)
        {
            CalculateChangePrice(31);

        }
        private void CalculateChangePrice(int NumberOfDays)
        {
            ObservableCollection<Product> products = new ObservableCollection<Product>();
            for (int i = 0; i < Products.Count; i++)
            {
                TimeSpan ts = DateFromSerchChangePrice.Subtract(Products[i].CreateDate);
                var Day = ts.Days;
                double maxVal = Products[i].Price, minVal = Products[i].Price;

                if (Day <= NumberOfDays)
                    for (int j = 0; j < Products.Count; j++)
                    {
                        TimeSpan ts2 = DateFromSerchChangePrice.Subtract(Products[j].CreateDate);
                        var Day2 = ts2.Days;
                        if (Day2 <= NumberOfDays)
                            if (Products[i].NameProduct == Products[j].NameProduct)
                            {
                                maxVal = maxVal > Products[j].Price ? maxVal : Products[j].Price;
                                minVal = minVal < Products[j].Price ? minVal : Products[j].Price;
                            }
                    }
                products.Add(new Product()
                {
                    NameProduct = Products[i].NameProduct,
                    Number = Products[i].Number,
                    CreateDate = Products[i].CreateDate,
                    Price = Products[i].Price,
                    PriceChange = maxVal - minVal,
                    PercentageIncreasePrice = (1 - minVal / maxVal) * 100
                });
            }
            ListProduct.ItemsSource = products;
        }
        private void PriceIncreaseForTheYear(object sender, RoutedEventArgs e)
        {
            CalculateChangePrice(365);
        }

        private void CencelChangePrice(object sender, RoutedEventArgs e)
        {
            ListProduct.ItemsSource = Products;
        }
    }

}
public class Product
{
    public int Number { get; set; }
    public string NameProduct { get; set; }
    public DateTime CreateDate { get; set; }
    public double Price { get; set; }
    public double PriceChange { get; set; } = 0;
    public double PercentageIncreasePrice { get; set; } = 0;

}

