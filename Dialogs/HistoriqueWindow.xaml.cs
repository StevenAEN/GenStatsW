using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GenStatsW.Utils;
using MySql.Data.MySqlClient;

namespace GenStatsW.Dialogs
{
    public partial class HistoriqueWindow : Window
    {
        private MySqlConnection conn;

        public MySQLQuerys querys;

        public HistoriqueWindow()
        {
            InitializeComponent();
            conn = DBUtils.GetDBConnection();
            querys = new MySQLQuerys(conn);
            LoadCommandes();
        }

        private void LoadCommandes()
        {
            List<Commande> commandes = querys.GetHistoriqueCommandes();

            comboBoxCommandes.DisplayMemberPath = "DisplayText";
            comboBoxCommandes.ItemsSource = commandes;
        }

        private void ComboBoxCommandes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxCommandes.SelectedItem is Commande selectedCommande)
            {
                dataGridArticles.ItemsSource = selectedCommande.Articles;
            }
        }
    }
}
