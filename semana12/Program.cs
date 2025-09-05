using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class TournamentRegistry
{
    private readonly HashSet<string> _equipos = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _jugadoresPorEquipo =
        new(StringComparer.OrdinalIgnoreCase);

    public bool AgregarEquipo(string equipo)
    {
        if (string.IsNullOrWhiteSpace(equipo)) return false;
        if (_equipos.Add(equipo))
        {
            _jugadoresPorEquipo[equipo] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return true;
        }
        return false;
    }

    public bool AgregarJugador(string equipo, string jugador)
    {
        if (!_equipos.Contains(equipo) || string.IsNullOrWhiteSpace(jugador)) return false;
        return _jugadoresPorEquipo[equipo].Add(jugador);
    }

    public bool EliminarJugador(string equipo, string jugador)
    {
        if (!_equipos.Contains(equipo)) return false;
        return _jugadoresPorEquipo[equipo].Remove(jugador);
    }

    public bool EliminarEquipo(string equipo)
    {
        if (!_equipos.Remove(equipo)) return false;
        return _jugadoresPorEquipo.Remove(equipo);
    }

    public IEnumerable<string> ListarEquipos() => _equipos.OrderBy(e => e);

    public IEnumerable<string> ListarJugadores(string equipo)
    {
        if (!_jugadoresPorEquipo.TryGetValue(equipo, out var set)) return Enumerable.Empty<string>();
        return set.OrderBy(j => j);
    }

    public (int totalEquipos, int totalJugadoresUnicos) Totales()
    {
        int equipos = _equipos.Count;
        int jugadores = _jugadoresPorEquipo.Values.Sum(s => s.Count);
        return (equipos, jugadores);
    }

    public IEnumerable<(string equipo, int cantidad)> RankingPorCantidad()
    {
        foreach (var kv in _jugadoresPorEquipo)
            yield return (kv.Key, kv.Value.Count);
    }

    public IEnumerable<string> BuscarJugadorGlobal(string nombreJugador)
    {
        foreach (var kv in _jugadoresPorEquipo)
            if (kv.Value.Contains(nombreJugador))
                yield return kv.Key;
    }
}

class Program
{
    static void Main()
    {
        var reg = new TournamentRegistry();

        // Datos de ejemplo
        foreach (var e in new[] { "Leones", "Tigres", "Águilas", "Pumas" })
        {
            if (reg.AgregarEquipo(e))
                Console.WriteLine($"Equipo agregado: {e}");
        }

        // Altas de jugadores
        Alta(reg, "Leones", "Juan Pérez");
        Alta(reg, "Leones", "Mario Ruiz");
        Alta(reg, "Leones", "Juan Pérez"); // duplicado en el mismo equipo (no se añade)
        Alta(reg, "Tigres", "Luis Soto");
        Alta(reg, "Tigres", "Carlos Lima");
        Alta(reg, "Águilas", "Ana Torres");
        Alta(reg, "Pumas", "Sofía Díaz");

        Console.WriteLine();
        Console.WriteLine("Equipos:");
        foreach (var e in reg.ListarEquipos())
            Console.WriteLine($" - {e}");

        Console.WriteLine();
        Console.WriteLine("Jugadores por equipo:");
        foreach (var e in reg.ListarEquipos())
        {
            var jugadores = string.Join(", ", reg.ListarJugadores(e));
            Console.WriteLine($" {e} -> [{jugadores}]");
        }

        Console.WriteLine();
        var (totEq, totJug) = reg.Totales();
        Console.WriteLine($"Total equipos: {totEq}");
        Console.WriteLine($"Total jugadores (únicos por equipo): {totJug}");

        Console.WriteLine();
        Console.WriteLine("Top por cantidad (desc):");
        foreach (var (equipo, cant) in reg.RankingPorCantidad().OrderByDescending(x => x.cantidad))
            Console.WriteLine($" {equipo} -> {cant}");

        Console.WriteLine();
        Console.WriteLine("Búsqueda global de 'Juan Pérez':");
        var donde = reg.BuscarJugadorGlobal("Juan Pérez").ToList();
        if (donde.Any())
            Console.WriteLine($" Se encontró en: {string.Join(", ", donde)}");
        else
            Console.WriteLine(" No se encontró.");

        // ---- Medición de tiempos (simulada) ----
        Console.WriteLine();
        Console.WriteLine("== Medición de tiempo ==");
        var rnd = new Random(123);
        var equipos = reg.ListarEquipos().ToArray();

        int N = 10_000;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < N; i++)
        {
            var equipo = equipos[rnd.Next(equipos.Length)];
            var jugador = $"Jugador_{i}";
            reg.AgregarJugador(equipo, jugador);
        }
        sw.Stop();
        Console.WriteLine($"Insertar {N:N0} jugadores tomó: {sw.Elapsed.TotalMilliseconds:F1} ms");

        sw.Restart();
        int encontrados = 0;
        for (int i = 0; i < N; i++)
        {
            var equipo = equipos[rnd.Next(equipos.Length)];
            var jugador = $"Jugador_{rnd.Next(N)}";
            if (reg.ListarJugadores(equipo).Contains(jugador))
                encontrados++;
        }
        sw.Stop();
        Console.WriteLine($"Consultar {N:N0} jugadores tomó: {sw.Elapsed.TotalMilliseconds:F1} ms");
        Console.WriteLine($"Coincidencias encontradas: {encontrados}");
    }

    static void Alta(TournamentRegistry reg, string equipo, string jugador)
    {
        if (!reg.AgregarJugador(equipo, jugador))
            Console.WriteLine($"El jugador '{jugador}' ya existe en {equipo} (no duplicado).");
        else
            Console.WriteLine($"Jugador agregado: {jugador} -> {equipo}");
    }
}

