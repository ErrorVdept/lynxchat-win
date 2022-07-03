using ChatClient.Data;
using MihaZupan;

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatClient.Services
{
    public class OnlineService
    {
        public static async Task UpdateOnlineServer()
        {
            try
            {
                var proxy = new HttpToSocks5Proxy("127.0.0.1", 9050);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter();


                var handler = new HttpClientHandler
                {
                    Proxy = proxy
                };
                // http://slxtxi5jxly6zzlj7wpjp3hpycql22miymxzvzfumakzfiapeat45did.onion/user
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    List<UserRequest> users = new List<UserRequest>();
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + AppWindow.Token);
                    //await httpClient.GetAsync("http://slxtxi5jxly6zzlj7wpjp3hpycql22miymxzvzfumakzfiapeat45did.onion/user");
                    var result = await httpClient.GetStringAsync(AppWindow.Server + "/user");
                    //Console.WriteLine(result.ToString());
                    users = JsonSerializer.Deserialize<List<UserRequest>>(result);


                    //Console.WriteLine(users.Count.ToString());

                    AppWindow.command.CommandText = "DELETE FROM User";
                    AppWindow.command.ExecuteNonQuery();
                    AppWindow.command.CommandText = "DELETE FROM Address";
                    AppWindow.command.ExecuteNonQuery();

                    AppWindow.command.CommandText = "UPDATE SQLITE_SEQUENCE SET seq = 0 WHERE name=@name";
                    AppWindow.command.Parameters.AddWithValue("@name", "User");
                    AppWindow.command.ExecuteNonQuery();
                    AppWindow.command.CommandText = "UPDATE SQLITE_SEQUENCE SET seq = 0 WHERE name=@name";
                    AppWindow.command.Parameters.AddWithValue("@name", "Address");
                    AppWindow.command.ExecuteNonQuery();
                    foreach (var user in users)
                    {


                        AppWindow.command.CommandText = "INSERT INTO User(username,online) VALUES (@name,@online) ";
                        AppWindow.command.Parameters.AddWithValue("@name", user.name);
                        AppWindow.command.Parameters.AddWithValue("@online", user.online);
                        AppWindow.command.ExecuteNonQuery();
                        foreach (var domen in user.domain)
                        {
                            AppWindow.command.CommandText = "INSERT INTO Address(username,domain) VALUES (@name,@domain) ";
                            AppWindow.command.Parameters.AddWithValue("@name", user.name);
                            AppWindow.command.Parameters.AddWithValue("@domain", domen);
                            AppWindow.command.ExecuteNonQuery();
                        }

                    }




                }
            }
            catch
            {
                Console.WriteLine("FUCK");
            }
        }


        public static async Task UpdateOnlineServerFromStart(string Server, string Token)
        {
            try
            {
                var proxy = new HttpToSocks5Proxy("127.0.0.1", 9050);
                SQLiteDataAdapter adapter = new SQLiteDataAdapter();


                var handler = new HttpClientHandler
                {
                    Proxy = proxy
                };
                // http://slxtxi5jxly6zzlj7wpjp3hpycql22miymxzvzfumakzfiapeat45did.onion/user
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    List<UserRequest> users = new List<UserRequest>();
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Token);
                    //await httpClient.GetAsync("http://slxtxi5jxly6zzlj7wpjp3hpycql22miymxzvzfumakzfiapeat45did.onion/user");
                    var result = await httpClient.GetStringAsync(Server);
                    //Console.WriteLine(result.ToString());
                    users = JsonSerializer.Deserialize<List<UserRequest>>(result);

                    SQLiteConnection con = new SQLiteConnection(@"Data Source=database.db;Cache=Shared");//SQL коннект
                    SQLiteCommand command = new SQLiteCommand("Select * from Message");// SQL команда
                    con.Open();
                    command.Connection = con;
                    //Console.WriteLine(users.Count.ToString());

                    command.CommandText = "DELETE FROM User";
                    command.ExecuteNonQuery();
                    command.CommandText = "DELETE FROM Address";
                    command.ExecuteNonQuery();

                    command.CommandText = "UPDATE SQLITE_SEQUENCE SET seq = 0 WHERE name=@name";
                    command.Parameters.AddWithValue("@name", "User");
                    command.ExecuteNonQuery();
                    command.CommandText = "UPDATE SQLITE_SEQUENCE SET seq = 0 WHERE name=@name";
                    command.Parameters.AddWithValue("@name", "Address");
                    command.ExecuteNonQuery();
                    foreach (var user in users)
                    {


                        command.CommandText = "INSERT INTO User(username,online) VALUES (@name,@online) ";
                        command.Parameters.AddWithValue("@name", user.name);
                        command.Parameters.AddWithValue("@online", user.online);
                        command.ExecuteNonQuery();
                        foreach (var domen in user.domain)
                        {
                            command.CommandText = "INSERT INTO Address(username,domain) VALUES (@name,@domain) ";
                            command.Parameters.AddWithValue("@name", user.name);
                            command.Parameters.AddWithValue("@domain", domen);
                            command.ExecuteNonQuery();
                        }

                    }



                    con.Close();
                }
            }
            catch
            {
                Console.WriteLine("FUCK");
            }
        }

    }
}
