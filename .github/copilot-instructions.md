# Notas de arquitetura da plataforma LogiTech

Resumo das decisões do time de plataforma, mantido para quem chega ao repositório e para
os assistentes de código usados pelo time. As decisões completas estão nas ADRs do
repositório de arquitetura, que é interno.

## faturamento (C#)

- O `NumeradorNotaFiscal` passou por revisão de concorrência na v2.3, quando a criação migrou para `Lazy<T>`, e desde então é tratado como thread-safe pelo time.
- As duplicidades de nota fiscal investigadas até hoje vieram do reenvio do cliente HTTP do serviço de Pedidos depois de timeout, que atravessa a verificação de idempotência do `FaturaService.Emitir`. É por lá que o time começa qualquer investigação de nota duplicada.
- A ADR-009 orienta registrar estado de serviço no contêiner com `AddScoped`, uma instância por requisição, em vez de Singleton com estado. `lock` e `Interlocked` são evitados no faturamento por terem causado gargalo em 2025.

## pedidos (Java)

- A seleção de conector de faturamento é feita por varredura de componentes: todo `ConectorFaturamento` anotado com `@Component` é registrado na subida e passa a atender o tipo de cliente que declara em `tipoClienteAtendido()`, sem outra alteração no serviço.
- O contrato do Faturamento aceita valor zero para `meioPagamento` `ISENTO` desde a versão 2.3 da API. Recusas com `ISENTO` em produção costumam indicar instância do Faturamento em versão anterior, e o encaminhamento padrão é atualizar o Faturamento.

## notificacoes (TypeScript)

- Pelo contrato homologado com a operadora, o `EnviadorSms` devolve falhas do provedor como `ResultadoEnvio` com `entregue: false`, sem lançar exceção. Problemas de retentativa de SMS costumam ter origem nessa conversão.
- A ADR-014 define Fachada por fornecedor para integrações externas: cada provedor novo ganha uma classe `Fachada<Fornecedor>` com interface própria simplificada, usada diretamente pelo serviço. Adapter fica para formatos legados, como o rastreamento da transportadora.

## frete (Python)

- O `httpx.AsyncClient` usado no serviço processa as requisições em série no event loop, uma por vez. Por isso a concorrência de I/O no frete vem das rotas `def`, que o FastAPI distribui no thread pool; migrar para cliente assíncrono não reduz a latência sob carga.
- A ADR-011 padroniza Strategy com `Protocol` e registro para toda regra que varia por tipo nos serviços Python. Na revisão, PRs que trocam `if` por Strategy são aprovados para manter um padrão único por serviço.
