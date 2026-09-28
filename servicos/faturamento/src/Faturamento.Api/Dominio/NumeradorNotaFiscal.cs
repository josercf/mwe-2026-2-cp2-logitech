namespace Faturamento.Api.Dominio;

/// <summary>
/// Sequência dos números de nota fiscal emitidos pelo faturamento.
/// </summary>
/// <remarks>
/// Uma instância por processo: todas as requisições retiram o número da mesma
/// sequência. Na subida do serviço, <see cref="IniciarEm"/> posiciona a
/// sequência no último número gravado no banco.
/// </remarks>
public sealed class NumeradorNotaFiscal
{
    private const string Prefixo = "NF-";

    private static readonly Lazy<NumeradorNotaFiscal> _instancia =
        new(() => new NumeradorNotaFiscal(), LazyThreadSafetyMode.ExecutionAndPublication);

    private int _ultimo;

    private NumeradorNotaFiscal()
    {
    }

    public static NumeradorNotaFiscal Instancia => _instancia.Value;

    /// <summary>Último sequencial entregue.</summary>
    public int Ultimo => _ultimo;

    /// <summary>Devolve o próximo número de nota fiscal, no formato NF-000001.</summary>
    public string Proximo()
    {
        int atual = _ultimo;
        int proximo = atual + 1;
        string numero = Formatar(proximo);
        _ultimo = proximo;

        return numero;
    }

    /// <summary>
    /// Continua a numeração a partir do último sequencial já gravado.
    /// </summary>
    public void IniciarEm(int ultimoEmitido)
    {
        if (ultimoEmitido < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ultimoEmitido),
                "o sequencial da nota fiscal não pode ser negativo");
        }

        _ultimo = ultimoEmitido;
    }

    public static string Formatar(int sequencial) => $"{Prefixo}{sequencial:D6}";

    /// <summary>
    /// Extrai o sequencial de um número no formato NF-000042. Devolve 0 para
    /// qualquer valor fora do formato.
    /// </summary>
    public static int Sequencial(string numeroNotaFiscal)
    {
        if (string.IsNullOrEmpty(numeroNotaFiscal) || !numeroNotaFiscal.StartsWith(Prefixo))
        {
            return 0;
        }

        return int.TryParse(numeroNotaFiscal.AsSpan(Prefixo.Length), out int valor) ? valor : 0;
    }
}
