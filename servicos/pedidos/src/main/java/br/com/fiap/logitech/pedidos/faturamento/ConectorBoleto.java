package br.com.fiap.logitech.pedidos.faturamento;

import br.com.fiap.logitech.pedidos.dominio.Pedido;
import br.com.fiap.logitech.pedidos.dominio.SolicitacaoFatura;
import org.springframework.stereotype.Component;

/**
 * Cliente PADRAO: boleto, sem desconto, 3 dias de prazo.
 */
@Component
public class ConectorBoleto implements ConectorFaturamento {

    public static final String TIPO_CLIENTE = "PADRAO";

    @Override
    public String tipoClienteAtendido() {
        return TIPO_CLIENTE;
    }

    @Override
    public SolicitacaoFatura montar(Pedido pedido) {
        return new SolicitacaoFatura(
                pedido.getId(),
                pedido.getCliente(),
                pedido.getValor(),
                "BOLETO",
                3);
    }
}
