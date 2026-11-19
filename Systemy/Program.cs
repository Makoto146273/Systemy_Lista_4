
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

class Program
{
    static void Main()
    {
        int liczbaWatkow = 8;

        List<int[]> wyniki = new List<int[]>();
        List<long> czasy = new List<long>();

        for (int powtorzenie = 1; powtorzenie <= 5; powtorzenie++)
        {
            Thread[] watki = new Thread[liczbaWatkow];

            List<int> kolejnosc = new List<int>();
            object blokada = new object();

            Stopwatch stoper = Stopwatch.StartNew();

            for (int i = 0; i < liczbaWatkow; i++)
            {
                int numer = i + 1;

                watki[i] = new Thread(() =>
                {
                    for (int krok = 1; krok <= 5; krok++)
                    {
                        lock (blokada)
                        {
                            kolejnosc.Add(numer);
                        }

                        Thread.Sleep(10);
                    }
                });

                watki[i].Name = "Auto-" + numer.ToString("D2");
                watki[i].Start();
            }

            for (int i = 0; i < liczbaWatkow; i++)
            {
                watki[i].Join();
            }

            stoper.Stop();

            wyniki.Add(kolejnosc.ToArray());
            czasy.Add(stoper.ElapsedMilliseconds);
        }

        Console.WriteLine("\n          KOLEJNOSC WYKONYWANIA WATKOW\n");

        for (int i = 0; i < wyniki.Count; i++)
        {
            Console.Write("Proba " + (i + 1) + ": ");

            foreach (int numer in wyniki[i])
            {
                Console.Write(numer + " ");
            }

            Console.WriteLine();
        }

        Console.WriteLine("\nCzasy wykonania:");

        for (int i = 0; i < czasy.Count; i++)
        {
            Console.WriteLine("Proba " + (i + 1) + ": " + czasy[i] + " ms");
        }

        Console.WriteLine("Kolejność wypisywania komunikatów jest róźna (często oprócz pierwszego kroku ze względu na kolejność uruchomienia wątków");
    }
}
