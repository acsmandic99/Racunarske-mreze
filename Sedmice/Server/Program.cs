using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Klase;


namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Socket udpSocket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram, ProtocolType.Udp);
            string localIP = GetLocalIPAddress();
            EndPoint udpEP = new IPEndPoint(IPAddress.Parse(localIP), 0);
            udpSocket.Bind(udpEP);
            udpSocket.Blocking = false;
            int brIgraca;
            Console.WriteLine("Unesite koliko ce igraca da bude u partiji");
            do
            {
                Console.WriteLine("Broj igraca mora biti 2 ili 4");
                brIgraca = Int32.Parse(Console.ReadLine());
                PokreniKlijente(brIgraca);
            } while (brIgraca != 2 && brIgraca != 4);
            

            Socket[] igraciSocket = new Socket[brIgraca];

            Socket tcpSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            EndPoint tcpEP = new IPEndPoint(IPAddress.Parse(localIP), 0);
            tcpSocket.Bind(tcpEP);
            tcpSocket.Listen(brIgraca);
            EndPoint tcpLocalEP = tcpSocket.LocalEndPoint as IPEndPoint;
            string[] split = tcpLocalEP.ToString().Split(':');
            int tcpPort = Int32.Parse(split[1]);


            EndPoint localEP = udpSocket.LocalEndPoint as IPEndPoint;
            Console.WriteLine("Server je pokrenut!");
            Console.WriteLine("IP adresa za konekciju: " + localEP.ToString());
            Console.WriteLine("Cekam konekcije...");
            int brPokusaja = 0;
            int brPrijavljenihIgraca = 0;

            byte[] buffer = new byte[8192];
            byte[] dataBuffer;

            Paket paket = new Paket();
            Igrac[] igraci = new Igrac[brIgraca];
            Igrac temp;
            BinaryFormatter bf = new BinaryFormatter();
            Dictionary<string, int> imeMaprianoNaIndex = new Dictionary<string, int>();
            int acceptedSocket = 0;
            List<EndPoint> clientEPList = new List<EndPoint>();
            //primanje igraca 
            while (brPrijavljenihIgraca < brIgraca)
            {
                try
                {
                    if(udpSocket.Poll(1000*500,SelectMode.SelectRead))
                    {
                        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        int bytesRecived = udpSocket.ReceiveFrom(buffer,SocketFlags.None,ref remoteEP);
                        using (MemoryStream ms = new MemoryStream(buffer))
                        {
                            //primamo ceo objekat igraca sa klijentovog dela pa zato kastujemo
                            temp = (Igrac)bf.Deserialize(ms);
                        }
                        if (imeMaprianoNaIndex.ContainsKey(temp.Ime))
                        {
                                
                            paket.succsess = false;
                            paket.message = "Ime je vec zauzeto,molim Vas izaberite jedinstveno ime.\nZauzeta imena: ";
                            foreach(var ime in imeMaprianoNaIndex)
                            {
                                paket.message += ime.Key + "\n";
                            }
                            using (MemoryStream ms = new MemoryStream())
                            {   
                                bf.Serialize(ms, paket);
                                dataBuffer = ms.ToArray();
                                udpSocket.SendTo(dataBuffer, remoteEP);
                            }
                        }
                        else
                        {
                            // ================= VALIDACIJA TIMOVA =======================
                            int countTim1 = 0;
                            int countTim2 = 0;
                            for (int k = 0; k < brPrijavljenihIgraca; k++)
                            {
                                if (igraci[k].Tim == 1) countTim1++;
                                else if (igraci[k].Tim == 2) countTim2++;
                            }

                            if (brIgraca == 2)
                            {
                                
                                // za 2 igraca – oba moraju biti razlicitih timova
                                if (countTim1 == 1 && temp.Tim == 1)
                                {
                                    paket.succsess = false;
                                    paket.message = "Tim 1 je vec zauzet! Molim odaberite Tim 2.";
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        bf.Serialize(ms, paket);
                                        dataBuffer = ms.ToArray();
                                    } 
                                    udpSocket.SendTo(dataBuffer, remoteEP);
                                    continue;
                                }
                                else if (countTim2 == 1 && temp.Tim == 2)
                                {
                                    paket.succsess = false;
                                    paket.message = "Tim 2 je vec zauzet! Molim odaberite Tim 1.";
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        bf.Serialize(ms, paket);
                                        dataBuffer = ms.ToArray();
                                    }
                                    udpSocket.SendTo(dataBuffer, remoteEP);
                                    continue;
                                }
                            }
                            else if (brIgraca == 4)
                            {
                                
                                // za 4 igraca – max 2 po timu
                                if (temp.Tim == 1 && countTim1 >= 2)
                                {
                                    paket.succsess = false;
                                    paket.message = "Tim 1 je vec popunjen (2 igraca)! Molim odaberite Tim 2.";
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        bf.Serialize(ms, paket);
                                        dataBuffer = ms.ToArray();
                                    }
                                    udpSocket.SendTo(dataBuffer, remoteEP);
                                    continue;
                                }
                                else if (temp.Tim == 2 && countTim2 >= 2)
                                {
                                    paket.succsess = false;
                                    paket.message = "Tim 2 je vec popunjen (2 igraca)! Molim odaberite Tim 1.";
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        bf.Serialize(ms, paket);
                                        dataBuffer = ms.ToArray();
                                    }
                                    udpSocket.SendTo(dataBuffer, remoteEP);
                                    continue;
                                }
                            }
                            // ============================================================
                            imeMaprianoNaIndex[temp.Ime] = brPrijavljenihIgraca;
                            igraci[brPrijavljenihIgraca] = temp;
                            Console.WriteLine("Igrac " + igraci[brPrijavljenihIgraca].Ime + " se uspesno prijavio.");
                            brPrijavljenihIgraca++;
                            paket.succsess = true;
                            paket.port = 0;
                            paket.message = "Uspesno ste se prijavili za igru,cekamo ostale igrace da nam se pridruze!";
                            using (MemoryStream ms = new MemoryStream())
                            {    
                                bf.Serialize(ms, paket);
                                dataBuffer = ms.ToArray();
                            }
                            udpSocket.SendTo(dataBuffer, remoteEP);
                            clientEPList.Add(remoteEP);
                        } 
                    }
                    else
                    {
                        //posle nekog vremena se gasi 
                        brPokusaja++;
                        //%100
                        if(brPokusaja%100==0)
                        {
                            Console.WriteLine("Cekam konekcije...");
                        }
                        else if (brPokusaja > 5000 && brPrijavljenihIgraca < brIgraca )
                        {
                            Console.WriteLine("Nedovoljno igraca se konektovao,zatvaram program...");
                            udpSocket.Close();
                            return;
                        }
                    }
                }
                catch(SocketException ex) { Console.WriteLine(ex.ToString()); }
            }
            //saljemo svima tcp port preko kog ce igra da se odvija
            PosaljiSvimaTCPPort(clientEPList, tcpPort, udpSocket);

            while (true)
            {
                if(tcpSocket.Poll(1000* 500,SelectMode.SelectRead))
                {
                    igraciSocket[acceptedSocket] = tcpSocket.Accept();
                    acceptedSocket++;
                }
                //tu se moze zaglaviti u beskonacnom loopu
                if (acceptedSocket < brIgraca)
                    continue;
                else
                    break;
            }
            //svi su se konektovali sada zelim da namestim da mi svaki igrac se nalazi na istom indexu u igraci i svaki
            //socket za tog igraca da se nalazi na istom tom indexu u igraciSocket
            Console.WriteLine("Svi su se konektovali");
            //namestanje indexa u nizovima
            foreach(Socket soket in igraciSocket)
            {
                using(MemoryStream ms = new MemoryStream())
                {
                    Paket pkt = new Paket();
                    pkt.syn = true;
                    bf.Serialize(ms,pkt);
                    dataBuffer = ms.ToArray();
                    soket.Send(dataBuffer);
                }
            }
            int index_namesteno = 0;
            while(index_namesteno < brIgraca)
            {
                for(int i = 0;i<brIgraca;i++)
                {
                    if (igraciSocket[i].Poll(1000*500,SelectMode.SelectRead))
                    {
                        Paket pkt;
                        int bytesRecived = igraciSocket[i].Receive(buffer);
                        using(MemoryStream ms = new MemoryStream(buffer))
                        {
                            pkt = (Paket)bf.Deserialize(ms);
                        }
                        //ovde cemo da vrsimo sinhronizaciju samo jednom na pocetku
                        if(pkt.syn == true)
                        {
                            int index = imeMaprianoNaIndex[pkt.message];
                            if(i != index)
                            {
                                Socket tempSok = igraciSocket[index];
                                igraciSocket[index] = igraciSocket[i];
                                igraciSocket[i] = tempSok;
                            }
                            index_namesteno++;
                        }
                    }
                }
            }
            //sad kad je sve ovo namesteno moze igra da pocne
            Console.WriteLine("Namestili smo indexe na nizovima");
            //provera da li je dobro sve 
            for(int i = 0;i<brIgraca;i++)
            {
                Console.WriteLine("index " + i + " " + igraci[i].Ime + " " + igraciSocket[i].RemoteEndPoint.ToString());
            }

            Igra igra = new Igra(igraci, igraciSocket);
            igra.Igraj();

            foreach(Socket soket in igraciSocket)
            {
                soket.Close();
            }
            udpSocket.Close();
            tcpSocket.Close();

            Console.ReadLine();
        }
        static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Nije pronadjena IPv4 adresa za ovaj host!");
        }

        public static void PosaljiSvimaTCPPort(List<EndPoint> endPoints,int tcpPort, Socket udpSocket)
        {
            Paket paket = new Paket();
            BinaryFormatter bf = new BinaryFormatter();
            byte[] dataBuffer;
            foreach (EndPoint ep in endPoints)
            {
                paket.succsess = true;
                paket.port = tcpPort;
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, paket);
                    dataBuffer = ms.ToArray();
                }
                udpSocket.SendTo(dataBuffer, ep);
            }
        }

        static void PokreniKlijente(int brojKlijenata)
        {
            for (int i = 0; i < brojKlijenata; i++)
            {
                // Putanja do izvršnog fajla klijenta (potrebno je kompajlirati ga)  
                string clientPath = @"..\..\..\Client\bin\Debug\Client.exe";
                Process klijentProces = new Process(); // Stvaranje novog procesa 
                klijentProces.StartInfo.FileName = clientPath; //Zadavanje putanje za pokretanje 
                klijentProces.StartInfo.Arguments = $"{i + 1}"; // Argument - broj klijenta  
                klijentProces.Start(); // Pokretanje klijenta  
                Console.WriteLine($"Pokrenut klijent #{i + 1}");
            }
        }


    }
}
