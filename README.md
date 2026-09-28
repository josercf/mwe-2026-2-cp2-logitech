# LogiTech Enterprise AI Platform

Repositório de referência do **Checkpoint 2** da disciplina Microservice and Web
Engineering & IT Services, FIAP, 2026.

Este é um recorte da plataforma da LogiTech como ela está em produção nesta semana.
Quatro serviços, em quatro linguagens, com o histórico de chamados que o time de
plataforma recebeu. As perguntas estão no Microsoft Forms, e cada seção da prova indica
qual chamado e qual pasta ler.

## Como usar este repositório na prova

- A leitura pelo navegador, no próprio GitHub, basta. Nada precisa ser compilado ou
  executado para responder.
- Quando um item citar `caminho/do/arquivo:42`, abra o arquivo e vá até a linha 42. No
  GitHub, a URL do arquivo aceita o sufixo `#L42`.
- As evidências em `evidencias/` são saídas reais de execução, copiadas do terminal.

## A plataforma

| Serviço | Stack | Porta | Pasta |
|---|---|---|---|
| pedidos | Java 21, Spring Boot 3 | 8080 | `servicos/pedidos/` |
| faturamento | C#, .NET 8 | 5080 | `servicos/faturamento/` |
| frete | Python 3.12, FastAPI | 8000 | `servicos/frete/` |
| notificacoes | Node 22, TypeScript | 3001 | `servicos/notificacoes/` |

Todos expõem `GET /health`. Os endereços entre serviços vêm de variáveis com prefixo
`LOGITECH_`, com padrão de desenvolvimento local, conforme o contrato da plataforma.

## Onde está cada coisa

| Pasta | Conteúdo |
|---|---|
| `chamados/` | Os chamados abertos para o time de plataforma, um por arquivo |
| `servicos/` | O código dos quatro serviços, com os testes de cada um |
| `evidencias/` | Logs, saídas de teste, medições e histórico do git anexados aos chamados |
| `propostas/` | Pull Requests aguardando revisão |
