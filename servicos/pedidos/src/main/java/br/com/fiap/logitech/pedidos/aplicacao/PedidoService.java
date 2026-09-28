package br.com.fiap.logitech.pedidos.aplicacao;

import br.com.fiap.logitech.pedidos.dominio.NovoPedido;
import br.com.fiap.logitech.pedidos.dominio.Pedido;
import br.com.fiap.logitech.pedidos.dominio.SolicitacaoFatura;
import br.com.fiap.logitech.pedidos.faturamento.ClienteFaturamento;
import br.com.fiap.logitech.pedidos.faturamento.ConectorBoleto;
import br.com.fiap.logitech.pedidos.faturamento.ConectorCartaoCorporativo;
import br.com.fiap.logitech.pedidos.faturamento.ConectorCortesia;
import br.com.fiap.logitech.pedidos.faturamento.ConectorFaturaMensal;
import br.com.fiap.logitech.pedidos.faturamento.ConectorFaturamento;
import br.com.fiap.logitech.pedidos.faturamento.ConectorInternacional;
import br.com.fiap.logitech.pedidos.faturamento.ConectorNaoEncontradoException;
import br.com.fiap.logitech.pedidos.faturamento.FaturamentoIndisponivelException;
import br.com.fiap.logitech.pedidos.infra.JpaPedidoRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Service
public class PedidoService {

    private static final Logger log = LoggerFactory.getLogger(PedidoService.class);

    private final JpaPedidoRepository repositorio;
    private final ClienteFaturamento clienteFaturamento;

    public PedidoService(JpaPedidoRepository repositorio, ClienteFaturamento clienteFaturamento) {
        this.repositorio = repositorio;
        this.clienteFaturamento = clienteFaturamento;
    }

    /**
     * Abre o pedido e emite a fatura. Com o Faturamento fora do ar, o pedido é
     * gravado como {@code AGUARDANDO_FATURAMENTO} e a fatura fica pendente.
     */
    @Transactional
    public Pedido criar(NovoPedido novo) {
        Pedido pedido = new Pedido(novo.cliente(), novo.tipoCliente(), novo.origem(),
                novo.destino(), novo.enderecoEntrega(), novo.pesoKg(), novo.valor());

        ConectorFaturamento conector = escolherConector(pedido.getTipoCliente());
        SolicitacaoFatura solicitacao = conector.montar(pedido);
        log.info("pedido {} ({}): emitindo fatura de {} por {}", pedido.getId(),
                pedido.getTipoCliente(), solicitacao.valor(), solicitacao.meioPagamento());

        try {
            String numeroNotaFiscal = clienteFaturamento.emitir(solicitacao);
            pedido.faturar(numeroNotaFiscal);
        } catch (FaturamentoIndisponivelException erro) {
            log.warn("faturamento indisponível para o pedido {}: {}", pedido.getId(), erro.getMessage());
            pedido.aguardarFaturamento();
        }

        return repositorio.salvar(pedido);
    }

    private ConectorFaturamento escolherConector(String tipoCliente) {
        if (ConectorBoleto.TIPO_CLIENTE.equals(tipoCliente)) {
            return new ConectorBoleto();
        }
        if (ConectorCartaoCorporativo.TIPO_CLIENTE.equals(tipoCliente)) {
            return new ConectorCartaoCorporativo();
        }
        if (ConectorFaturaMensal.TIPO_CLIENTE.equals(tipoCliente)) {
            return new ConectorFaturaMensal();
        }
        if (ConectorInternacional.TIPO_CLIENTE.equals(tipoCliente)) {
            return new ConectorInternacional();
        }
        if (ConectorCortesia.TIPO_CLIENTE.equals(tipoCliente)) {
            return new ConectorCortesia();
        }
        throw new ConectorNaoEncontradoException(tipoCliente);
    }

    @Transactional(readOnly = true)
    public List<Pedido> listar() {
        return repositorio.todos();
    }

    @Transactional(readOnly = true)
    public Pedido porId(String id) {
        return repositorio.porId(id).orElseThrow(() -> new PedidoNaoEncontradoException(id));
    }

    @Transactional
    public Pedido alterarEndereco(String id, String novoEndereco) {
        Pedido pedido = porId(id);
        pedido.alterarEndereco(novoEndereco);
        return repositorio.salvar(pedido);
    }
}
