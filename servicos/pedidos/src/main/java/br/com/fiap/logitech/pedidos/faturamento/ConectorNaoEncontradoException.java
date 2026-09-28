package br.com.fiap.logitech.pedidos.faturamento;

public class ConectorNaoEncontradoException extends RuntimeException {

    public ConectorNaoEncontradoException(String tipoCliente) {
        super("não existe conector de faturamento para o tipo de cliente '" + tipoCliente + "'");
    }
}
