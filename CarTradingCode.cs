using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CarTradingApp
{
    public partial class LoginForm : Form
    {
        private string connectionString = "Server=NHLANHLANHLEKO\SQLEXPRESS;Database=CarTradingDB;Trusted_Connection=True;";

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (AuthenticateUser(username, password, out string role, out string department))
            {
                MessageBox.Show("Login successful!");
                MainForm mainForm = new MainForm(username, role, department);
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private bool AuthenticateUser(string username, string password, out string role, out string department)
        {
            role = "";
            department = "";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Role, Department FROM Users WHERE Username = @Username AND PasswordHash = HASHBYTES('SHA2_256', @Password)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    role = reader["Role"].ToString();
                    department = reader["Department"].ToString();
                    return true;
                }
            }
            return false;
        }
    }

    public partial class MainForm : Form
    {
        private string username;
        private string role;
        private string department;

        public MainForm(string username, string role, string department)
        {
            InitializeComponent();
            this.username = username;
            this.role = role;
            this.department = department;
            LoadCars();
        }

        private void LoadCars()
        {
            using (SqlConnection conn = new SqlConnection("Server=your_server;Database=CarTradingDB;Trusted_Connection=True;"))
            {
                string query = role == "Manager" ? "SELECT * FROM Cars" : "SELECT * FROM Cars WHERE AddedBy IN (SELECT UserID FROM Users WHERE Department = @Department)";
                SqlCommand cmd = new SqlCommand(query, conn);
                if (role != "Manager")
                {
                    cmd.Parameters.AddWithValue("@Department", department);
                }
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lstCars.Items.Add($"{reader["CarID"]} - {reader["Make"]} {reader["Model"]} - {reader["Year"]} (${reader["Price"]}) - {reader["Status"]}");
                }
            }
        }

        private void btnRecordSale_Click(object sender, EventArgs e)
        {
            if (lstCars.SelectedItem != null)
            {
                using (SqlConnection conn = new SqlConnection("Server=your_server;Database=CarTradingDB;Trusted_Connection=True;"))
                {
                    string selectedCar = lstCars.SelectedItem.ToString();
                    string[] carDetails = selectedCar.Split(' ');
                    int carID = int.Parse(carDetails[0]);
                    string buyerName = txtBuyerName.Text;
                    decimal salePrice = decimal.Parse(txtSalePrice.Text);
                    
                    string query = "INSERT INTO Transactions (CarID, BuyerName, SalePrice, ProcessedBy) VALUES (@CarID, @BuyerName, @SalePrice, (SELECT UserID FROM Users WHERE Username = @Username));";
                    string updateCarStatus = "UPDATE Cars SET Status = 'Sold' WHERE CarID = @CarID";
                    
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@CarID", carID);
                    cmd.Parameters.AddWithValue("@BuyerName", buyerName);
                    cmd.Parameters.AddWithValue("@SalePrice", salePrice);
                    cmd.Parameters.AddWithValue("@Username", username);
                    
                    SqlCommand cmdUpdate = new SqlCommand(updateCarStatus, conn);
                    cmdUpdate.Parameters.AddWithValue("@CarID", carID);
                    
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    cmdUpdate.ExecuteNonQuery();
                    
                    MessageBox.Show("Sale recorded successfully.");
                    LoadCars();
                }
            }
            else
            {
                MessageBox.Show("Please select a car to record the sale.");
            }
        }

        private void btnViewSalesReport_Click(object sender, EventArgs e)
        {
            lstTransactions.Items.Clear();
            using (SqlConnection conn = new SqlConnection("Server=your_server;Database=CarTradingDB;Trusted_Connection=True;"))
            {
                string query = "SELECT Cars.Make, Cars.Model, Cars.Year, Transactions.BuyerName, Transactions.SalePrice, Users.Username FROM Transactions INNER JOIN Cars ON Transactions.CarID = Cars.CarID INNER JOIN Users ON Transactions.ProcessedBy = Users.UserID";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lstTransactions.Items.Add($"{reader["Make"]} {reader["Model"]} ({reader["Year"]}) - Buyer: {reader["BuyerName"]} - Price: ${reader["SalePrice"]} - Processed by: {reader["Username"]}");
                }
            }
        }
    }
}
