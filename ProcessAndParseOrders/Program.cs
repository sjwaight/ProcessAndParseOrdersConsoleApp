using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Configuration;

namespace ProcessAndParseOrders
{
    class Program
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["WWConnectionString"].ConnectionString;
        private static string getDetailsStoredProc = "Integration.GetSupplierTransactions";
        private static string saveDetailsStoredProc = "Integration.InsertSupplierTransaction";
        private static List<SupplierTransaction> transactions = new List<SupplierTransaction>();

        static void Main(string[] args)
        {
            var supplierId = 10;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(getDetailsStoredProc, connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Add parameters to the stored procedure
                    command.Parameters.AddWithValue("@SupplierID", supplierId);

                    connection.Open();

                    // Execute the stored procedure
                    SqlDataReader reader = command.ExecuteReader();

                    // Process the results
                    while (reader.Read())
                    {
                        // Do something with the data
                        transactions.Add(new SupplierTransaction
                        {
                            SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                            TransactionAmount = reader.GetDecimal(reader.GetOrdinal("TransactionAmount")),
                            SupplierTransactionID = reader.GetInt32(reader.GetOrdinal("SupplierTransactionID"))
                        });
                    }

                    reader.Close();
                }
            }

            // calculate the total balance of transactions for the supplier
            decimal totalAmount = transactions.Sum(item => item.TransactionAmount);

            Console.WriteLine($"Supplier ID: {supplierId} | Transaction Total {totalAmount}");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(saveDetailsStoredProc, connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@supplierId", supplierId);
                    command.Parameters.AddWithValue("@totalAmount", totalAmount);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
