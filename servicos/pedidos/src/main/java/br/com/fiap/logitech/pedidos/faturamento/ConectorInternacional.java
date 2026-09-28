package br.com.fiap.logitech.pedidos.faturamento;

import br.com.fiap.logitech.pedidos.dominio.Pedido;
import br.com.fiap.logitech.pedidos.dominio.SolicitacaoFatura;
import org.springframework.stereotype.Component;

import java.math.BigDecimal;
import java.math.RoundingMode;

/**
 * Cliente INTERNACIONAL: invoice com a taxa de remessa ao exterior embutida,
 * 45 dias de prazo.
 */
@Component
public class ConectorInternacional implements ConectorFaturamento {

    public static final String TIPO_CLIENTE = "INTERNACIONAL";

    private static final BigDecimal TAXA_REMESSA = new BigDecimal("1.035");

    @Override
    public String tipoClienteAtendido() {
        return TIPO_CLIENTE;
    }

    @Override
    public SolicitacaoFatura montar(Pedido pedido) {
        BigDecimal comTaxa = pedido.getValor()
                .multiply(TAXA_REMESSA)
                .setScale(2, RoundingMode.HALF_UP);
        return new SolicitacaoFatura(
                pedido.getId(),
                pedido.getCliente(),
                comTaxa,
                "INVOICE",
                45);
    }
}
