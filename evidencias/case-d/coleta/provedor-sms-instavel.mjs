// Simula o gateway do provedor de SMS atual durante a instabilidade de 24/09:
// a primeira chamada para cada telefone recebe 503, as seguintes são aceitas.
// Cada requisição recebida vira uma linha no stdout.
import http from 'node:http';

const PORTA = Number(process.env.PORTA ?? 9091);
const chamadasPorTelefone = new Map();
let sequencia = 0;

http
  .createServer((req, res) => {
    let texto = '';
    req.on('data', (parte) => (texto += parte));
    req.on('end', () => {
      const { telefone } = JSON.parse(texto || '{}');
      const chamadas = (chamadasPorTelefone.get(telefone) ?? 0) + 1;
      chamadasPorTelefone.set(telefone, chamadas);
      const status = chamadas === 1 ? 503 : 200;
      console.log(`[${new Date().toISOString()}] provedor ${req.method} ${req.url} telefone=${telefone} chamada=${chamadas} status=${status}`);
      res.writeHead(status, { 'content-type': 'application/json' });
      sequencia += 1;
      res.end(status === 200 ? JSON.stringify({ protocolo: `SMS-${String(sequencia).padStart(6, '0')}` }) : '{"erro":"indisponivel"}');
    });
  })
  .listen(PORTA);
