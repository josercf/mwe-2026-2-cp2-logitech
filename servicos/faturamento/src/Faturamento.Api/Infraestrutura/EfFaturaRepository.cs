using Faturamento.Api.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.Api.Infraestrutura;

/// <summary>
/// Persistência de faturas em PostgreSQL, via EF Core.
/// </summary>
public class EfFaturaRepository : IFaturaRepository
{
    private readonly FaturamentoDbContext _banco;

    public EfFaturaRepository(FaturamentoDbContext banco)
    {
        _banco = banco;
    }

    public Fatura Salvar(Fatura fatura)
    {
        _banco.Faturas.Add(fatura);
        _banco.SaveChanges();
        return fatura;
    }

    public Fatura? PorPedido(string pedidoId)
    {
        return _banco.Faturas.AsNoTracking().FirstOrDefault(f => f.PedidoId == pedidoId);
    }

    public IReadOnlyList<Fatura> Todas()
    {
        return _banco.Faturas.AsNoTracking().OrderBy(f => f.EmitidaEm).ToList();
    }
}
