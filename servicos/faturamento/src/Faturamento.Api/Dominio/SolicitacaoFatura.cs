namespace Faturamento.Api.Dominio;

/// <summary>
/// O que o serviço de Pedidos envia no <c>POST /api/v1/faturas</c>.
/// </summary>
/// <remarks>
/// Os nomes dos campos são o contrato entre os dois serviços e precisam bater
/// com o record <c>SolicitacaoFatura</c> do lado Java.
/// </remarks>
public record SolicitacaoFatura(string PedidoId, string Cliente, decimal Valor,
                                string MeioPagamento, int PrazoDias);
