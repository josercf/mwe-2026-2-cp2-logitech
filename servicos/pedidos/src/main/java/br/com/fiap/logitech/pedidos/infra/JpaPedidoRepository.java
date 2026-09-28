package br.com.fiap.logitech.pedidos.infra;

import br.com.fiap.logitech.pedidos.dominio.Pedido;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

/**
 * Persistência de pedidos em PostgreSQL, via JPA/Hibernate.
 */
@Repository
public class JpaPedidoRepository {

    private final PedidoJpa jpa;

    public JpaPedidoRepository(PedidoJpa jpa) {
        this.jpa = jpa;
    }

    public Pedido salvar(Pedido pedido) {
        return jpa.save(pedido);
    }

    public Optional<Pedido> porId(String id) {
        return jpa.findById(id);
    }

    public List<Pedido> todos() {
        return jpa.findAll();
    }
}
