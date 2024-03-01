namespace lezione65z.Controllers
{
    public class VerbaleModel
    {
        public DateTime DataViolazione { get; set; }
        public string IndirizzoViolazione { get; set; }
        public string Nominativo_Agente { get; set; }
        public DateTime DataTrascrizioneVerbale { get; set; }
        public decimal Importo { get; set; }
        public int DecurtamentoPunti { get; set; }
        public int IdAnagrafica { get; set; }
        public int IdViolazione { get; set; }
    }
}
