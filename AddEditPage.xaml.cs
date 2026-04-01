using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
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
using Path = System.IO.Path;

namespace Vakhitova_LanguageSchool
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private Client _client;
        private string _selectedPhotoFullPath;

        public AddEditPage(Client client)
        {
            InitializeComponent();
            _client = client ?? new Client();

            if (_client.ID != 0) // редактирование
            {
                IDTB.Text = _client.ID.ToString();
                LastNameTB.Text = _client.LastName;
                FirstNameTB.Text = _client.FirstName;
                PatronymicTB.Text = _client.Patronymic;
                EmailTB.Text = _client.Email;
                PhoneTB.Text = _client.Phone;
                BirthdayDP.SelectedDate = _client.Birthday;
                if (_client.GenderCode == "м")
                    MaleRB.IsChecked = true;
                else if (_client.GenderCode == "ж")
                    FemaleRB.IsChecked = true;

                if (!string.IsNullOrEmpty(_client.PhotoPath))
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _client.PhotoPath.Replace('/', '\\'));
                    if (File.Exists(fullPath))
                    {
                        PhotoImage.Source = new BitmapImage(new Uri(fullPath));
                        _selectedPhotoFullPath = fullPath;
                    }

                    else
                    {
                        // заглушка
                        SetDefaultPhoto();
                    }
                }

                else
                {
                    SetDefaultPhoto();
                }
            }

            else // добавление
            {
                IDTB.Visibility = Visibility.Collapsed;
                IDLabel.Visibility = Visibility.Collapsed;

                // если фото не указывают
                SetDefaultPhoto();
            }
        }

        private void SetDefaultPhoto()
        {
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Клиенты", "keypad.jpg");
            if (File.Exists(defaultPath))
            {
                PhotoImage.Source = new BitmapImage(new Uri(defaultPath));
            }
        }

        private void SelectPhotoBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceFile = openFileDialog.FileName;
                    if (!File.Exists(sourceFile))
                        return;

                    string targetFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Клиенты");
                    Directory.CreateDirectory(targetFolder);

                    string fileName = Path.GetFileName(sourceFile);
                    string destPath = Path.Combine(targetFolder, fileName);

                    int count = 1;
                    while (File.Exists(destPath))
                    {
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                        string ext = Path.GetExtension(fileName);
                        string newName = $"{nameWithoutExt}_{count}{ext}";
                        destPath = Path.Combine(targetFolder, newName);
                        count++;
                    }

                    File.Copy(sourceFile, destPath);

                    _client.PhotoPath = $"Клиенты/{Path.GetFileName(destPath)}".Replace('\\', '/');
                    _selectedPhotoFullPath = destPath;

                    PhotoImage.Source = new BitmapImage(new Uri(destPath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при копировании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var context = Vakhitova_LanguageSchoolEntities.GetContext();

                // добавление в контекст нового клиента
                if (_client.ID == 0)
                    context.Client.Add(_client);

                // заполнение полей
                _client.LastName = LastNameTB.Text.Trim();
                _client.FirstName = FirstNameTB.Text.Trim();
                _client.Patronymic = PatronymicTB.Text.Trim();
                _client.Email = EmailTB.Text.Trim();
                _client.Phone = PhoneTB.Text.Trim();
                _client.Birthday = BirthdayDP.SelectedDate.Value;
                _client.GenderCode = MaleRB.IsChecked == true ? "м" : "ж";

                // установка даты регистрации нового клиента
                if (_client.RegistrationDate == default(DateTime))
                    _client.RegistrationDate = DateTime.Now;

                if (_selectedPhotoFullPath == null && _client.ID == 0)
                {
                    _client.PhotoPath = "";
                }

                context.SaveChanges();
                NavigationService.GoBack();
            }
            catch (DbEntityValidationException ex)
            {
                string errors = "";
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errors += $"{validationError.PropertyName}: {validationError.ErrorMessage}\n";
                    }
                }
                MessageBox.Show($"Ошибка валидации данных:\n{errors}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            var nameRegex = new Regex(@"^[а-яА-ЯёЁa-zA-Z\s\-]+$");
            if (!nameRegex.IsMatch(LastNameTB.Text) || string.IsNullOrWhiteSpace(LastNameTB.Text))
            {
                MessageBox.Show("Фамилия должна содержать только буквы, пробел или дефис.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!nameRegex.IsMatch(FirstNameTB.Text) || string.IsNullOrWhiteSpace(FirstNameTB.Text))
            {
                MessageBox.Show("Имя должно содержать только буквы, пробел или дефис.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!nameRegex.IsMatch(PatronymicTB.Text) || string.IsNullOrWhiteSpace(PatronymicTB.Text))
            {
                MessageBox.Show("Отчество должно содержать только буквы, пробел или дефис.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (LastNameTB.Text.Length > 50 || FirstNameTB.Text.Length > 50 || PatronymicTB.Text.Length > 50)
            {
                MessageBox.Show("Фамилия, имя и отчество не могут быть длиннее 50 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (EmailTB.Text.Length > 255)
            {
                MessageBox.Show("Email не может быть длиннее 255 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(EmailTB.Text) || string.IsNullOrWhiteSpace(EmailTB.Text))
            {
                MessageBox.Show("Введите корректный email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (PhoneTB.Text.Length > 20)
            {
                MessageBox.Show("Телефон не может быть длиннее 20 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var phoneRegex = new Regex(@"^[\d+\-\(\)\s]+$");
            if (!phoneRegex.IsMatch(PhoneTB.Text) || string.IsNullOrWhiteSpace(PhoneTB.Text))
            {
                MessageBox.Show("Телефон может содержать только цифры и символы +, -, (, ), пробел.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!BirthdayDP.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату рождения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (BirthdayDP.SelectedDate.Value > DateTime.Now)
            {
                MessageBox.Show("Дата рождения не может быть в будущем.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (MaleRB.IsChecked != true && FemaleRB.IsChecked != true)
            {
                MessageBox.Show("Выберите пол.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
    
}
