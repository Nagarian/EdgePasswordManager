using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Windows.Security.Credentials;

namespace PasswordManager
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Item> items;
        private readonly PasswordVault vault;
        private List<Item> initialItems;

        public MainWindow()
        {
            InitializeComponent();
            vault = new PasswordVault();
            var cred = vault.RetrieveAll();
            items = new ObservableCollection<Item>();

            foreach (var item in cred)
            {
                items.Add(new Item
                {
                    UserName = item.UserName,
                    ResourceName = item.Resource,
                    OtherInfo = string.Join(Environment.NewLine, item.Properties.Select(x => $"{x.Key}{x.Value}"))
                });
            }

            initialItems = items.OrderBy(x => x.DomainName).ToList();
            //items = new ObservableCollection<Item>(initialItems);
            list.ItemsSource = initialItems;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(list.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("DomainName");
            view.GroupDescriptions.Add(groupDescription);
            view.Filter = ItemFilter;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is Item item)
            {
                var vaultItem = vault.FindAllByResource(item.ResourceName).FirstOrDefault(x => x.UserName == item.UserName);

                if (MessageBox.Show($"Are you sure to delete credentials ' {item.UserName} ' from '{item.ResourceUri}' ?", "This action is irreversible", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    vault.Remove(vaultItem);
                    initialItems.Remove(item);
                    CollectionViewSource.GetDefaultView(list.ItemsSource).Refresh();
                }
            }
        }

        private bool ItemFilter(object item)
        {
            if (String.IsNullOrEmpty(searchBox.Text))
                return true;
            else
                return ((item as Item).ResourceName.IndexOf(searchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void MakeASearch(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(list.ItemsSource).Refresh();
        }

        private void GoTo_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is Item item)
            {
                Process.Start(item.ResourceName);
            }
        }
    }

    class Item
    {
        public string ResourceName { get; set; }

        public Uri ResourceUri { get => new Uri(ResourceName); }

        public string DomainName { get => GetDomain.GetDomainFromUrl(ResourceName); }

        public string UserName { get; set; }

        public string OtherInfo { get; set; }
    }
}
