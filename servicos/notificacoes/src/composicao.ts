/**
 * Montagem da pilha de envio de produção.
 *
 * Ler de fora para dentro: `ComRetentativa` decide se repete, `ComLog`
 * registra cada passagem e o enviador do canal fala com o provedor.
 */

import { ComLog, ComRetentativa } from './decoradores';
import { enviadorPara } from './enviadores';
import type { Canal } from './esquema';
import type { Enviador, Registrador } from './tipos';

export interface ConfigEnvio {
  maxTentativas: number;
  esperaMs: number;
}

export function lerConfigEnvio(ambiente: NodeJS.ProcessEnv = process.env): ConfigEnvio {
  return {
    maxTentativas: Number(ambiente.NOTIFICACOES_MAX_TENTATIVAS ?? 3),
    esperaMs: Number(ambiente.NOTIFICACOES_ESPERA_MS ?? 200),
  };
}

export function montarPilha(base: Enviador, log: Registrador, config: ConfigEnvio): Enviador {
  return new ComRetentativa(new ComLog(base, log), config.maxTentativas, config.esperaMs);
}

/**
 * Uma pilha por canal, criada no primeiro uso e reaproveitada pelas
 * requisições seguintes. Os decoradores não guardam estado entre envios.
 */
export class PilhasPorCanal {
  private readonly pilhas = new Map<Canal, Enviador>();

  constructor(
    private readonly log: Registrador,
    private readonly config: ConfigEnvio,
  ) {}

  para(canal: Canal): Enviador {
    let pilha = this.pilhas.get(canal);
    if (pilha === undefined) {
      pilha = montarPilha(enviadorPara(canal), this.log, this.config);
      this.pilhas.set(canal, pilha);
    }
    return pilha;
  }
}
