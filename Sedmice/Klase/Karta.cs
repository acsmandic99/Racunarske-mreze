using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klase
{
    [Serializable]

    public enum Znak { Tref, Pik, Karo, Herc }
    [Serializable]
    public enum VrednostKarte { Sedam, Osam, Devet, Deset, J, Q, K, A }
    [Serializable]
    public class Karta
    {
        private VrednostKarte broj;
        private Znak znak;

        public override string ToString()
        {
            string retVal = "";
            switch (broj)
            {
                case VrednostKarte.Sedam:
                    retVal += "7";
                    break;
                case VrednostKarte.Osam:
                    retVal += "8";
                    break;
                case VrednostKarte.Devet:
                    retVal += "9";
                    break;
                case VrednostKarte.Deset:
                    retVal += "10";
                    break;
                case VrednostKarte.J:
                    retVal += "J";
                    break;
                case VrednostKarte.Q:
                    retVal += "Q";
                    break;
                case VrednostKarte.K:
                    retVal += "K";
                    break;
                case VrednostKarte.A:
                    retVal += "A";
                    break;
            }
            switch (znak)
            {
                case Znak.Tref:
                    retVal += " Tref";
                    break;
                case Znak.Pik:
                    retVal += " Pik";
                    break;
                case Znak.Herc:
                    retVal += " Herc";
                    break;
                case Znak.Karo:
                    retVal += " Karo";
                    break;
            }
            return retVal;
        }

        public Karta(Znak znak, VrednostKarte broj)
        {
            Znak = znak;
            Broj = broj;
        }

        public Znak Znak
        {
            get { return znak; }
            set { znak = value; }
        }

        public VrednostKarte Broj
        {
            get { return broj; }
            set { broj = value; }
        }

    }

}
