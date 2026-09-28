import { describe, expect, it } from 'vitest';

import { AdaptadorRastreioLegado, type RastreioLegado } from '../src/rastreio';

const RESPOSTA_DA_PARCEIRA: RastreioLegado = {
  COD_OBJ: ' BR9912345 ',
  DT_ULT_MOV: '08/09/2026 14:32',
  SIT: 'EM_TRANSITO',
  UF_ULT: 'sp',
  DESC_SIT: 'Objeto em transito para a unidade de destino',
};

describe('AdaptadorRastreioLegado', () => {
  const adaptador = new AdaptadorRastreioLegado();

  it('traduz a resposta da parceira para o formato da LogiTech', () => {
    expect(adaptador.adaptar(RESPOSTA_DA_PARCEIRA)).toEqual({
      codigoRastreio: 'BR9912345',
      status: 'em_transito',
      atualizadoEm: '2026-09-08T14:32:00',
      uf: 'SP',
      descricao: 'Objeto em transito para a unidade de destino',
    });
  });

  it('não deixa vazar campo do vocabulário da parceira', () => {
    const rastreio = adaptador.adaptar(RESPOSTA_DA_PARCEIRA) as unknown as Record<string, unknown>;
    for (const campo of ['COD_OBJ', 'DT_ULT_MOV', 'SIT', 'UF_ULT', 'DESC_SIT']) {
      expect(rastreio[campo]).toBeUndefined();
    }
  });

  it('trata situação desconhecida sem derrubar a consulta', () => {
    const rastreio = adaptador.adaptar({ ...RESPOSTA_DA_PARCEIRA, SIT: 'EXTRAVIADO_TEMP' });
    expect(rastreio.status).toBe('desconhecido');
  });

  it('devolve data vazia quando a parceira manda formato inesperado', () => {
    const rastreio = adaptador.adaptar({ ...RESPOSTA_DA_PARCEIRA, DT_ULT_MOV: 'ontem à tarde' });
    expect(rastreio.atualizadoEm).toBe('');
  });

  it('tolera descrição ausente', () => {
    const rastreio = adaptador.adaptar({
      ...RESPOSTA_DA_PARCEIRA,
      DESC_SIT: undefined as unknown as string,
    });
    expect(rastreio.descricao).toBe('');
  });
});
