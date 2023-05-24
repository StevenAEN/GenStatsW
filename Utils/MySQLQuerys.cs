using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GenStatsW.Dialogs;
using MySql.Data.MySqlClient;
using GenStatsW.Utils;

namespace GenStatsW.Utils
{
    public class MySQLQuerys
    {
        private static MySqlConnectionPool? connectionPool;
        private MySqlConnection conn;


        public List<Commande> GetHistoriqueCommandes()
        {
            List<Commande> commandes = new List<Commande>();

            try
            {
                using (MySqlConnection connection = connectionPool!.GetConnection())
                {
                    string query = @"
        SELECT 
            cf.date, 
            u.username AS agent,
            f.nom_fournisseur,
            a.nom_article,
            a.prix_achat AS prix_unitaire,
            pf.quantite
        FROM commandes_fournisseurs cf
        JOIN panier_fournisseurs pf ON cf.id = pf.commande_id
        JOIN articles a ON pf.article_id = a.id
        JOIN users u ON cf.agent_id = u.id
        JOIN fournisseurs f ON cf.fournisseur_id = f.id
        ORDER BY cf.date DESC
    ";

                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime date = reader.GetDateTime("date");
                            string agent = reader.GetString("agent");
                            string nomFournisseur = reader.GetString("nom_fournisseur");
                            string nomArticle = reader.GetString("nom_article");
                            decimal prixUnitaire = reader.GetDecimal("prix_unitaire");
                            int quantite = reader.GetInt32("quantite");

                            Article article = new Article
                            {
                                Nom = nomArticle,
                                PrixUnitaire = prixUnitaire,
                                Quantite = quantite
                            };

                            Commande commande = commandes.FirstOrDefault(c => c.Date.Date == date.Date && c.Agent == agent && c.NomFournisseur == nomFournisseur);

                            if (commande == null)
                            {
                                commande = new Commande
                                {
                                    Date = date,
                                    Agent = agent,
                                    NomFournisseur = nomFournisseur,
                                    Articles = new List<Article>()
                                };
                                commandes.Add(commande);
                            }

                            commande.Articles.Add(article);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur lors de la récupération de l'historique des commandes : " + e.Message);
            }

            return commandes;
        }











        public void ModifierQuantitePanierFournisseur(int commandeId, int articleId, int quantite)
        {
            //MessageBox.Show("Nouvelle quantité : " + quantite.ToString());
            try
            {
                using (MySqlConnection connection = connectionPool!.GetConnection())
                {
                    string query = "UPDATE panier_fournisseurs SET quantite = @quantite WHERE commande_id = @commandeId AND article_id = @articleId";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@quantite", quantite);
                    command.Parameters.AddWithValue("@commandeId", commandeId);
                    command.Parameters.AddWithValue("@articleId", articleId);

                    //MessageBox.Show($"Commande ID : {commandeId}, Article ID : {articleId}, Quantité : {quantite}");

                    int rowsAffected = command.ExecuteNonQuery();

                    //MessageBox.Show($"Nombre de lignes affectées : {rowsAffected}");

                    MessageBox.Show("Quantité modifiée avec succès.");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur lors de la modification de la quantité : " + e.Message);
            }
        }

        public void SupprimerArticlePanierFournisseur(int commandeId, int articleId)
        {
            try
            {
                using (MySqlConnection connection = connectionPool!.GetConnection())
                {
                    string query = "DELETE FROM panier_fournisseurs WHERE commande_id = @commandeId AND article_id = @articleId";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@commandeId", commandeId);
                    command.Parameters.AddWithValue("@articleId", articleId);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Article supprimé du panier avec succès.");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur lors de la suppression de l'article du panier : " + e.Message);
            }
        }


        public void ValiderCommande(int commandeId, int agentId)
        {
            try
            {
                using (MySqlConnection connection = connectionPool!.GetConnection())
                {
                    //MessageBox.Show(commandeId + " " + agentId);
                    string query = "UPDATE commandes_fournisseurs SET status = 1, date = NOW(), agent_id = @agentId WHERE id = @commandeId";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@commandeId", commandeId);
                    command.Parameters.AddWithValue("@agentId", agentId);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Commande validée avec succès.");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur lors de la validation de la commande : " + e.Message);
            }
        }



        public int GetLatestCommandeIdForFournisseur(int fournisseurId, bool createNewCommande = true)
        {
            int latestCommandeId = -1;

            try
            {
                using (MySqlConnection connection = connectionPool!.GetConnection())
                {
                    string query = $"SELECT id FROM commandes_fournisseurs WHERE fournisseur_id = @fournisseurId AND status = 0 ORDER BY id DESC LIMIT 1";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@fournisseurId", fournisseurId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            latestCommandeId = reader.GetInt32("id");
                        }
                    }

                    if (latestCommandeId == -1 && createNewCommande)
                    {
                        // Aucune commande en cours trouvée, créer une nouvelle commande
                        query = $"INSERT INTO commandes_fournisseurs (fournisseur_id, status) VALUES (@fournisseurId, 0)";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@fournisseurId", fournisseurId);
                        command.ExecuteNonQuery();

                        // Récupérer l'ID de la nouvelle commande
                        query = "SELECT LAST_INSERT_ID()";
                        command = new MySqlCommand(query, connection);
                        latestCommandeId = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur lors de l'exécution de la requête : " + e.Message);
            }

            return latestCommandeId;
        }


        public void AjouterArticleAuPanierFournisseurs(int commande_id, int articleId, int quantite)
        {
            try
            {
                using (MySqlConnection connection = connectionPool!.GetConnection())
                {
                    // Vérifier si l'article est déjà présent dans le panier
                    string checkQuery = "SELECT COUNT(*) FROM panier_fournisseurs WHERE commande_id = @commande_id AND article_id = @article_id";
                    MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@commande_id", commande_id);
                    checkCommand.Parameters.AddWithValue("@article_id", articleId);

                    int existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (existingCount > 0)
                    {
                        // L'article est déjà présent, mettre à jour la quantité
                        string updateQuery = "UPDATE panier_fournisseurs SET quantite = quantite + @quantite WHERE commande_id = @commande_id AND article_id = @article_id";
                        MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection);
                        updateCommand.Parameters.AddWithValue("@commande_id", commande_id);
                        updateCommand.Parameters.AddWithValue("@article_id", articleId);
                        updateCommand.Parameters.AddWithValue("@quantite", quantite);
                        updateCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        // L'article n'est pas encore présent, l'ajouter au panier
                        string insertQuery = "INSERT INTO panier_fournisseurs (commande_id, article_id, quantite) VALUES (@commande_id, @article_id, @quantite)";
                        MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection);
                        insertCommand.Parameters.AddWithValue("@commande_id", commande_id);
                        insertCommand.Parameters.AddWithValue("@article_id", articleId);
                        insertCommand.Parameters.AddWithValue("@quantite", quantite);
                        insertCommand.ExecuteNonQuery();
                    }

                    Console.WriteLine("Article ajouté au panier avec succès.");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur lors de l'exécution de la requête : " + e.Message);
            }
        }


        public DataTable GetPanierFournisseur(int fournisseurId)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (MySqlConnection connection = connectionPool!.GetConnection())
                {
                    string query = $"SELECT pf.article_id as Article_Id, a.nom_article as Article, pf.quantite as Quantite, a.prix_achat as PrixAchat, (pf.quantite * a.prix_achat) as SousTotal " +
                                   $"FROM panier_fournisseurs pf " +
                                   $"JOIN commandes_fournisseurs cf ON pf.commande_id = cf.id " +
                                   $"JOIN articles a ON pf.article_id = a.id " +
                                   $"WHERE cf.status = 0 AND cf.fournisseur_id = {fournisseurId}";


                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur lors de la récupération des données du panier : " + e.Message);
            }

            return dataTable;
        }

