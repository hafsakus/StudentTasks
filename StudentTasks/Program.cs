using System;
using System.Data.SqlClient;

namespace StudentTasks
{
    internal class Program
    {
        static string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=StudentTask;Integrated Security=True;TrustServerCertificate=True;";

        static void Main(string[] args)
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("        ÖĞRENCİ GİRİŞ SİSTEMİ           ");
            Console.WriteLine("----------------------------------------\n");

            Console.Write("Kullanıcı adı: ");
            string inputUsername = Console.ReadLine();

            Console.Write("Şifre: ");
            string inputPassword = Console.ReadLine();

            int loggedInUserId = -1;
            string loggedInUserName = "";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string loginQuery = "SELECT Id, Name FROM Users WHERE UserName = @user AND Password = @pass";

                    using (SqlCommand loginCmd = new SqlCommand(loginQuery, connection))
                    {
                        loginCmd.Parameters.AddWithValue("@user", inputUsername);
                        loginCmd.Parameters.AddWithValue("@pass", inputPassword);

                        using (SqlDataReader reader = loginCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                loggedInUserId = Convert.ToInt32(reader["Id"]);
                                loggedInUserName = reader["Name"].ToString();
                                Console.WriteLine($"\nGiriş Başarılı! Hoş geldin, {loggedInUserName} ✅");
                            }
                            else
                            {
                                Console.WriteLine("\n❌ Hatalı kullanıcı adı veya şifre!");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n⚠️ Bağlantı Hatası: " + ex.Message);
                }
            }

            if (loggedInUserId != -1)
            {
                bool continueApp = true;

                while (continueApp)
                {
                    Console.WriteLine("\n========================================");
                    Console.WriteLine($"         ÖĞRENCİ AJANDASI ({loggedInUserName})");
                    Console.WriteLine("========================================");
                    Console.WriteLine("1 - Ödevlerimi Listele");
                    Console.WriteLine("2 - Yeni Ödev Ekle");
                    Console.WriteLine("3 - Ödev Durumu Güncelle");
                    Console.WriteLine("4 - Ödev Sil");
                    Console.WriteLine("5 - Çıkış Yap");
                    Console.Write("\nSeçiminiz (1-5): ");

                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            ListAssignments(loggedInUserId);
                            break;
                        case "2":
                            AddAssignment(loggedInUserId);
                            break;
                        case "3":
                            UpdateAssignmentStatus(loggedInUserId);
                            break;
                        case "4":
                            DeleteAssignment(loggedInUserId);
                            break;
                        case "5":
                            Console.WriteLine("\nÇıkış yapılıyor...");
                            continueApp = false;
                            break;
                        default:
                            Console.WriteLine("\n⚠️ Geçersiz seçim!");
                            break;
                    }
                }
            }

            Console.WriteLine("\nKapatmak için bir tuşa basın...");
            Console.ReadKey();
        }

        // --- LİSTELEME ---
        static void ListAssignments(int userId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Id, ClassName, Subject, Status FROM Assignment WHERE UserId = @userId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine("\n---------------- ÖDEV LİSTENİZ ----------------");
                            bool hasData = false;

                            while (reader.Read())
                            {
                                hasData = true;
                                int id = Convert.ToInt32(reader["Id"]);
                                string className = reader["ClassName"].ToString();
                                string subject = reader["Subject"].ToString();
                                string status = reader["Status"].ToString();

                                Console.WriteLine($"[{id}] Ders: {className,-12} | Konu: {subject,-20} | Durum: {status}");
                            }

                            if (!hasData)
                            {
                                Console.WriteLine("Henüz kayıtlı bir ödeviniz yok.");
                            }
                            Console.WriteLine("-----------------------------------------------");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Listeleme Hatası: " + ex.Message);
                }
            }
        }

        // --- EKLEME ---
        static void AddAssignment(int userId)
        {
            Console.WriteLine("\n--- YENİ ÖDEV EKLE ---");
            Console.Write("Ders Adı: ");
            string className = Console.ReadLine();

            Console.Write("Ödev Konusu: ");
            string subject = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(className) || string.IsNullOrWhiteSpace(subject))
            {
                Console.WriteLine("\n⚠️ Boş veri girilemez!");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO Assignment (UserId, ClassName, Subject, Status) VALUES (@userId, @class, @subject, @status)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@class", className);
                        cmd.Parameters.AddWithValue("@subject", subject);
                        cmd.Parameters.AddWithValue("@status", "Devam Ediyor");

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            Console.WriteLine("\n🎯 Ödev başarıyla eklendi!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ekleme Hatası: " + ex.Message);
                }
            }
        }

        // --- GÜNCELLEME ---
        static void UpdateAssignmentStatus(int userId)
        {
            Console.WriteLine("\n--- ÖDEV DURUMU GÜNCELLE ---");
            Console.Write("Güncellenecek Ödev ID: ");

            if (!int.TryParse(Console.ReadLine(), out int assignmentId))
            {
                Console.WriteLine("⚠️ Geçerli bir sayı girmelisiniz!");
                return;
            }

            Console.WriteLine("Yeni Durum: 1- Tamamlandı | 2- Devam Ediyor");
            Console.Write("Seçiminiz (1-2): ");
            string statusChoice = Console.ReadLine();
            string newStatus = (statusChoice == "1") ? "Tamamlandı" : "Devam Ediyor";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Assignment SET Status = @status WHERE Id = @id AND UserId = @userId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@status", newStatus);
                        cmd.Parameters.AddWithValue("@id", assignmentId);
                        cmd.Parameters.AddWithValue("@userId", userId);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            Console.WriteLine("\n✅ Ödev durumu başarıyla güncellendi!");
                        }
                        else
                        {
                            Console.WriteLine("\n❌ Belirtilen ID'de bir ödev bulunamadı!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Güncelleme Hatası: " + ex.Message);
                }
            }
        }

        // --- SİLME ---
        static void DeleteAssignment(int userId)
        {
            Console.WriteLine("\n--- ÖDEV SİL ---");
            Console.Write("Silinecek Ödev ID: ");

            if (!int.TryParse(Console.ReadLine(), out int assignmentId))
            {
                Console.WriteLine("⚠️ Geçerli bir sayı girmelisiniz!");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Assignment WHERE Id = @id AND UserId = @userId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", assignmentId);
                        cmd.Parameters.AddWithValue("@userId", userId);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            Console.WriteLine("\n🗑️ Ödev başarıyla silindi!");
                        }
                        else
                        {
                            Console.WriteLine("\n❌ Belirtilen ID'de bir ödev bulunamadı!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Silme Hatası: " + ex.Message);
                }
            }
        }
    }
}