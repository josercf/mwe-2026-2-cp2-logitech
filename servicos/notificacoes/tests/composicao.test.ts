import { describe, expect, it } from 'vitest';

import { lerConfigEnvio, montarPilha, PilhasPorCanal } from '../src/composicao';
import { EnviadorEmail } from '../src/enviadores';
import { LogEmMemoria } from '../src/log';

const AVISO = {
  canal: 'email',
  destinatario: 'cliente@logitech.example',
  mensagem: 'Seu pedido 4471 sai para entrega hoje.',
};

describe('composição da pilha de envio', () => {
  it('lê a configuração padrão de produção', () => {
    expect(lerConfigEnvio({})).toEqual({ maxTentativas: 3, esperaMs: 200 });
  });

  it('respeita a configuração vinda do ambiente', () => {
    const config = lerConfigEnvio({ NOTIFICACOES_MAX_TENTATIVAS: '5', NOTIFICACOES_ESPERA_MS: '0' });
    expect(config).toEqual({ maxTentativas: 5, esperaMs: 0 });
  });

  it('registra tentativa e sucesso num envio de produção', async () => {
    const log = new LogEmMemoria();
    const pilha = montarPilha(new EnviadorEmail(), log, { maxTentativas: 3, esperaMs: 0 });

    const resultado = await pilha.enviar(AVISO);

    expect(log.linhas.map((linha) => linha.split(' ')[0])).toEqual(['tentativa', 'sucesso']);
    expect(resultado.entregue).toBe(true);
    expect(resultado.tentativas).toBe(1);
    expect(resultado.identificador).toMatch(/^EML-/);
  });

  it('reaproveita a mesma pilha para o mesmo canal', () => {
    const pilhas = new PilhasPorCanal(new LogEmMemoria(), { maxTentativas: 3, esperaMs: 0 });

    expect(pilhas.para('email')).toBe(pilhas.para('email'));
    expect(pilhas.para('email')).not.toBe(pilhas.para('whatsapp'));
  });
});
