using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Lista de precios
        List<int> precios = new List<int> { 50, 75, 46, 22, 80, 65, 8 };

        // Buscar el menor y el mayor usando funciones de lista
        int precioMinimo = int.MaxValue;
        int precioMaximo = int.MinValue;

        foreach (int precio in precios)
        {
            if (precio < precioMinimo)
                precioMinimo = precio;

            if (precio > precioMaximo)
                precioMaximo = precio;
        }

        // Mostrar los resultados
        Console.WriteLine($"El precio menor es: {precioMinimo}");
        Console.WriteLine($"El precio mayor es: {precioMaximo}");
    }
}

