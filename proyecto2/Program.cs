using System;
using System.Collections.Generic;

namespace ParqueDiversiones
{
    class Persona
    {
        public string Nombre { get; set; }

        public Persona(string nombre)
        {
            Nombre = nombre;
        }

        public override string ToString()
        {
            return Nombre;
        }
    }

    class AsignadorAsientos
    {
        private Queue<Persona> colaEspera;
        private int asientosTotales = 30;
        private List<Persona> asientosAsignados;

        public AsignadorAsientos()
        {
            colaEspera = new Queue<Persona>();
            asientosAsignados = new List<Persona>();
        }

        public void AgregarPersona(string nombre)
        {
            if (asientosAsignados.Count < asientosTotales)
            {
                Persona nueva = new Persona(nombre);
                colaEspera.Enqueue(nueva);
                Console.WriteLine($" {nombre} agregado a la cola.");
            }
            else
            {
                Console.WriteLine(" Todos los asientos ya fueron asignados.");
            }
        }

        public void AsignarAsientos()
        {
            while (colaEspera.Count > 0 && asientosAsignados.Count < asientosTotales)
            {
                Persona persona = colaEspera.Dequeue();
                asientosAsignados.Add(persona);
                Console.WriteLine($" Asiento asignado a: {persona.Nombre}");
            }
        }

        public void VerCola()
        {
            Console.WriteLine("\n Personas en cola:");
            foreach (var p in colaEspera)
                Console.WriteLine($"- {p.Nombre}");
        }

        public void VerAsientosAsignados()
        {
            Console.WriteLine("\n Asientos ya asignados:");
            for (int i = 0; i < asientosAsignados.Count; i++)
            {
                Console.WriteLine($"Asiento {i + 1}: {asientosAsignados[i].Nombre}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            AsignadorAsientos asignador = new AsignadorAsientos();

            // Simulación
            asignador.AgregarPersona("Luis");
            asignador.AgregarPersona("Ana");
            asignador.AgregarPersona("Carlos");
            asignador.AgregarPersona("María");

            asignador.VerCola();
            asignador.AsignarAsientos();
            asignador.VerAsientosAsignados();
        }
    }
}
