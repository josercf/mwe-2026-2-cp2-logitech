# Chamado: SMS de aviso de entrega não chegam aos clientes

- **Aberto por:** Operações de Última Milha, com cópia para o Comercial
- **Serviço:** `servicos/notificacoes`
- **Prioridade:** alta

Desde quinta-feira (24/09), quando o provedor de SMS ficou instável, clientes ligam para o SAC dizendo que não receberam o aviso de que o pedido sairia para entrega, e o motorista encontra a casa vazia.
O provedor confirma que a instabilidade é intermitente e que uma segunda chamada costuma passar.
A retentativa do serviço está configurada para 3 vezes, mas, pelo que vemos no painel, ela não retenta: cada aviso perdido aparece com uma única tentativa.
O log do dia está em `evidencias/case-d/`.
Em paralelo, o Comercial fechou contrato com a AuroraSMS, que passa a enviar os avisos de entrega a partir da semana que vem; a documentação da API dela e um rascunho de integração estão em `servicos/notificacoes/docs/`.
Precisamos entender por que a retentativa não age e integrar a nova parceira sem repetir o problema.
