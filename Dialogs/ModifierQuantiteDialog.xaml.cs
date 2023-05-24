using System.Windows;

namespace GenStatsW.Dialogs
{
    public partial class ModifierQuantiteDialog : Window
    {
        public int Quantite { get; set; }
        public int QuantiteActuelle { get; set; }

        public ModifierQuantiteDialog()
        {
            InitializeComponent();
            QuantiteActuelle = Quantite; // Initialiser la quantité actuelle avec la quantité passée en paramètre
            DataContext = this; // Définir le DataContext sur cette fenêtre pour que la liaison fonctionne
        }

        private void ValiderButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void AnnulerButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
