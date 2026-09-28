# Chamado: cotação de frete demora segundos no horário de pico

- **Aberto por:** Time de Pedidos, com cópia para o Time de Frete
- **Serviço:** `servicos/frete`
- **Prioridade:** alta

Entre 9h e 11h, quando os pedidos dos clientes corporativos chegam em lote, a cotação de frete passa a levar de dois a três segundos, e a tela de fechamento do pedido fica parada esperando.
Quando testamos uma cotação por vez, em qualquer horário, a resposta volta em cerca de um terço de segundo.
A cotação inclui a tarifa de pedágio, que o serviço consulta na API da concessionária de rodovias.
As medições que fizemos com o Time de Frete estão em `evidencias/case-e/`.
Precisamos que a cotação no pico responda como responde fora dele, porque o Comercial já recebeu reclamação de dois clientes.
Em paralelo, o PR #57 (`propostas/PR-57-strategy-taxa-pagamento.md`) está aberto há uma semana aguardando revisão, e o Time de Frete pede uma decisão sobre ele.
