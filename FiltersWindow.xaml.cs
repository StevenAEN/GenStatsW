using System.Windows;
using System.ComponentModel;

namespace GenStatsW
{
    // Classe FiltersWindow

    public partial class FiltersWindow : Window
    {
        private Gestionnaire gestionnaire;

        public FiltersWindow(Gestionnaire gestionnaire)
        {
            InitializeComponent();
            this.gestionnaire = gestionnaire;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Assigner les valeurs des champs de saisie aux propriétés correspondantes
            string? NomFilter = NomTextBox.Text;
            string? GencodeFilter = GencodeTextBox.Text;

            int? StockMinFilter = null;
            int? StockMaxFilter = null;
            decimal? PrixMinFilter = null;
            decimal? PrixMaxFilter = null;

            int tempStockMin, tempStockMax;
            decimal tempPrixMin, tempPrixMax;

            if (int.TryParse(StockMinTextBox.Text, out tempStockMin))
            {
                StockMinFilter = tempStockMin;
            }

            if (int.TryParse(StockMaxTextBox.Text, out tempStockMax))
            {
                StockMaxFilter = tempStockMax;
            }

            if (decimal.TryParse(PrixMinTextBox.Text, out tempPrixMin))
            {
                PrixMinFilter = tempPrixMin;
            }

            if (decimal.TryParse(PrixMaxTextBox.Text, out tempPrixMax))
            {
                PrixMaxFilter = tempPrixMax;
            }

            // Vérifier si tous les filtres sont vides, nulls ou -1
            if (string.IsNullOrWhiteSpace(NomFilter) &&
                string.IsNullOrWhiteSpace(GencodeFilter) &&
                (!StockMinFilter.HasValue || StockMinFilter == -1) &&
                (!StockMaxFilter.HasValue || StockMaxFilter == -1) &&
                (!PrixMinFilter.HasValue || PrixMinFilter == -1) &&
                (!PrixMaxFilter.HasValue || PrixMaxFilter == -1))
            {
                return; // Ne pas appeler la fonction de filtrage si tous les filtres sont vides, nulls ou -1
            }

            // Charger les articles avec les filtres appliqués
            if (gestionnaire.querys != null)
            {
                gestionnaire.ArticlesDataTable = gestionnaire.querys.LoadArticlesWithFilters(
                    NomFilter ?? string.Empty,
                    GencodeFilter ?? string.Empty,
                    StockMinFilter,
                    StockMaxFilter,
                    PrixMinFilter,
                    PrixMaxFilter
                );

                gestionnaire.OnPropertyChanged(nameof(gestionnaire.ArticlesDataTable)); // Notify the UI of the updated property value
            }

            // Fermer la fenêtre de dialogue
            Close();
        }
    }
}
