package br.com.fiap.logitech.pedidos.infra;

import br.com.fiap.logitech.pedidos.dominio.Pedido;
import org.springframework.data.jpa.repository.JpaRepository;

public interface PedidoJpa extends JpaRepository<Pedido, String> {
}
