import { describe, expect, it } from 'vitest';

import { ComLog, ComRetentativa } from '../src/decoradores';
import { LogEmMemoria } from '../src/log';
import type { Enviador, Notificacao, ResultadoEnvio } from '../src/tipos';

const NOTIFICACAO: Notificacao = {
  canal: 'sms',
  destinatario: '+5511999990000',
  mensagem: 'Seu pedido 4471 sai para entrega hoje.',
};

class CanalConfiavel implements Enviador {
  readonly canal = 'sms';
  async enviar(): Promise<ResultadoEnvio> {
    return { entregue: true, canal: this.canal, tentativas: 1, identificador: 'SMS-TESTE01' };
  }
}

class CanalQuebrado implements Enviador {
  readonly canal = 'sms';
  async enviar(): Promise<ResultadoEnvio> {
    throw new Error('provedor fora do ar');
  }
}

/** Falha nas primeiras `falhasAte` chamadas e entrega depois. */
class CanalInstavel implements Enviador {
  readonly canal = 'sms';
  chamadas = 0;

  constructor(private readonly falhasAte: number) {}

  async enviar(): Promise<ResultadoEnvio> {
    this.chamadas += 1;
    if (this.chamadas <= this.falhasAte) {
      throw new Error(`timeout do provedor na chamada ${this.chamadas}`);
    }
    return { entregue: true, canal: this.canal, tentativas: 1, identificador: 'SMS-OK9' };
  }
}

describe('ComLog', () => {
  it('registra tentativa e sucesso, nessa ordem', async () => {
    const log = new LogEmMemoria();
    const resultado = await new ComLog(new CanalConfiavel(), log).enviar(NOTIFICACAO);

    expect(log.linhas).toHaveLength(2);
    expect(log.linhas[0]).toMatch(/^tentativa\b/);
    expect(log.linhas[1]).toMatch(/^sucesso\b/);
    expect(resultado.entregue).toBe(true);
  });

  it('põe canal e destinatário na linha de tentativa', async () => {
    const log = new LogEmMemoria();
    await new ComLog(new CanalConfiavel(), log).enviar(NOTIFICACAO);

    expect(log.linhas[0]).toContain('canal=sms');
    expect(log.linhas[0]).toContain('destinatario=+5511999990000');
  });

  it('põe o identificador do provedor na linha de sucesso', async () => {
    const log = new LogEmMemoria();
    await new ComLog(new CanalConfiavel(), log).enviar(NOTIFICACAO);

    expect(log.linhas[1]).toContain('identificador=SMS-TESTE01');
  });

  it('registra a falha com o motivo informado pelo canal', async () => {
    const log = new LogEmMemoria();
    await new ComLog(new CanalQuebrado(), log).enviar(NOTIFICACAO).catch(() => undefined);

    expect(log.linhas).toHaveLength(2);
    expect(log.linhas[1]).toMatch(/^falha\b/);
    expect(log.linhas[1]).toContain('motivo=provedor fora do ar');
  });

  it('não altera o resultado de um envio bem-sucedido', async () => {
    const semLog = await new CanalConfiavel().enviar();
    const comLog = await new ComLog(new CanalConfiavel(), new LogEmMemoria()).enviar(NOTIFICACAO);

    expect(comLog).toEqual(semLog);
  });

  it('se passa pelo enviador embrulhado, inclusive no canal', () => {
    const decorado = new ComLog(new CanalConfiavel(), new LogEmMemoria());
    expect(decorado.canal).toBe('sms');
  });
});

describe('ComRetentativa', () => {
  it('repete até o canal entregar e informa a tentativa que funcionou', async () => {
    const instavel = new CanalInstavel(2);
    const resultado = await new ComRetentativa(instavel, 3, 0).enviar(NOTIFICACAO);

    expect(resultado.entregue).toBe(true);
    expect(resultado.tentativas).toBe(3);
    expect(instavel.chamadas).toBe(3);
  });

  it('desiste depois do limite e relança o último erro', async () => {
    const instavel = new CanalInstavel(99);
    const enviador = new ComRetentativa(instavel, 3, 0);

    await expect(enviador.enviar(NOTIFICACAO)).rejects.toThrow('na chamada 3');
    expect(instavel.chamadas).toBe(3);
  });

  it('espera entre as tentativas sem bloquear o event loop', async () => {
    const enviador = new ComRetentativa(new CanalInstavel(1), 3, 60);

    const comeco = Date.now();
    await enviador.enviar(NOTIFICACAO);

    expect(Date.now() - comeco).toBeGreaterThanOrEqual(50);
  });

  it('recusa configuração com menos de uma tentativa', () => {
    expect(() => new ComRetentativa(new CanalConfiavel(), 0)).toThrow('no mínimo 1');
  });
});
