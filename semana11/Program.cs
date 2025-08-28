using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    // Diccionarios principales (claves normalizadas sin acentos, en minúsculas)
    static Dictionary<string, string> esToEn = new Dictionary<string, string>();
    static Dictionary<string, string> enToEs = new Dictionary<string, string>();

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        CargarPalabrasBase();

        while (true)
        {
            Console.WriteLine("\n==================== MENÚ ====================");
            Console.WriteLine("1. Traducir una frase");
            Console.WriteLine("2. Agregar palabras al diccionario");
            Console.WriteLine("0. Salir");
            Console.Write("Seleccione una opción: ");
            string op = Console.ReadLine()?.Trim();

            switch (op)
            {
                case "1":
                    MenuTraducir();
                    break;
                case "2":
                    MenuAgregar();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Opción no válida. Intente de nuevo.");
                    break;
            }
        }
    }

    static void MenuTraducir()
    {
        Console.WriteLine("\nSeleccione dirección de traducción:");
        Console.WriteLine("1. Español → Inglés");
        Console.WriteLine("2. Inglés → Español");
        Console.Write("Opción: ");
        string dir = Console.ReadLine()?.Trim();

        Console.Write("\nIngrese la frase: ");
        string frase = Console.ReadLine() ?? "";

        switch (dir)
        {
            case "1":
                Console.WriteLine("\nTraducción:");
                Console.WriteLine(TraducirFrase(frase, esToEn));
                break;
            case "2":
                Console.WriteLine("\nTraducción:");
                Console.WriteLine(TraducirFrase(frase, enToEs));
                break;
            default:
                Console.WriteLine("Dirección no válida.");
                break;
        }
    }

    static void MenuAgregar()
    {
        Console.WriteLine("\n¿En qué dirección quiere agregar?");
        Console.WriteLine("1. Español → Inglés");
        Console.WriteLine("2. Inglés → Español");
        Console.Write("Opción: ");
        string dir = Console.ReadLine()?.Trim();

        if (dir == "1")
        {
            Console.Write("Palabra en español: ");
            string es = (Console.ReadLine() ?? "").Trim();
            Console.Write("Traducción en inglés: ");
            string en = (Console.ReadLine() ?? "").Trim();
            if (AgregarPar(es, en)) Console.WriteLine("✅ Agregado.");
            else Console.WriteLine("⚠️ No se agregó (revise que no estén vacías).");
        }
        else if (dir == "2")
        {
            Console.Write("Word in English: ");
            string en = (Console.ReadLine() ?? "").Trim();
            Console.Write("Traducción en español: ");
            string es = (Console.ReadLine() ?? "").Trim();
            if (AgregarPar(es, en)) Console.WriteLine("✅ Agregado.");
            else Console.WriteLine("⚠️ No se agregó (revise que no estén vacías).");
        }
        else
        {
            Console.WriteLine("Dirección no válida.");
        }
    }

    /// <summary>
    /// Traduce una frase completa, sustituyendo solo tokens de palabra que existan en el diccionario.
    /// Respeta puntuación y formato (MAYÚSCULAS, Capitalización).
    /// </summary>
    static string TraducirFrase(string frase, Dictionary<string, string> dic)
    {
        if (string.IsNullOrWhiteSpace(frase)) return frase;

        // Partir en: palabras (letras), números o separadores/puntuación
        var partes = Regex.Matches(frase, @"\p{L}+|\p{N}+|[^\p{L}\p{N}\s]+|\s+");
        var sb = new StringBuilder();

        foreach (Match m in partes)
        {
            string token = m.Value;

            // ¿Es palabra (solo letras)?
            if (Regex.IsMatch(token, @"^\p{L}+$"))
            {
                string clave = Normalizar(token);
                if (dic.TryGetValue(clave, out string traduccion))
                {
                    sb.Append(AplicarCapitalizacion(token, traduccion));
                }
                else
                {
                    sb.Append(token); // no se conoce: se deja igual
                }
            }
            else
            {
                // números, espacios, puntuación, etc.
                sb.Append(token);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Agrega un par ES↔EN a ambos diccionarios (maneja normalización).
    /// </summary>
    static bool AgregarPar(string esp, string eng)
    {
        if (string.IsNullOrWhiteSpace(esp) || string.IsNullOrWhiteSpace(eng)) return false;

        string kES = Normalizar(esp);
        string kEN = Normalizar(eng);

        // Guardamos en ambos sentidos. El valor se almacena en minúsculas "base".
        esToEn[kES] = eng.ToLowerInvariant();
        enToEs[kEN] = esp.ToLowerInvariant();
        return true;
    }

    /// <summary>
    /// Aplica el patrón de capitalización del original a la traducción:
    /// - TODO MAYÚSCULAS
    /// - Solo Inicial (Title-like)
    /// - Todo minúsculas (por defecto)
    /// </summary>
    static string AplicarCapitalizacion(string original, string traducidaBase)
    {
        if (string.IsNullOrEmpty(traducidaBase)) return traducidaBase;

        bool esTodoMayus = original.ToUpperInvariant() == original;
        bool esTodoMinus = original.ToLowerInvariant() == original;

        if (esTodoMayus) return traducidaBase.ToUpperInvariant();
        if (esTodoMinus) return traducidaBase.ToLowerInvariant();

        // Capitalización tipo "Título" (primera en mayúscula, resto minúscula)
        var lower = traducidaBase.ToLowerInvariant();
        return char.ToUpper(lower[0]) + (lower.Length > 1 ? lower.Substring(1) : "");
    }

    /// <summary>
    /// Normaliza: minúsculas + sin diacríticos (acentos).
    /// </summary>
    static string Normalizar(string s)
    {
        if (s == null) return "";
        string lower = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var ch in lower)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    static void CargarPalabrasBase()
    {
        // Lista base sugerida (seleccionando una traducción por palabra en caso de múltiples)
        // También añadimos sinónimos en español cuando aplica (ej. "forma"→"way")
        AgregarPar("tiempo", "time");
        AgregarPar("persona", "person");
        AgregarPar("año", "year");
        AgregarPar("camino", "way");
        AgregarPar("forma", "way");
        AgregarPar("día", "day");
        AgregarPar("cosa", "thing");
        AgregarPar("hombre", "man");
        AgregarPar("mundo", "world");
        AgregarPar("vida", "life");
        AgregarPar("mano", "hand");
        AgregarPar("parte", "part");
        AgregarPar("niño", "child");
        AgregarPar("niña", "child");
        AgregarPar("ojo", "eye");
        AgregarPar("mujer", "woman");
        AgregarPar("lugar", "place");
        AgregarPar("trabajo", "work");
        AgregarPar("semana", "week");
        AgregarPar("caso", "case");
        AgregarPar("punto", "point");
        AgregarPar("tema", "point");
        AgregarPar("gobierno", "government");
        AgregarPar("empresa", "company");
        AgregarPar("compañía", "company");

        // Agregamos también las entradas EN→ES correspondientes automáticamente
        // (AgregarPar ya se encarga de ambos sentidos)
    }
}

