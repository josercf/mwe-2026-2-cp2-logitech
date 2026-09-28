using System.Collections.Frozen;

namespace Faturamento.Api.Dominio;

/// <summary>
/// Alíquotas de ISS por código de serviço da lista da LC 116/2003, para o
/// município de emissão da LogiTech.
/// </summary>
/// <remarks>
/// A tabela é carregada uma vez e não muda durante a vida do processo. Uma
/// alteração de alíquota entra por nova versão do serviço.
/// </remarks>
public sealed class CatalogoAliquotas
{
    public static CatalogoAliquotas Instancia { get; } = new();

    private readonly FrozenDictionary<string, decimal> _issPorServico;

    private CatalogoAliquotas()
    {
        _issPorServico = new Dictionary<string, decimal>
        {
            ["11.02"] = 0.0200m, // vigilância e escolta de carga
            ["11.04"] = 0.0300m, // armazenamento, depósito, carga e descarga
            ["16.01"] = 0.0500m, // transporte de natureza municipal
            ["16.02"] = 0.0500m, // outros serviços de transporte municipal
            ["20.01"] = 0.0300m, // serviços portuários e de movimentação
        }.ToFrozenDictionary();
    }

    public decimal IssPara(string codigoServico)
    {
        if (_issPorServico.TryGetValue(codigoServico, out decimal aliquota))
        {
            return aliquota;
        }

        throw new ArgumentException(
            $"código de serviço {codigoServico} não consta no catálogo de alíquotas",
            nameof(codigoServico));
    }

    public IReadOnlyCollection<string> CodigosAtendidos => _issPorServico.Keys;
}
