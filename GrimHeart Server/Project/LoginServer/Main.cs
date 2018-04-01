using DarkRift;
using DarkRift.Server;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

class Main : Plugin {
    public override bool ThreadSafe => false;

    public override Version Version => new Version(1, 0, 0);

    const int OS_ANYSERVER = 29;

    [DllImport("shlwapi.dll", SetLastError = true, EntryPoint = "#437")]
    private static extern bool IsOS(int os);

    Dictionary<IClient, Connection> Connections = new Dictionary<IClient, Connection>();

    public Main(PluginLoadData pluginLoadData) : base(pluginLoadData) {
        ClientManager.ClientConnected += clientConnected;
        ClientManager.ClientDisconnected += clientDisconnected;


        if (IsWindowsServer()) {
            Console.WriteLine("Server");
            GrimHeart.GrimHeartReference.MySQL = new MySqlConnector(Environment.GetEnvironmentVariable("GRIMHEART_DB_CONNECTION"));
        } else {
            GrimHeart.GrimHeartReference.MySQL = new MySqlConnector(Environment.GetEnvironmentVariable("GRIMHEART_DB_CONNECTION"));
        }
        
        Console.WriteLine("Started Login Server");
    }

    public static bool IsWindowsServer() {
        return IsOS(OS_ANYSERVER);
    }

    private void clientConnected(object sender, ClientConnectedEventArgs con) {
        Connections.Add(con.Client, new Connection());
        con.Client.MessageReceived += messageReceived;
    }

    private void clientDisconnected(object sender, ClientDisconnectedEventArgs con) {
        GrimHeart.GrimHeartReference.removeChar(con.Client);

        if (Connections.ContainsKey(con.Client))
            Connections.Remove(con.Client);
    }

    private void messageReceived(object sender, MessageReceivedEventArgs con) {
        using (Message message = con.GetMessage() as Message) {
            if (message.Tag == Tags.Register) {
                using (DarkRiftReader reader = message.GetReader()) {
                    string username = reader.ReadString();
                    string password = reader.ReadString();
                    bool rememberMe = reader.ReadBoolean();

                    if (username.Length < 1 || password.Length < 3) {
                        sendClientError("You shouldn't see this... (user/pass < 1)", true, con.Client);
                        return;
                    }

                    if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9]+$")) {
                        sendClientError("You shouldn't see this... (alphanumeric)", true, con.Client);
                        return;
                    }

                    lock (MySqlConnector.Connection) {
                        using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                            cmd.CommandText = "SELECT COUNT(*) from accounts WHERE Username = @Username";
                            cmd.Parameters.AddWithValue("@Username", username);
                            using (MySqlDataReader result = cmd.ExecuteReader()) {
                                if (result.HasRows) {
                                    while (result.Read()) {
                                        if ((long)result[0] > 0) {
                                            sendClientError("Username already exists!", false, con.Client);
                                            return;
                                        }
                                    }
                                } else {
                                    result.Close();
                                }
                            }
                        }
                    }

                    String hashedPassword = Password.hashPassword(password);

                    lock (MySqlConnector.Connection) {
                        using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                            cmd.CommandText = "INSERT INTO accounts(username, password) VALUES(@Username, @Password)";
                            cmd.Parameters.AddWithValue("@Username", username);
                            cmd.Parameters.AddWithValue("@Password", hashedPassword);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    int id = -1;
                    lock (MySqlConnector.Connection) {
                        using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                            cmd.CommandText = "SELECT username, id from accounts WHERE Username = @Username";
                            cmd.Parameters.AddWithValue("@Username", username);
                            using (MySqlDataReader result = cmd.ExecuteReader()) {
                                if (result.HasRows) {
                                    while (result.Read()) {
                                        username = (string)result[0];
                                        id = (int)result[1];
                                    }
                                } else {
                                    result.Close();
                                }
                            }
                        }
                    }

                    Console.WriteLine("Registered " + username + " under ID " + id);

                    string sessionKey = "";

                    if (rememberMe) {
                        bool newKey = false;
                        lock (MySqlConnector.Connection) {
                            using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                                cmd.CommandText = "SELECT sessionkey from sessions WHERE accountid = @accountid AND datecreated > NOW() - INTERVAL 30 DAY";
                                cmd.Parameters.AddWithValue("@accountid", id);
                                using (MySqlDataReader result = cmd.ExecuteReader()) {
                                    if (result.HasRows) {
                                        while (result.Read()) {
                                            sessionKey = (string)result[0];
                                        }
                                    } else {
                                        newKey = true;
                                        sessionKey = Session.randomString(32);
                                        result.Close();
                                    }
                                }
                            }
                        }

                        if (newKey) {
                            lock (MySqlConnector.Connection) {
                                using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                                    cmd.CommandText = "INSERT INTO sessions(accountid, sessionkey) VALUES(@accountid, @sessionkey)";
                                    cmd.Parameters.AddWithValue("@accountid", id);
                                    cmd.Parameters.AddWithValue("@sessionkey", sessionKey);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    using (DarkRiftWriter loginWriter = DarkRiftWriter.Create()) {
                        loginWriter.Write(id);
                        loginWriter.Write(username);
                        loginWriter.Write(sessionKey);

                        using (Message loginMessage = Message.Create(Tags.Login, loginWriter))
                            con.Client.SendMessage(loginMessage, SendMode.Reliable);
                    }

                    Connections[con.Client].AccountID = id;
                    Connections[con.Client].AccountName = username;
                }
            }

            if (message.Tag == Tags.Login) {
                using (DarkRiftReader reader = message.GetReader()) {
                    string username = reader.ReadString().ToLower();
                    string password = reader.ReadString();
                    bool rememberMe = reader.ReadBoolean();

                    Console.WriteLine("Attempting log in for " + username);

                    if (username.Length < 1 || password.Length < 1) {
                        sendClientError("You shouldn't see this... (user/pass < 1)", true, con.Client);
                        return;
                    }

                    if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9]+$")) {
                        sendClientError("You shouldn't see this... (alphanumeric)", true, con.Client);
                        return;
                    }

                    string passHash = "";
                    int ID = -1;
                    string sqlUsername = "";
                    lock (MySqlConnector.Connection) {
                        using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                            cmd.CommandText = "SELECT id, username, password from accounts WHERE Username = @Username";
                            cmd.Parameters.AddWithValue("@Username", username);
                            using (MySqlDataReader result = cmd.ExecuteReader()) {
                                if (result.HasRows) {
                                    while (result.Read()) {
                                        ID = (int)result[0];
                                        passHash = (string)result[2];
                                        sqlUsername = (string)result[1];
                                    }
                                } else {
                                    sendClientError("Incorrect username/password!", true, con.Client);
                                    result.Close();
                                }
                            }
                        }
                    }

