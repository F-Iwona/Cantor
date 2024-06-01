using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x415

namespace Cantor
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string NBPinfo = "https://static.nbp.pl/dane/kursy/xml/LastA.xml"; // tu adres pliku z danymi kursowymi
        List<ListA> exchangeRate;
        public MainPage()
        {
            this.InitializeComponent();
            exchangeRate = new List<ListA>();
        }
        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var postman = new HttpClient();
                var data = await postman.GetStringAsync(new Uri(NBPinfo));
                var xmlData = XDocument.Parse(data);
                var positionOnly = xmlData.Descendants("pozycja");
                exchangeRate = (from position in positionOnly
                                select new ListA()
                                {
                                    currency_code = position.Element("kod_waluty").Value,
                                    avarage_rate = position.Element("kurs_sredni").Value,
                                    currency_name = position.Element("nazwa_waluty").Value,
                                    converter = position.Element("przelicznik").Value
                                }).ToList();

                exchangeRate.Insert(0, new ListA { currency_code = "PLN", avarage_rate = "1,0000", converter = "1" });

                lbxFromCurrency.ItemsSource = exchangeRate;
                lbxToCurrency.ItemsSource = exchangeRate;

                lbxToCurrency.SelectedIndex = 0;
                lbxFromCurrency.SelectedIndex = 0;

                Convert();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog("Wystąpił błąd podczas pobierania danych z serwera: " + ex.Message);
                await dialog.ShowAsync();
            }
        }

        private void Convert()
        {
            double sum;
            if (double.TryParse(txtSum.Text.Replace('.', ','), out sum))
            {
                var entryRate = double.Parse((lbxFromCurrency.SelectedItem as ListA).avarage_rate);
                var targetRate = double.Parse((lbxToCurrency.SelectedItem as ListA).avarage_rate);
                var sumPLN = sum * entryRate;

                var targetSum = sumPLN / targetRate;

                tbConverted.Text = string.Format("{0:N2}", targetSum);
            }
            else
            {
                tbConverted.Text = string.Empty;
            }
        }

        private void txtSum_TextChanged(object sender, TextChangedEventArgs e)
        {
            Convert();
        }

        private void lbxFromCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Convert();
        }

        private void lbxToCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Convert();
        }
    }
}
