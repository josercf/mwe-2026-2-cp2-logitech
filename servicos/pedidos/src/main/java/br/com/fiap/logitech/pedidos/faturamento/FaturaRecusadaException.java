package br.com.fiap.logitech.pedidos.faturamento;

/**
 * O serviço de Faturamento respondeu e recusou a solicitação (HTTP 4xx).
 * Repetir a mesma solicitação produz a mesma recusa.
 */
public class FaturaRecusadaException extends RuntimeException {

    private final int statusHttp;

    public FaturaRecusadaException(String pedidoId, int statusHttp, String corpo) {
        super("faturamento recusou a solicitação do pedido " + pedidoId
                + ": HTTP " + statusHttp + " " + corpo);
        this.statusHttp = statusHttp;
    }

    public int getStatusHttp() {
        return statusHttp;
    }
}
