using System.Collections.Concurrent;
using Faturamento.Api.Dominio;

namespace Carga;

internal sealed class RepositorioEmMemoria : IFaturaRepository
{
    private readonly ConcurrentDictionary<string, Fatura> _dados = new();

    public Fatura Salvar(Fatura fatura)
    {
        _dados[fatura.PedidoId] = fatura;
        return fatura;
    }

    public Fatura? PorPedido(string pedidoId)
    {
        return _dados.TryGetValue(pedidoId, out Fatura? fatura) ? fatura : null;
    }

    public IReadOnlyList<Fatura> Todas()
    {
        return _dados.Values.OrderBy(f => f.EmitidaEm).ToList();
    }
}
