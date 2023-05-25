using GenStatsW.Dialogs;
using GenStatsW.Utils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Globalization;
using System.Windows.Media;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Configuration;
namespace GenStatsW
{
    public partial class Gestionnaire : Window, INotifyPropertyChanged
    {

        private MySqlConnection conn;

        public MySQLQuerys querys;

        private LoginWindow? loginWindow;

        public DataTable ArticlesDataTable { get; set; }
        public DataTable ClientsDataTable { get; set; }
        public DataTable FournisseursDataTable { get; set; }
        public DataTable CommandesFournisseursDataTable { get; set; }
        public List<int> Quantites { get; set; } = Enumerable.Range(1, 100).ToList();


        public int SelectedFournisseurId { get; set; }

        private void LoadFournisseurs()
        {
            if (FournisseursDataTable != null)
            {
                FournisseursDataTable.Clear();
                querys = new MySQLQuerys(conn);
                FournisseursDataTable = querys.GetFournisseursData();
                FournisseursDataTable.AcceptChanges();
                OnPropertyChanged(nameof(FournisseursDataTable));
                FournisseursComboBox.IsEnabled = true;
            }
        }



        public void GenererBonDeCommande(DataTable detailsCommande)
        {
            // Récupérer les informations du fournisseur
            DataRow? fournisseurRow = FournisseursDataTable.AsEnumerable()
                .FirstOrDefault(row => row["id"] != DBNull.Value && Convert.ToInt32(row["id"]) == SelectedFournisseurId);

            if (fournisseurRow == null)
            {
                MessageBox.Show("Impossible de trouver les informations du fournisseur.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string? nomFournisseur = fournisseurRow["nom_fournisseur"].ToString();
            string? adresseFournisseur = fournisseurRow["adresse"].ToString();
            string? mailFournisseur = fournisseurRow["mail"].ToString();

            // Création du document PDF
            Document document = new Document();
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);

            // Ouvrir le document PDF
            document.Open();

            // Ajouter le titre
            Paragraph titre = new Paragraph("Bon de commande");
            titre.Alignment = Element.ALIGN_CENTER;
            titre.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18f, BaseColor.BLUE); // Utilisation de la couleur bleue
            document.Add(titre);

            // Ajouter les informations du fournisseur
            Paragraph infoFournisseur = new Paragraph($"Fournisseur : {nomFournisseur}");
            infoFournisseur.SpacingBefore = 10f;
            document.Add(infoFournisseur);

            // Ajouter les informations de la société
            string nomSociete = "Développement Amusant Inc."; // Nom de l'entreprise fictive rigolote
            string adresseSociete = "123 Rue du Code, 75000 Ville-Amusante";
            string mailSociete = "contact@amusant.dev";
            string telephoneSociete = "01 234 567 89";
            string numeroTvaSociete = "FR123456789";

            Paragraph infoSociete = new Paragraph($"Société : {nomSociete}");
            infoSociete.SpacingAfter = 10f;
            infoSociete.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f);
            document.Add(infoSociete);

            Paragraph adresseSocieteParagraph = new Paragraph($"Adresse : {adresseSociete}");
            adresseSocieteParagraph.SpacingAfter = 5f;
            document.Add(adresseSocieteParagraph);

            if (!string.IsNullOrEmpty(mailSociete))
            {
                Paragraph mailSocieteParagraph = new Paragraph($"Email : {mailSociete}");
                mailSocieteParagraph.SpacingAfter = 5f;
                document.Add(mailSocieteParagraph);
            }

            if (!string.IsNullOrEmpty(telephoneSociete))
            {
                Paragraph telephoneSocieteParagraph = new Paragraph($"Téléphone : {telephoneSociete}");
                telephoneSocieteParagraph.SpacingAfter = 5f;
                document.Add(telephoneSocieteParagraph);
            }

            if (!string.IsNullOrEmpty(numeroTvaSociete))
            {
                Paragraph tvaSocieteParagraph = new Paragraph($"Numéro de TVA : {numeroTvaSociete}");
                tvaSocieteParagraph.SpacingAfter = 10f;
                document.Add(tvaSocieteParagraph);
            }

