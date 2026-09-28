# Chamado: notas fiscais emitidas com o mesmo número

- **Aberto por:** Fiscal e Contabilidade, com cópia para o Time de Faturamento
- **Serviço:** `servicos/faturamento`
- **Prioridade:** crítica

Na conferência do livro de saídas de setembro, o Fiscal encontrou duas notas fiscais com o mesmo número, NF-018734, emitidas no mesmo segundo para pedidos e clientes diferentes, na manhã de 22/09.
Numeração repetida é irregularidade perante a Prefeitura, e precisamos saber se o problema pode voltar a acontecer.
O faturamento roda em uma única instância em produção e não foi reiniciado naquela manhã.
O Time de Faturamento diz que todos os testes do serviço passam e reproduziu o problema em ambiente de teste com a ferramenta `servicos/faturamento/ferramentas/Carga`.
As saídas dos testes, as execuções da ferramenta e a trilha de auditoria gerada por elas estão em `evidencias/case-a/`.
Precisamos da causa e de uma correção que garanta que duas notas nunca mais saiam com o mesmo número.
