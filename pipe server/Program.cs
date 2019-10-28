﻿using System;
using System.Collections;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

//mesaji bu program aliyor

namespace pipe_server
{
    class Program
    {
        ArrayList eksenListe = new ArrayList();
        ArrayList mesafeListe = new ArrayList();
        static void Main(string[] args)
        {
           
            /*using (NamedPipeServerStream pipeStream = new NamedPipeServerStream("mypipe"))
            {
                Console.WriteLine("Server calisiyor. Client'i aciniz.");
                pipeStream.WaitForConnection();
                StreamReader sr = new StreamReader(pipeStream);

                string tmp;
                while ((tmp = sr.ReadLine()) != null)
                {
                    Console.WriteLine("(Mesaj Geldi) "+ tmp);
                }

            }
            using (NamedPipeServerStream pipeStream = new NamedPipeServerStream("mypipe"))
            {
                Console.WriteLine("Server calisiyor. Client'i aciniz.");
                pipeStream.WaitForConnection();
                StreamReader sr = new StreamReader(pipeStream);

                string tmp;
                while ((tmp = sr.ReadLine()) != null)
                {
                    Console.WriteLine("(Mesaj Geldi) " + tmp);
                }

            }*/
            Program Server = new Program();

            Thread ServerThread = new Thread(Server.ThreadStartServer);

            ServerThread.Start();


        }

        public void ThreadStartServer()
        {
            // Create a name pipe
            while (true)
            {
                using (NamedPipeServerStream pipeStream = new NamedPipeServerStream("samplePipe"))
                {
                    Console.WriteLine("[Server] Pipe created {0}", pipeStream.GetHashCode());

                    // Wait for a connection
                    pipeStream.WaitForConnection();
                    Console.WriteLine("[Server] Pipe connection established");

                    using (StreamReader sr = new StreamReader(pipeStream))
                    {
                        string temp;
                        if (sr.ReadLine() != "#RPLIDAR SCAN DATA")
                        {
                            Console.WriteLine("Yanlış Bilgi");
                            continue;
                        }
                        else
                        {
                            eksenListe.Clear();
                            mesafeListe.Clear();
                        }
                        // We read a line from the pipe and print it together with the current time
                        while ((temp = sr.ReadLine()) != null)
                        {
                           // Console.WriteLine("{0}: {1}", DateTime.Now, temp);
                           
                            //if (temp == null) break;
                            string[] x = temp.Split(' ');
                            double eksen = Convert.ToDouble(x[0].Split('.')[0]) + ((1 / Math.Pow(10, x[0].Split('.')[1].Length)) * Convert.ToDouble(x[0].Split('.')[1]));
                            double mesafe = Convert.ToDouble(x[1].Split('.')[0]) + ((1 / Math.Pow(10, x[1].Split('.')[1].Length)) * Convert.ToDouble(x[1].Split('.')[1]));
                            if (mesafe != 0)
                            {
                                eksenListe.Add(eksen);
                                mesafeListe.Add(mesafe);
                                Console.WriteLine(temp);
                            }
                            //Console.WriteLine("***"+eksen+"---"+mesafe);
                            
                        }
                        daireKotnrol2(eksenListe, mesafeListe);
                    }
                }
            }
           

            Console.WriteLine("Connection lost");
                Console.ReadKey();
            }

