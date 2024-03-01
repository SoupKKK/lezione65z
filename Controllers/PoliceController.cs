using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;


namespace lezione65z.Controllers
{
    public class PoliceController : Controller
    {
        private string connString = "Server=DESKTOP-F6A69FR\\SQLEXPRESS; Initial Catalog=Municipale; Integrated Security=true; TrustServerCertificate=True";

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AnagrafaTrasgressore()
        {
            var model = new List<AnagraficaModel>();
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                var query = @"
            SELECT A.*, V.descrizione AS DescrizioneViolazione
            FROM ANAGRAFICA A
            LEFT JOIN VERBALE VB ON A.idanagrafica = VB.idanagrafica
            LEFT JOIN TIPO_VIOLAZIONE V ON VB.idviolazione = V.idviolazione";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var anagraficaId = Convert.ToInt32(reader["idanagrafica"]);

                        // Cerca l'utente nel model
                        var anagrafica = model.FirstOrDefault(a => a.IdAnagrafica == anagraficaId);

                        // Se l'utente non è presente nel model, aggiungilo
                        if (anagrafica == null)
                        {
                            anagrafica = new AnagraficaModel
                            {
                                IdAnagrafica = anagraficaId,
                                Cognome = reader["Cognome"].ToString(),
                                Nome = reader["Nome"].ToString(),
                                Indirizzo = reader["Indirizzo"].ToString(),
                                Città = reader["Città"].ToString(),
                                CAP = reader["CAP"].ToString(),
                                Cod_Fisc = reader["Cod_Fisc"].ToString(),
                                DescrizioniViolazioni = new List<string>()
                            };

                            model.Add(anagrafica);
                        }

                        // Aggiungi la descrizione della violazione solo se presente
                        if (!reader.IsDBNull(reader.GetOrdinal("DescrizioneViolazione")))
                        {
                            anagrafica.DescrizioniViolazioni.Add(reader["DescrizioneViolazione"].ToString());
                        }
                    }
                }
            }

            return View(model);
        }



       

        [HttpGet]
        public IActionResult CompilaVerbale()
        {
            // Implementa la logica per compilare il verbale qui
            return View();
        }



        [HttpGet]
        public IActionResult InserisciDati()
        {
            return View();
        }

        [HttpPost]
        public IActionResult InserisciDati(VerbaleModel verbaleModel, AnagraficaModel anagraficaModel, ViolazioneModel violazioneModel)
        {
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                // Cerca una persona con lo stesso nome e cognome
                var checkAnagraficaQuery = @"
            SELECT idanagrafica
            FROM ANAGRAFICA
            WHERE Cognome = @Cognome AND Nome = @Nome";

                int idAnagrafica;

                using (var cmdCheckAnagrafica = new SqlCommand(checkAnagraficaQuery, conn))
                {
                    cmdCheckAnagrafica.Parameters.AddWithValue("@Cognome", anagraficaModel.Cognome);
                    cmdCheckAnagrafica.Parameters.AddWithValue("@Nome", anagraficaModel.Nome);

                    var existingAnagraficaId = cmdCheckAnagrafica.ExecuteScalar();

                    if (existingAnagraficaId != null)
                    {
                        // Persona già presente nel database, usa l'ID esistente
                        idAnagrafica = Convert.ToInt32(existingAnagraficaId);
                    }
                    else
                    {
                        // Inserimento dati Anagrafica
                        var anagraficaQuery = @"
                    INSERT INTO ANAGRAFICA (Cognome, Nome, Indirizzo, Città, CAP, Cod_Fisc)
                    VALUES (@Cognome, @Nome, @Indirizzo, @Città, @CAP, @Cod_Fisc);
                    SELECT SCOPE_IDENTITY();";

                        using (var cmdAnagrafica = new SqlCommand(anagraficaQuery, conn))
                        {
                            cmdAnagrafica.Parameters.AddWithValue("@Cognome", anagraficaModel.Cognome);
                            cmdAnagrafica.Parameters.AddWithValue("@Nome", anagraficaModel.Nome);
                            cmdAnagrafica.Parameters.AddWithValue("@Indirizzo", anagraficaModel.Indirizzo);
                            cmdAnagrafica.Parameters.AddWithValue("@Città", anagraficaModel.Città);
                            cmdAnagrafica.Parameters.AddWithValue("@CAP", anagraficaModel.CAP);
                            cmdAnagrafica.Parameters.AddWithValue("@Cod_Fisc", anagraficaModel.Cod_Fisc);

                            // Esecuzione della query e recupero dell'ID anagrafica appena inserito
                            var idAnagraficaDecimal = (decimal)cmdAnagrafica.ExecuteScalar();
                            idAnagrafica = Convert.ToInt32(idAnagraficaDecimal);
                        }
                    }
                }

                // Inserimento dati Tipo Violazione
                var violazioneQuery = @"
            INSERT INTO TIPO_VIOLAZIONE (descrizione)
            VALUES (@Descrizione);
            SELECT SCOPE_IDENTITY();";

                using (var cmdViolazione = new SqlCommand(violazioneQuery, conn))
                {
                    cmdViolazione.Parameters.AddWithValue("@Descrizione", violazioneModel.Descrizione);

                    // Esecuzione della query e recupero dell'ID violazione appena inserito
                    var idViolazioneDecimal = (decimal)cmdViolazione.ExecuteScalar();
                    var idViolazione = Convert.ToInt32(idViolazioneDecimal);

                    // Inserimento dati Verbale
                    var verbaleQuery = @"
                INSERT INTO VERBALE (DataViolazione, IndirizzoViolazione, Nominativo_Agente, DataTrascrizioneVerbale, Importo, DecurtamentoPunti, idanagrafica, idviolazione)
                VALUES (@DataViolazione, @IndirizzoViolazione, @Nominativo_Agente, @DataTrascrizioneVerbale, @Importo, @DecurtamentoPunti, @IdAnagrafica, @IdViolazione)";

                    using (var cmdVerbale = new SqlCommand(verbaleQuery, conn))
                    {
                        cmdVerbale.Parameters.AddWithValue("@DataViolazione", verbaleModel.DataViolazione);
                        cmdVerbale.Parameters.AddWithValue("@IndirizzoViolazione", verbaleModel.IndirizzoViolazione);
                        cmdVerbale.Parameters.AddWithValue("@Nominativo_Agente", verbaleModel.Nominativo_Agente);
                        cmdVerbale.Parameters.AddWithValue("@DataTrascrizioneVerbale", verbaleModel.DataTrascrizioneVerbale);
                        cmdVerbale.Parameters.AddWithValue("@Importo", verbaleModel.Importo);
                        cmdVerbale.Parameters.AddWithValue("@DecurtamentoPunti", verbaleModel.DecurtamentoPunti);
                        cmdVerbale.Parameters.AddWithValue("@IdAnagrafica", idAnagrafica); // Utilizza l'ID anagrafica appena recuperato
                        cmdVerbale.Parameters.AddWithValue("@IdViolazione", idViolazione); // Utilizza l'ID violazione appena recuperato

                        cmdVerbale.ExecuteNonQuery();
                    }
                }
            }

            return RedirectToAction("InserisciDati");
        }


    //QUERY 1
    [HttpGet]
    public IActionResult TotaleVerbaliTrascritti()
    {
        using (var conn = new SqlConnection(connString))
        {
            conn.Open();

            var query = @"
                SELECT a.IdAnagrafica, a.Cognome, a.Nome, COUNT(v.IdVerbale) AS TotaleVerbali
                FROM ANAGRAFICA a
                LEFT JOIN VERBALE v ON a.IdAnagrafica = v.IdAnagrafica
                GROUP BY a.IdAnagrafica, a.Cognome, a.Nome";
        
            var model = new List<TotaleVerbaliModel>();

            using (var cmd = new SqlCommand(query, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var totaleVerbali = new TotaleVerbaliModel
                    {
                        IdAnagrafica = Convert.ToInt32(reader["IdAnagrafica"]),
                        Cognome = reader["Cognome"].ToString(),
                        Nome = reader["Nome"].ToString(),
                        TotaleVerbali = Convert.ToInt32(reader["TotaleVerbali"])
                    };

                    model.Add(totaleVerbali);
                }
            }

            return View(model);
        }

    }
        //QUERY 2
        [HttpGet]
        public IActionResult TotalePuntiDecurtati()
        {
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                var query = @"
            SELECT a.IdAnagrafica, a.Cognome, a.Nome, SUM(v.DecurtamentoPunti) AS TotalePuntiDecurtati
            FROM ANAGRAFICA a
            LEFT JOIN VERBALE v ON a.IdAnagrafica = v.IdAnagrafica
            GROUP BY a.IdAnagrafica, a.Cognome, a.Nome";

                var model = new List<TotalePuntiModel>();

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var totalePunti = new TotalePuntiModel
                        {
                            IdAnagrafica = Convert.ToInt32(reader["IdAnagrafica"]),
                            Cognome = reader["Cognome"].ToString(),
                            Nome = reader["Nome"].ToString(),
                            TotalePuntiDecurtati = Convert.ToInt32(reader["TotalePuntiDecurtati"])
                        };

                        model.Add(totalePunti);
                    }
                }

                return View(model);
            }
        }

        //QUERY 3
        [HttpGet]
        public IActionResult ViolazioniSopra10Punti()
        {
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                var query = @"
            SELECT v.Importo, a.Cognome, a.Nome, v.DataViolazione, v.DecurtamentoPunti
            FROM VERBALE v
            JOIN ANAGRAFICA a ON v.IdAnagrafica = a.IdAnagrafica
            WHERE v.DecurtamentoPunti > 10";

                var model = new List<ViolazioniSopra10PuntiModel>();

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var violazioneSopra10Punti = new ViolazioniSopra10PuntiModel
                        {
                            Importo = Convert.ToDecimal(reader["Importo"]),
                            Cognome = reader["Cognome"].ToString(),
                            Nome = reader["Nome"].ToString(),
                            DataViolazione = Convert.ToDateTime(reader["DataViolazione"]),
                            DecurtamentoPunti = Convert.ToInt32(reader["DecurtamentoPunti"])
                        };

                        model.Add(violazioneSopra10Punti);
                    }
                }

                return View(model);
            }
        }

        //QUERY 4
        [HttpGet]
        public IActionResult ViolazioniImportoMaggiore400()
        {
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                var query = @"
            SELECT v.Importo, a.Cognome, a.Nome, v.DataViolazione, v.DecurtamentoPunti
            FROM VERBALE v
            JOIN ANAGRAFICA a ON v.IdAnagrafica = a.IdAnagrafica
            WHERE v.Importo > 400";

                var model = new List<ViolazioniImportoMaggiore400Model>();

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var violazioneImportoMaggiore400 = new ViolazioniImportoMaggiore400Model
                        {
                            Importo = Convert.ToDecimal(reader["Importo"]),
                            Cognome = reader["Cognome"].ToString(),
                            Nome = reader["Nome"].ToString(),
                            DataViolazione = Convert.ToDateTime(reader["DataViolazione"]),
                            DecurtamentoPunti = Convert.ToInt32(reader["DecurtamentoPunti"])
                        };

                        model.Add(violazioneImportoMaggiore400);
                    }
                }

                return View(model);
            }
        }



    }


}
