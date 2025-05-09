using Cargo_transportationPIS.Facades;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

namespace Cargo_transportationPIS.Pages
{
    /// <summary>
    /// Логика взаимодействия для ManagerView.xaml
    /// </summary>
    public partial class ManagerView : Page
    {
        private readonly BaseTransportFacade _transportFacade;
        private GruzzPISEntities _context;

        public ManagerView()
        {
            InitializeComponent();
            _transportFacade = new BaseTransportFacade();
            _context = new GruzzPISEntities();
            LoadRequests();
        }

        private void LoadRequests()
        {
            listview.ItemsSource = GruzzPISEntities.GetContext().Заявки.Where(x=>x.ID_статус==1).ToList();

            var count_col = listview.Items.Count;
            tt1.Text = count_col.ToString();
        }

        private void stagesDevelopment_Click(object sender, RoutedEventArgs e)
        {
            if (listview.SelectedItem is Заявки selectedRequest)
            {
                try
                {
                    _transportFacade.CreateTransport(selectedRequest.ID_заявки);

                     _context.SaveChanges();

                    MessageBox.Show("Маршрут успешно создан!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                      LoadRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании маршрута: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите заявку для создания маршрута",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void redd_Click(object sender, RoutedEventArgs e)
        {
            if (listview.SelectedItem is Заявки selectedRequest)
            {
                try
                {
                    // Отклоняем заявку
                    _transportFacade.CancelRequest(selectedRequest.ID_заявки);

                    MessageBox.Show("Заявка отклонена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем список заявок
                    LoadRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отклонении заявки: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
