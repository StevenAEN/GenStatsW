using System;
using System.Collections.Generic;
using System.Linq;

namespace GenStatsW.Dialogs
{
    public class Commande
    {
        public DateTime Date { get; set; }
        public string Heure => Date.ToString("HH:mm");
        public string NomFournisseur { get; set; }
        public string Agent { get; set; }
        public List<Article> Articles { get; set; }

        public int NbArticles => Articles?.Count ?? 0;

        public decimal Total
        {
            get
            {
                return Articles?.Sum(a => a.SousTotal) ?? 0;
            }
        }

        public string DisplayText
        {
            get
            {
                return $"[{Date.ToString("dd-MM-yyyy HH:mm")}] [Vendeur: {Agent}] [Fournisseur: {NomFournisseur}] [Articles: {NbArticles}] [Total: {Total.ToString("C2")} €]";
            }
        }
    }

    public class Article
    {
        public string Nom { get; set; }
        public decimal PrixUnitaire { get; set; }
        public int Quantite { get; set; }

        public decimal SousTotal => PrixUnitaire * Quantite;
    }
}