        public DataTable LoadArticlesWithFilters(string nomFilter, string gencodeFilter, int? stockMinFilter, int? stockMaxFilter, decimal? prixMinFilter, decimal? prixMaxFilter)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT * FROM articles WHERE 1 = 1");

            if (!string.IsNullOrEmpty(nomFilter))
            {
                queryBuilder.Append($" AND nom_article LIKE '%{nomFilter}%'");
            }

            if (!string.IsNullOrEmpty(gencodeFilter))
            {
                queryBuilder.Append($" AND gencode LIKE '%{gencodeFilter}%'");
            }

            if (stockMinFilter.HasValue && stockMinFilter.Value != -1)
            {
                queryBuilder.Append($" AND stock >= {stockMinFilter.Value}");
            }

            if (stockMaxFilter.HasValue && stockMaxFilter.Value != -1)
            {
                queryBuilder.Append($" AND stock <= {stockMaxFilter.Value}");
            }

            if (prixMinFilter.HasValue && prixMinFilter.Value != -1)
            {
                queryBuilder.Append($" AND prix_vente >= {prixMinFilter.Value}");
            }

            if (prixMaxFilter.HasValue && prixMaxFilter.Value != -1)
            {
                queryBuilder.Append($" AND prix_vente <= {prixMaxFilter.Value}");
            }

