namespace lezione65z.Controllers
{
    public class AnagraficaModel
    {
        public int IdAnagrafica { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string Indirizzo { get; set; }
        public string Città { get; set; }
        public string CAP { get; set; }
        public string Cod_Fisc { get; set; }
        public List<string> DescrizioniViolazioni { get; set; } 
    }

}
