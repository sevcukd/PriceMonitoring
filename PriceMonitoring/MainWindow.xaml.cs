using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Path = System.IO.Path;
using MessageBox = System.Windows.Forms.MessageBox;
using System.ComponentModel;

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


        /// <summary>
        /// Зміна шляху при прописі його вручну
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PathToFileCanged(object sender, TextChangedEventArgs e)
        {
            PathToFile = PathToFileTextBox.Text;
        }
        /// <summary>
        /// Пошук вже готового файлу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Отримання даних з файлу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Збереження даних у новий файл або перезапис вже існуючого
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Додавання нового поля в таблиці і заповнення його
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewProductButton(object sender, RoutedEventArgs e)
        {
            int NewNumber = 0;
            for (int i = 0; i < Products.Count; i++)
                if (NewNumber < Products[i].Number)
                    NewNumber = Products[i].Number;


            Products.Add(new Product() { Number = NewNumber + 1, NameProduct = "", CreateDate = DateTime.Now, Price = 0 });
        }
        /// <summary>
        /// сортування по назві товару і якщо є декілька цін на нього тоді щей по даті
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortByNameButton(object sender, RoutedEventArgs e)
        {

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListProduct.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("NameProduct", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("CreateDate", ListSortDirection.Ascending));
        }
        /// <summary>
        /// Пошук продуктів по обраному дню
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerchFromDateTime(object sender, RoutedEventArgs e)
        {
            view = (CollectionView)CollectionViewSource.GetDefaultView(ListProduct.ItemsSource);
            view.Filter = UserFilter;
            CollectionViewSource.GetDefaultView(ListProduct.ItemsSource).Refresh();
        }
       /// <summary>
       /// Фільтр який виконує відбір по даті (не потрібні дані просто приховуються)
       /// </summary>
       /// <param name="item"></param>
       /// <returns></returns>
        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(DateFromSerch.ToString("MM/dd/yyyy")))
                return true;
            else
                return ((item as Product).CreateDate.ToString("MM/dd/yyyy").IndexOf(DateFromSerch.ToString("MM/dd/yyyy"), StringComparison.OrdinalIgnoreCase) >= 0);
        }
        /// <summary>
        /// Відключення фільтра для показу всіх товарів (відміна пошуку по даті)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CencelSerchFromDateTime(object sender, RoutedEventArgs e)
        {
            view.Filter = null;
        }
        /// <summary>
        /// Зміна цін на товар за місяць від заданої дати
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PriceChangePerMonth(object sender, RoutedEventArgs e)
        {
            CalculateChangePrice(31);

        }
        /// <summary>
        /// Обчислення зміни ціна на товар по заданим дням
        /// </summary>
        /// <param name="NumberOfDays">Кількість днів для глибини пошуку</param>
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
        /// <summary>
        /// Зміна цін на товар за рік від заданої дати
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PriceIncreaseForTheYear(object sender, RoutedEventArgs e)
        {
            CalculateChangePrice(365);
        }
        /// <summary>
        /// Відміна обрахування цін
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CencelChangePrice(object sender, RoutedEventArgs e)
        {
            ListProduct.ItemsSource = Products;
        }
    }

}
/// <summary>
/// Клас для заповнення даних
/// </summary>
public class Product
{
    /// <summary>
    /// Номер товару
    /// </summary>
    public int Number { get; set; }
    /// <summary>
    /// Назва товару
    /// </summary>
    public string NameProduct { get; set; }
    /// <summary>
    /// Дата заведення ціни на товар
    /// </summary>
    public DateTime CreateDate { get; set; }
    /// <summary>
    /// Ціна на товар
    /// </summary>
    public double Price { get; set; }
    /// <summary>
    /// Абсолютна зміна ціни за період
    /// </summary>
    public double PriceChange { get; set; } = 0;
    /// <summary>
    /// Відсоток зміни ціни за період
    /// </summary>
    public double PercentageIncreasePrice { get; set; } = 0;

}

