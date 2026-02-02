using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klase
{
    [Serializable]
    public class Igrac
    {
        private string ime = "Anonimac";
        private string prezime = "Anonimni";
        private int brojPoena = 0;
        private int tim;
        private List<Karta> karteURuci = new List<Karta>();
        private List<Karta> osvojeneKarte = new List<Karta>();

        public Igrac(int tim, string prezime, string ime)
        {
            Tim = tim;
            Prezime = prezime;
            Ime = ime;
        }

        public Karta OdigrajKartu(int index)
        {
            Karta karta = karteURuci[index];
            karteURuci.RemoveAt(index);
            return karta;
        }
        public void IspisiKarte()
        {
            ConsoleColor orgBoja = Console.ForegroundColor;
            int i = 1;
            foreach (Karta karta in karteURuci)
            {
                Console.Write(i + " - " + "( ");
                if (karta.Znak == Znak.Herc || karta.Znak == Znak.Karo)
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.ForegroundColor = orgBoja;
                Console.Write(karta.ToString() + "),  ");
                i++;
            }
            Console.WriteLine();
        }
        public void IzvuciKartu(Karta novaKarta)
        {
            if (novaKarta != null)
                karteURuci.Add(novaKarta);
        }
        public Karta KartaNaIndexu(int index)
        {
            return karteURuci[index];
        }
        public void NosiKarte(Karta osvojenaKarta)
        {
            osvojeneKarte.Add(osvojenaKarta);
        }
        public int IzbrojOsvojenePoene()
        {
            int poeni = 0;
            foreach (Karta karta in osvojeneKarte)
            {
                if (karta.Broj == VrednostKarte.Deset || karta.Broj == VrednostKarte.A)
                    poeni++;
            }
            return poeni;
        }

        public int KarteURuciCount
        {
            get { return karteURuci.Count; }
        }
        public int Tim
        {
            get { return tim; }
            set { tim = value; }
        }


        public int BrojPoena
        {
            get { return brojPoena; }
            set { brojPoena = value; }
        }


        public string Prezime
        {
            get { return prezime; }
            set { prezime = value; }
        }


        public string Ime
        {
            get { return ime; }
            set { ime = value; }
        }

    }
}