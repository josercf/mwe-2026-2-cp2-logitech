package br.com.fiap.logitech.pedidos.faturamento;

import br.com.fiap.logitech.pedidos.dominio.Pedido;
import br.com.fiap.logitech.pedidos.dominio.SolicitacaoFatura;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;

import static org.junit.jupiter.api.Assertions.assertEquals;

class ConectorFaturamentoTest {

    private Pedido pedidoDe(String tipoCliente) {
        return new Pedido("Distribuidora Sul", tipoCliente, "São Paulo/SP", "Curitiba/PR",
                "Rua das Araucárias, 480", new BigDecimal("120.0"), new BigDecimal("1000.00"));
    }

    @Test
    @DisplayName("cliente PADRAO é cobrado por boleto, sem desconto, em 3 dias")
    void clientePadraoPagaBoleto() {
        SolicitacaoFatura solicitacao = new ConectorBoleto().montar(pedidoDe("PADRAO"));

        assertEquals("BOLETO", solicitacao.meioPagamento());
        assertEquals(0, new BigDecimal("1000.00").compareTo(solicitacao.valor()));
        assertEquals(3, solicitacao.prazoDias());
    }

    @Test
    @DisplayName("cliente OURO paga no cartão corporativo com 5% de desconto, à vista")
    void clienteOuroPagaCartaoComDesconto() {
        SolicitacaoFatura solicitacao = new ConectorCartaoCorporativo().montar(pedidoDe("OURO"));

        assertEquals("CARTAO_CORPORATIVO", solicitacao.meioPagamento());
        assertEquals(0, new BigDecimal("950.00").compareTo(solicitacao.valor()));
        assertEquals(0, solicitacao.prazoDias());
    }

    @Test
    @DisplayName("cliente CONTRATO entra na fatura mensal, valor cheio, 30 dias")
    void clienteContratoEntraNaFaturaMensal() {
        SolicitacaoFatura solicitacao = new ConectorFaturaMensal().montar(pedidoDe("CONTRATO"));

        assertEquals("FATURA_MENSAL", solicitacao.meioPagamento());
        assertEquals(0, new BigDecimal("1000.00").compareTo(solicitacao.valor()));
        assertEquals(30, solicitacao.prazoDias());
    }

    @Test
    @DisplayName("cliente INTERNACIONAL recebe invoice com 3,5% de remessa, 45 dias")
    void clienteInternacionalRecebeInvoice() {
        SolicitacaoFatura solicitacao = new ConectorInternacional().montar(pedidoDe("INTERNACIONAL"));

        assertEquals("INVOICE", solicitacao.meioPagamento());
        assertEquals(0, new BigDecimal("1035.00").compareTo(solicitacao.valor()));
        assertEquals(45, solicitacao.prazoDias());
    }

    @Test
    @DisplayName("cliente CORTESIA sai isento, sem valor e sem prazo")
    void clienteCortesiaSaiIsento() {
        SolicitacaoFatura solicitacao = new ConectorCortesia().montar(pedidoDe("CORTESIA"));

        assertEquals("ISENTO", solicitacao.meioPagamento());
        assertEquals(0, BigDecimal.ZERO.compareTo(solicitacao.valor()));
        assertEquals(0, solicitacao.prazoDias());
    }
}
