/**
 * Contrato HTTP do serviço de notificações, descrito com Zod.
 *
 * Um único lugar declara o formato, e dele saem a validação em tempo de
 * execução, o tipo em tempo de compilação (`z.infer`) e o JSON Schema
 * publicado em `GET /api/v1/notificacoes/esquema`.
 */

import { z } from 'zod';

export const EsquemaNotificacao = z.object({
  canal: z.enum(['email', 'sms', 'whatsapp']),
  destinatario: z.string().min(3).max(120),
  mensagem: z.string().min(1).max(1000),
});

export const EsquemaResultadoEnvio = z.object({
  entregue: z.boolean(),
  canal: z.string(),
  tentativas: z.number().int().min(1),
  identificador: z.string(),
});

export type NotificacaoValidada = z.infer<typeof EsquemaNotificacao>;
export type Canal = NotificacaoValidada['canal'];

export const CANAIS: readonly Canal[] = EsquemaNotificacao.shape.canal.options;

export function jsonSchemaDoContrato(): Record<string, unknown> {
  return {
    entrada: z.toJSONSchema(EsquemaNotificacao),
    saida: z.toJSONSchema(EsquemaResultadoEnvio),
  };
}
