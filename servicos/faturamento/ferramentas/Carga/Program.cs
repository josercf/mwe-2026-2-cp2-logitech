using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Carga;
using Faturamento.Api.Aplicacao;
using Faturamento.Api.Dominio;

// Dispara emissões de fatura contra o FaturaService, com repositório em memória,
// e confere a numeração das notas emitidas.
//
// Uso: dotnet run --project ferramentas/Carga -- [--emissoes N] [--trabalhadores W]
//                                                [--inicio ULTIMO] [--sequencial]
//                                                [--log arquivo]
//
// --inicio faz o papel da subida do serviço: posiciona a numeração no último
// sequencial gravado, como Program.cs faz a partir do banco.

int emissoes = LerInteiro(args, "--emissoes", 1000);
int trabalhadores = args.Contains("--sequencial") ? 1 : LerInteiro(args, "--trabalhadores", 16);
string caminhoLog = LerTexto(args, "--log", "carga-auditoria.log");
int inicio = LerInteiro(args, "--inicio", 0);

NumeradorNotaFiscal.Instancia.IniciarEm(inicio);
Aquecer();

using var log = new StreamWriter(caminhoLog) { AutoFlush = false };
var auditoria = new RegistroAuditoria(log);
var repositorio = new RepositorioEmMemoria();
var emitidas = new ConcurrentBag<(string Pedido, string Nota)>();
var instancias = new ConcurrentDictionary<int, byte>();
using var largada = new Barrier(trabalhadores);

var threads = Enumerable.Range(0, trabalhadores).Select(indice => new Thread(() =>
{
    largada.SignalAndWait();
    instancias.TryAdd(RuntimeHelpers.GetHashCode(NumeradorNotaFiscal.Instancia), 0);

    var servico = new FaturaService(repositorio, auditoria);
    for (int i = indice; i < emissoes; i += trabalhadores)
    {
        string pedido = $"PED-{i + 1:D5}";
        decimal valor = 150m + (i * 37 % 4850);
        Fatura fatura = servico.Emitir(new SolicitacaoFatura(pedido, "Distribuidora Sul", valor, "BOLETO", 3));
        emitidas.Add((pedido, fatura.NumeroNotaFiscal));
    }
})).ToList();

var relogio = System.Diagnostics.Stopwatch.StartNew();
threads.ForEach(t => t.Start());
threads.ForEach(t => t.Join());
relogio.Stop();
log.Flush();

var porNota = emitidas.GroupBy(e => e.Nota).ToList();
var repetidas = porNota.Where(g => g.Count() > 1).OrderBy(g => g.Key).ToList();
int faturasComNotaRepetida = repetidas.Sum(g => g.Count());

Console.WriteLine("carga do faturamento");
Console.WriteLine($"modo: {(trabalhadores == 1 ? "sequencial" : "concorrente")}");
Console.WriteLine($"emissões solicitadas: {emissoes}");
Console.WriteLine($"trabalhadores: {trabalhadores}");
Console.WriteLine($"numeração iniciada em: {inicio}");
Console.WriteLine($"processador lógico disponível: {Environment.ProcessorCount}");
Console.WriteLine($"tempo: {relogio.ElapsedMilliseconds} ms");
Console.WriteLine($"instâncias de NumeradorNotaFiscal observadas: {instancias.Count}");
Console.WriteLine($"faturas emitidas: {emitidas.Count}");
Console.WriteLine($"pedidos distintos: {emitidas.Select(e => e.Pedido).Distinct().Count()}");
Console.WriteLine($"números de nota distintos: {porNota.Count}");
Console.WriteLine($"último sequencial do numerador: {NumeradorNotaFiscal.Instancia.Ultimo}");
Console.WriteLine($"números de nota repetidos: {repetidas.Count}");
Console.WriteLine($"faturas com número repetido: {faturasComNotaRepetida}");

foreach (var grupo in repetidas.Take(10))
{
    Console.WriteLine($"  {grupo.Key}: {string.Join(", ", grupo.Select(e => e.Pedido).OrderBy(p => p))}");
}
if (repetidas.Count > 10)
{
    Console.WriteLine($"  ... mais {repetidas.Count - 10}");
}

Console.WriteLine($"trilha de auditoria: {caminhoLog}");

// Executa uma vez cada caminho de código antes da largada, para a primeira
// rodada de emissões não medir a compilação do JIT.
static void Aquecer()
{
    _ = NumeradorNotaFiscal.Formatar(0);
    _ = CatalogoAliquotas.Instancia.IssPara("16.01");
    _ = new Fatura("aquecimento", "aquecimento", 1m, "BOLETO", 0, NumeradorNotaFiscal.Formatar(0), 0m);
    new RegistroAuditoria(TextWriter.Null).Registrar("aquecimento",
        new Fatura("aquecimento", "aquecimento", 1m, "BOLETO", 0, NumeradorNotaFiscal.Formatar(0), 0m));
}

static int LerInteiro(string[] args, string nome, int padrao)
{
    int i = Array.IndexOf(args, nome);
    return i >= 0 && i + 1 < args.Length && int.TryParse(args[i + 1], out int valor) ? valor : padrao;
}

static string LerTexto(string[] args, string nome, string padrao)
{
    int i = Array.IndexOf(args, nome);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : padrao;
}
