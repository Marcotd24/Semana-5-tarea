// Proyecto: GraficaDeGrafos (render con SkiaSharp - multiplataforma)
// Plataforma: .NET 8 (Console)
// Requisitos:
//   dotnet SDK 8+
//   Paquete NuGet: SkiaSharp (sin dependencias de System.Drawing)
// Ejecución rápida:
//   dotnet new console -n GraficaDeGrafos
//   Sustituye el Program.cs por este archivo
//   dotnet add package SkiaSharp --version 2.88.6
//   dotnet run
// Salidas:
//   ./salidas/grafo1.png
//   ./salidas/grafo2.png
//   Consola: reportería (nodos, aristas, grados, componentes, etc.) y tiempos de ejecución

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using SkiaSharp;

// ==========================
// MODELO DE DATOS
// ==========================
class Graph
{
    public bool Directed { get; }
    private readonly Dictionary<string, HashSet<string>> _adj = new(StringComparer.OrdinalIgnoreCase);

    public Graph(bool directed) => Directed = directed;

    public IEnumerable<string> Nodes => _adj.Keys;

    public int NodeCount => _adj.Count;

    public int EdgeCount
    {
        get
        {
            var sum = _adj.Sum(kv => kv.Value.Count);
            return Directed ? sum : sum / 2; // en no dirigidos contamos cada arista dos veces
        }
    }

    public void AddNode(string v)
    {
        if (!_adj.ContainsKey(v)) _adj[v] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public void AddEdge(string u, string v)
    {
        AddNode(u);
        AddNode(v);
        _adj[u].Add(v);
        if (!Directed) _adj[v].Add(u);
    }

    public IEnumerable<(string u, string v)> Edges()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (u, neighs) in _adj)
        {
            foreach (var v in neighs)
            {
                if (Directed)
                {
                    yield return (u, v);
                }
                else
                {
                    var key = string.Compare(u, v, true, CultureInfo.InvariantCulture) < 0 ? $"{u}|{v}" : $"{v}|{u}";
                    if (seen.Add(key)) yield return (u, v);
                }
            }
        }
    }

    public int Degree(string v) => Directed ? InDegree(v) + OutDegree(v) : _adj.TryGetValue(v, out var s) ? s.Count : 0;
    public int OutDegree(string v) => _adj.TryGetValue(v, out var s) ? s.Count : 0;
    public int InDegree(string v) => Directed ? _adj.Sum(kv => kv.Value.Contains(v) ? 1 : 0) : Degree(v);

    public Dictionary<string, List<string>> AdjacencyList()
        => _adj.ToDictionary(kv => kv.Key, kv => kv.Value.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);

    // Componentes conexas (no dirigido)
    public List<List<string>> ConnectedComponents()
    {
        var comps = new List<List<string>>();
        if (Directed) return comps; // opcionalmente se podrían calcular SCC
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var v in Nodes)
        {
            if (visited.Contains(v)) continue;
            var comp = new List<string>();
            var stack = new Stack<string>();
            stack.Push(v); visited.Add(v);
            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                comp.Add(cur);
                foreach (var w in _adj[cur])
                    if (!visited.Contains(w)) { visited.Add(w); stack.Push(w); }
            }
            comps.Add(comp.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList());
        }
        return comps;
    }
}

// ==========================
// PARSER DESDE BLOQUE DE NOTAS
// ==========================
static class GraphParser
{
    /* Formatos aceptados por línea:
       - No dirigido:  A B   (espacio/tablación/coma). Ej:  A B,  C D,  u v
       - Dirigido:     A->B  (con flecha)
       - Comentarios:  # ...
       - Nodos aislados: Z
    */
    public static Graph Parse(string text)
    {
        var lines = text.Replace("\r", "").Split('\n');
        bool? directed = null;
        var edges = new List<(string u, string v, bool isEdge)>();
        var nodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;

            if (line.Contains("->"))
            {
                var parts = line.Split("->", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 2)
                {
                    edges.Add((parts[0], parts[1], true));
                    nodes.Add(parts[0]); nodes.Add(parts[1]);
                    directed ??= true;
                }
                continue;
            }

            var tokens = line.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 1)
            {
                nodes.Add(tokens[0]);
                edges.Add((tokens[0], tokens[0], false));
            }
            else if (tokens.Length >= 2)
            {
                edges.Add((tokens[0], tokens[1], true));
                nodes.Add(tokens[0]); nodes.Add(tokens[1]);
                directed ??= false;
            }
        }

        var g = new Graph(directed ?? false);
        foreach (var n in nodes) g.AddNode(n);
        foreach (var (u, v, isEdge) in edges)
            if (isEdge && !(u == v && !g.Directed)) g.AddEdge(u, v);
        return g;
    }
}

