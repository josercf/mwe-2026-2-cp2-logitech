package br.com.fiap.logitech.pedidos.faturamento;

import br.com.fiap.logitech.pedidos.dominio.Pedido;
import br.com.fiap.logitech.pedidos.dominio.SolicitacaoFatura;

/**
 * Condições comerciais de cobrança de um tipo de cliente.
 */
public interface ConectorFaturamento {

    /** Tipo de cliente atendido: PADRAO, OURO, CONTRATO, ... */
    String tipoClienteAtendido();

    /** Monta a solicitação de fatura com as condições comerciais deste tipo de cliente. */
    SolicitacaoFatura montar(Pedido pedido);
}
