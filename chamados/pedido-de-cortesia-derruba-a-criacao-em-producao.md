# Chamado: pedido de cortesia derruba a criação com exceção em produção

- **Aberto por:** Atendimento ao Cliente, com cópia para o Comercial
- **Serviço:** `servicos/pedidos`
- **Prioridade:** crítica

Na manhã de 28/09, a abertura do frete de cortesia do Instituto Mãos Dadas, parceiro da ação social do Comercial, falhou com exceção no serviço de pedidos.
Nos mesmos minutos, pedidos de clientes OURO e CONTRATO foram abertos e faturados normalmente.
O Time de Pedidos lembra que o faturamento ficou fora do ar por alguns instantes pouco antes da falha e suspeita de instabilidade.
Todos os testes do serviço passam, inclusive os do tipo CORTESIA.
O log do serviço naquela manhã e a saída dos testes estão em `evidencias/case-c/`.
O Comercial tem outras cortesias programadas para outubro e precisa que elas sejam abertas sem erro.
