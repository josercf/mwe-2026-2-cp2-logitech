using Faturamento.Api.Dominio;
using Xunit;

namespace Faturamento.Tests;

public class NumeradorNotaFiscalTests
{
    [Fact]
    public void ChamadasSequenciaisDevolvemNumerosCrescentes()
    {
        NumeradorNotaFiscal numerador = NumeradorNotaFiscal.Instancia;
        numerador.IniciarEm(0);

        string primeiro = numerador.Proximo();
        string segundo = numerador.Proximo();

        Assert.Equal("NF-000001", primeiro);
        Assert.Equal("NF-000002", segundo);
    }

    [Fact]
    public void MilChamadasSequenciaisNaoRepetemNumero()
    {
        NumeradorNotaFiscal numerador = NumeradorNotaFiscal.Instancia;
        numerador.IniciarEm(0);

        List<string> numeros = Enumerable.Range(0, 1000).Select(_ => numerador.Proximo()).ToList();

        Assert.Equal(1000, numeros.Distinct().Count());
        Assert.Equal(1000, numerador.Ultimo);
    }

    [Fact]
    public void FormatoDoNumeroSegueOContratoDaPlataforma()
    {
        NumeradorNotaFiscal numerador = NumeradorNotaFiscal.Instancia;
        numerador.IniciarEm(0);

        string numero = numerador.Proximo();

        Assert.StartsWith("NF-", numero);
        Assert.Equal(9, numero.Length);
    }

    [Fact]
    public void NumeracaoContinuaDoUltimoGravadoNoBanco()
    {
        NumeradorNotaFiscal numerador = NumeradorNotaFiscal.Instancia;
        numerador.IniciarEm(NumeradorNotaFiscal.Sequencial("NF-004213"));

        Assert.Equal("NF-004214", numerador.Proximo());
    }

    [Fact]
    public void SequencialForaDoFormatoViraZero()
    {
        Assert.Equal(0, NumeradorNotaFiscal.Sequencial("000042"));
        Assert.Equal(0, NumeradorNotaFiscal.Sequencial("NF-abc"));
    }

    [Fact]
    public async Task AcessosConcorrentesRecebemAMesmaInstancia()
    {
        Task<NumeradorNotaFiscal>[] acessos = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => NumeradorNotaFiscal.Instancia))
            .ToArray();

        NumeradorNotaFiscal[] instancias = await Task.WhenAll(acessos);

        Assert.All(instancias, instancia => Assert.Same(instancias[0], instancia));
    }
}
