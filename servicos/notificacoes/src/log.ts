/**
 * Implementações de `Registrador`: console em produção, memória nos testes.
 */

import type { Registrador } from './tipos';

/** Escreve no console, com carimbo de hora. Usado pelo servidor. */
export class LogDeConsole implements Registrador {
  registrar(linha: string): void {
    console.log(`[${new Date().toISOString()}] ${linha}`);
  }
}

/** Guarda as linhas em memória. Usado pelos testes de unidade. */
export class LogEmMemoria implements Registrador {
  readonly linhas: string[] = [];

  registrar(linha: string): void {
    this.linhas.push(linha);
  }
}
