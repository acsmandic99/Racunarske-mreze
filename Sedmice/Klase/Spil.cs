using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Klase
{

    [Serializable]
    public class Spil
    {
        private List<Karta> spil = new List<Karta>(32);
        public Spil()
        {
            foreach (Znak znak in Enum.GetValues(typeof(Znak)))
            {
                foreach (VrednostKarte vrednost in Enum.GetValues(typeof(VrednostKarte)))
                {
                    spil.Add(new Karta(znak, vrednost));
                }
            }
        }

        public void Ispisi()
        {
            foreach (Karta karta in spil)
            {
                Console.WriteLine(karta.ToString());
            }
        }
        public void Promesaj()
        {
            Random rnd = new Random();
            for (int i = spil.Count - 1; i > 0; i--)
            {
                int idx = rnd.Next(i + 1);
                Karta temp = spil[idx];
                spil[idx] = spil[i];
                spil[i] = temp;
            }
        }

        public int Count
        {
            get { return spil.Count; }
        }

        public Karta IzvuciKartu()
        {
            if (spil.Count - 1 >= 0)
            {
                Karta izvucena = spil[spil.Count - 1];
                spil.RemoveAt(spil.Count - 1);
                return izvucena;
            }
            else
                return null;
        }

    }
}
