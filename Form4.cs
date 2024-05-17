using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using Dapper;

namespace _26_day
{
    public partial class Form4 : Form
    {
        private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ConcertDB;Integrated Security=True";

        public Form4()
        {
            InitializeComponent();
            LoadConcertTypes(); // Загрузка типов концертов при инициализации формы
            LoadTicketTypes();  // Загрузка типов билетов при инициализации формы
        }


        // Метод для загрузки типов концертов в ComboBox1
        private void LoadConcertTypes()
        {
            comboBox1.Items.Clear(); // Очистка списка перед загрузкой

            // Запрос к базе данных для получения типов концертов
            string query = "SELECT DISTINCT GroupName FROM Concerts;";

            // Выполнить запрос и получить результаты с помощью Dapper
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var concertTypes = connection.Query<string>(query);
                comboBox1.Items.AddRange(concertTypes.ToArray());
            }
        }

        // Метод для загрузки типов билетов в ComboBox2
        private void LoadTicketTypes()
        {
            comboBox2.Items.Clear(); // Очистка списка перед загрузкой

            // Запрос к базе данных для получения типов билетов
            string query = "SELECT DISTINCT TicketType FROM Tickets;";

            // Выполнить запрос и получить результаты с помощью Dapper
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var ticketTypes = connection.Query<string>(query);
                comboBox2.Items.AddRange(ticketTypes.ToArray());
            }
        }

        // Обработчик нажатия кнопки "Купить билет"
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string groupName = comboBox1.SelectedItem?.ToString();
                string ticketType = comboBox2.SelectedItem?.ToString();
                int ticketCount = int.Parse(textBox1.Text);

                BuyTickets(groupName, ticketType, ticketCount);

                MessageBox.Show("Билеты успешно куплены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }

        // Метод для выполнения операции покупки билетов
        private void BuyTickets(string groupName, string ticketType, int ticketCount)
        {
            // Получаем ID концерта с помощью Dapper
            string concertQuery = "SELECT ConcertID FROM Concerts WHERE GroupName = @GroupName AND TicketType = @TicketType;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                int concertID = connection.QueryFirstOrDefault<int>(concertQuery, new { GroupName = groupName, TicketType = ticketType });

                if (concertID != default(int))
                {
                    string buyQuery = "UPDATE Concerts SET TicketCount = TicketCount - @TicketCount WHERE ConcertID = @ConcertID AND TicketType = @TicketType;";
                    connection.Execute(buyQuery, new { TicketCount = ticketCount, ConcertID = concertID, TicketType = ticketType });

                    AddSaleData(concertID, ticketType, ticketCount);
                }
            }
        }

        // Метод для добавления данных о продаже в таблицу Sales
        private void AddSaleData(int concertID, string ticketType, int ticketCount)
        {
            string insertSaleQuery = "INSERT INTO Sales (ConcertID, TicketType, SaleDate, SalePrice, BuyerName) VALUES (@ConcertID, @TicketType, @SaleDate, @SalePrice, @BuyerName);";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                connection.Execute(insertSaleQuery, new { ConcertID = concertID, TicketType = ticketType, SaleDate = DateTime.Now, SalePrice = CalculateSalePrice(ticketCount, ticketType), BuyerName = textBox2.Text });
            }
        }

        // Метод для расчета стоимости продажи
        private decimal CalculateSalePrice(int ticketCount, string ticketType)
        {
            decimal ticketPrice = 0;

            // Запрос к базе данных для получения цены за билет определенного типа
            string query = "SELECT TicketPrice FROM Concerts WHERE TicketType = @TicketType AND GroupName = @GroupName; ";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                ticketPrice = connection.QueryFirstOrDefault<decimal>(query, new { TicketType = ticketType, GroupName = comboBox1.Text });
            }
            return ticketPrice * ticketCount;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string TicketType = comboBox2.Text;
                string GroupName = comboBox1.Text;
                int ticketCount = int.Parse(textBox1.Text);

                decimal ticketPrices = 0;

                if (!string.IsNullOrEmpty(textBox1.Text))
                {
                    string query = "SELECT TicketPrice FROM Concerts WHERE TicketType = @TicketType AND GroupName = @GroupName; ";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        ticketPrices = connection.QueryFirstOrDefault<decimal>(query, new { TicketType = TicketType, GroupName = GroupName });
                    }
                    label6.Text = (ticketPrices * ticketCount).ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
