#!/usr/bin/env bash
# Sobe o provedor instável e o serviço de notificações com a configuração de
# produção, envia os avisos de entrega do dia e guarda log e respostas.
# Uso, a partir da raiz do repositório:  bash evidencias/case-d/coleta/coletar.sh
set -euo pipefail
RAIZ="$(cd "$(dirname "$0")/../../.." && pwd)"
SAIDA="$RAIZ/evidencias/case-d"
SERVICO="$RAIZ/servicos/notificacoes"

node "$SAIDA/coleta/provedor-sms-instavel.mjs" > "$SAIDA/provedor-sms.log" &
PID_PROVEDOR=$!
(cd "$SERVICO" && PORT=3001 SMS_PROVEDOR_URL=http://localhost:9091 NOTIFICACOES_MAX_TENTATIVAS=3 \
  exec npx tsx src/servidor.ts) > "$SAIDA/notificacoes.log" &
PID_SERVICO=$!
trap 'kill $PID_SERVICO $PID_PROVEDOR 2>/dev/null || true' EXIT

for _ in $(seq 1 50); do curl -sf http://localhost:3001/health >/dev/null && break; sleep 0.2; done

: > "$SAIDA/respostas-http.txt"
enviar() {
  local corpo="$1"
  printf '> POST /api/v1/notificacoes %s\n' "$corpo" >> "$SAIDA/respostas-http.txt"
  curl -s -o /dev/stdout -w '\n< HTTP %{http_code}\n\n' -H 'content-type: application/json' \
    -d "$corpo" http://localhost:3001/api/v1/notificacoes >> "$SAIDA/respostas-http.txt"
}
enviar '{"canal":"sms","destinatario":"+5511987650001","mensagem":"LogiTech: seu pedido 88412 sai para entrega hoje entre 13h e 17h."}'
enviar '{"canal":"email","destinatario":"cliente.88413@logitech.example","mensagem":"Seu pedido 88413 sai para entrega hoje."}'
enviar '{"canal":"sms","destinatario":"+5521998760002","mensagem":"LogiTech: seu pedido 88414 sai para entrega hoje entre 8h e 12h."}'
enviar '{"canal":"sms","destinatario":"+5531976540003","mensagem":"LogiTech: seu pedido 88415 sai para entrega hoje entre 13h e 17h."}'
sleep 1

# Conferência direta no provedor, sem passar pelo serviço: segunda chamada
# para o mesmo telefone do primeiro aviso.
{
  printf '> POST http://localhost:9091/v1/mensagens {"telefone":"+5511987650001"}\n'
  curl -s -w '\n< HTTP %{http_code}\n' -H 'content-type: application/json' \
    -d '{"telefone":"+5511987650001","texto":"LogiTech: seu pedido 88412 sai para entrega hoje entre 13h e 17h."}' \
    http://localhost:9091/v1/mensagens
} > "$SAIDA/conferencia-provedor.txt"
sleep 0.5
