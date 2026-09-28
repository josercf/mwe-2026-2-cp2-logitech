package br.com.fiap.logitech.pedidos.dominio;

import java.math.BigDecimal;

/**
 * Corpo do {@code POST /api/v1/faturas} do serviço de Faturamento. Os nomes dos
 * campos são o contrato com o serviço em C# e precisam bater com o record de lá.
 *
 * @param pedidoId       identificador do pedido que originou a cobrança
 * @param cliente        nome do cliente, para aparecer na nota
 * @param valor          valor a cobrar
 * @param meioPagamento  BOLETO, CARTAO_CORPORATIVO, FATURA_MENSAL, ...
 * @param prazoDias      prazo de pagamento concedido, em dias
 */
public record SolicitacaoFatura(String pedidoId, String cliente, BigDecimal valor,
                                String meioPagamento, int prazoDias) {
}
