/**
 * API HTTP do serviço de notificações da LogiTech (Node 22, porta 3001).
 *
 * Contrato da ADR-006: `GET /health` e `POST /api/v1/notificacoes` recebendo
 * `{canal, destinatario, mensagem}`. As rotas `/esquema` e `/rastreio/:codigo`
 * são internas e não entram no contrato da plataforma.
 */

import http from 'node:http';

import { lerConfigEnvio, PilhasPorCanal } from './composicao';
import { CANAIS, EsquemaNotificacao, jsonSchemaDoContrato } from './esquema';
import { LogDeConsole } from './log';
import { AdaptadorRastreioLegado, type RastreioLegado } from './rastreio';

const PORTA = Number(process.env.PORT ?? 3001);
const URL_PARCEIRA = process.env.LOGITECH_RASTREIO_PARCEIRA_URL ?? 'http://localhost:9090';

const log = new LogDeConsole();
const config = lerConfigEnvio();
const pilhas = new PilhasPorCanal(log, config);
const adaptador = new AdaptadorRastreioLegado();

function responder(res: http.ServerResponse, status: number, corpo: unknown): void {
  const texto = JSON.stringify(corpo);
  res.writeHead(status, {
    'content-type': 'application/json; charset=utf-8',
    'content-length': Buffer.byteLength(texto),
  });
  res.end(texto);
}

function lerCorpo(req: http.IncomingMessage): Promise<string> {
  return new Promise((resolva, rejeite) => {
    const partes: Buffer[] = [];
    req.on('data', (parte: Buffer) => partes.push(parte));
    req.on('end', () => resolva(Buffer.concat(partes).toString('utf-8')));
    req.on('error', rejeite);
  });
}

async function postNotificacoes(req: http.IncomingMessage, res: http.ServerResponse): Promise<void> {
  let json: unknown;
  try {
    json = JSON.parse(await lerCorpo(req));
  } catch {
    responder(res, 400, { erro: 'corpo não é um JSON válido' });
    return;
  }

  const validacao = EsquemaNotificacao.safeParse(json);
  if (!validacao.success) {
    responder(res, 422, {
      erro: 'notificação fora do contrato',
      detalhes: validacao.error.issues.map((i) => `${i.path.join('.')}: ${i.message}`),
      canaisDisponiveis: [...CANAIS].sort(),
    });
    return;
  }

  try {
    const resultado = await pilhas.para(validacao.data.canal).enviar(validacao.data);
    responder(res, resultado.entregue ? 202 : 502, resultado);
  } catch (erro) {
    responder(res, 502, {
      erro: 'canal não conseguiu entregar a notificação',
      motivo: erro instanceof Error ? erro.message : String(erro),
    });
  }
}

async function getRastreio(codigo: string, res: http.ServerResponse): Promise<void> {
  try {
    const resposta = await fetch(`${URL_PARCEIRA}/consulta?objeto=${encodeURIComponent(codigo)}`);
    if (!resposta.ok) {
      responder(res, 502, { erro: `parceira respondeu ${resposta.status}` });
      return;
    }
    const bruto = (await resposta.json()) as RastreioLegado;
    responder(res, 200, adaptador.adaptar(bruto));
  } catch (erro) {
    responder(res, 502, {
      erro: 'rastreamento da parceira indisponível',
      motivo: erro instanceof Error ? erro.message : String(erro),
    });
  }
}

export const servidor = http.createServer((req, res) => {
  const url = new URL(req.url ?? '/', `http://localhost:${PORTA}`);
  const rota = `${req.method} ${url.pathname}`;

  if (rota === 'GET /health') {
    responder(res, 200, { status: 'ok' });
    return;
  }
  if (rota === 'GET /api/v1/notificacoes/esquema') {
    responder(res, 200, jsonSchemaDoContrato());
    return;
  }
  if (rota === 'POST /api/v1/notificacoes') {
    void postNotificacoes(req, res);
    return;
  }
  if (req.method === 'GET' && url.pathname.startsWith('/api/v1/rastreio/')) {
    void getRastreio(url.pathname.slice('/api/v1/rastreio/'.length), res);
    return;
  }

  responder(res, 404, { erro: 'rota não encontrada', rota });
});

if (process.argv[1]?.endsWith('servidor.ts')) {
  servidor.listen(PORTA, () => {
    log.registrar(`notificacoes ouvindo em http://localhost:${PORTA}`);
    log.registrar(`pilha de envio maxTentativas=${config.maxTentativas} esperaMs=${config.esperaMs}`);
  });
}
