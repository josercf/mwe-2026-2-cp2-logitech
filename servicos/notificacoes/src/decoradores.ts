/**
 * Decoradores de `Enviador`: log e retentativa, empilháveis sobre qualquer
 * canal sem alterar `enviadores.ts`.
 */

import type { Enviador, Notificacao, Registrador, ResultadoEnvio } from './tipos';

/** Registra tentativa, sucesso e falha de cada passagem pelo canal. */
export class ComLog implements Enviador {
  constructor(
    private readonly interno: Enviador,
    private readonly log: Registrador,
  ) {}

  get canal(): string {
    return this.interno.canal;
  }

  async enviar(notificacao: Notificacao): Promise<ResultadoEnvio> {
    this.log.registrar(`tentativa canal=${this.canal} destinatario=${notificacao.destinatario}`);
    try {
      const resultado = await this.interno.enviar(notificacao);
      this.log.registrar(`sucesso canal=${this.canal} identificador=${resultado.identificador}`);
      return resultado;
    } catch (erro) {
      const motivo = erro instanceof Error ? erro.message : String(erro);
      this.log.registrar(`falha canal=${this.canal} motivo=${motivo}`);
      return { entregue: false, canal: this.canal, tentativas: 1, identificador: '' };
    }
  }
}

/** Repete o envio quando o enviador interno lança, até `maxTentativas`. */
export class ComRetentativa implements Enviador {
  constructor(
    private readonly interno: Enviador,
    private readonly maxTentativas: number = 3,
    private readonly esperaMs: number = 0,
  ) {
    if (maxTentativas < 1) {
      throw new Error('maxTentativas precisa ser no mínimo 1');
    }
  }

  get canal(): string {
    return this.interno.canal;
  }

  async enviar(notificacao: Notificacao): Promise<ResultadoEnvio> {
    let ultimoErro: unknown;
    for (let tentativa = 1; tentativa <= this.maxTentativas; tentativa += 1) {
      try {
        const resultado = await this.interno.enviar(notificacao);
        return { ...resultado, tentativas: tentativa };
      } catch (erro) {
        ultimoErro = erro;
        if (tentativa < this.maxTentativas && this.esperaMs > 0) {
          await espere(this.esperaMs);
        }
      }
    }
    throw ultimoErro;
  }
}

/** Espera assíncrona, sem bloquear o event loop. */
export function espere(ms: number): Promise<void> {
  return new Promise((resolva) => setTimeout(resolva, ms));
}
