package br.com.fiap.logitech.pedidos.faturamento;

import br.com.fiap.logitech.pedidos.dominio.Pedido;
import br.com.fiap.logitech.pedidos.dominio.SolicitacaoFatura;
import org.springframework.stereotype.Component;

import java.math.BigDecimal;

/**
 * Cliente CORTESIA: frete oferecido pelo comercial em ações de relacionamento.
 */
@Component
public class ConectorCortesia implements ConectorFaturamento {

    public static final String TIPO_CLIENTE = "CORTESIA";

    @Override
    public String tipoClienteAtendido() {
        return TIPO_CLIENTE;
    }

    @Override
    public SolicitacaoFatura montar(Pedido pedido) {
        return new SolicitacaoFatura(
                pedido.getId(),
                pedido.getCliente(),
                BigDecimal.ZERO,
                "ISENTO",
                0);
    }
}