                    if (Password.verifyPassword(password, passHash)) {
                        //check if user is already logged in
                        foreach (Connection connection in Connections.Values) {
                            if (connection.AccountName != "" && connection.AccountName.ToLower() == username) {
                                sendClientError("Already logged in!", true, con.Client);
                                return;
                            }
                        }

                        Console.WriteLine(username + " has logged in (via password).");

                        string sessionKey = "";

                        if (rememberMe) {
                            bool newKey = false;
                            lock (MySqlConnector.Connection) {
                                using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                                    cmd.CommandText = "SELECT sessionkey from sessions WHERE accountid = @accountid AND datecreated > NOW() - INTERVAL 30 DAY";
                                    cmd.Parameters.AddWithValue("@accountid", ID);
                                    using (MySqlDataReader result = cmd.ExecuteReader()) {
                                        if (result.HasRows) {
                                            while (result.Read()) {
                                                sessionKey = (string)result[0];
                                            }
                                        } else {
                                            newKey = true;
                                            sessionKey = Session.randomString(32);
                                            result.Close();
                                        }
                                    }
                                }
                            }

                            if (newKey) {
                                lock (MySqlConnector.Connection) {
                                    using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                                        cmd.CommandText = "INSERT INTO sessions(accountid, sessionkey) VALUES(@accountid, @sessionkey)";
                                        cmd.Parameters.AddWithValue("@accountid", ID);
                                        cmd.Parameters.AddWithValue("@sessionkey", sessionKey);

                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        Console.WriteLine("Sending session key " + sessionKey);

                        using (DarkRiftWriter loginWriter = DarkRiftWriter.Create()) {
                            loginWriter.Write(ID);
                            loginWriter.Write(sqlUsername);
                            loginWriter.Write(sessionKey);

                            using (Message loginMessage = Message.Create(Tags.Login, loginWriter))
                                con.Client.SendMessage(loginMessage, SendMode.Reliable);
                        }

                        Connections[con.Client].AccountID = ID;
                        Connections[con.Client].AccountName = sqlUsername;

                    } else {
                        sendClientError("Incorrect username/password!", true, con.Client);
                    }

                }
            }
            if (message.Tag == Tags.SignOut) {
                Console.WriteLine("Logging out " + Connections[con.Client].AccountName);
                Connections[con.Client].AccountName = null;
                Connections[con.Client].AccountID = -1;

                using (DarkRiftWriter signOutWriter = DarkRiftWriter.Create()) {
                    using (Message signOutMessage = Message.Create(Tags.SignOut, signOutWriter))
                        con.Client.SendMessage(signOutMessage, SendMode.Reliable);
                }
            }