            // Vérifier si la requête n'a pas de conditions de filtrage
            if (queryBuilder.Length <= "SELECT * FROM articles WHERE 1 = 1".Length)
            {
                // Aucun filtre spécifié, renvoyer une DataTable vide
                return new DataTable();
            }

            DataTable result = new DataTable();

            using (MySqlCommand command = new MySqlCommand(queryBuilder.ToString(), conn))
            {
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(result);
                }
            }

            return result;
        }

        public MySQLQuerys(MySqlConnection conn)
        {
            this.conn = conn; 
            if (connectionPool == null)
            {
                string connectionString = conn.ConnectionString;
                connectionPool = new MySqlConnectionPool(connectionString);
            }
        }
        public DataTable GetFournisseursData()
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (MySqlConnection connection = connectionPool!.GetConnection())
                {
                    string query = "SELECT * FROM fournisseurs";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur lors de la récupération des données des fournisseurs : " + e.Message);
            }

            return dataTable;
        }

        public DataTable GetArticlesData()
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (MySqlConnection connection = connectionPool!.GetConnection())
                {
                    string query = "SELECT a.id, a.nom_article, a.gencode, a.stock, a.prix_vente, a.prix_achat, a.fournisseur_id, " +
                                   "(SELECT COUNT(*) FROM panier_fournisseurs pf JOIN commandes_fournisseurs cf ON pf.commande_id = cf.id WHERE cf.status = 0 AND pf.article_id = a.id) AS commandes_fournisseurs, " +
                                   "(SELECT COUNT(*) FROM panier_clients pc JOIN bons b ON pc.bon_id = b.id WHERE b.status = 0 AND pc.article_id = a.id) AS commandes_clients " +
                                   "FROM articles a";
                    //MessageBox.Show(query);
                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur lors de la récupération des données des articles : " + e.Message);
            }

            return dataTable;
        }
        public DataTable GetClientsData()
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (MySqlConnection connection = connectionPool!.GetConnection())
                {
                    string query = @"SELECT c.id, c.nom, c.prenom, c.adresse, c.telephone, c.mail,
                            COUNT(DISTINCT CASE WHEN b.type_bon = 1 THEN b.id END) AS bdc_en_cours,
                            COUNT(DISTINCT CASE WHEN b.type_bon = 2 THEN b.id END) AS bdv_en_cours
                            FROM clients c
                            LEFT JOIN bons b ON c.id = b.client_id
                            GROUP BY c.id";

                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur lors de la récupération des données des clients : " + e.Message);
            }

            return dataTable;
        }
        public void UpdateArticle(DataRow row)
        {
            try
            {
                using (MySqlConnection connection = connectionPool!.GetConnection())
                {
                    string query = "UPDATE articles SET nom_article = @nom_article, gencode = @gencode, stock = @stock, prix_vente = @prix_vente, prix_achat = @prix_achat WHERE id = @id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", row["id"]);
                    command.Parameters.AddWithValue("@nom_article", row["nom_article"]);
                    command.Parameters.AddWithValue("@gencode", row["gencode"]);
                    command.Parameters.AddWithValue("@stock", row["stock"]);
                    command.Parameters.AddWithValue("@prix_vente", row["prix_vente"]);
                    command.Parameters.AddWithValue("@prix_achat", row["prix_achat"]);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur lors de la mise à jour de l'article : " + e.Message);
            }

        }

        public Tuple<string?, int?> CheckLogin(string username, string password)
        {
            using (MySqlConnection connection = connectionPool!.GetConnection())
            {
                string query = "SELECT u.id, r.role_name FROM users u JOIN role r ON u.role = r.id WHERE u.username = @username AND u.password = @password";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int userId = reader.GetInt32("id");
                        string role = reader.GetString("role_name");
                        return new Tuple<string?, int?>(role, userId);

                    }
                    else
                    {
                        return new Tuple<string?, int?>(null, null);
                    }
                }
            }
        }

    }

    class MySqlConnectionPool
    {
        private string connectionString;
        private Stack<MySqlConnection> connectionPool;

        public MySqlConnectionPool(string connectionString)
        {
            this.connectionString = connectionString;
            this.connectionPool = new Stack<MySqlConnection>();
        }

        public MySqlConnection GetConnection()
        {
            lock (connectionPool)
            {
                if (connectionPool.Count > 0)
                {
                    return connectionPool.Pop();
                }
            }

            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public void ReturnConnection(MySqlConnection connection)
        {
            lock (connectionPool)
            {
                connectionPool.Push(connection);
            }
        }
    }
}
