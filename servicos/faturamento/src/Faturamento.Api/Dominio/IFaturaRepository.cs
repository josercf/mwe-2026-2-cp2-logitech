namespace Faturamento.Api.Dominio;

/// <summary>
/// Coleção de faturas emitidas, do ponto de vista do domínio.
/// </summary>
public interface IFaturaRepository
{
    Fatura Salvar(Fatura fatura);

    Fatura? PorPedido(string pedidoId);

    IReadOnlyList<Fatura> Todas();
}
