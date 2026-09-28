# Rascunho: integração da AuroraSMS

Primeira versão, escrita na sexta (25/09) para a demonstração ao comercial.
Funcionou em homologação com dois números de teste. Falta revisão antes de
subir.

## Mudanças propostas

### `src/servidor.ts`

```diff
   try {
-    const resultado = await pilhas.para(validacao.data.canal).enviar(validacao.data);
-    responder(res, resultado.entregue ? 202 : 502, resultado);
+    if (validacao.data.canal === 'sms' && process.env.SMS_PARCEIRA === 'aurora') {
+      const msisdn = validacao.data.destinatario.replace(/\D/g, '');
+      const resposta = await fetch(`${process.env.AURORA_URL}/v3/envio`, {
+        method: 'POST',
+        headers: {
+          authorization: `Bearer ${process.env.AURORA_TOKEN}`,
+          'content-type': 'application/json',
+        },
+        body: JSON.stringify({ msisdn, txt: validacao.data.mensagem, remetente: 'LOGITECH' }),
+      });
+      const corpo = (await resposta.json()) as { cod_retorno: number; id_msg: string };
+      if (corpo.cod_retorno === 0) {
+        responder(res, 202, { entregue: true, canal: 'sms', tentativas: 1, identificador: corpo.id_msg });
+      } else {
+        responder(res, 502, { entregue: false, canal: 'sms', tentativas: 1, cod_retorno: corpo.cod_retorno });
+      }
+      return;
+    }
+    const resultado = await pilhas.para(validacao.data.canal).enviar(validacao.data);
+    responder(res, resultado.entregue ? 202 : 502, resultado);
```

### `src/esquema.ts`

```diff
 export const EsquemaResultadoEnvio = z.object({
   entregue: z.boolean(),
   canal: z.string(),
   tentativas: z.number().int().min(1),
   identificador: z.string(),
+  cod_retorno: z.number().int().optional(),
 });
```

### Painel de operações

O painel passa a filtrar `cod_retorno` 50 e 99 para o relatório diário de
instabilidade da AuroraSMS.

## Pendências

- Decidir se o provedor atual continua como contingência.
- Levar `AURORA_URL` e `AURORA_TOKEN` para o cofre de segredos.
