using Faturamento.Api.Aplicacao;
using Faturamento.Api.Dominio;
using Faturamento.Api.Infraestrutura;
using Microsoft.EntityFrameworkCore;

// Serviço de Faturamento da LogiTech Enterprise.
// C# / .NET 8, porta 5080, rotas conforme o contrato da plataforma (ADR-006).

var builder = WebApplication.CreateBuilder(args);

string porta = Environment.GetEnvironmentVariable("LOGITECH_FATURAMENTO_PORT") ?? "5080";
builder.WebHost.UseUrls($"http://0.0.0.0:{porta}");

string urlBanco = Environment.GetEnvironmentVariable("LOGITECH_DB_URL")
                  ?? "jdbc:postgresql://localhost:5432/logitech";
string usuario = Environment.GetEnvironmentVariable("LOGITECH_DB_USER") ?? "logitech";
string senha = Environment.GetEnvironmentVariable("LOGITECH_DB_PASSWORD") ?? "logitech";

builder.Services.AddDbContext<FaturamentoDbContext>(opcoes =>
    opcoes.UseNpgsql(ConexaoPostgres.Traduzir(urlBanco, usuario, senha)));

builder.Services.AddSingleton(new RegistroAuditoria(Console.Out));
builder.Services.AddScoped<IFaturaRepository, EfFaturaRepository>();
builder.Services.AddScoped<FaturaService>();

var app = builder.Build();

PrepararBanco(app);

app.MapGet("/health", () => Results.Ok(new { status = "ok", servico = "faturamento" }));

app.MapPost("/api/v1/faturas", (SolicitacaoFatura solicitacao, FaturaService servico) =>
{
    try
    {
        Fatura fatura = servico.Emitir(solicitacao);
        return Results.Created($"/api/v1/faturas/{fatura.PedidoId}", FaturaResposta.De(fatura));
    }
    catch (ArgumentException erro)
    {
        return Results.BadRequest(new { erro = erro.Message });
    }
});

app.MapGet("/api/v1/faturas/{pedidoId}", (string pedidoId, FaturaService servico) =>
{
    Fatura? fatura = servico.PorPedido(pedidoId);
    return fatura is null
        ? Results.NotFound(new { erro = $"não há fatura emitida para o pedido {pedidoId}" })
        : Results.Ok(FaturaResposta.De(fatura));
});

app.MapGet("/api/v1/faturas", (FaturaService servico) =>
    Results.Ok(servico.Todas().Select(FaturaResposta.De)));

app.MapGet("/api/v1/auditoria", (RegistroAuditoria auditoria) =>
    Results.Ok(auditoria.Recentes()));

app.Run();

// Cria o schema na primeira subida e posiciona a numeração no último número
// gravado. O banco pode ainda não aceitar conexão quando o serviço sobe, por
// isso a tentativa é repetida antes de desistir.
static void PrepararBanco(WebApplication app)
{
    const int tentativas = 10;
    for (int tentativa = 1; tentativa <= tentativas; tentativa++)
    {
        try
        {
            using var escopo = app.Services.CreateScope();
            var banco = escopo.ServiceProvider.GetRequiredService<FaturamentoDbContext>();
            banco.Database.EnsureCreated();

            List<string> emitidas = banco.Faturas.Select(f => f.NumeroNotaFiscal).ToList();
            int ultimo = emitidas.Count == 0 ? 0 : emitidas.Max(NumeradorNotaFiscal.Sequencial);
            NumeradorNotaFiscal.Instancia.IniciarEm(ultimo);

            Console.WriteLine($"[faturamento] schema pronto, numeração continua a partir de {ultimo}");
            return;
        }
        catch (Exception erro) when (tentativa < tentativas)
        {
            Console.WriteLine($"[faturamento] banco ainda não respondeu " +
                              $"(tentativa {tentativa} de {tentativas}): {erro.Message}");
            Thread.Sleep(2000);
        }
    }

    throw new InvalidOperationException(
        "não foi possível preparar o banco. Confira se o PostgreSQL está de pé e se " +
        "LOGITECH_DB_URL, LOGITECH_DB_USER e LOGITECH_DB_PASSWORD apontam para ele.");
}

/// <summary>Representação da fatura que sai pela API.</summary>
public record FaturaResposta(string PedidoId, string NumeroNotaFiscal, decimal Valor,
                             decimal ValorIss, string MeioPagamento, int PrazoDias,
                             DateTimeOffset EmitidaEm, DateTimeOffset Vencimento)
{
    public static FaturaResposta De(Fatura fatura) => new(
        fatura.PedidoId,
        fatura.NumeroNotaFiscal,
        fatura.Valor,
        fatura.ValorIss,
        fatura.MeioPagamento,
        fatura.PrazoDias,
        fatura.EmitidaEm,
        fatura.Vencimento());
}
