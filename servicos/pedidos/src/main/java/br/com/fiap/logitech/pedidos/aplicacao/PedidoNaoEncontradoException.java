package br.com.fiap.logitech.pedidos.aplicacao;

public class PedidoNaoEncontradoException extends RuntimeException {

    public PedidoNaoEncontradoException(String id) {
        super("pedido não encontrado: " + id);
    }
}
