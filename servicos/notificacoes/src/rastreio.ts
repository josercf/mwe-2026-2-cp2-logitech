/**
 * Adaptador do rastreamento da transportadora parceira.
 *
 * Única classe do serviço que conhece o formato legado da parceira: campos
 * abreviados em maiúsculas, data em `dd/MM/aaaa HH:mm` e situação própria.
 */

export interface RastreioLegado {
  COD_OBJ: string;
  DT_ULT_MOV: string;
  SIT: string;
  UF_ULT: string;
  DESC_SIT: string;
}

export type StatusRastreio =
  | 'postado'
  | 'em_transito'
  | 'saiu_para_entrega'
  | 'entregue'
  | 'devolvido'
  | 'desconhecido';

export interface Rastreio {
  codigoRastreio: string;
  status: StatusRastreio;
  atualizadoEm: string;
  uf: string;
  descricao: string;
}

export const MAPA_DE_SITUACAO: Record<string, StatusRastreio> = {
  POSTADO: 'postado',
  EM_TRANSITO: 'em_transito',
  SAIU_ENTREGA: 'saiu_para_entrega',
  ENTREGUE: 'entregue',
  DEVOLVIDO_REMETENTE: 'devolvido',
};

const DATA_DA_PARCEIRA = /^(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2})$/;

function paraIso(data: string | undefined): string {
  const partes = DATA_DA_PARCEIRA.exec((data ?? '').trim());
  if (partes === null) {
    return '';
  }
  const [, dia, mes, ano, hora, minuto] = partes;
  return `${ano}-${mes}-${dia}T${hora}:${minuto}:00`;
}

export class AdaptadorRastreioLegado {
  adaptar(bruto: RastreioLegado): Rastreio {
    return {
      codigoRastreio: (bruto.COD_OBJ ?? '').trim(),
      status: MAPA_DE_SITUACAO[bruto.SIT] ?? 'desconhecido',
      atualizadoEm: paraIso(bruto.DT_ULT_MOV),
      uf: (bruto.UF_ULT ?? '').toUpperCase(),
      descricao: bruto.DESC_SIT ?? '',
    };
  }
}
