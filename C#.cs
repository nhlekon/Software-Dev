using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace CalculatorApp
{
    public partial class CalculatorForm : Form
    {
        private string connectionString = "Server=your_server;Database=CalculatorDB;Trusted_Connection=True;";
        private const string apiKey = "your_api_key";
        private const string apiUrl = "https://api.exchangerate-api.com/v4/latest/";

        public CalculatorForm()
        {
            InitializeComponent();
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            string expression = txtExpression.Text;
            try
            {
                double result = EvaluateExpression(expression);
                txtResult.Text = result.ToString();
                SaveCalculation("Standard", expression, result.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid expression: " + ex.Message);
            }
        }

        private double EvaluateExpression(string expression)
        {
            return Convert.ToDouble(new DataTable().Compute(expression, null));
        }

        private void SaveCalculation(string type, string expression, string result)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO CalculationHistory (Type, Expression, Result) VALUES (@Type, @Expression, @Result)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Type", type);
                cmd.Parameters.AddWithValue("@Expression", expression);
                cmd.Parameters.AddWithValue("@Result", result);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void btnCalculateInterest_Click(object sender, EventArgs e)
        {
            try
            {
                double principal = Convert.ToDouble(txtPrincipal.Text);
                double rate = Convert.ToDouble(txtRate.Text) / 100;
                int time = Convert.ToInt32(txtTime.Text);
                double interest;

                if (rbSimpleInterest.Checked)
                {
                    interest = principal * rate * time;
                }
                else
                {
                    interest = principal * Math.Pow((1 + rate), time) - principal;
                }

                txtInterestResult.Text = interest.ToString("F2");
                SaveCalculation("Interest", $"P={principal}, R={rate*100}%, T={time}", interest.ToString("F2"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid input: " + ex.Message);
            }
        }

        private async void btnConvertCurrency_Click(object sender, EventArgs e)
        {
            string fromCurrency = cmbFromCurrency.SelectedItem.ToString();
            string toCurrency = cmbToCurrency.SelectedItem.ToString();
            double amount;
            if (!double.TryParse(txtAmount.Text, out amount))
            {
                MessageBox.Show("Invalid amount");
                return;
            }

            double rate = await GetExchangeRate(fromCurrency, toCurrency);
            double convertedAmount = amount * rate;
            txtConvertedResult.Text = convertedAmount.ToString("F2");
            SaveCalculation("Currency", $"{amount} {fromCurrency} to {toCurrency}", convertedAmount.ToString("F2"));
        }

        private async Task<double> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"{apiUrl}{fromCurrency}?apikey={apiKey}";
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseBody);
                    return json["rates"][toCurrency].Value<double>();
                }
                else
                {
                    MessageBox.Show("Failed to fetch exchange rates.");
                    return 0;
                }
            }
        }
    }
}