            // Créer le tableau des articles de la commande
            PdfPTable table = new PdfPTable(4);
            table.DefaultCell.Border = Rectangle.NO_BORDER;
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 2f, 2f, 1.5f, 2f });
            table.SpacingAfter = 10f;

            // Ajouter les en-têtes des colonnes
            PdfPCell cellHeader = new PdfPCell(new Phrase("Article"));
            cellHeader.BackgroundColor = BaseColor.LIGHT_GRAY; // Utilisation de la couleur gris clair
            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(cellHeader);

            cellHeader = new PdfPCell(new Phrase("Quantité"));
            cellHeader.BackgroundColor = BaseColor.LIGHT_GRAY; // Utilisation de la couleur gris clair
            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(cellHeader);

            cellHeader = new PdfPCell(new Phrase("Prix unitaire"));
            cellHeader.BackgroundColor = BaseColor.LIGHT_GRAY; // Utilisation de la couleur gris clair
            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(cellHeader);

            cellHeader = new PdfPCell(new Phrase("Sous-total"));
            cellHeader.BackgroundColor = BaseColor.LIGHT_GRAY; // Utilisation de la couleur gris clair
            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            table.AddCell(cellHeader);

            // Ajouter les lignes des détails de la commande
            foreach (DataRow row in detailsCommande.Rows)
            {
                string? article = Convert.ToString(row["Article"]);
                int quantite = Convert.ToInt32(row["Quantite"]);
                decimal prixAchat = Convert.ToDecimal(row["PrixAchat"]);
                decimal sousTotal = Convert.ToDecimal(row["SousTotal"]);

                table.AddCell(article);
                table.AddCell(quantite.ToString());
                table.AddCell(prixAchat.ToString("0.00"));
                table.AddCell(sousTotal.ToString("0.00"));
            }

            // Ajouter le tableau au document PDF
            document.Add(table);

            // Calculer le total et la TVA
            decimal totalHt = GetTotalPrice();
            decimal tauxTva = 0.20m; // Taux de TVA fictif de 20%
            decimal tva = totalHt * tauxTva;
            decimal totalTtc = totalHt + tva;

            // Ajouter le total en gras
            Paragraph totalHtParagraph = new Paragraph($"Total HT : {totalHt.ToString("0.00")} €");
            totalHtParagraph.Alignment = Element.ALIGN_RIGHT;
            totalHtParagraph.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f);
            document.Add(totalHtParagraph);

            Paragraph tvaParagraph = new Paragraph($"TVA ({(tauxTva * 100).ToString()}%) : {tva.ToString("0.00")} €");
            tvaParagraph.Alignment = Element.ALIGN_RIGHT;
            tvaParagraph.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f);
            document.Add(tvaParagraph);

            Paragraph totalTtcParagraph = new Paragraph($"Total TTC : {totalTtc.ToString("0.00")} €");
            totalTtcParagraph.Alignment = Element.ALIGN_RIGHT;
            totalTtcParagraph.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f);
            totalTtcParagraph.SpacingBefore = 10f;
            document.Add(totalTtcParagraph);

            // Ajouter les informations en bas de page
            Paragraph infoBasDePage = new Paragraph();
            infoBasDePage.SpacingBefore = 20f;
            infoBasDePage.Add("Siège social :");
            infoBasDePage.Add(Environment.NewLine);
            infoBasDePage.Add(adresseSociete);
            infoBasDePage.Add(Environment.NewLine);
            infoBasDePage.Add($"Téléphone : {telephoneSociete}");
            infoBasDePage.Add(Environment.NewLine);
            infoBasDePage.Add($"Email : {mailSociete}");
            infoBasDePage.Add(Environment.NewLine);
            infoBasDePage.Add($"Numéro de TVA : {numeroTvaSociete}");
            infoBasDePage.Alignment = Element.ALIGN_CENTER;
            document.Add(infoBasDePage);

            // Fermer le document PDF
            document.Close();

            // Télécharger le fichier PDF
            byte[] pdfBytes = stream.ToArray();
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{timestamp}_BonDeCommande_{CommandeId.Text.Trim()}.pdf";
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Genstats", "Bon de commande");
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, fileName);
            File.WriteAllBytes(filePath, pdfBytes);

            // Ouvrir le dossier et sélectionner le fichier
            //Process.Start("explorer.exe", $"/select, \"{filePath}\"");

            // Proposer l'envoi automatique du bon de commande par e-mail
            if (!string.IsNullOrEmpty(mailFournisseur))
            {
                string subject = $"Bon de commande de {nomSociete}";

                string logoUrl = "https://i.imgur.com/dyjovOF.png";

                StringBuilder bodyBuilder = new StringBuilder();
                bodyBuilder.AppendLine("<html><body>");
                bodyBuilder.AppendLine($"<img src='{logoUrl}' alt='{nomSociete}' /><br/>");
                bodyBuilder.AppendLine($"<h2 style='color: blue;'>Cher Fournisseur <strong>{nomFournisseur}</strong>,</h2><br/>");
                bodyBuilder.AppendLine($"<p>Veuillez trouver ci-joint le bon de commande de <strong>{nomSociete}</strong> d'un montant de <strong>{totalTtc.ToString("0.00")} € TTC</strong>.</p>");
                bodyBuilder.AppendLine("<p>Nous vous prions d’accuser réception de cette commande et de nous informer des délais de livraison.</p><br/>");
                bodyBuilder.AppendLine("<p>Cordialement,</p>");
                bodyBuilder.AppendLine($"<p><strong>{nomSociete}</strong></p>");
                bodyBuilder.AppendLine($"<p>{adresseSociete}</p>");
                bodyBuilder.AppendLine($"<p>Téléphone : {telephoneSociete}</p>");
                bodyBuilder.AppendLine($"<p>Email : {mailSociete}</p>");
                bodyBuilder.AppendLine($"<p>Numéro de TVA : {numeroTvaSociete}</p>");
                bodyBuilder.AppendLine("</body></html>");

                string body = bodyBuilder.ToString();

                bool sendMail = MessageBox.Show("Souhaitez-vous envoyer automatiquement le bon de commande par e-mail ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                if (sendMail)
                {
                    try
                    {
                        string smtpServer = ConfigurationManager.AppSettings["SmtpServer"] ?? string.Empty;
                        int port;
                        if (int.TryParse(ConfigurationManager.AppSettings["Port"], out port))
                        {
                            string email = ConfigurationManager.AppSettings["Email"] ?? string.Empty;
                            string password = ConfigurationManager.AppSettings["Password"] ?? string.Empty;

                            if (!string.IsNullOrEmpty(smtpServer) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                            {
                                // Utilisez les valeurs récupérées comme nécessaire
                                // Par exemple :
                                MailMessage mail = new MailMessage();
                                SmtpClient smtpClient = new SmtpClient(smtpServer, port);

                                mail.From = new MailAddress(email);
                                mail.To.Add(mailFournisseur);
                                mail.Subject = subject;
                                mail.Body = body;
                                mail.Attachments.Add(new Attachment(filePath));
                                mail.IsBodyHtml = true; // Spécifie que le corps est au format HTML

                                smtpClient.Credentials = new NetworkCredential(email, password);
                                smtpClient.EnableSsl = true;

                                smtpClient.Send(mail);

                                MessageBox.Show("Le bon de commande a été envoyé par e-mail.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Les informations de configuration SMTP sont incorrectes ou manquantes.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Le port SMTP spécifié dans la configuration est invalide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Une erreur s'est produite lors de l'envoi du bon de commande par e-mail : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }




        public Gestionnaire(LoginWindow loginWindow)
        {
            InitializeComponent();
            conn = DBUtils.GetDBConnection();
            this.loginWindow = loginWindow;
            Title = $"Gestionnaire - Identifié en tant que {loginWindow?.Username ?? "Utilisateur inconnu"} ({loginWindow?.UserId?.ToString() ?? "Id inconnu"}) [{loginWindow?.Role ?? "Role inconnu"}]";

            DataContext = this;


            ArticlesDataTable = new DataTable();
            ClientsDataTable = new DataTable();
            FournisseursDataTable = new DataTable();
            CommandesFournisseursDataTable = new DataTable();
            querys = new MySQLQuerys(conn);

            if (loginWindow?.Role != "Gestionnaire")
            {
                articlesGrid.IsReadOnly = true;
                clientsGrid.IsReadOnly = true;
            }
            articleComboBox.SelectionChanged += ArticleComboBox_SelectionChanged;
            QuantiteComboBox.SelectionChanged += QuantiteComboBox_SelectionChanged;

            LoadArticles();
            LoadClients();
            LoadFournisseurs();
        }
        private void AjouterArticle_Click(object sender, RoutedEventArgs e)
        {
            if (articleComboBox.SelectedItem is DataRowView selectedItem && QuantiteComboBox.SelectedItem is int quantite)
            {
                try
                {
                    var articleId = Convert.ToInt32(selectedItem["id"]);
                    var article = Convert.ToString(selectedItem["nom_article"]);
                    var prix_achat = Convert.ToDecimal(selectedItem["prix_achat"]);

                    // Ajouter à la base de données SQL
                    var commandeId = querys.GetLatestCommandeIdForFournisseur(SelectedFournisseurId);
                    CommandeId.Text = commandeId.ToString();

                    if (commandeId != 0)
                    {
                        querys.AjouterArticleAuPanierFournisseurs(commandeId, articleId, quantite);

                        // Vérifier si l'article est déjà présent dans CommandesFournisseursDataTable
                        DataRow? existingRow = CommandesFournisseursDataTable.AsEnumerable()
                            .FirstOrDefault(row => Convert.ToString(row["Article"]!) == Convert.ToString(article) && Convert.ToInt32(row["Article_Id"]) == articleId);


                        if (existingRow != null)
                        {
                            // L'article est déjà présent, mettre à jour la quantité
                            existingRow["Quantite"] = Convert.ToInt32(existingRow["Quantite"]) + quantite;
                            existingRow["SousTotal"] = Convert.ToInt32(existingRow["Quantite"]) * prix_achat;
                        }
                        else
                        {
                            // L'article n'est pas encore présent, l'ajouter à CommandesFournisseursDataTable
                            DataRow newRow = CommandesFournisseursDataTable.NewRow();
                            newRow["Article"] = article;
                            newRow["Article_Id"] = articleId;
                            newRow["Quantite"] = quantite;
                            newRow["PrixAchat"] = prix_achat;
                            newRow["SousTotal"] = quantite * prix_achat;
                            CommandesFournisseursDataTable.Rows.Add(newRow);
                        }

                        CommandesFournisseursDataTable.AcceptChanges();
                        if (CommandesFournisseursDataTable.Rows.Count > 0)
                        {
                            ValiderCommandeButton.IsEnabled = true;
                        }
                        // Notify the UI of the updated property value
                        OnPropertyChanged(nameof(CommandesFournisseursDataTable));
                    }
                    else
                    {
                        MessageBox.Show("Aucune commande en cours trouvée. Créez une nouvelle commande avant d'ajouter des articles.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ArticleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (articleComboBox.SelectedItem != null)
            {
                QuantiteComboBox.IsEnabled = true;
            }
            else
            {
                QuantiteComboBox.IsEnabled = false;
                AjouterArticleButton.IsEnabled = false; // Désactive le bouton Ajouter au panier si aucun article n'est sélectionné
            }
        }
        private void QuantiteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QuantiteComboBox.SelectedItem != null)
            {
                AjouterArticleButton.IsEnabled = true;
            }
            else
            {
                AjouterArticleButton.IsEnabled = false;
            }
        }

        private void FournisseursComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is DataRowView selectedItem)
            {
                if (selectedItem["id"] != DBNull.Value) // Vérifier si la valeur n'est pas null
                {
                    SelectedFournisseurId = Convert.ToInt32(selectedItem["id"]);

                    DataView articlesView = ArticlesDataTable.DefaultView;
                    articlesView.RowFilter = $"fournisseur_id = {SelectedFournisseurId}";
                    DataTable filteredArticles = articlesView.ToTable();
                    articleComboBox.ItemsSource = filteredArticles.DefaultView;
                    articleComboBox.IsEnabled = true;
                    var commande_id = querys.GetLatestCommandeIdForFournisseur(SelectedFournisseurId, false);
                    CommandeId.Text = commande_id.ToString();
                    // Récupérer le panier pour le fournisseur sélectionné
                    CommandesFournisseursDataTable = querys.GetPanierFournisseur(SelectedFournisseurId);
                    CommandesFournisseursDataTable.AcceptChanges();
                    OnPropertyChanged(nameof(CommandesFournisseursDataTable));
                    if (CommandesFournisseursDataTable.Rows.Count > 0)
                    {
                        ValiderCommandeButton.IsEnabled = true;
                    }
                    else
                    {
                        ValiderCommandeButton.IsEnabled = false;
                    }
                }
            }
        }



        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {


            if (loginWindow?.Role != "Gestionnaire")
            {
                MessageBox.Show("Vous n'êtes pas autorisé à modifier les cellules.");
                e.Cancel = true;
                return;
            }
            var dataGrid = (DataGrid)sender;
            var editedItem = e.Row.Item as DataRowView;
            var editedColumn = e.Column as DataGridBoundColumn;

            if (dataGrid != null && editedItem != null && editedColumn != null)
            {
                var columnName = editedColumn.SortMemberPath;
                var editedElement = e.EditingElement as TextBox;
                var newValue = editedElement?.Text;



                bool isValid = false;
                object? convertedValue = null;


                switch (columnName)
                {
                    case "nom_article":
                        isValid = ValidateValue<string>(newValue, out string stringValue);
                        convertedValue = stringValue;
                        break;
                    case "gencode":
                        isValid = ValidateValue<int>(newValue, out int intValue);
                        convertedValue = intValue;
                        break;
                    case "stock":
                        isValid = ValidateValue<int>(newValue, out int stockValue);
                        convertedValue = stockValue;
                        break;
                    case "prix_vente":
                    case "prix_achat":
                        isValid = ValidateValue<decimal>(newValue, out decimal decimalValue);
                        convertedValue = decimalValue;
                        break;
                    default:
                        break;
                }

                if (!isValid)
                {
                    MessageBox.Show("Donnée invalide!");
                    e.Cancel = true;
                }
                else
                {
                    editedItem[columnName] = convertedValue;
                    querys?.UpdateArticle(editedItem.Row);
                }
            }
        }

        private bool ValidateValue<T>(string? value, out T convertedValue)
        {
            bool isValid = false;
            convertedValue = default!;

            if (!string.IsNullOrEmpty(value))
            {
                if (typeof(T) == typeof(string))
                {
                    convertedValue = (T)(object)value!;
                    isValid = true;
                }
                else if (typeof(T) == typeof(int))
                {
                    isValid = int.TryParse(value, out int intValue);
                    convertedValue = (T)(object)intValue;
                }
                else if (typeof(T) == typeof(decimal))
                {
                    value = value!.Replace(',', '.');
                    isValid = decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal decimalValue);
                    convertedValue = (T)(object)decimalValue;
                }
            }

            return isValid;
        }





        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TabControl tabControl = (TabControl)sender;
            //int selectedIndex = tabControl.SelectedIndex;

            //switch (selectedIndex)
            //{
            //    case 0:
            //        LoadArticles();
            //        break;
            //    case 1:
            //        LoadClients();
            //        break;
            //}
        }
        private void HistoriqueButton_Click(object sender, RoutedEventArgs e)
        {
            HistoriqueWindow historiqueWindow = new HistoriqueWindow();
            historiqueWindow.ShowDialog();
        }
        private void ValiderCommandeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CommandesFournisseursDataTable.Rows.Count == 0)
            {

                ValiderCommandeButton.IsEnabled = false;
                return;
            }

            int totalCount = GetTotalCount();
            int distinctCount = CommandesFournisseursDataTable.Rows.Count;

            MessageBoxResult result = MessageBox.Show($"Voulez-vous passer cette commande ? Elle contient {totalCount} articles ({distinctCount} articles différents) pour un total de {GetTotalPrice()} €.",
                                                      "Confirmation",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Générer et télécharger le bon de commande en PDF
                GenererBonDeCommande(CommandesFournisseursDataTable);

                int commandeId = int.Parse(CommandeId.Text);
                querys.ValiderCommande(commandeId, this.loginWindow?.UserId ?? default);

                CommandesFournisseursDataTable.Clear();
                CommandesFournisseursDataTable.AcceptChanges();
                OnPropertyChanged(nameof(CommandesFournisseursDataTable));
                ValiderCommandeButton.IsEnabled = false;
            }
        }

        private int GetTotalCount()
        {
            int totalCount = 0;

            foreach (DataRow row in CommandesFournisseursDataTable.Rows)
            {
                int quantity = Convert.ToInt32(row["Quantite"]);
                totalCount += quantity;
            }

            return totalCount;
        }

        private decimal GetTotalPrice()
        {
            decimal totalPrice = 0;

            foreach (DataRow row in CommandesFournisseursDataTable.Rows)
            {
                decimal price = Convert.ToDecimal(row["SousTotal"]);
                totalPrice += price;
            }

            return totalPrice;
        }
        private void SupprimerArticle_Click(object sender, RoutedEventArgs e)
        {
            if (commandeFournisseurGrid.SelectedItem is DataRowView selectedItem)
            {
                int articleId = Convert.ToInt32(selectedItem["Article_Id"]);
                int commandeId = int.Parse(CommandeId.Text.Trim());

                MessageBox.Show($"ID de l'article : {articleId} - Commande {commandeId}");

                // Appeler la méthode pour supprimer l'article du panier fournisseur
                querys.SupprimerArticlePanierFournisseur(commandeId, articleId);
                CommandesFournisseursDataTable.Rows.Remove(selectedItem.Row);
            }
        }


        private void ModifierQuantite_Click(object sender, RoutedEventArgs e)
        {
            if (commandeFournisseurGrid.SelectedItem is DataRowView selectedItem)
            {
                // Afficher une boîte de dialogue pour modifier la quantité de l'article
                var modifierQuantiteDialog = new ModifierQuantiteDialog();
                modifierQuantiteDialog.Owner = this;
                modifierQuantiteDialog.Quantite = Convert.ToInt32(selectedItem["Quantite"]);
                modifierQuantiteDialog.QuantiteActuelle = Convert.ToInt32(selectedItem["Quantite"]);

                if (modifierQuantiteDialog.ShowDialog() == true)
                {
                    int nouvelleQuantite = modifierQuantiteDialog.Quantite;
                    int articleId = Convert.ToInt32(selectedItem["Article_Id"]);
                    int commandeId = int.Parse(CommandeId.Text.Trim());

                    // Mettre à jour la quantité dans le panier fournisseur
                    querys.ModifierQuantitePanierFournisseur(commandeId, articleId, nouvelleQuantite);

                    // Mettre à jour la quantité dans la DataGrid
                    selectedItem["Quantite"] = nouvelleQuantite;
                    decimal prixAchat = Convert.ToDecimal(selectedItem["PrixAchat"]);
                    selectedItem["SousTotal"] = nouvelleQuantite * prixAchat;
                    CommandesFournisseursDataTable.AcceptChanges();
                    OnPropertyChanged(nameof(CommandesFournisseursDataTable));
                }
            }
        }

        private void LoadArticles()
        {
            if (ArticlesDataTable != null)
            {

                //loadingArticle.IsBusy = true;
                ArticlesDataTable.Clear();
                querys = new MySQLQuerys(conn);
                ArticlesDataTable = querys.GetArticlesData();
                ArticlesDataTable.AcceptChanges();
                OnPropertyChanged(nameof(ArticlesDataTable)); // Notify the UI of the updated property value

                //loadingArticle.IsBusy = false;
            }
        }
        private void FiltersButton_Click(object sender, RoutedEventArgs e)
        {
            // Ouvrir une fenêtre de dialogue pour les filtres
            var filtersWindow = new FiltersWindow(this);  // Pass this instance of Gestionnaire to FiltersWindow
            filtersWindow.Owner = this;
            filtersWindow.ShowDialog();
        }

        private void LoadClients()
        {
            if (ClientsDataTable != null)
            {

                //loadingClients.IsBusy = true;
                querys = new MySQLQuerys(conn);
                ClientsDataTable = querys.GetClientsData();
                ClientsDataTable.AcceptChanges();
                OnPropertyChanged(nameof(ClientsDataTable)); // Notify the UI of the updated property value

                //loadingClients.IsBusy = false;

            }

        }

        private void RechargerButton_Click(object sender, RoutedEventArgs e)
        {
            LoadArticles();
        }


    }
}