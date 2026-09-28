using Faturamento.Api.Dominio;

namespace Faturamento.Api.Aplicacao;

/// <summary>
/// Trilha de auditoria das faturas: escreve uma linha por evento na saída do
/// serviço e mantém os eventos mais recentes para <c>GET /api/v1/auditoria</c>.
/// </summary>
/// <remarks>
/// Registrado como <c>AddSingleton</c>: uma trilha por processo, alimentada por
/// todas as requisições.
/// </remarks>
public sealed class RegistroAuditoria
{
    private const int Capacidade = 500;

    private readonly object _trava = new();
    private readonly Queue<EventoAuditoria> _recentes = new();
    private readonly TextWriter _saida;
    private readonly string _host = Environment.MachineName;
    private readonly int _pid = Environment.ProcessId;

    public RegistroAuditoria(TextWriter saida)
    {
        _saida = saida;
    }

    public void Registrar(string acao, Fatura fatura)
    {
        var evento = new EventoAuditoria(
            DateTimeOffset.UtcNow,
            Environment.CurrentManagedThreadId,
            acao,
            fatura.PedidoId,
            fatura.NumeroNotaFiscal,
            fatura.Valor);

        lock (_trava)
        {
            _recentes.Enqueue(evento);
            if (_recentes.Count > Capacidade)
            {
                _recentes.Dequeue();
            }

            _saida.WriteLine(evento.ComoLinha(_host, _pid));
        }
    }

    public IReadOnlyList<EventoAuditoria> Recentes()
    {
        lock (_trava)
        {
            return _recentes.ToList();
        }
    }
}

public record EventoAuditoria(DateTimeOffset Em, int Thread, string Acao, string PedidoId,
                              string NumeroNotaFiscal, decimal Valor)
{
    public string ComoLinha(string host, int pid) =>
        $"{Em:yyyy-MM-ddTHH:mm:ss.fffZ} INFO faturamento host={host} pid={pid} t={Thread} " +
        $"{Acao} pedido={PedidoId} nf={NumeroNotaFiscal} valor={Valor:F2}";
}
