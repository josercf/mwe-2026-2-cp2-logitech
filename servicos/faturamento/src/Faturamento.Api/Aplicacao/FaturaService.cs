using Faturamento.Api.Dominio;

namespace Faturamento.Api.Aplicacao;

/// <summary>
/// Regra de negócio do Bounded Context de Faturamento.
/// </summary>
/// <remarks>
/// Emitir fatura é idempotente por pedido: o serviço de Pedidos pode repetir a
/// chamada depois de um timeout, e a repetição devolve a fatura já emitida.
/// </remarks>
public class FaturaService
{
    private const string CodigoServicoTransporte = "16.01";

    private readonly IFaturaRepository _repositorio;
    private readonly RegistroAuditoria _auditoria;

    public FaturaService(IFaturaRepository repositorio, RegistroAuditoria auditoria)
    {
        _repositorio = repositorio;
        _auditoria = auditoria;
    }

    public Fatura Emitir(SolicitacaoFatura solicitacao)
    {
        Fatura? jaEmitida = _repositorio.PorPedido(solicitacao.PedidoId);
        if (jaEmitida is not null)
        {
            _auditoria.Registrar("fatura reapresentada", jaEmitida);
            return jaEmitida;
        }

        decimal aliquotaIss = CatalogoAliquotas.Instancia.IssPara(CodigoServicoTransporte);
        string numero = NumeradorNotaFiscal.Instancia.Proximo();

        var fatura = new Fatura(
            solicitacao.PedidoId,
            solicitacao.Cliente,
            solicitacao.Valor,
            solicitacao.MeioPagamento,
            solicitacao.PrazoDias,
            numero,
            aliquotaIss);

        Fatura salva = _repositorio.Salvar(fatura);
        _auditoria.Registrar("fatura emitida", salva);
        return salva;
    }

    public Fatura? PorPedido(string pedidoId) => _repositorio.PorPedido(pedidoId);

    public IReadOnlyList<Fatura> Todas() => _repositorio.Todas();
}