            if (message.Tag == Tags.Play) {
                if (Connections[con.Client].AccountID != -1) {
                    int[] items = new int[9];
                    bool newChar = false;
                    lock (MySqlConnector.Connection) {
                        using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                            cmd.CommandText = "SELECT * from characters WHERE accountid = @accountid AND dead = 0";
                            cmd.Parameters.AddWithValue("@accountid", Connections[con.Client].AccountID);
                            using (MySqlDataReader result = cmd.ExecuteReader()) {
                                if (result.HasRows) {
                                    while (result.Read()) {
                                        for (int i = 0; i < 9; i++) {
                                            items[i] = (int)result[i + 3];
                                        }
                                    }
                                } else {
                                    newChar = true;
                                    result.Close();
                                }
                            }
                        }
                    }

                    if (newChar) {
                        lock (MySqlConnector.Connection) {
                            using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                                cmd.CommandText = "INSERT INTO characters(accountid) VALUES(@accountid)";
                                cmd.Parameters.AddWithValue("@accountid", Connections[con.Client].AccountID);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        items[1] = 1;
                    }

                    using (DarkRiftWriter playWriter = DarkRiftWriter.Create()) {

                        using (Message playMessage = Message.Create(Tags.Play, playWriter))
                            con.Client.SendMessage(playMessage, SendMode.Reliable);
                    }

                    Console.WriteLine("[" + Connections[con.Client].AccountID + "] " + Connections[con.Client].AccountName + " is now loading");

                    GrimHeart.GrimHeartReference.loadingChar(Connections[con.Client].AccountID, Connections[con.Client].AccountName, con.Client, items);
                }
            }

            if (message.Tag == Tags.InGame) {
                Console.WriteLine("[" + Connections[con.Client].AccountID + "] " + Connections[con.Client].AccountName + " is now playing");
                GrimHeart.GrimHeartReference.newChar(con.Client);
            }

            if (message.Tag == Tags.SessionLogIn) {
                using (DarkRiftReader reader = message.GetReader()) {
                    string sessionKey = reader.ReadString();

                    int accountID = -1;
                    string username = "";

                    lock (MySqlConnector.Connection) {
                        using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                            cmd.CommandText = "SELECT accountid from sessions WHERE sessionkey = @sessionkey AND datecreated > NOW() - INTERVAL 30 DAY";
                            cmd.Parameters.AddWithValue("@sessionkey", sessionKey);
                            using (MySqlDataReader result = cmd.ExecuteReader()) {
                                if (result.HasRows) {
                                    while (result.Read()) {
                                        accountID = (int)result[0];
                                    }
                                } else {
                                    result.Close();
                                }
                            }
                        }
                    }

                    if (accountID != -1) {
                        lock (MySqlConnector.Connection) {
                            using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                                cmd.CommandText = "SELECT username from accounts WHERE id = @id";
                                cmd.Parameters.AddWithValue("@id", accountID);
                                using (MySqlDataReader result = cmd.ExecuteReader()) {
                                    if (result.HasRows) {
                                        while (result.Read()) {
                                            username = (string)result[0];
                                        }
                                    } else {
                                        sendClientError("Incorrect username/password!", true, con.Client);
                                        result.Close();
                                    }
                                }
                            }
                        }

                        foreach (Connection connection in Connections.Values) {
                            if (connection.AccountName != "" && connection.AccountName.ToLower() == username.ToLower()) {
                                return;
                            }
                        }

                        Console.WriteLine(username + " has logged in (via session).");
                        using (DarkRiftWriter loginWriter = DarkRiftWriter.Create()) {
                            loginWriter.Write(accountID);
                            loginWriter.Write(username);
                            loginWriter.Write("");

                            using (Message loginMessage = Message.Create(Tags.Login, loginWriter))
                                con.Client.SendMessage(loginMessage, SendMode.Reliable);
                        }

                        Connections[con.Client].AccountID = accountID;
                        Connections[con.Client].AccountName = username;
                    }
                }
            }
        }
    }

    void sendClientError(string error, bool login, IClient client) {
        using (DarkRiftWriter errorWriter = DarkRiftWriter.Create()) {
            errorWriter.Write(error);

            using (Message errorMessage = Message.Create(login ? Tags.LoginError : Tags.RegisterError, errorWriter))
                client.SendMessage(errorMessage, SendMode.Reliable);
        }
    }
}
