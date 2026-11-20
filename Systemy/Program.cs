
using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    const int liczbaWatkow = 8;
    const int liczbaInkrementacji = 1_000_000;
    const int liczbaProb = 5;

    static void Main()
    {
        Console.WriteLine("Liczba watkow: " + liczbaWatkow);
        Console.WriteLine("Inkrementacje na watek: " + liczbaInkrementacji);
        Console.WriteLine("Oczekiwana wartosc licznika: " +
            (liczbaWatkow * liczbaInkrementacji));

        long czasBezLock = TestBezSynchronizacji();
        long czasLock = TestLock();
        long czasInterlocked = TestInterlocked();

        WyswietlWnioski(czasBezLock, czasLock, czasInterlocked);
    }

    static long TestBezSynchronizacji()
    {
        long sumaCzasu = 0;

        Console.WriteLine("\n--- BEZ SYNCHRONIZACJI ---");

        for (int proba = 1; proba <= liczbaProb; proba++)
        {
            int licznik = 0;
            Thread[] watki = new Thread[liczbaWatkow];

            Stopwatch stoper = Stopwatch.StartNew();

            for (int i = 0; i < liczbaWatkow; i++)
            {
                watki[i] = new Thread(() =>
                {
                    for (int j = 0; j < liczbaInkrementacji; j++)
                    {
                        licznik++;
                    }
                });

                watki[i].Start();
            }

            foreach (Thread watek in watki)
            {
                watek.Join();
            }

            stoper.Stop();

            sumaCzasu += stoper.ElapsedMilliseconds;

            Console.WriteLine(
                "Proba " + proba +
                ": licznik = " + licznik +
                ", czas = " + stoper.ElapsedMilliseconds + " ms"
            );
        }

        long sredniCzas = sumaCzasu / liczbaProb;

        Console.WriteLine("Sredni czas: " + sredniCzas + " ms");

        return sredniCzas;
    }

    static long TestLock()
    {
        long sumaCzasu = 0;
        object blokada = new object();

        Console.WriteLine("\n--- Z UZYCIEM LOCK ---");

        for (int proba = 1; proba <= liczbaProb; proba++)
        {
            int licznik = 0;
            Thread[] watki = new Thread[liczbaWatkow];

            Stopwatch stoper = Stopwatch.StartNew();

            for (int i = 0; i < liczbaWatkow; i++)
            {
                watki[i] = new Thread(() =>
                {
                    for (int j = 0; j < liczbaInkrementacji; j++)
                    {
                        lock (blokada)
                        {
                            licznik++;
                        }
                    }
                });

                watki[i].Start();
            }

            foreach (Thread watek in watki)
            {
                watek.Join();
            }

            stoper.Stop();

            sumaCzasu += stoper.ElapsedMilliseconds;

            Console.WriteLine(
                "Proba " + proba +
                ": licznik = " + licznik +
                ", czas = " + stoper.ElapsedMilliseconds + " ms"
            );
        }

        long sredniCzas = sumaCzasu / liczbaProb;

        Console.WriteLine("Sredni czas: " + sredniCzas + " ms");

        return sredniCzas;
    }

    static long TestInterlocked()
    {
        long sumaCzasu = 0;

        Console.WriteLine("\n--- Z UZYCIEM INTERLOCKED ---");

        for (int proba = 1; proba <= liczbaProb; proba++)
        {
            int licznik = 0;
            Thread[] watki = new Thread[liczbaWatkow];

            Stopwatch stoper = Stopwatch.StartNew();

            for (int i = 0; i < liczbaWatkow; i++)
            {
                watki[i] = new Thread(() =>
                {
                    for (int j = 0; j < liczbaInkrementacji; j++)
                    {
                        Interlocked.Increment(ref licznik);
                    }
                });

                watki[i].Start();
            }

            foreach (Thread watek in watki)
            {
                watek.Join();
            }

            stoper.Stop();

            sumaCzasu += stoper.ElapsedMilliseconds;

            Console.WriteLine(
                "Proba " + proba +
                ": licznik = " + licznik +
                ", czas = " + stoper.ElapsedMilliseconds + " ms"
            );
        }

        long sredniCzas = sumaCzasu / liczbaProb;

        Console.WriteLine("Sredni czas: " + sredniCzas + " ms");

        return sredniCzas;
    }

    static void WyswietlWnioski(
        long czasBezLock,
        long czasLock,
        long czasInterlocked)
    {
        Console.WriteLine("\n========== ODPOWIEDZI NA PYTANIA ==========");

        Console.WriteLine("\n* Zasada DRY nie jest zachowana, ponieważ połączenie trzech wariantów w jednej metodzie mogłoby wprowadzić dodatkowe instrukcje warunkowe do pętli, co wpłynęłoby na wiarygodność pomiarów czasu działania algorytmów.");

        Console.WriteLine("\n1. Dlaczego operacja licznik++ nie jest atomowa? Gdzie moze wystapic przelaczenie watkow?");
        Console.WriteLine("Operacja licznik++ sklada sie z kilku instrukcji: odczytania aktualnej wartosci, dodania 1 i zapisania wyniku. W kodzie IL odpowiadaja im m.in. instrukcje ladowania wartosci (ldfld), dodawania (add) i zapisywania (stfld). Przelaczenie watkow moze nastapic pomiedzy odczytem a zapisem. Wtedy dwa watki moga odczytac ta sama wartosc i zapisac ten sam wynik, przez co jedna inkrementacja zostanie utracona.");

        Console.WriteLine("\n2. Dlaczego lock jest wolniejszy od Interlocked, ale czesciej potrzebny?");
        Console.WriteLine("Lock jest bardziej uniwersalny, poniewaz pozwala zabezpieczyc kilka instrukcji lub operacji na wielu zmiennych, a Interlocked sluzy glownie do pojedynczych operacji atomowych.");
    }
}
