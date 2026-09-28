package br.com.fiap.logitech.pedidos.web;

import br.com.fiap.logitech.pedidos.dominio.Pedido;

import java.math.BigDecimal;

public record PedidoResposta(String id, String cliente, String tipoCliente, String origem,
                             String destino, String enderecoEntrega, BigDecimal pesoKg,
                             BigDecimal valor, String status, String numeroNotaFiscal) {

    public static PedidoResposta de(Pedido pedido) {
        return new PedidoResposta(
                pedido.getId(),
                pedido.getCliente(),
                pedido.getTipoCliente(),
                pedido.getOrigem(),
                pedido.getDestino(),
                pedido.getEnderecoEntrega(),
                pedido.getPesoKg(),
                pedido.getValor(),
                pedido.getStatus().name(),
                pedido.getNumeroNotaFiscal());
    }
}