        private bool daireKotnrol2(ArrayList eksen, ArrayList mesafe)
        {
            Boolean artis;
            bool kontrol = false;
            int baslangicEkseni = -1;
            int bitisEkseni = -1;
            int hata;


            for (int i = 1; i < eksen.Count - 1; i++)
            {
                bool dongu = true;
                hata = 0;
                int sayac = 0;
                double kayma = 0.1;
                double daireFark = -1;
                double daireFark2 = 0;
                #region merkezden uzaklaşan mı yoksa merkeze yaklaşanmı bir üçgen olduğunu buluyorz. Bir kenar uzaklaşığ bir köşe yaklaşıyorsa bir sonraki noktaya atlıyor.
                if (Convert.ToDouble(mesafe[i - 1]) >= Convert.ToDouble(mesafe[i]))
                {
                    if (Convert.ToDouble(mesafe[i + 1]) >= Convert.ToDouble(mesafe[i]))
                    {
                        artis = true;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (Convert.ToDouble(mesafe[i - 1]) < Convert.ToDouble(mesafe[i]))
                {
                    if (Convert.ToDouble(mesafe[i + 1]) < Convert.ToDouble(mesafe[i]))
                    {
                        artis = false;
                    }
                    else
                    {
                        continue;
                    }
                }
                else { continue; }
                #endregion
                int k = i, f = i, t = i, p = i;

                for (int j = 1; ((i + j + 1) < eksen.Count || (i - j - 1) > 0); j++)
                {

                    if (artis)
                    {
                        #region Liste döngüsü sağlamak için yazdım
                        if ((i + j + 1) < eksen.Count) { k = (i + j + 1); t = k; }
                        else if ((i + j + 1) == eksen.Count)
                        {
                            double fark11 = Math.Abs(Convert.ToDouble(eksen[f + 1]) - Convert.ToDouble(eksen[f]));
                            double fark21 = Math.Abs(Convert.ToDouble(eksen[eksen.Count - 1]) - Convert.ToDouble(eksen[0]));
                            double fark1Merkez1 = Math.Abs(Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[p]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[p]);
                            double fark2Merkez1 = Math.Abs(Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[eksen.Count - 1]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[eksen.Count - 1]);

                            if ((Convert.ToDouble(mesafe[f + 1]) < Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[f + 1]) * kayma) > Convert.ToDouble(mesafe[f])) &&
                            (Convert.ToDouble(mesafe[k - 1]) < Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[k - 1]) * kayma) > Convert.ToDouble(mesafe[k]))
                           && (fark11 < 2.3 || (fark11 > 355 && fark11 < 360)) && (fark21 < 2.3 || (fark21 > 355 && fark21 < 360)) &&
                           (fark1Merkez1 > fark2Merkez1 - 20 && fark1Merkez1 < fark2Merkez1 + 20 && fark2Merkez1 > fark1Merkez1 - 20 && fark2Merkez1 < fark1Merkez1 + 20)
                            && daireFark <= (Math.Abs(fark1Merkez1 - fark2Merkez1)) && ((daireFark2 <= 0 && (fark1Merkez1 - fark2Merkez1) <= 0) || (daireFark2 >= 0 && (fark1Merkez1 - fark2Merkez1) >= 0)))
                            {
                                daireFark = Math.Abs(fark1Merkez1 - fark2Merkez1);
                                daireFark2 = fark1Merkez1 - fark2Merkez1;
                               
                                baslangicEkseni = f;
                                bitisEkseni = k;
                                sayac++;
                                
                                if (sayac > 10) { kayma = 0.07; }
                                else if (sayac > 15) { kayma = 0.085; }
                                else if (sayac > 20) { kayma = 0.1; }
                                else if (sayac > 30) { kayma = 0.15; }
                                k = 1;
                                t = 0;
                            }
                            else
                            {
                                break;
                            }

                        }
                        else { k++; t++; }
                        if ((i - j - 1) > 0) { f = (i - j - 1); p = f; }
                        else if ((i - j - 1) == -1)
                        {
                            double fark11 = Math.Abs(Convert.ToDouble(eksen[0]) - Convert.ToDouble(eksen[eksen.Count - 1]));
                            double fark21 = Math.Abs(Convert.ToDouble(eksen[k - 1]) - Convert.ToDouble(eksen[k]));
                            double fark1Merkez1 = Math.Abs(Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[0]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[0]);
                            double fark2Merkez1 = Math.Abs(Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[t]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[t]);

                            if ((Convert.ToDouble(mesafe[f + 1]) < Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[f + 1]) * kayma) > Convert.ToDouble(mesafe[f])) &&
                            (Convert.ToDouble(mesafe[k - 1]) < Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[k - 1]) * kayma) > Convert.ToDouble(mesafe[k]))
                           && (fark11 < 2.3 || (fark11 > 355 && fark11 < 360)) && (fark21 < 2.3 || (fark21 > 355 && fark21 < 360)) &&
                           (fark1Merkez1 > fark2Merkez1 - 20 && fark1Merkez1 < fark2Merkez1 + 20 && fark2Merkez1 > fark1Merkez1 - 20 && fark2Merkez1 < fark1Merkez1 + 20)
                           && daireFark <= (Math.Abs(fark1Merkez1 - fark2Merkez1)) && ((daireFark2 <= 0 && (fark1Merkez1 - fark2Merkez1) <= 0) || (daireFark2 >= 0 && (fark1Merkez1 - fark2Merkez1) >= 0)))
                            {
                                daireFark = Math.Abs(fark1Merkez1 - fark2Merkez1);
                                daireFark2 = fark1Merkez1 - fark2Merkez1;
                            
                                baslangicEkseni = f;
                                bitisEkseni = k;
                                sayac++;
                               
                                if (sayac > 10) { kayma = 0.07; }
                                else if (sayac > 15) { kayma = 0.085; }
                                else if (sayac > 20) { kayma = 0.1; }
                                else if (sayac > 30) { kayma = 0.15; }
                                f = eksen.Count - 2;
                                p = eksen.Count - 1;

                            }
                            else
                            {
                                break;
                            }

                        }
                        else { f--; p--; if (f < 0) { f = eksen.Count - 2; p = eksen.Count - 1; } }
                        #endregion

                        double fark1 = Math.Abs(Convert.ToDouble(eksen[f + 1]) - Convert.ToDouble(eksen[f]));
                        double fark2 = Math.Abs(Convert.ToDouble(eksen[k - 1]) - Convert.ToDouble(eksen[k]));
                        double fark1Merkez = Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[p]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[p];
                        double fark2Merkez = Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[t]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[t];


                        if ((Convert.ToDouble(mesafe[f + 1]) < Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[f + 1]) * kayma) > Convert.ToDouble(mesafe[f])) &&
                            (Convert.ToDouble(mesafe[k - 1]) < Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[k - 1]) * kayma) > Convert.ToDouble(mesafe[k]))
                           && (fark1 < 2.3 || (fark1 > 355 && fark1 < 360)) && (fark2 < 2.3 || (fark2 > 355 && fark2 < 360)) &&
                           (fark1Merkez > fark2Merkez - 20 && fark1Merkez < fark2Merkez + 20 && fark2Merkez > fark1Merkez - 20 && fark2Merkez < fark1Merkez + 20)
                           && (daireFark < (Math.Abs(fark1Merkez - fark2Merkez) - daireFark / 4) || (sayac < 3 && daireFark < 0.99 && daireFark > -0.99)) && ((daireFark2 <= 0 && (fark1Merkez - fark2Merkez) <= 0) || (daireFark2 >= 0 && (fark1Merkez - fark2Merkez) >= 0)))
                        {
                            daireFark = Math.Abs(fark1Merkez - fark2Merkez);
                            daireFark2 = fark1Merkez - fark2Merkez;
                            
                            baslangicEkseni = f;
                            bitisEkseni = k;
                            sayac++;
                         
                            if (sayac > 10) { kayma = 0.07; }
                            else if (sayac > 15) { kayma = 0.085; }
                            else if (sayac > 20) { kayma = 0.1; }
                            else if (sayac > 30) { kayma = 0.15; }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else//artis = false
                    {
                        #region Liste döngüsü sağlamak için yazdım
                        if ((i + j + 1) < eksen.Count) { k = (i + j + 1); t = k; }
                        else if ((i + j + 1) == eksen.Count)
                        {
                            double fark11 = Math.Abs(Convert.ToDouble(eksen[f + 1]) - Convert.ToDouble(eksen[f]));
                            double fark21 = Math.Abs(Convert.ToDouble(eksen[eksen.Count - 1]) - Convert.ToDouble(eksen[0]));
                            double fark1Merkez1 = Math.Abs(Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[f]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[f]);
                            double fark2Merkez1 = Math.Abs(Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[eksen.Count - 1]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[eksen.Count - 1]);

                            if ((Convert.ToDouble(mesafe[f + 1]) > Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[f + 1]) * kayma) < Convert.ToDouble(mesafe[f])) &&
                             (Convert.ToDouble(mesafe[k - 1]) > Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[k - 1]) * kayma) < Convert.ToDouble(mesafe[k]) && daireFark < Math.Abs(fark1Merkez1 - fark2Merkez1))
                                 && (fark11 < 2.3 || (fark11 > 355 && fark11 < 360)) && (fark21 < 2.3 || (fark21 > 355 && fark21 < 360)) &&
                                (fark1Merkez1 > fark2Merkez1 - 20 && fark1Merkez1 < fark2Merkez1 + 20 && fark2Merkez1 > fark1Merkez1 - 20 && fark2Merkez1 < fark1Merkez1 + 20)
                                && daireFark <= (Math.Abs(fark1Merkez1 - fark2Merkez1)) && ((daireFark2 <= 0 && (fark1Merkez1 - fark2Merkez1) <= 0) || (daireFark2 >= 0 && (fark1Merkez1 - fark2Merkez1) >= 0)))
                            {
                                daireFark = Math.Abs(fark1Merkez1 - fark2Merkez1);
                                daireFark2 = fark1Merkez1 - fark2Merkez1;
                                
                                baslangicEkseni = f;
                                bitisEkseni = k;
                                sayac++;
                                
                                kayma = 1.5 / sayac; if (kayma > 0.3) kayma = 0.3;
                                k = 1;
                                t = 0;
                            }
                            else
                            {
                                break;
                            }

                        }
                        else { k++; t++; }
                        if ((i - j - 1) > 0) { f = (i - j - 1); p = f; }
                        else if ((i - j - 1) == -1)
                        {
                            double fark11 = Math.Abs(Convert.ToDouble(eksen[0]) - Convert.ToDouble(eksen[eksen.Count - 1]));
                            double fark21 = Math.Abs(Convert.ToDouble(eksen[k - 1]) - Convert.ToDouble(eksen[k]));
                            double fark1Merkez1 = Math.Abs(Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[0]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[0]);
                            double fark2Merkez1 = Math.Abs(Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[t]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[t]);

                            if ((Convert.ToDouble(mesafe[f + 1]) > Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[f + 1]) * kayma) < Convert.ToDouble(mesafe[f])) &&
                             (Convert.ToDouble(mesafe[k - 1]) > Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[k - 1]) * kayma) < Convert.ToDouble(mesafe[k]) && daireFark < Math.Abs(fark1Merkez1 - fark2Merkez1))
                                 && (fark11 < 2.3 || (fark11 > 355 && fark11 < 360)) && (fark21 < 2.3 || (fark21 > 355 && fark21 < 360)) &&
                                (fark1Merkez1 > fark2Merkez1 - 20 && fark1Merkez1 < fark2Merkez1 + 20 && fark2Merkez1 > fark1Merkez1 - 20 && fark2Merkez1 < fark1Merkez1 + 20)
                                && daireFark <= (Math.Abs(fark1Merkez1 - fark2Merkez1)) && ((daireFark2 <= 0 && (fark1Merkez1 - fark2Merkez1) <= 0) || (daireFark2 >= 0 && (fark1Merkez1 - fark2Merkez1) >= 0)))
                            {
                                daireFark = Math.Abs(fark1Merkez1 - fark2Merkez1);
                                daireFark2 = fark1Merkez1 - fark2Merkez1;
                                
                                baslangicEkseni = f;
                                bitisEkseni = k;
                                sayac++;
                               
                                kayma = 1.5 / sayac; if (kayma > 0.3) kayma = 0.3;
                                f = eksen.Count - 2;
                                p = eksen.Count - 1;
                            }
                            else
                            {
                                break;
                            }

                        }
                        else { f--; p--; }
                        #endregion
                        double fark1 = Math.Abs(Convert.ToDouble(eksen[f + 1]) - Convert.ToDouble(eksen[f]));
                        double fark2 = Math.Abs(Convert.ToDouble(eksen[k - 1]) - Convert.ToDouble(eksen[k]));
                        double fark1Merkez = Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[p]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[p];
                        double fark2Merkez = Math.Sin(DegreeToRadian(Math.Abs(Convert.ToDouble(eksen[t]) - Convert.ToDouble(eksen[i])))) * (double)mesafe[t];


                        if ((Convert.ToDouble(mesafe[f + 1]) > Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[f + 1]) * kayma) < Convert.ToDouble(mesafe[f])) &&
                            (Convert.ToDouble(mesafe[k - 1]) > Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[k - 1]) * kayma) < Convert.ToDouble(mesafe[k]))
                                && (fark1 < 2.3 || (fark1 > 355 && fark1 < 360)) && (fark2 < 2.3 || (fark2 > 355 && fark2 < 360)) &&
                                (fark1Merkez > fark2Merkez - 20 && fark1Merkez < fark2Merkez + 20 && fark2Merkez > fark1Merkez - 20 && fark2Merkez < fark1Merkez + 20)
                                && daireFark < (Math.Abs(fark1Merkez - fark2Merkez) - daireFark / 4) && ((daireFark2 <= 0 && (fark1Merkez - fark2Merkez) <= 0) || (daireFark2 >= 0 && (fark1Merkez - fark2Merkez) >= 0))
                                )
                        {
                            daireFark = Math.Abs(fark1Merkez - fark2Merkez);
                            daireFark2 = fark1Merkez - fark2Merkez;
                            
                            baslangicEkseni = f;
                            bitisEkseni = k;
                            sayac++;
                            
                            kayma = 1.5 / sayac; if (kayma > 0.3) kayma = 0.3;
                        }
                        else
                        {
                            hata++;
                            if (hata == 1) continue;
                            dongu = false;
                            break;
                        }
                    }
                }
                double sinir;
                try
                {
                    sinir = (double)mesafe[i] * 0.0025;

                    if ((double)mesafe[i] < 1000) sinir = 2;
                    else if ((double)mesafe[i] < 1300) sinir = 3;
                    else if ((double)mesafe[i] < 2000) sinir = 4;
                    else if ((double)mesafe[i] < 2500) sinir = 5;
                    else if ((double)mesafe[i] >= 2500) sinir = 6;
                }
                catch (Exception)
                {
                    sinir = (double)mesafe[i - 1] * 0.0025;
                    if ((double)mesafe[i] < 1000) sinir = 2;
                    else if ((double)mesafe[i - 1] < 1300) sinir = 3;
                    else if ((double)mesafe[i - 1] < 2000) sinir = 4;
                    else if ((double)mesafe[i - 1] < 2500) sinir = 5;
                    else if ((double)mesafe[i - 1] >= 2500) sinir = 6;
                }

                sinir = (sinir > 8) ? 8 : sinir;


                if (sayac >=2)
                {
                    if (Math.Abs(Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[i + 1])) < (double)mesafe[i] * 0.01 && Math.Abs(Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[i - 1])) < (double)mesafe[i] * 0.01)
                    {
                        Console.WriteLine(eksen[bitisEkseni] + ", " + eksen[i] + " ve " + eksen[baslangicEkseni] + " eksenleri arasında bir daire var.");
                        kontrol = true;

                        if (i < bitisEkseni && baslangicEkseni < i)
                        { eksenListe.RemoveRange(baslangicEkseni, (bitisEkseni - baslangicEkseni)); mesafeListe.RemoveRange(baslangicEkseni, (bitisEkseni - baslangicEkseni)); }
                        else if (i > bitisEkseni && baslangicEkseni < i)
                        { eksenListe.RemoveRange(baslangicEkseni, (eksenListe.Count - baslangicEkseni)); eksenListe.RemoveRange(0, bitisEkseni); mesafeListe.RemoveRange(baslangicEkseni, (mesafeListe.Count - baslangicEkseni)); mesafeListe.RemoveRange(0, bitisEkseni); }
                        else if (i < bitisEkseni && baslangicEkseni > i)
                        { eksenListe.RemoveRange(baslangicEkseni, (eksenListe.Count - baslangicEkseni)); eksenListe.RemoveRange(0, bitisEkseni); mesafeListe.RemoveRange(baslangicEkseni, (mesafeListe.Count - baslangicEkseni)); mesafeListe.RemoveRange(0, bitisEkseni); }
                    }
                }
            }//dıştakı for sonu
            ucgenKotnrol2(eksenListe, mesafeListe);
            return kontrol;
        }//metod sonu
        private bool ucgenKotnrol2(ArrayList eksen, ArrayList mesafe)
        {
            Boolean artis;
            bool kontrol = false;
            int baslangicEkseni = -1;
            int bitisEkseni = -1;
            int hata;

            for (int i = 1; i < eksen.Count - 1; i++)
            {
                bool dongu = true;
                hata = 0;
                int sayac = 0;
                double kayma = 0.1;
                #region merkezden uzaklaşan mı yoksa merkeze yaklaşanmı bir üçgen olduğunu buluyorz. Bir kenar uzaklaşığ bir köşe yaklaşıyorsa bir sonraki noktaya atlıyor.
                if (Convert.ToDouble(mesafe[i - 1]) > Convert.ToDouble(mesafe[i]))
                {
                    if (Convert.ToDouble(mesafe[i + 1]) > Convert.ToDouble(mesafe[i]))
                    {
                        artis = true;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (Convert.ToDouble(mesafe[i - 1]) < Convert.ToDouble(mesafe[i]))
                {
                    if (Convert.ToDouble(mesafe[i + 1]) < Convert.ToDouble(mesafe[i]))
                    {
                        artis = false;
                    }
                    else
                    {
                        continue;
                    }
                }
                else { continue; }
                #endregion
                int k = i, f = i;

                for (int j = 1; ((i + j + 1) < eksen.Count || (i - j - 1) > 0); j++)
                {

                    if (artis)
                    {
                        #region Liste döngüsü sağlamak için yazdım
                        if ((i + j + 1) < eksen.Count) { k = (i + j + 1); }
                        else if ((i + j + 1) == eksen.Count)
                        {
                            double fark11 = Math.Abs(Convert.ToDouble(eksen[f + 1]) - Convert.ToDouble(eksen[f]));
                            double fark21 = Math.Abs(Convert.ToDouble(eksen[eksen.Count - 1]) - Convert.ToDouble(eksen[0]));
                            if ((Convert.ToDouble(mesafe[f + 1]) < Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[f + 1]) * kayma) > Convert.ToDouble(mesafe[f])) &&
                            (Convert.ToDouble(mesafe[k - 1]) < Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[k - 1]) * kayma) > Convert.ToDouble(mesafe[k]))
                           && (fark11 < 2.3 || (fark11 > 355 && fark11 < 360)) && (fark21 < 2.3 || (fark21 > 355 && fark21 < 360)))
                            {
                                baslangicEkseni = f;
                                bitisEkseni = k;
                                sayac++;
                                
                                if (sayac > 10) { kayma = 0.07; }
                                else if (sayac > 15) { kayma = 0.085; }
                                else if (sayac > 20) { kayma = 0.1; }
                                else if (sayac > 30) { kayma = 0.15; }
                                k = 1;
                            }
                            else
                            {
                                break;
                            }

                        }
                        else { k++; }
                        if ((i - j - 1) > 0) { f = (i - j - 1); }
                        else if ((i - j - 1) == 0)
                        {
                            double fark11 = Math.Abs(Convert.ToDouble(eksen[0]) - Convert.ToDouble(eksen[eksen.Count - 1]));
                            double fark21 = Math.Abs(Convert.ToDouble(eksen[k - 1]) - Convert.ToDouble(eksen[k]));
                            if ((Convert.ToDouble(mesafe[f + 1]) < Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[f + 1]) * kayma) > Convert.ToDouble(mesafe[f])) &&
                            (Convert.ToDouble(mesafe[k - 1]) < Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[k - 1]) * kayma) > Convert.ToDouble(mesafe[k]))
                           && (fark11 < 2.3 || (fark11 > 355 && fark11 < 360)) && (fark21 < 2.3 || (fark21 > 355 && fark21 < 360)))
                            {
                                baslangicEkseni = f;
                                bitisEkseni = k;
                                sayac++;
                               
                                if (sayac > 10) { kayma = 0.07; }
                                else if (sayac > 15) { kayma = 0.085; }
                                else if (sayac > 20) { kayma = 0.1; }
                                else if (sayac > 30) { kayma = 0.15; }
                                f = eksen.Count - 2;

                            }
                            else
                            {
                                break;
                            }

                        }
                        else { f--; if (f < 0) f = eksen.Count - 2; }
                        #endregion

                        double fark1 = Math.Abs(Convert.ToDouble(eksen[f + 1]) - Convert.ToDouble(eksen[f]));
                        double fark2 = Math.Abs(Convert.ToDouble(eksen[k - 1]) - Convert.ToDouble(eksen[k]));
                        if ((Convert.ToDouble(mesafe[f + 1]) < Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[f + 1]) * kayma) > Convert.ToDouble(mesafe[f])) &&
                            (Convert.ToDouble(mesafe[k - 1]) < Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) + Convert.ToDouble(mesafe[k - 1]) * kayma) > Convert.ToDouble(mesafe[k]))
                           && (fark1 < 2.3 || (fark1 > 355 && fark1 < 360)) && (fark2 < 2.3 || (fark2 > 355 && fark2 < 360)))
                        {
                            baslangicEkseni = f;
                            bitisEkseni = k;
                            sayac++;
                           
                            if (sayac > 10) { kayma = 0.07; }
                            else if (sayac > 15) { kayma = 0.085; }
                            else if (sayac > 20) { kayma = 0.1; }
                            else if (sayac > 30) { kayma = 0.15; }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        #region Liste döngüsü sağlamak için yazdım
                        if ((i + j + 1) < eksen.Count) { k = (i + j + 1); }
                        else if ((i + j + 1) == eksen.Count)
                        {
                            double fark11 = Math.Abs(Convert.ToDouble(eksen[f + 1]) - Convert.ToDouble(eksen[f]));
                            double fark21 = Math.Abs(Convert.ToDouble(eksen[eksen.Count - 1]) - Convert.ToDouble(eksen[0]));
                            if ((Convert.ToDouble(mesafe[f + 1]) > Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[f + 1]) * kayma) < Convert.ToDouble(mesafe[f])) &&
                             (Convert.ToDouble(mesafe[k - 1]) > Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[k - 1]) * kayma) < Convert.ToDouble(mesafe[k]))
                                 && (fark11 < 2.3 || (fark11 > 355 && fark11 < 360)) && (fark21 < 2.3 || (fark21 > 355 && fark21 < 360)))
                            {
                                baslangicEkseni = f;
                                bitisEkseni = k;
                                sayac++;
                                
                                kayma = 1.5 / sayac; if (kayma > 0.3) kayma = 0.3;
                                k = 1;
                            }
                            else
                            {
                                break;
                            }

                        }
                        else { k++; }
                        if ((i - j - 1) > 0) { f = (i - j - 1); }
                        else if ((i - j - 1) ==-1)
                        {
                            double fark11 = Math.Abs(Convert.ToDouble(eksen[0]) - Convert.ToDouble(eksen[eksen.Count - 1]));
                            double fark21 = Math.Abs(Convert.ToDouble(eksen[k - 1]) - Convert.ToDouble(eksen[k]));
                            if ((Convert.ToDouble(mesafe[f + 1]) > Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[f + 1]) * kayma) < Convert.ToDouble(mesafe[f])) &&
                             (Convert.ToDouble(mesafe[k - 1]) > Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[k - 1]) * kayma) < Convert.ToDouble(mesafe[k]))
                                 && (fark11 < 2.3 || (fark11 > 355 && fark11 < 360)) && (fark21 < 2.3 || (fark21 > 355 && fark21 < 360)))
                            {
                                baslangicEkseni = f;
                                bitisEkseni = k;
                                sayac++;
                                
                                kayma = 1.5 / sayac; if (kayma > 0.3) kayma = 0.3;
                                f = eksen.Count - 2;

                            }
                            else
                            {
                                break;
                            }

                        }
                        else { f--; }
                        #endregion
                        double fark1 = Math.Abs(Convert.ToDouble(eksen[f + 1]) - Convert.ToDouble(eksen[f]));
                        double fark2 = Math.Abs(Convert.ToDouble(eksen[k - 1]) - Convert.ToDouble(eksen[k]));

                        if ((Convert.ToDouble(mesafe[f + 1]) > Convert.ToDouble(mesafe[f]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[f + 1]) * kayma) < Convert.ToDouble(mesafe[f])) &&
                            (Convert.ToDouble(mesafe[k - 1]) > Convert.ToDouble(mesafe[k]) && (Convert.ToDouble(mesafe[i]) - Convert.ToDouble(mesafe[k - 1]) * kayma) < Convert.ToDouble(mesafe[k]))
                                && (fark1 < 2.3 || (fark1 > 355 && fark1 < 360)) && (fark2 < 2.3 || (fark2 > 355 && fark2 < 360)))
                        {
                            baslangicEkseni = f;
                            bitisEkseni = k;
                            sayac++;
                           
                            kayma = 1.5 / sayac; if (kayma > 0.3) kayma = 0.3;
                        }
                        else
                        {
                            hata++;
                            if (hata == 1) continue;
                            dongu = false;
                            break;
                        }
                    }
                }//içteki for sonu
                 /* if(sayac > 0 && dongu)
                  {
                      if (baslangicEkseni <= 0)
                      {

                      }else if (bitisEkseni >= eksen.Count)
                      {

                      }
                  }*///Liste döngüsü için bu kısım tamamlanmalı
                if (sayac >= 3)
                {

                    Console.WriteLine(eksen[bitisEkseni] + ", " + eksen[i] + " ve " + eksen[baslangicEkseni] + " eksenleri arasında bir ucgen var.");
                    kontrol = true;
                    if (i < bitisEkseni && baslangicEkseni < i)
                    { eksenListe.RemoveRange(baslangicEkseni, (bitisEkseni - baslangicEkseni)); mesafeListe.RemoveRange(baslangicEkseni, (bitisEkseni - baslangicEkseni)); }
                    else if (i > bitisEkseni && baslangicEkseni < i)
                    { eksenListe.RemoveRange(baslangicEkseni, (eksenListe.Count - baslangicEkseni)); eksenListe.RemoveRange(0, bitisEkseni); mesafeListe.RemoveRange(baslangicEkseni, (mesafeListe.Count - baslangicEkseni)); mesafeListe.RemoveRange(0, bitisEkseni); }
                    else if (i < bitisEkseni && baslangicEkseni > i)
                    { eksenListe.RemoveRange(baslangicEkseni, (eksenListe.Count - baslangicEkseni)); eksenListe.RemoveRange(0, bitisEkseni); mesafeListe.RemoveRange(baslangicEkseni, (mesafeListe.Count - baslangicEkseni)); mesafeListe.RemoveRange(0, bitisEkseni); }
                }
            }//dıştakı for sonu
            dortgenKotnrol2(eksenListe, mesafeListe);
            return kontrol;
        }//metod sonu
        private bool dortgenKotnrol2(ArrayList eksen, ArrayList mesafe)
        {
            Boolean kontrol = false;
            int baslangicEkseni = -1;
            int bitisEkseni = -1;
            int hata;
            int sayac;
            double kayma;
            for (int i = 0; i < eksen.Count; i++)
            {
                sayac = 0;
                baslangicEkseni = -1;
                bitisEkseni = -1;
                kayma = Convert.ToDouble(mesafe[i]) * 0.02;
                //artis = (Convert.ToDouble(mesafe[i]) > Convert.ToDouble(mesafe[i - 1]) )? true : false;
                for (int j = i; j < eksen.Count - 1; j++)
                {
                    double mesafe1 = Convert.ToDouble(mesafe[j]);// * Math.Cos(DegreeToRadian(Convert.ToDouble(eksen[j])));
                    double mesafe2 = Convert.ToDouble(mesafe[j + 1]);// * Math.Cos(DegreeToRadian(Convert.ToDouble(eksen[j+1])));
                    double fark = Math.Abs(Convert.ToDouble(eksen[j]) - Convert.ToDouble(eksen[j + 1]));

                    if ((mesafe1 > (mesafe2 - kayma) && mesafe1 < (mesafe2 + kayma)) && (fark < 2.3 || (fark > 355 && fark < 360)))
                    {
                        if (sayac == 0) { baslangicEkseni = i; }
                        sayac++;
                       

                    }
                    else
                    {
                        bitisEkseni = j;
                        break;
                    }

                }//içteki for sonu
                if (sayac > 0 && bitisEkseni == -1)
                {
                    bitisEkseni = eksen.Count - 1;//Diziler döngüsel olmalı buradaki hatalar her objede kesinlikl düzeltilmelidir.
                    double mesafe10 = Convert.ToDouble(mesafe[bitisEkseni]);// * Math.Cos(DegreeToRadian(Convert.ToDouble(eksen[j])));
                    double mesafe20 = Convert.ToDouble(mesafe[0]);// * Math.Cos(DegreeToRadian(Convert.ToDouble(eksen[j+1])));    
                    if ((mesafe10 > (mesafe20 - kayma) && mesafe10 < (mesafe20 + kayma)))
                    {
                        sayac++;
                        for (int j = 0; j < eksen.Count - 1; j++)
                        {
                            double mesafe1 = Convert.ToDouble(mesafe[bitisEkseni]);// * Math.Cos(DegreeToRadian(Convert.ToDouble(eksen[j])));
                            double mesafe2 = Convert.ToDouble(mesafe[j + 1]);// * Math.Cos(DegreeToRadian(Convert.ToDouble(eksen[j+1])));    
                            double fark = Math.Abs(Convert.ToDouble(eksen[j]) - Convert.ToDouble(eksen[j + 1]));
                            if ((mesafe1 > (mesafe2 - kayma) && mesafe1 < (mesafe2 + kayma)) && (fark < 2.3 || (fark > 355 && fark < 360)))
                            {
                                if (sayac == 0) { baslangicEkseni = i; }
                                sayac++;
                               

                            }
                            else
                            {
                                bitisEkseni = j;
                                i = eksen.Count;// tekrar i'yi değiştirmemek için kontrol olarak ekledim
                                break;
                            }
                        }

                    }
                }
                double sinir;
                try
                {
                    sinir = ((5000 / (double)mesafe[i]) > 5) ? 8000 / ((double)mesafe[i]) : 5;
                }
                catch (Exception)
                {

                    sinir = ((5000 / (double)mesafe[i - 1]) > 5) ? 8000 / ((double)mesafe[i - 1]) : 5;
                }

                sinir = (sinir > 10) ? 10 : sinir;
                if (sayac >= sinir)
                {

                    Console.WriteLine(eksen[baslangicEkseni] + " - " + eksen[bitisEkseni] + " açıları arasında bir dörtgen var.");
                    kontrol = true;

                }
                i = (i != eksen.Count && i < bitisEkseni) ? bitisEkseni : i;



            }


            return kontrol;
        }//metod sonu
        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
    }

