using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klase
{
    [Serializable]
    public class Paket
    {
        public bool succsess;
        public string message;
        public int port;
        public bool syn;
        public bool igra;
        public Igrac igrac;
        public List<Karta> stanjeStola;
        public string NaPotezu;
        public int kartaZaIgranje;
        public StanjeIgre stanjeIgre;
        public char novaIgra;
        public string specijalnaPoruka;
    }
}