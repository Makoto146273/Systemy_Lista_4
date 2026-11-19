
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

        Console.WriteLine("\n1. Dlaczego bez synchronizacji wynik moze byc niepoprawny?");
        Console.WriteLine(
            "Operacja licznik++ sklada sie z odczytu, zwiekszenia " +
            "i zapisu wartosci. Kilka watkow moze jednoczesnie " +
            "odczytac ta sama wartosc, przez co niektore inkrementacje " +
            "zostana utracone."
        );

        Console.WriteLine("\n2. Dlaczego lock zapewnia poprawny wynik?");
        Console.WriteLine(
            "Lock pozwala tylko jednemu watkowi jednoczesnie " +
            "wykonywac kod znajdujacy sie w sekcji krytycznej. " +
            "Dzieki temu inkrementacje nie koliduja ze soba."
        );

        Console.WriteLine("\n3. Jak dziala Interlocked.Increment?");
        Console.WriteLine(
            "Interlocked.Increment wykonuje atomowe zwiekszenie " +
            "wartosci zmiennej. Operacja jest niepodzielna, " +
            "dlatego nie dochodzi do utraty inkrementacji."
        );

        Console.WriteLine("\n4. Czy operacja ++ jest atomowa?");
        Console.WriteLine(
            "Nie. Operacja ++ sklada sie z kilku krokow: " +
            "odczytu wartosci, zwiekszenia jej i zapisania wyniku."
        );

        Console.WriteLine("\n5. Porownanie srednich czasow:");

        Console.WriteLine("Bez synchronizacji: " + czasBezLock + " ms");
        Console.WriteLine("Lock: " + czasLock + " ms");
        Console.WriteLine("Interlocked: " + czasInterlocked + " ms");

        Console.WriteLine("\n6. Ktora metoda byla najszybsza?");

        if (czasBezLock <= czasLock && czasBezLock <= czasInterlocked)
            Console.WriteLine("Bez synchronizacji.");
        else if (czasLock <= czasInterlocked)
            Console.WriteLine("Lock.");
        else
            Console.WriteLine("Interlocked.");

        Console.WriteLine("\n7. Wniosek:");
        Console.WriteLine(
            "Synchronizacja zapewnia poprawny wynik, ale moze " +
            "zwiekszac czas wykonania programu. Lock chroni cala " +
            "sekcje krytyczna, natomiast Interlocked wykonuje " +
            "atomowa operacje na pojedynczej zmiennej."
        );
    }
}
