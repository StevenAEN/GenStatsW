using GenStatsW.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace GenStatsW
{
    public partial class LoginWindow : Window
    {
        private MySqlConnection? conn;
        private MySQLQuerys? querys;
        public string? Role { get; set; }
        public string? Username { get; set; }
        public int? UserId { get; set; }


        public LoginWindow()
        {
            InitializeComponent();
            conn = DBUtils.GetDBConnection();
            querys = conn != null ? new MySQLQuerys(conn) : null;

            // Les lignes ci-dessous permettent de sauter l'identification, à retirer dans la version finale
            this.Username = "Steven";
            this.Role = "Gestionnaire";
            this.UserId = 3;
            Gestionnaire gestionnaire = new Gestionnaire(this);
            gestionnaire.Show();
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = userBox.Text.Trim();
            string password = passwordBox.Password.Trim();

            if (querys != null)
            {
                Tuple<string?, int?> loginResult = querys.CheckLogin(username, password);

                string? role = loginResult.Item1;
                int? userId = loginResult.Item2;

                if (!string.IsNullOrEmpty(role) && userId.HasValue)
                {
                    Username = username;
                    Role = role;
                    UserId = userId.Value;
                    MessageBox.Show("Connexion réussie ! Rôle : " + role);

                    Gestionnaire gestionnaire = new Gestionnaire(this);
                    gestionnaire.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("Échec de la connexion. Vérifiez vos informations d'identification.");
                    passwordBox.Password = string.Empty;
                }
            }
        }

    }
}