// ==========================
// RENDERIZACIÓN A PNG con SkiaSharp (layout circular)
// ==========================
static class GraphRenderer
{
    public static void SavePng(Graph g, string path, int width = 1000, int height = 800)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var info = new SKImageInfo(width, height);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        float nodeRadius = 22f;
        var center = new SKPoint(width / 2f, height / 2f);
        float R = Math.Min(width, height) * 0.36f;

        var nodes = g.Nodes.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
        int n = Math.Max(1, nodes.Count);

        var pos = new Dictionary<string, SKPoint>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < n; i++)
        {
            var angle = (float)(2 * Math.PI * i / n - Math.PI / 2);
            pos[nodes[i]] = new SKPoint(
                center.X + (float)(R * Math.Cos(angle)),
                center.Y + (float)(R * Math.Sin(angle))
            );
        }

        using var edgePaint = new SKPaint { IsAntialias = true, StrokeWidth = 2f, Color = new SKColor(0, 0, 0, 120), Style = SKPaintStyle.Stroke };
        using var nodeFill = new SKPaint { IsAntialias = true, Color = new SKColor(245, 245, 255) };
        using var nodeStroke = new SKPaint { IsAntialias = true, Color = new SKColor(0, 0, 160, 80), Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
        using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.Black, TextSize = 14, Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold) };
        using var footPaint = new SKPaint { IsAntialias = true, Color = SKColors.Black, TextSize = 12 };

        // Aristas
        foreach (var (u, v) in g.Edges())
        {
            var pu = pos[u];
            var pv = pos[v];
            var vec = new SKPoint(pv.X - pu.X, pv.Y - pu.Y);
            var len = MathF.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
            if (len < 1e-3) continue;
            var ux = vec.X / len; var uy = vec.Y / len;
            var start = new SKPoint(pu.X + ux * nodeRadius, pu.Y + uy * nodeRadius);
            var end = new SKPoint(pv.X - ux * nodeRadius, pv.Y - uy * nodeRadius);

            canvas.DrawLine(start, end, edgePaint);

            if (g.Directed)
            {
                // flecha simple en el extremo
                float ah = 8f; // tamaño flecha
                var left = new SKPoint(
                    end.X - ux * ah - uy * ah * 0.6f,
                    end.Y - uy * ah + ux * ah * 0.6f);
                var right = new SKPoint(
                    end.X - ux * ah + uy * ah * 0.6f,
                    end.Y - uy * ah - ux * ah * 0.6f);
                using var arrowPaint = new SKPaint { IsAntialias = true, Color = new SKColor(0, 0, 0, 180), Style = SKPaintStyle.Fill };
                using var arrowPath = new SKPath();
arrowPath.MoveTo(end);
arrowPath.LineTo(left);
arrowPath.LineTo(right);
arrowPath.Close();
canvas.DrawPath(arrowPath, arrowPaint);

            }
        }

        // Nodos
        foreach (var v in nodes)
        {
            var p = pos[v];
            canvas.DrawCircle(p, nodeRadius, nodeFill);
            canvas.DrawCircle(p, nodeRadius, nodeStroke);

            var tw = textPaint.MeasureText(v);
            var tx = p.X - tw / 2f;
            var ty = p.Y + textPaint.TextSize * 0.35f; // centrado vertical aproximado
            canvas.DrawText(v, tx, ty, textPaint);
        }

        // Pie
        var subtitle = g.Directed ? "Grafo dirigido (layout circular)" : "Grafo no dirigido (layout circular)";
        canvas.DrawText(subtitle, 10, height - 20, footPaint);

        using var img = surface.Snapshot();
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        data.SaveTo(fs);
    }
}

