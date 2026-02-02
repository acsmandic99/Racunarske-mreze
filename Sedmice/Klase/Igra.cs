using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;


namespace Klase
{
    [Serializable]
    public enum StanjeIgre { INICIJALIZACIJA, REDOVNA_IGRA, ODGOVOR_PRVOG_IGRACA, ZAVRSETAK_KRUGA, ZAVRSETAK_IGRE, UPIT_ZA_NOVU_IGRU };
    public class Igra
    {
        Igrac[] igraci;
        Socket[] igraciSocket;
        Spil spil = new Spil();
        List<Karta> sto = new List<Karta>();
        static BinaryFormatter bf = new BinaryFormatter();
        static byte[] buffer = new byte[8192];
        StanjeIgre STANJE = StanjeIgre.INICIJALIZACIJA;
        int igracNaPotezu = 0;
        bool udaren = false;
        int prviIgrac = 0;
        int IgracKojiJePoslednjiTukao = 0;
        int brojPokusaja = 0;
        public Igra(Igrac[] igraci, Socket[] igraciSocket)
        {
            this.igraci = igraci;
            this.igraciSocket = igraciSocket;
        }

        public void Igraj()
        {
            while (true)
            {
                if (STANJE == StanjeIgre.INICIJALIZACIJA)
                {
                    spil = new Spil();
                    spil.Promesaj();
                    DodeliKarteIgracima();
                    Paket paket = new Paket();
                    paket.message = "Igrac na potezu je: " + igraci[igracNaPotezu].Ime;
                    STANJE = StanjeIgre.REDOVNA_IGRA;
                    PosaljiIgracimaStanje(paket);
                    udaren = false;
                }
                else if (STANJE == StanjeIgre.REDOVNA_IGRA)
                {
                    for (int i = 0; i < igraciSocket.Length; i++)
                    {
                        if (igraciSocket[i].Poll(1000 * 200, SelectMode.SelectRead))
                        {
                            if (i == igracNaPotezu)
                            {
                                igraciSocket[i].Receive(buffer);
                                Paket paket = OtpakujPaket();
                                sto.Add(igraci[i].OdigrajKartu(paket.kartaZaIgranje));
                                if (sto.Count > 1)
                                    if (sto[sto.Count - 1].Broj == sto[0].Broj || sto[sto.Count - 1].Broj == VrednostKarte.Sedam)
                                    {
                                        udaren = true;
                                        IgracKojiJePoslednjiTukao = i;
                                        Console.WriteLine("Igrac " + igraci[i].Ime + " je udario " + sto[0].ToString() + " sa " + sto[sto.Count - 1].ToString());
                                    }
                                igracNaPotezu++;
                                if (igracNaPotezu == igraci.Length)
                                    igracNaPotezu = 0;
                                paket = new Paket();
                                paket.message = "Igrac " + igraci[i].Ime + " je odigrao " + sto[sto.Count - 1].ToString();
                                Console.WriteLine("Igrac " + igraci[i].Ime + " je odigrao " + sto[sto.Count - 1].ToString());
                                paket.message += "\nIgrac na potezu je: " + igraci[igracNaPotezu].Ime;



                                if (udaren == true && igracNaPotezu == prviIgrac)
                                {
                                    STANJE = StanjeIgre.ODGOVOR_PRVOG_IGRACA;
                                    paket.specijalnaPoruka = "5 - Ne igrajte ni jednu kartu - zavrsite krug.";
                                    paket.igra = true;
                                    paket.NaPotezu = igraci[igracNaPotezu].Ime;
                                }
                                else if (udaren == false && igracNaPotezu == prviIgrac)
                                { STANJE = StanjeIgre.ZAVRSETAK_KRUGA; break; }

                                PosaljiIgracimaStanje(paket);
                            }
                        }
                    }
                }
                else if (STANJE == StanjeIgre.ODGOVOR_PRVOG_IGRACA)
                {
                    if (igraciSocket[igracNaPotezu].Poll(1000 * 100, SelectMode.SelectRead))
                    {
                        igraciSocket[igracNaPotezu].Receive(buffer);
                        Paket paket = OtpakujPaket();
                        if (paket.kartaZaIgranje != -1)
                        {
                            if (igraci[igracNaPotezu].KartaNaIndexu(paket.kartaZaIgranje).Broj == sto[0].Broj || igraci[igracNaPotezu].KartaNaIndexu(paket.kartaZaIgranje).Broj == VrednostKarte.Sedam)
                            {
                                udaren = false;
                                IgracKojiJePoslednjiTukao = igracNaPotezu;
                                sto.Add(igraci[igracNaPotezu].OdigrajKartu(paket.kartaZaIgranje));

                                paket.message = "Igrac " + igraci[igracNaPotezu].Ime + " je odigrao " + sto[sto.Count - 1].ToString();
                                Console.WriteLine(paket.message);
                                igracNaPotezu++;
                                if (igracNaPotezu == igraci.Length)
                                    igracNaPotezu = 0;

                                paket.message += "\nIgrac na potezu je: " + igraci[igracNaPotezu].Ime;
                                STANJE = StanjeIgre.REDOVNA_IGRA;
                                PosaljiIgracimaStanje(paket);

                            }
                            else
                            {
                                paket = new Paket();
                                paket.specijalnaPoruka = "Molim Vas odigrajte kartu sa korektnim brojem! Ako nemate taj znak odigrajte opciju 5 - zavrsi krug!";
                                paket.NaPotezu = igraci[igracNaPotezu].Ime;
                                PosaljiPaketPosebnomIgracu(paket, igracNaPotezu);
                            }
                        }
                        else
                        {
                            udaren = false;
                            STANJE = StanjeIgre.ZAVRSETAK_KRUGA;
                        }
                    }
                }
                else if (STANJE == StanjeIgre.ZAVRSETAK_KRUGA)
                {
                    if (spil.Count == 0 && igraci[0].KarteURuciCount == 0)
                        STANJE = StanjeIgre.ZAVRSETAK_IGRE;
                    else
                    {
                        STANJE = StanjeIgre.REDOVNA_IGRA;
                        ZapocniNoviKrug();
                    }
                }
                else if (STANJE == StanjeIgre.ZAVRSETAK_IGRE)
                {
                    Paket paket = new Paket();
                    foreach (Karta karta in sto)
                    {
                        igraci[IgracKojiJePoslednjiTukao].NosiKarte(karta);
                    }
                    // Bodovi po timovima
                    Dictionary<int, int> poeniTimova = new Dictionary<int, int>();

                    foreach (Igrac igrac in igraci)
                    {
                        int poeni = igrac.IzbrojOsvojenePoene();
                        if (poeniTimova.ContainsKey(igrac.Tim))
                            poeniTimova[igrac.Tim] += poeni;
                        else
                            poeniTimova[igrac.Tim] = poeni;
                    }

                    // Prikaz poena svih igraca
                    paket.message = "";
                    foreach (Igrac igrac in igraci)
                    {
                        paket.message += $"Igrac {igrac.Ime} {igrac.Prezime} (Tim {igrac.Tim}) je skupio {igrac.IzbrojOsvojenePoene()} poena.\n";
                    }

                    // Sumarni poeni po timovima
                    paket.message += "\n";
                    foreach (var kvp in poeniTimova)
                    {
                        paket.message += $"Tim {kvp.Key} ukupno: {kvp.Value} poena.\n";
                    }

                    // Odredjivanje pobednika
                    int pobednickiTim = -1;
                    if (poeniTimova.Count == 2)
                    {
                        var enumerator = poeniTimova.GetEnumerator();
                        enumerator.MoveNext();
                        var prvi = enumerator.Current;
                        enumerator.MoveNext();
                        var drugi = enumerator.Current;

                        if (prvi.Value > drugi.Value)
                            pobednickiTim = prvi.Key;
                        else if (drugi.Value > prvi.Value)
                            pobednickiTim = drugi.Key;
                    }

                    if (pobednickiTim != -1)
                    {
                        paket.message += $"\nPobednik je Tim {pobednickiTim}!";
                    }
                    else
                    {
                        paket.message += "\nNereseno! Igraci koji su poslednji tukli nose pobedu za svoj tim!";
                        pobednickiTim = igraci[IgracKojiJePoslednjiTukao].Tim;
                        paket.message += $"\nPobednik je Tim {pobednickiTim} (jer je poslednji tukao).";
                    }

                    paket.message += "\nDa li zelite da odigrate novu partiju? Y/N";
                    PosaljiIgracimaStanje(paket);
                    int spremnih_igraca = 0;
                    while (STANJE == StanjeIgre.ZAVRSETAK_IGRE)
                    {
                        for (int i = 0; i < igraci.Length; i++)
                        {
                            if (igraciSocket[i].Poll(1000 * 300, SelectMode.SelectRead))
                            {
                                igraciSocket[i].Receive(buffer);
                                Paket primljenPaket = OtpakujPaket();
                                if (primljenPaket.novaIgra.ToString().ToUpper().Equals("Y"))
                                {
                                    spremnih_igraca++;
                                    paket.message = "Igrac " + igraci[i].Ime + " je prihvatio zahtev za novu partiju!";
                                    PosaljiIgracimaStanje(paket);
                                }
                                else
                                {
                                    paket.message = "Igraci su odbili da se nastavi igra...Server se gasi!";
                                    PosaljiIgracimaStanje(paket);
                                    return;
                                }
                                if (spremnih_igraca == igraci.Length)
                                {
                                    STANJE = StanjeIgre.INICIJALIZACIJA;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void PosaljiPaketPosebnomIgracu(Paket paket, int indexIgraca)
        {
            byte[] db;
            paket.stanjeIgre = STANJE;
            paket.igrac = igraci[indexIgraca];
            paket.stanjeStola = sto;

            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, paket);
                db = ms.ToArray();
            }
            igraciSocket[indexIgraca].Send(db);
        }
        public Paket OtpakujPaket()
        {
            Paket otpakovanPaket;
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                otpakovanPaket = (Paket)bf.Deserialize(ms);
            }
            return otpakovanPaket;
        }
        public void ZapocniNoviKrug()
        {
            foreach (Karta karta in sto)
            {
                igraci[IgracKojiJePoslednjiTukao].NosiKarte(karta);
            }
            sto.Clear();
            udaren = false;
            igracNaPotezu = IgracKojiJePoslednjiTukao;
            prviIgrac = igracNaPotezu;
            Paket paketNoviKrug = new Paket();
            paketNoviKrug.message = "Igrac " + igraci[IgracKojiJePoslednjiTukao].Ime + " je osvojio sto.\nNa potezu je igrac: " + igraci[igracNaPotezu].Ime;
            Console.WriteLine(paketNoviKrug.message);
            DodeliKarteIgracima();
            PosaljiIgracimaStanje(paketNoviKrug);
        }
        public void DodeliKarteIgracima()
        {
            Console.WriteLine("============DODELA KARATA===========");
            int i = igracNaPotezu;
            while (igraci[i].KarteURuciCount < 4 && spil.Count > 0)
            {
                Karta karta = spil.IzvuciKartu();
                Console.WriteLine("Igrac  " + igraci[i].Ime + " izvukao " + karta);
                igraci[i].IzvuciKartu(karta);
                i++;
                if (i == igraci.Length)
                    i = 0;
                Console.WriteLine("Ostalo u spilu " + spil.Count);
            }
            Console.WriteLine("============KRAJ DODELA KARATA===========");


        }
        public void PosaljiIgracimaStanje(Paket paket)
        {
            paket.stanjeIgre = STANJE;

            for (int i = 0; i < igraci.Length; i++)
            {
                byte[] dataBuffer;
                paket.igra = true;

                paket.stanjeStola = sto;
                paket.igrac = igraci[i];
                paket.NaPotezu = igraci[igracNaPotezu].Ime;
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, paket);
                    dataBuffer = ms.ToArray();
                }
                igraciSocket[i].Send(dataBuffer);
            }
        }

        public string ispisiSto()
        {
            string retVal = "Sto:\n";
            for (int i = 0; i < sto.Count; i++)
            {
                retVal += sto[i].ToString() + "   ";
            }

            return retVal;
        }
    }
}
