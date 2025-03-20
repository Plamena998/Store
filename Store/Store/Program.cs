using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
namespace StoreApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string connectionString = $"Server=LAPTOP-RDQ454BS\\SQLEXPRESS; Integrated Security=True; Encrypt=False; TrustServerCertificate=True;";
            string database = "Store";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string checkDbQuery = $"SELECT database_id FROM sys.databases WHERE name = '{database}'";
                using (SqlCommand checkcmd = new SqlCommand(checkDbQuery, connection))
                {
                    var result = checkcmd.ExecuteScalar();

                    if (result == null)
                    {
                        Console.WriteLine($"Database {database} doesn't exist.");
                        string createDbQuery = $"CREATE DATABASE {database}";
                        using (SqlCommand createcmd = new SqlCommand(createDbQuery, connection))
                        {
                            createcmd.ExecuteNonQuery();
                            Console.WriteLine($"Database {database} successfully created!");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Database {database} already exists.");
                    }
                }
            }
            string databaseConnectionString = $"Server=LAPTOP-RDQ454BS\\SQLEXPRESS; Database={database}; Integrated Security=True; Encrypt=False; TrustServerCertificate=True;";

            CreateTables(databaseConnectionString);
            InsertProducts(databaseConnectionString);
            InsertClients(databaseConnectionString);
            CountPurchases(databaseConnectionString);
            DeleteProduct(databaseConnectionString, 2);
        }
        static void CreateTables(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string createTablesQuery = @"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Products') AND type in (N'U'))
            BEGIN
                CREATE TABLE Products 
                (
                    ProductID INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL,
                    Price DECIMAL(10,2) NOT NULL
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Clients') AND type in (N'U'))
            BEGIN
                CREATE TABLE Clients 
                (
                    ClientID INT IDENTITY(1,1) PRIMARY KEY,
                    FullName NVARCHAR(100) NOT NULL,
                    ProductID INT NULL,
                    FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE SET NULL
                );
            END";
                try
                {
                    using (SqlCommand command = new SqlCommand(createTablesQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Tables checked/created successfully!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during table creation: {ex.Message}");
                }
            }
        }

        static void InsertProducts(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string insertProducts = @"
            IF NOT EXISTS (SELECT * FROM Products WHERE Name = 'Laptop')
                INSERT INTO Products (Name, Price) VALUES ('Laptop', 2000.50);

            IF NOT EXISTS (SELECT * FROM Products WHERE Name = 'Phone')
                INSERT INTO Products (Name, Price) VALUES ('Phone', 1800.00);

            IF NOT EXISTS (SELECT * FROM Products WHERE Name = 'Watch')
                INSERT INTO Products(Name, Price) VALUES('Watch', 1200.00);";

                using (SqlCommand insertcmd = new SqlCommand(insertProducts, connection))
                {
                    insertcmd.ExecuteNonQuery();
                    Console.WriteLine("Successfully added products.");
                }
            }
        }

        static void InsertClients(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string insertClients = @"
            IF NOT EXISTS (SELECT * FROM Clients WHERE FullName = 'Anna Petrova')
                INSERT INTO Clients (FullName, ProductID) VALUES ('Anna Petrova', 1);

            IF NOT EXISTS (SELECT * FROM Clients WHERE FullName = 'Borislav Kostov')
                INSERT INTO Clients (FullName, ProductID) VALUES ('Borislav Kostov', 1);

            IF NOT EXISTS (SELECT * FROM Clients WHERE FullName = 'Katerina Boneva')
                INSERT INTO Clients (FullName, ProductID) VALUES ('Katerina Boneva', 2);

            IF NOT EXISTS (SELECT * FROM Clients WHERE FullName = 'Diana Veleva')
                INSERT INTO Clients (FullName, ProductID) VALUES ('Diana Veleva', 2);

            IF NOT EXISTS(SELECT * FROM Clients WHERE FullName = 'Radoslav Dragnev')
                INSERT INTO Clients(FullName, ProductID) VALUES('Radoslav Dragnev', 3); ";

                using (SqlCommand insertcmd = new SqlCommand(insertClients, connection))
                {
                    insertcmd.ExecuteNonQuery();
                    Console.WriteLine("Successfully added clients.");
                }
            }
        }
        static void CountPurchases(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT p.Name AS ProductName, COUNT(c.ClientID) AS BuyersCount
            FROM Products p
            LEFT JOIN Clients c ON p.ProductID = c.ProductID
            GROUP BY p.Name
            ORDER BY BuyersCount DESC;";

                using (SqlCommand groupcmd = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = groupcmd.ExecuteReader())
                    {
                        Console.WriteLine("Product Purchase Count:");
                        while (reader.Read())
                        {
                            string productName = reader["ProductName"].ToString();
                            int buyersCount = Convert.ToInt32(reader["BuyersCount"]);
                            Console.WriteLine($"{productName}: {buyersCount} buyers");
                        }
                    }
                }
            }
        }

        static void DeleteProduct(string connectionString, int productId)
        {
            using (SqlConnection cnct = new SqlConnection(connectionString))
            {
                cnct.Open();

                string removeQuery = @"
            DELETE 
            FROM Clients 
            WHERE ProductId = @Id";

                string deleteQuery = @"
            DELETE 
            FROM Products 
            WHERE ProductId=@Id";

                using (SqlCommand removecmd = new SqlCommand(removeQuery, cnct))
                {
                    removecmd.Parameters.AddWithValue("@Id", productId);
                    int affectedRows = removecmd.ExecuteNonQuery();
                    if (affectedRows > 0)
                    {
                        Console.WriteLine("Successfuly removed from Clients.");

                        using (SqlCommand deletecmd = new SqlCommand(deleteQuery, cnct))
                        {
                            deletecmd.Parameters.AddWithValue("@Id", productId);
                            int affectRows = deletecmd.ExecuteNonQuery();

                            if (affectRows > 0)
                            {
                                Console.WriteLine($"Affected rows with {productId} deleted successfuly from Products.");
                            }
                            else
                            {

                                Console.WriteLine("No deleted rows from Products.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No removed rows from Clients.");
                    }
                }

            }
        }
    }
}

