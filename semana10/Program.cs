using System;
using System.Collections.Generic;
using System.Linq;

namespace CampaniaVacunacion
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Conjunto de 500 ciudadanos ficticios
            HashSet<string> ciudadanos = new HashSet<string>();
            for (int i = 1; i <= 500; i++)
            {
                ciudadanos.Add("Ciudadano " + i);
            }

            // 2. Conjunto de vacunados con Pfizer (75 ciudadanos)
            HashSet<string> pfizer = new HashSet<string>();
            for (int i = 1; i <= 75; i++)
            {
                pfizer.Add("Ciudadano " + i); // Ejemplo: primeros 75
            }

            // 3. Conjunto de vacunados con AstraZeneca (75 ciudadanos)
            HashSet<string> astrazeneca = new HashSet<string>();
            for (int i = 50; i < 125; i++)  // algunos se solapan con Pfizer
            {
                astrazeneca.Add("Ciudadano " + i);
            }

            // 4. Listados solicitados:

            // a) Ciudadanos que no se han vacunado
            HashSet<string> vacunados = new HashSet<string>(pfizer);
            vacunados.UnionWith(astrazeneca); // Pfizer ∪ AstraZeneca
            HashSet<string> noVacunados = new HashSet<string>(ciudadanos);
            noVacunados.ExceptWith(vacunados); // Ciudadanos – Vacunados

            // b) Ciudadanos con ambas dosis (Pfizer ∩ AstraZeneca)
            HashSet<string> ambasDosis = new HashSet<string>(pfizer);
            ambasDosis.IntersectWith(astrazeneca);

            // c) Solo Pfizer (Pfizer – AstraZeneca)
            HashSet<string> soloPfizer = new HashSet<string>(pfizer);
            soloPfizer.ExceptWith(astrazeneca);

            // d) Solo AstraZeneca (AstraZeneca – Pfizer)
            HashSet<string> soloAstraZeneca = new HashSet<string>(astrazeneca);
            soloAstraZeneca.ExceptWith(pfizer);

            // 5. Mostrar resultados
            Console.WriteLine("=== Ciudadanos no vacunados ===");
            Console.WriteLine("Total: " + noVacunados.Count);

            Console.WriteLine("\n=== Ciudadanos con ambas dosis (Pfizer y AstraZeneca) ===");
            Console.WriteLine("Total: " + ambasDosis.Count);

            Console.WriteLine("\n=== Ciudadanos con solo Pfizer ===");
            Console.WriteLine("Total: " + soloPfizer.Count);

            Console.WriteLine("\n=== Ciudadanos con solo AstraZeneca ===");
            Console.WriteLine("Total: " + soloAstraZeneca.Count);

            Console.ReadKey();
        }
    }
}
