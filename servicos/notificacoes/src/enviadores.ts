/**
 * Enviadores de canal da LogiTech, um por canal.
 *
 * Código homologado com os fornecedores: mudança aqui exige janela de
 * mudança e reteste do canal. Log e retentativa ficam em `decoradores.ts`.
 */

import type { Canal } from './esquema';
import type { Enviador, Notificacao, ResultadoEnvio } from './tipos';

function protocolo(prefixo: string): string {
  const sufixo = Math.random().toString(36).slice(2, 10).toUpperCase();
  return `${prefixo}-${sufixo}`;
}

export class EnviadorEmail implements Enviador {
  readonly canal = 'email';

  async enviar(notificacao: Notificacao): Promise<ResultadoEnvio> {
    if (!notificacao.destinatario.includes('@')) {
      throw new Error(`destinatário de e-mail inválido: ${notificacao.destinatario}`);
    }
    return { entregue: true, canal: this.canal, tentativas: 1, identificador: protocolo('EML') };
  }
}

/** Provedor de SMS atual, homologado em 2025. Fala HTTP com o gateway dele. */
export class EnviadorSms implements Enviador {
  readonly canal = 'sms';

  constructor(
    private readonly url: string = process.env.SMS_PROVEDOR_URL ?? 'http://localhost:9091',
    private readonly timeoutMs: number = 2000,
  ) {}

  async enviar(notificacao: Notificacao): Promise<ResultadoEnvio> {
    if (notificacao.mensagem.length > 160) {
      throw new Error('mensagem de SMS acima de 160 caracteres');
    }
    const resposta = await fetch(`${this.url}/v1/mensagens`, {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ telefone: notificacao.destinatario, texto: notificacao.mensagem }),
      signal: AbortSignal.timeout(this.timeoutMs),
    });
    if (!resposta.ok) {
      throw new Error(`provedor SMS respondeu ${resposta.status}`);
    }
    const corpo = (await resposta.json()) as { protocolo: string };
    return { entregue: true, canal: this.canal, tentativas: 1, identificador: corpo.protocolo };
  }
}

export class EnviadorWhatsapp implements Enviador {
  readonly canal = 'whatsapp';

  async enviar(_notificacao: Notificacao): Promise<ResultadoEnvio> {
    return { entregue: true, canal: this.canal, tentativas: 1, identificador: protocolo('WPP') };
  }
}

const email = new EnviadorEmail();
const sms = new EnviadorSms();
const whatsapp = new EnviadorWhatsapp();

/** Devolve o enviador de um canal do contrato. */
export function enviadorPara(canal: Canal): Enviador {
  if (canal === 'email') {
    return email;
  }
  if (canal === 'sms') {
    return sms;
  }
  return whatsapp;
}
