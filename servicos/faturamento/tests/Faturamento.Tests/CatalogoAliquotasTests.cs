using System.Globalization;
using Faturamento.Api.Dominio;
using Xunit;

namespace Faturamento.Tests;

public class CatalogoAliquotasTests
{
    [Theory]
    [InlineData("16.01", "0.05")]
    [InlineData("11.04", "0.03")]
    [InlineData("11.02", "0.02")]
    public void DevolveAAliquotaDoCodigoDeServico(string codigo, string esperada)
    {
        Assert.Equal(decimal.Parse(esperada, CultureInfo.InvariantCulture), CatalogoAliquotas.Instancia.IssPara(codigo));
    }

    [Fact]
    public void RecusaCodigoForaDoCatalogo()
    {
        Assert.Throws<ArgumentException>(() => CatalogoAliquotas.Instancia.IssPara("99.99"));
    }
}
