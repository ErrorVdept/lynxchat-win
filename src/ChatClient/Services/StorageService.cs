using ChatClient.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace ChatClient.Services
{
    public class StorageService
    {
        public ObservableCollection<UserController> LoadUsers()
        {
            ObservableCollection<UserController> usr = new ObservableCollection<UserController>();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter();
            DataTable dt = new DataTable("Users");
            DataTable dt2 = new DataTable("Msg");
            AppWindow.command.CommandText = "SELECT username,online FROM User";
            adapter.SelectCommand = AppWindow.command;
            adapter.Fill(dt);

            AppWindow.command.CommandText = "SELECT chat FROM Message Where unread=@unread";
            AppWindow.command.Parameters.AddWithValue("@unread", true);
            adapter.SelectCommand = AppWindow.command;
            adapter.Fill(dt2);
            List<String> usersWhereUnread = new List<String>();
            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                usersWhereUnread.Add(dt2.Rows[i][0].ToString());

            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (AppWindow.CurrentUsername != dt.Rows[i][0].ToString())
                {
                    if (usersWhereUnread.Contains(dt.Rows[i][0].ToString()))
                    {
                        usr.Add(new UserController(dt.Rows[i][0].ToString(), Convert.ToBoolean(dt.Rows[i][1]), true));
                    }
                    else
                    {
                        usr.Add(new UserController(dt.Rows[i][0].ToString(), Convert.ToBoolean(dt.Rows[i][1]), false));
                    }
                }


            }


            return usr;
        }
        public ObservableCollection<MessageInList> LoadMessages(string chat)
        {

            ObservableCollection<MessageInList> msg = new ObservableCollection<MessageInList>();
            if (File.Exists("Resources/Keys/" + chat + ".lynx"))
            {
                SQLiteDataAdapter adapter = new SQLiteDataAdapter();
                DataTable dt = new DataTable("Messages");

                AppWindow.command.CommandText = "SELECT username,message,time,iv,ownername,type,file,filename FROM Message Where chat=@chat";
                AppWindow.command.Parameters.AddWithValue("@chat", chat);
                adapter.SelectCommand = AppWindow.command;
                adapter.Fill(dt);
                
                Abonent cl = Abonent.LoadKeysFromFile(chat + ".lynx");


                AppWindow.abonent.PrivateKey = cl.PrivateKey;
                AppWindow.abonent.SharedKey = cl.SharedKey;





                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i][4].ToString() == AppWindow.CurrentUsername)
                    {
                        
                        var msgIv = Convert.FromBase64String(dt.Rows[i][3].ToString());
                            
                        string decryptedMessage = "";
                        AppWindow.abonent.IV = msgIv;
                            
                        if (dt.Rows[i][5].ToString() == "message")
                        {
                            var dataToByte = Convert.FromBase64String(dt.Rows[i][1].ToString());
                            decryptedMessage = AppWindow.abonent.Receive(dataToByte);
                                
                            msg.Insert(0, new MessageInList(dt.Rows[i][0].ToString(), decryptedMessage, dt.Rows[i][2].ToString(), dt.Rows[i][3].ToString()));
                        }
                        else
                        {

                            var databyte = Convert.FromBase64String(dt.Rows[i][6].ToString());
                            string decryptedFile = AppWindow.abonent.Receive(databyte);
                            msg.Insert(0, new MessageInList(dt.Rows[i][0].ToString(), decryptedMessage, dt.Rows[i][2].ToString(), dt.Rows[i][3].ToString(), dt.Rows[i][5].ToString(), dt.Rows[i][7].ToString(), decryptedFile));
                        }
                        
                        
                    }

                }
                foreach (var user in AppWindow.users)
                {
                    if (chat == user.Username)
                    {
                        user.HasMessage = false;
                        AppWindow.command.CommandText = "UPDATE Message SET unread=@unread Where chat=@chat";
                        AppWindow.command.Parameters.AddWithValue("@chat", chat);
                        AppWindow.command.Parameters.AddWithValue("@unread", false);
                        AppWindow.command.ExecuteNonQuery();
                    }

                }

            }

            return msg;
        }
        public static List<String> GetAddress()
        {
            List<string> list = new List<string>();

            DataTable dt = new DataTable("Messages");
            SQLiteDataAdapter adapter = new SQLiteDataAdapter();

            AppWindow.command.CommandText = "SELECT domain FROM Address Where username=@name";
            AppWindow.command.Parameters.AddWithValue("@name", AppWindow.selectedUser);
            adapter.SelectCommand = AppWindow.command;
            adapter.Fill(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                list.Add(dt.Rows[i][0].ToString());
            }

            return list;
        }

        public List<FileInStorage> LoadFiles()
        {

            List<FileInStorage> files = new List<FileInStorage>();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter();
            DataTable dt = new DataTable("Messages");

            AppWindow.command.CommandText = "SELECT username,chat,iv,file,filename,time FROM Message Where ownername=@owner AND type=@type";
            AppWindow.command.Parameters.AddWithValue("@owner", AppWindow.CurrentUsername);
            AppWindow.command.Parameters.AddWithValue("@type", "file");
            adapter.SelectCommand = AppWindow.command;
            adapter.Fill(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (File.Exists("Resources/Keys/" + dt.Rows[i][1].ToString() + ".lynx"))
                {
                    Abonent cl = Abonent.LoadKeysFromFile(dt.Rows[i][1].ToString() + ".lynx");
                    AppWindow.abonent.PrivateKey = cl.PrivateKey;
                    AppWindow.abonent.SharedKey = cl.SharedKey;

                    var msgIv = Convert.FromBase64String(dt.Rows[i][2].ToString());

                    
                    AppWindow.abonent.IV = msgIv;

                   
                    var databyte = Convert.FromBase64String(dt.Rows[i][3].ToString());
                    string decryptedFile = AppWindow.abonent.Receive(databyte);
                    files.Insert(0, new FileInStorage(dt.Rows[i][0].ToString(), decryptedFile, dt.Rows[i][4].ToString(), dt.Rows[i][5].ToString()));
                    
                }
            }
                

            return files;
        }


        public static void SaveMessage(string username, string message, string chat, string iv, string time, bool unread, string owner)
        {
            AppWindow.command.CommandText = "INSERT INTO Message(username,message,chat,iv,time,unread,ownername,type) VALUES (@username,@message,@chat,@iv,@time, @unread,@ownername,@type)";
            AppWindow.command.Parameters.AddWithValue("@username", username);
            AppWindow.command.Parameters.AddWithValue("@message", message);
            AppWindow.command.Parameters.AddWithValue("@chat", chat);
            AppWindow.command.Parameters.AddWithValue("@iv", iv);
            AppWindow.command.Parameters.AddWithValue("@time", time);
            AppWindow.command.Parameters.AddWithValue("@unread", unread);
            AppWindow.command.Parameters.AddWithValue("@ownername", owner);
            AppWindow.command.Parameters.AddWithValue("@type", "message");
            AppWindow.command.ExecuteNonQuery();
        }
        public static void SaveFileMessage(string username, string filename, string chat, string iv, string time, bool unread, string owner,string fileData)
        {
            AppWindow.command.CommandText = "INSERT INTO Message(username,message,chat,iv,time,unread,ownername,type,file,filename) VALUES (@username,@message,@chat,@iv,@time, @unread,@ownername,@type,@file,@filename)";
            AppWindow.command.Parameters.AddWithValue("@username", username);
            AppWindow.command.Parameters.AddWithValue("@message", "file");
            AppWindow.command.Parameters.AddWithValue("@chat", chat);
            AppWindow.command.Parameters.AddWithValue("@iv", iv);
            AppWindow.command.Parameters.AddWithValue("@time", time);
            AppWindow.command.Parameters.AddWithValue("@unread", unread);
            AppWindow.command.Parameters.AddWithValue("@ownername", owner);
            AppWindow.command.Parameters.AddWithValue("@type", "file");
            AppWindow.command.Parameters.AddWithValue("@file", fileData);
            AppWindow.command.Parameters.AddWithValue("@filename", filename);
            AppWindow.command.ExecuteNonQuery();
        }
    }
}
