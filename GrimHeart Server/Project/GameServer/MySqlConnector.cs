using DarkRift;
using DarkRift.Server;
using MySql.Data.MySqlClient;
using System;

public class MySqlConnector {
    public static MySqlConnection Connection;
    public string MyConnectionString;

    public MySqlConnector(string connectionString) {
        Console.WriteLine("Connecting to Database");
        MyConnectionString = connectionString;
        try {
            Connection = new MySqlConnection(MyConnectionString);
            Connection.Open();
        } catch (MySqlException ex) {
            Console.WriteLine(ex.Message);
            return;
        }

        Console.WriteLine("Connected to Database");
    }
}