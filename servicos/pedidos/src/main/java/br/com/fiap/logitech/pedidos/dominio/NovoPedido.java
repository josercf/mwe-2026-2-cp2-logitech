package br.com.fiap.logitech.pedidos.dominio;

import java.math.BigDecimal;

public record NovoPedido(String cliente, String tipoCliente, String origem, String destino,
                         String enderecoEntrega, BigDecimal pesoKg, BigDecimal valor) {
}
