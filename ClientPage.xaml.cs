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
using System.Data.Entity;

namespace Vakhitova_LanguageSchool
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        private List<Client> _filteredClients;
        private int pageSize = 10;
        private int currentPage = 1;

        public ClientPage()
        {
            InitializeComponent();

            var currentClients = Vakhitova_LanguageSchoolEntities.GetContext().Client
                .Include(c => c.ClientService).ToList();
            ClientsListView.ItemsSource = currentClients;

            _filteredClients = currentClients;

            PageListCB.SelectedIndex = 0;

            currentPage = 1;
            ChangePage();
        }

        private void ChangePage()
        {
            if (_filteredClients == null) 
                return; 

            if (_filteredClients.Count == 0)
            {
                ClientsListView.ItemsSource = new List<Client>();
                PageListBox.Items.Clear();
                TBCount.Text = "0";
                TBAllRecords.Text = " из 0";
                return;
            }

            int totalPages = (_filteredClients.Count + pageSize - 1) / pageSize;

            PageListBox.Items.Clear();
            for (int i = 1; i <= totalPages; i++)
            {
                PageListBox.Items.Add(i);
            }
            PageListBox.SelectedItem = currentPage;

            var clientsPage = _filteredClients
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ClientsListView.ItemsSource = clientsPage;

            TBCount.Text = clientsPage.Count.ToString();
            TBAllRecords.Text = " из " + _filteredClients.Count.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            if (_filteredClients == null) 
                return;
            int totalPages = (_filteredClients.Count + pageSize - 1) / pageSize;
            if (currentPage > 1)
            {
                currentPage--;
                ChangePage();
            }
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            if (_filteredClients == null) 
                return;
            int totalPages = (_filteredClients.Count + pageSize - 1) / pageSize;
            if (currentPage < totalPages)
            {
                currentPage++;
                ChangePage();
            }
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_filteredClients == null) 
                return;
            if (PageListBox.SelectedItem is int page && page != currentPage)
            {
                currentPage = page;
                ChangePage();
            }
        }

        private void PageListCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_filteredClients == null) 
                return;
            if (PageListCB.SelectedItem is TextBlock textBlock)
            {
                string content = textBlock.Text;
                if (content == "Все")
                {
                    pageSize = _filteredClients.Count > 0 ? _filteredClients.Count : 1;
                }
                else
                {
                    pageSize = int.Parse(content);
                }
                currentPage = 1;
                ChangePage();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // получение выбранных клиентов
            var selected = ClientsListView.SelectedItems.Cast<Client>().ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Не выбран ни один клиент.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // проверка есть ли у кого-то посещения
            if (selected.Any(c => c.VisitsCount > 0))
            {
                MessageBox.Show("Удаление невозможно: у некоторых клиентов есть посещения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // подтверждение удаления
            if (MessageBox.Show($"Удалить {selected.Count} клиента(ов)?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var context = Vakhitova_LanguageSchoolEntities.GetContext();

                // прикрепляем и удаляем каждого клиента
                foreach (var client in selected)
                {
                    if (context.Entry(client).State == System.Data.Entity.EntityState.Detached)
                        context.Client.Attach(client);
                    context.Client.Remove(client);
                }

                context.SaveChanges();

                // обновляем локальный список и сбрасываем пагинацию
                _filteredClients = context.Client.Include(c => c.ClientService).ToList();
                currentPage = 1;
                ChangePage();

                MessageBox.Show("Удаление выполнено.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClientsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsListView.SelectedItems.Count > 0)
            {
                DeleteBtn.Visibility = Visibility.Visible;
            }

            else
            {
                DeleteBtn.Visibility = Visibility.Hidden;
            }
        }
    }
}
