using System.Text.RegularExpressions;
using Faturamento.Api.Aplicacao;
using Faturamento.Api.Dominio;
using Xunit;

namespace Faturamento.Tests;

public class FaturaServiceTests
{
    private static SolicitacaoFatura Solicitacao(string pedidoId = "pedido-1",
                                                 string meio = "BOLETO",
                                                 int prazo = 3,
                                                 decimal valor = 1000.00m)
    {
        return new SolicitacaoFatura(pedidoId, "Distribuidora Sul", valor, meio, prazo);
    }

    private static FaturaService ServicoNovo()
    {
        NumeradorNotaFiscal.Instancia.IniciarEm(0);
        return new FaturaService(new FaturaRepositorioEmMemoria(),
                                 new RegistroAuditoria(TextWriter.Null));
    }

    [Fact]
    public void EmitirGeraNotaFiscalNoFormatoDoContrato()
    {
        FaturaService servico = ServicoNovo();

        Fatura fatura = servico.Emitir(Solicitacao());

        Assert.Matches(new Regex(@"^NF-\d{6}$"), fatura.NumeroNotaFiscal);
        Assert.Equal("pedido-1", fatura.PedidoId);
        Assert.Equal(1000.00m, fatura.Valor);
    }

    [Fact]
    public void EmitirParaPedidosDiferentesGeraNotasDiferentes()
    {
        FaturaService servico = ServicoNovo();

        Fatura primeira = servico.Emitir(Solicitacao("pedido-1"));
        Fatura segunda = servico.Emitir(Solicitacao("pedido-2"));
        Fatura terceira = servico.Emitir(Solicitacao("pedido-3"));

        Assert.Equal(new[] { "NF-000001", "NF-000002", "NF-000003" },
                     new[] { primeira.NumeroNotaFiscal, segunda.NumeroNotaFiscal, terceira.NumeroNotaFiscal });
    }

    [Fact]
    public void EmitirDuasVezesParaOMesmoPedidoNaoGeraDuasNotas()
    {
        FaturaService servico = ServicoNovo();

        Fatura primeira = servico.Emitir(Solicitacao());
        Fatura segunda = servico.Emitir(Solicitacao());

        Assert.Equal(primeira.NumeroNotaFiscal, segunda.NumeroNotaFiscal);
        Assert.Single(servico.Todas());
    }

    [Fact]
    public void PrazoDoClienteViraVencimentoDaFatura()
    {
        FaturaService servico = ServicoNovo();

        Fatura fatura = servico.Emitir(Solicitacao(prazo: 30, meio: "FATURA_MENSAL"));

        Assert.Equal("FATURA_MENSAL", fatura.MeioPagamento);
        Assert.Equal(fatura.EmitidaEm.AddDays(30), fatura.Vencimento());
    }

    [Fact]
    public void IssDoTransporteMunicipalEntraNaFatura()
    {
        FaturaService servico = ServicoNovo();

        Fatura fatura = servico.Emitir(Solicitacao(valor: 1234.50m));

        Assert.Equal(61.73m, fatura.ValorIss);
    }

    [Fact]
    public void ValorNaoPositivoEhRecusadoPeloDominio()
    {
        FaturaService servico = ServicoNovo();

        Assert.Throws<ArgumentException>(() => servico.Emitir(Solicitacao("pedido-2", valor: 0m)));
    }

    [Fact]
    public void EmissaoFicaRegistradaNaAuditoria()
    {
        NumeradorNotaFiscal.Instancia.IniciarEm(0);
        var auditoria = new RegistroAuditoria(TextWriter.Null);
        var servico = new FaturaService(new FaturaRepositorioEmMemoria(), auditoria);

        servico.Emitir(Solicitacao("pedido-9"));

        EventoAuditoria evento = Assert.Single(auditoria.Recentes());
        Assert.Equal("fatura emitida", evento.Acao);
        Assert.Equal("NF-000001", evento.NumeroNotaFiscal);
    }
}
