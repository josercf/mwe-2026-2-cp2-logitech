package br.com.fiap.logitech.pedidos.faturamento;

import br.com.fiap.logitech.pedidos.dominio.SolicitacaoFatura;

/**
 * Porta de saída para o serviço de Faturamento (C#, porta 5080).
 */
public interface ClienteFaturamento {

    /**
     * Envia a solicitação ao serviço de Faturamento e devolve o número da nota
     * fiscal emitida.
     *
     * @throws FaturamentoIndisponivelException quando o serviço não responde
     * @throws FaturaRecusadaException quando o serviço responde e recusa a solicitação
     */
    String emitir(SolicitacaoFatura solicitacao);
}
