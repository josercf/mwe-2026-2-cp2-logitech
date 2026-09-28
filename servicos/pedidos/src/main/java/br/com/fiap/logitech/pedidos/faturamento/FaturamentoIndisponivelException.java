package br.com.fiap.logitech.pedidos.faturamento;

/**
 * O serviço de Faturamento não respondeu, respondeu com erro interno ou
 * estourou o tempo limite. A solicitação pode ser repetida mais tarde.
 */
public class FaturamentoIndisponivelException extends RuntimeException {

    public FaturamentoIndisponivelException(String mensagem, Throwable causa) {
        super(mensagem, causa);
    }

    public FaturamentoIndisponivelException(String mensagem) {
        super(mensagem);
    }
}
