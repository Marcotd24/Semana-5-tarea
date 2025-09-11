using System;
using System.Collections.Generic;
using System.Linq;

namespace CatalogoRevistas
{
    /// <summary>
    /// Aplicación de consola para gestionar un catálogo de revistas y buscar títulos
    /// usando dos enfoques: búsqueda lineal (iterativa) y búsqueda binaria (recursiva).
    /// Requisitos cubiertos:
    ///  - Al menos 10 títulos precargados
    ///  - Menú interactivo para buscar y mostrar resultados ("Encontrado" / "No encontrado")
    ///  - Código comentado y estructurado
    ///  - Buenas prácticas básicas (encapsulamiento, validaciones, manejo de entradas)
    /// </summary>
    internal static class Program
    {
        private static void Main()
        {
            var catalogo = new CatalogoRevistas();

            // Precargamos al menos 10 títulos (puede editar / añadir los suyos)
            catalogo.InsertarOrdenado("National Geographic");
            catalogo.InsertarOrdenado("Science");
            catalogo.InsertarOrdenado("Nature");
            catalogo.InsertarOrdenado("The Economist");
            catalogo.InsertarOrdenado("Time");
            catalogo.InsertarOrdenado("Forbes");
            catalogo.InsertarOrdenado("Wired");
            catalogo.InsertarOrdenado("IEEE Spectrum");
            catalogo.InsertarOrdenado("Harvard Business Review");
            catalogo.InsertarOrdenado("MIT Technology Review");

            MenuPrincipal(catalogo);
        }

        /// <summary>
        /// Muestra el menú principal y gestiona la interacción con el usuario.
        /// </summary>
        private static void MenuPrincipal(CatalogoRevistas catalogo)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("==============================");
                Console.WriteLine("   Catálogo de Revistas 1.0   ");
                Console.WriteLine("==============================\n");
                Console.WriteLine("1) Buscar (búsqueda lineal iterativa)");
                Console.WriteLine("2) Buscar (búsqueda binaria recursiva)");
                Console.WriteLine("3) Mostrar catálogo");
                Console.WriteLine("4) Agregar título al catálogo");
                Console.WriteLine("0) Salir\n");
                Console.Write("Seleccione una opción: ");

                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        BuscarIterativa(catalogo);
                        break;
                    case "2":
                        BuscarRecursiva(catalogo);
                        break;
                    case "3":
                        MostrarCatalogo(catalogo);
                        break;
                    case "4":
                        AgregarTitulo(catalogo);
                        break;
                    case "0":
                        Console.WriteLine("\n¡Hasta pronto!");
                        return;
                    default:
                        Console.WriteLine("\nOpción no válida. Presione una tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void BuscarIterativa(CatalogoRevistas catalogo)
        {
            Console.Write("\nIngrese el título a buscar (iterativa): ");
            var consulta = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(consulta))
            {
                Console.WriteLine("\nEntrada vacía. Presione una tecla para continuar...");
                Console.ReadKey();
                return;
            }

            bool encontrado = catalogo.BusquedaLinealIterativa(consulta);
            Console.WriteLine(encontrado ? "\nEncontrado" : "\nNo encontrado");
            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        private static void BuscarRecursiva(CatalogoRevistas catalogo)
        {
            Console.Write("\nIngrese el título a buscar (binaria recursiva): ");
            var consulta = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(consulta))
            {
                Console.WriteLine("\nEntrada vacía. Presione una tecla para continuar...");
                Console.ReadKey();
                return;
            }

            bool encontrado = catalogo.BusquedaBinariaRecursiva(consulta);
            Console.WriteLine(encontrado ? "\nEncontrado" : "\nNo encontrado");
            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        private static void MostrarCatalogo(CatalogoRevistas catalogo)
        {
            Console.WriteLine("\nCatálogo (ordenado alfabéticamente):\n");
            int i = 1;
            foreach (var t in catalogo.Titulos)
            {
                Console.WriteLine($"{i,2}. {t}");
                i++;
            }
            Console.WriteLine("\nTotal de títulos: " + catalogo.Titulos.Count);
            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        private static void AgregarTitulo(CatalogoRevistas catalogo)
        {
            Console.Write("\nIngrese el nuevo título: ");
            var titulo = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(titulo))
            {
                Console.WriteLine("\nNo se agregó (entrada vacía).");
            }
            else
            {
                bool insertado = catalogo.InsertarOrdenado(titulo);
                Console.WriteLine(insertado
                    ? "\nTítulo agregado y catálogo actualizado."
                    : "\nEl título ya existe en el catálogo (no se duplican).");
            }
            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Clase que encapsula el catálogo de revistas y los algoritmos de búsqueda.
    /// </summary>
    public class CatalogoRevistas
    {
        private readonly List<string> _titulos;
        private static readonly StringComparer Comparador = StringComparer.OrdinalIgnoreCase;

        public CatalogoRevistas()
        {
            _titulos = new List<string>();
        }

        /// <summary>
        /// Exposición de solo lectura del catálogo (ordenado).
        /// </summary>
        public IReadOnlyList<string> Titulos => _titulos;

        /// <summary>
        /// Inserta un título manteniendo el orden alfabético sin duplicados (case-insensitive).
        /// </summary>
        public bool InsertarOrdenado(string titulo)
        {
            string limpio = Normalizar(titulo);
            if (string.IsNullOrEmpty(limpio)) return false;

            // Evita duplicados (ignorando mayúsculas/minúsculas)
            if (_titulos.Any(t => Comparador.Equals(t, limpio))) return false;

            // Encontrar la posición de inserción (búsqueda binaria sobre la lista actual)
            int index = _titulos.BinarySearch(limpio, Comparador);
            if (index < 0) index = ~index; // Si no está, BinarySearch devuelve bitwise complement del índice de inserción
            _titulos.Insert(index, limpio);
            return true;
        }

        /// <summary>
        /// Búsqueda lineal (iterativa). Recorre la lista de principio a fin.
        /// Complejidad: O(n).
        /// </summary>
        public bool BusquedaLinealIterativa(string tituloBuscado)
        {
            string needle = Normalizar(tituloBuscado);
            foreach (var actual in _titulos)
            {
                if (Comparador.Equals(actual, needle))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Búsqueda binaria (recursiva) sobre la lista ordenada.
        /// Complejidad: O(log n).
        /// </summary>
        public bool BusquedaBinariaRecursiva(string tituloBuscado)
        {
            string needle = Normalizar(tituloBuscado);
            return BinariaRec(_titulos, needle, 0, _titulos.Count - 1);
        }

        private static bool BinariaRec(IList<string> datos, string needle, int izq, int der)
        {
            if (izq > der) return false; // Caso base: intervalo vacío

            int medio = izq + (der - izq) / 2;
            int cmp = Comparador.Compare(datos[medio], needle);

            if (cmp == 0) return true; // Encontrado
            if (cmp > 0)
                return BinariaRec(datos, needle, izq, medio - 1); // Buscar en el subarreglo izquierdo
            else
                return BinariaRec(datos, needle, medio + 1, der);  // Buscar en el subarreglo derecho
        }

        /// <summary>
        /// Normaliza entradas: quita espacios sobrantes y unifica a título tal cual (sin cambiar tildes),
        /// pero todas las comparaciones son case-insensitive mediante StringComparer.
        /// </summary>
        private static string Normalizar(string? s) => (s ?? string.Empty).Trim();
    }
}

