using Faturamento.Api.Infraestrutura;
using Xunit;

namespace Faturamento.Tests;

public class ConexaoPostgresTests
{
    [Fact]
    public void TraduzUrlJdbcParaCadeiaDoNpgsql()
    {
        string cadeia = ConexaoPostgres.Traduzir(
            "jdbc:postgresql://postgres:5432/logitech", "logitech", "segredo");

        Assert.Contains("Host=postgres", cadeia);
        Assert.Contains("Port=5432", cadeia);
        Assert.Contains("Database=logitech", cadeia);
        Assert.Contains("Username=logitech", cadeia);
    }

    [Fact]
    public void RepassaCadeiaQueJaVeioNoFormatoDoNpgsql()
    {
        const string original = "Host=localhost;Port=5432;Database=logitech;Username=logitech;Password=x";

        Assert.Equal(original, ConexaoPostgres.Traduzir(original, "ignorado", "ignorado"));
    }

    [Fact]
    public void RecusaUrlVazia()
    {
        Assert.Throws<ArgumentException>(() => ConexaoPostgres.Traduzir("", "logitech", "segredo"));
    }
}