// ==========================
// REPORTERÍA Y UTILIDADES
// ==========================
static class Reporter
{
    public static void PrintSummary(Graph g, string titulo)
    {
        Console.WriteLine(new string('=', 70));
        Console.WriteLine($"{titulo}");
        Console.WriteLine(new string('=', 70));
        Console.WriteLine($"Dirigido: {g.Directed}");
        Console.WriteLine($"Nodos:    {g.NodeCount}");
        Console.WriteLine($"Aristas:  {g.EdgeCount}");
        Console.WriteLine($"Densidad (aprox.): {Density(g):F4}");

        Console.WriteLine("\nGrados (nodo: grado | in/out si aplica):");
        foreach (var v in g.Nodes.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            if (g.Directed)
                Console.WriteLine($"  {v}: total={g.Degree(v)}, in={g.InDegree(v)}, out={g.OutDegree(v)}");
            else
                Console.WriteLine($"  {v}: {g.Degree(v)}");
        }

        Console.WriteLine("\nLista de adyacencia:");
        foreach (var (v, list) in g.AdjacencyList().OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
            Console.WriteLine($"  {v} -> {string.Join(", ", list)}");

        if (!g.Directed)
        {
            var comps = g.ConnectedComponents();
            Console.WriteLine($"\nComponentes conexas: {comps.Count}");
            for (int i = 0; i < comps.Count; i++)
                Console.WriteLine($"  C{i + 1}: {{ {string.Join(", ", comps[i])} }}");
        }
        else
        {
            Console.WriteLine("\n(Nota) Grafo dirigido: puede calcular SCC si se requiere (no implementado por brevedad).");
        }
        Console.WriteLine();
    }

    private static double Density(Graph g)
    {
        var n = g.NodeCount;
        if (n <= 1) return 0;
        var m = g.EdgeCount;
        var denom = g.Directed ? n * (n - 1) : n * (n - 1) / 2.0;
        return m / denom;
    }
}

// ==========================
// PROGRAMA PRINCIPAL
// ==========================
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Directory.CreateDirectory("salidas");

        // Ejemplo 1 (NO dirigido) – reemplazable por grafo1.txt
        string ejemplo1 = @"
# Grafo no dirigido simple (red de amistad)
A B
A C
B C
C D
E
".Trim();

        // Ejemplo 2 (DIRIGIDO) – reemplazable por grafo2.txt
        string ejemplo2 = @"
# Grafo dirigido (flujo de tareas)
Start->A
A->B
A->C
B->D
C->D
D->End
".Trim();

        if (File.Exists("grafo1.txt")) ejemplo1 = File.ReadAllText("grafo1.txt");
        if (File.Exists("grafo2.txt")) ejemplo2 = File.ReadAllText("grafo2.txt");

        var swTotal = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();
        var g1 = GraphParser.Parse(ejemplo1);
        sw.Stop();
        var tParse1 = sw.ElapsedMilliseconds;

        sw.Restart();
        Reporter.PrintSummary(g1, "Resumen del Grafo 1 (No dirigido)");
        sw.Stop();
        var tReport1 = sw.ElapsedMilliseconds;

        sw.Restart();
        GraphRenderer.SavePng(g1, Path.Combine("salidas", "grafo1.png"));
        sw.Stop();
        var tRender1 = sw.ElapsedMilliseconds;

        Console.WriteLine($"Tiempos Grafo 1 (ms): parse={tParse1}, reportería={tReport1}, render={tRender1}\n");

        sw.Restart();
        var g2 = GraphParser.Parse(ejemplo2);
        sw.Stop();
        var tParse2 = sw.ElapsedMilliseconds;

        sw.Restart();
        Reporter.PrintSummary(g2, "Resumen del Grafo 2 (Dirigido)");
        sw.Stop();
        var tReport2 = sw.ElapsedMilliseconds;

        sw.Restart();
        GraphRenderer.SavePng(g2, Path.Combine("salidas", "grafo2.png"));
        sw.Stop();
        var tRender2 = sw.ElapsedMilliseconds;

        swTotal.Stop();
        Console.WriteLine($"Tiempos Grafo 2 (ms): parse={tParse2}, reportería={tReport2}, render={tRender2}");
        Console.WriteLine($"Tiempo total del programa: {swTotal.ElapsedMilliseconds} ms\n");

        // ========== Sección para el informe ==========
        Console.WriteLine("=== Para el Informe ===");
        Console.WriteLine("Estructura de datos utilizada: Lista de adyacencia (Dictionary<string, HashSet<string>>).\n" +
                          "Ventajas: Inserción y consulta de vecinos O(1) promedio; memoria eficiente para grafos dispersos.\n" +
                          "Desventajas: Determinar existencia de arista no incidente requiere buscar en la lista del nodo;\n" +
                          "            para grafos muy densos, una matriz de adyacencia puede ser más eficiente.");

        Console.WriteLine("\nAgente de IA utilizado: ChatGPT (GPT-5).\n" +
                          "Descripción del uso: Se apoyó mínimamente en la generación de ideas y estructura general.\n" +
                          "Porcentaje estimado de código generado con IA: 5%.\n" +
                          "El estudiante desarrolló, ejecutó, probó y documentó la mayor parte del proyecto.");

        Console.WriteLine("\nCómo generar las capturas de pantalla: \n" +
                          "  1) Ejecuta el programa y captura la consola con la reportería.\n" +
                          "  2) Abre salidas/grafo1.png y salidas/grafo2.png y toma capturas de las imágenes.\n" +
                          "  3) Sube todo a GitHub con README (ver instrucciones en cabecera del archivo).");

        
    }
}


