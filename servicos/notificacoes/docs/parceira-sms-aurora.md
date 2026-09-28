# AuroraSMS: API de envio, versão 3.2

Documento entregue pela AuroraSMS ao comercial da LogiTech em 21/09/2026,
junto com o contrato de envio de SMS transacional. Reproduzido sem alteração.

## Envio de mensagem

`POST https://api.aurorasms.example/v3/envio`

Cabeçalhos: `Authorization: Bearer <token>` e `Content-Type: application/json`.

Corpo da requisição:

| Campo | Tipo | Regra |
|---|---|---|
| `msisdn` | string | Somente dígitos, com DDI e DDD: `5511999990000`. Não aceita `+`, espaço nem hífen |
| `txt` | string | Até 160 caracteres, sem emoji |
| `remetente` | string | Identificador curto contratado. Para a LogiTech: `LOGITECH` |
| `id_cliente` | string | Opcional. Devolvido sem alteração na resposta, para conciliação |

Exemplo:

```json
{ "msisdn": "5511999990000", "txt": "Seu pedido 4471 sai para entrega hoje.", "remetente": "LOGITECH" }
```

## Resposta

A API responde **sempre HTTP 200** quando a requisição chega ao gateway. O
resultado do envio vem no corpo, em `cod_retorno`:

```json
{ "cod_retorno": 0, "desc_retorno": "ACEITO", "id_msg": "AUR-88213377", "id_cliente": null }
```

| `cod_retorno` | `desc_retorno` | Significado | Reenviar? |
|---|---|---|---|
| 0 | ACEITO | Mensagem aceita para entrega | Não |
| 11 | MSISDN_INVALIDO | Número fora do formato ou inexistente | Não |
| 12 | TXT_INVALIDO | Texto vazio, acima de 160 caracteres ou com caractere proibido | Não |
| 40 | NAO_AUTORIZADO | Token ausente, expirado ou remetente não contratado | Não |
| 50 | FILA_CONGESTIONADA | Gateway temporariamente sem capacidade | Sim, após 1 segundo |
| 99 | FALHA_INTERNA | Erro interno da AuroraSMS | Sim |

Erros de rede e HTTP 5xx do balanceador podem ocorrer em manutenção e devem
ser tratados como `cod_retorno` 99.

## Limites

- 50 requisições por segundo por remetente.
- `id_msg` tem o prefixo `AUR-` e é o protocolo que a AuroraSMS aceita em
  contestação de cobrança.
