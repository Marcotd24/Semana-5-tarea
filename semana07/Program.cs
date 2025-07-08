using System;
using System.Collections.Generic;

namespace PilasEjercicios
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Selecciona una opción:");
            Console.WriteLine("1. Verificar paréntesis balanceados");
            Console.WriteLine("2. Resolver Torres de Hanoi");
            Console.Write("Opción: ");
            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    EjecutarVerificacionParentesis();
                    break;
                case "2":
                    EjecutarTorresDeHanoi();
                    break;
                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
        }

        // ------------------ Ejercicio 1 ------------------

        /// <summary>
        /// Verifica si una expresión tiene paréntesis, llaves y corchetes balanceados.
        /// </summary>
        static bool EstaBalanceado(string expresion)
        {
            Stack<char> pila = new Stack<char>();

            foreach (char c in expresion)
            {
                if (c == '(' || c == '{' || c == '[')
                    pila.Push(c);
                else if (c == ')' || c == '}' || c == ']')
                {
                    if (pila.Count == 0)
                        return false;

                    char tope = pila.Pop();
                    if ((c == ')' && tope != '(') ||
                        (c == '}' && tope != '{') ||
                        (c == ']' && tope != '['))
                        return false;
                }
            }

            return pila.Count == 0;
        }

        static void EjecutarVerificacionParentesis()
        {
            Console.WriteLine("\nIngresa una expresión matemática:");
            string expresion = Console.ReadLine();

            if (EstaBalanceado(expresion))
                Console.WriteLine("Fórmula balanceada.");
            else
                Console.WriteLine("Fórmula no balanceada.");
        }

        // ------------------ Ejercicio 2 ------------------

        static Stack<int> origen = new Stack<int>();
        static Stack<int> destino = new Stack<int>();
        static Stack<int> auxiliar = new Stack<int>();

        /// <summary>
        /// Muestra los pasos para resolver las Torres de Hanoi.
        /// </summary>
        static void MoverDisco(int n, Stack<int> desde, Stack<int> hacia, Stack<int> aux, string nombreDesde, string nombreHacia, string nombreAux)
        {
            if (n == 1)
            {
                int disco = desde.Pop();
                hacia.Push(disco);
                Console.WriteLine($"Mover disco {disco} de {nombreDesde} a {nombreHacia}");
            }
            else
            {
                MoverDisco(n - 1, desde, aux, hacia, nombreDesde, nombreAux, nombreHacia);
                MoverDisco(1, desde, hacia, aux, nombreDesde, nombreHacia, nombreAux);
                MoverDisco(n - 1, aux, hacia, desde, nombreAux, nombreHacia, nombreDesde);
            }
        }

        static void EjecutarTorresDeHanoi()
        {
            Console.Write("\nIngrese el número de discos: ");
            int numDiscos = int.Parse(Console.ReadLine());

            origen.Clear();
            destino.Clear();
            auxiliar.Clear();

            for (int i = numDiscos; i >= 1; i--)
                origen.Push(i);

            Console.WriteLine("\nSolución paso a paso:");
            MoverDisco(numDiscos, origen, destino, auxiliar, "Origen", "Destino", "Auxiliar");
        }
    }
}
