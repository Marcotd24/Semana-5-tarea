using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        List<int> numeros = new List<int>();

        Console.WriteLine("Introduce 6 números ganadores de la lotería:");
        for (int i = 0; i < 6; i++)
        {
            Console.Write($"Número {i + 1}: ");
            int num = Convert.ToInt32(Console.ReadLine());
            numeros.Add(num);
        }

        numeros.Sort();

        Console.WriteLine("\nNúmeros ganadores ordenados:");
        foreach (int numero in numeros)
        {
            Console.Write(numero + " ");
        }
    }
}
