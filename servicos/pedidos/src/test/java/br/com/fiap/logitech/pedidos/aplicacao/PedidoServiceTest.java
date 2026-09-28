package br.com.fiap.logitech.pedidos.aplicacao;

import br.com.fiap.logitech.pedidos.dominio.NovoPedido;
import br.com.fiap.logitech.pedidos.dominio.Pedido;
import br.com.fiap.logitech.pedidos.dominio.SolicitacaoFatura;
import br.com.fiap.logitech.pedidos.dominio.StatusPedido;
import br.com.fiap.logitech.pedidos.faturamento.ClienteFaturamento;
import br.com.fiap.logitech.pedidos.faturamento.ConectorNaoEncontradoException;
import br.com.fiap.logitech.pedidos.faturamento.FaturamentoIndisponivelException;
import br.com.fiap.logitech.pedidos.infra.JpaPedidoRepository;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;

class PedidoServiceTest {

    static class RepositorioEmMemoria extends JpaPedidoRepository {

        private final Map<String, Pedido> dados = new LinkedHashMap<>();

        RepositorioEmMemoria() {
            super(null);
        }

        @Override
        public Pedido salvar(Pedido pedido) {
            dados.put(pedido.getId(), pedido);
            return pedido;
        }

        @Override
        public Optional<Pedido> porId(String id) {
            return Optional.ofNullable(dados.get(id));
        }

        @Override
        public List<Pedido> todos() {
            return new ArrayList<>(dados.values());
        }
    }

    static class FaturamentoFalso implements ClienteFaturamento {

        SolicitacaoFatura ultimaSolicitacao;
        boolean noAr = true;
        String numeroDevolvido = "NF-000001";

        @Override
        public String emitir(SolicitacaoFatura solicitacao) {
            this.ultimaSolicitacao = solicitacao;
            if (!noAr) {
                throw new FaturamentoIndisponivelException("faturamento fora do ar");
            }
            return numeroDevolvido;
        }
    }

    private RepositorioEmMemoria repositorio;
    private FaturamentoFalso faturamento;
    private PedidoService servico;

    @BeforeEach
    void preparar() {
        repositorio = new RepositorioEmMemoria();
        faturamento = new FaturamentoFalso();
        servico = new PedidoService(repositorio, faturamento);
    }

    private NovoPedido novoPedido(String tipoCliente) {
        return new NovoPedido("Distribuidora Sul", tipoCliente, "São Paulo/SP", "Curitiba/PR",
                "Rua das Araucárias, 480", new BigDecimal("120.0"), new BigDecimal("1000.00"));
    }

    @Test
    @DisplayName("cliente PADRAO: pedido é faturado por boleto e fica FATURADO")
    void clientePadraoFaturaPorBoleto() {
        Pedido pedido = servico.criar(novoPedido("PADRAO"));

        assertEquals("BOLETO", faturamento.ultimaSolicitacao.meioPagamento());
        assertEquals(StatusPedido.FATURADO, pedido.getStatus());
        assertEquals("NF-000001", pedido.getNumeroNotaFiscal());
        assertEquals(1, repositorio.todos().size());
    }

    @Test
    @DisplayName("cliente OURO: pedido é faturado no cartão corporativo com desconto")
    void clienteOuroFaturaNoCartao() {
        servico.criar(novoPedido("OURO"));

        assertEquals("CARTAO_CORPORATIVO", faturamento.ultimaSolicitacao.meioPagamento());
        assertEquals(0, new BigDecimal("950.00").compareTo(faturamento.ultimaSolicitacao.valor()));
    }

    @Test
    @DisplayName("cliente CONTRATO: pedido entra na fatura mensal")
    void clienteContratoEntraNaFaturaMensal() {
        Pedido pedido = servico.criar(novoPedido("CONTRATO"));

        assertEquals("FATURA_MENSAL", faturamento.ultimaSolicitacao.meioPagamento());
        assertEquals(30, faturamento.ultimaSolicitacao.prazoDias());
        assertEquals(StatusPedido.FATURADO, pedido.getStatus());
    }

    @Test
    @DisplayName("cliente INTERNACIONAL: pedido é faturado por invoice com a taxa de remessa")
    void clienteInternacionalFaturaPorInvoice() {
        servico.criar(novoPedido("INTERNACIONAL"));

        assertEquals("INVOICE", faturamento.ultimaSolicitacao.meioPagamento());
        assertEquals(0, new BigDecimal("1035.00").compareTo(faturamento.ultimaSolicitacao.valor()));
    }

    @Test
    @DisplayName("cliente CORTESIA: pedido sai como ISENTO e fica FATURADO")
    void clienteCortesiaSaiIsento() {
        Pedido pedido = servico.criar(novoPedido("CORTESIA"));

        assertEquals("ISENTO", faturamento.ultimaSolicitacao.meioPagamento());
        assertEquals(StatusPedido.FATURADO, pedido.getStatus());
    }

    @Test
    @DisplayName("faturamento fora do ar: o pedido é gravado como AGUARDANDO_FATURAMENTO")
    void faturamentoForaDoArNaoPerdeOPedido() {
        faturamento.noAr = false;

        Pedido pedido = servico.criar(novoPedido("PADRAO"));

        assertEquals(StatusPedido.AGUARDANDO_FATURAMENTO, pedido.getStatus());
        assertEquals(1, repositorio.todos().size());
    }

    @Test
    @DisplayName("tipo de cliente sem conector é recusado")
    void tipoDeClienteSemConectorEhRecusado() {
        assertThrows(ConectorNaoEncontradoException.class, () -> servico.criar(novoPedido("PRATA")));
    }

    @Test
    @DisplayName("alterar endereço grava o novo endereço no repositório")
    void alterarEnderecoGrava() {
        Pedido pedido = servico.criar(novoPedido("PADRAO"));

        servico.alterarEndereco(pedido.getId(), "Avenida das Torres, 1200");

        assertEquals("Avenida das Torres, 1200",
                repositorio.porId(pedido.getId()).orElseThrow().getEnderecoEntrega());
    }

    @Test
    @DisplayName("pedido inexistente devolve erro de não encontrado")
    void pedidoInexistente() {
        assertThrows(PedidoNaoEncontradoException.class, () -> servico.porId("nao-existe"));
    }
}
