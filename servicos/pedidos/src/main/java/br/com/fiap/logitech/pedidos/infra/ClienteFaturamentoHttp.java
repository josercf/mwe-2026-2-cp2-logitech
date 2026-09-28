package br.com.fiap.logitech.pedidos.infra;

import br.com.fiap.logitech.pedidos.dominio.SolicitacaoFatura;
import br.com.fiap.logitech.pedidos.faturamento.ClienteFaturamento;
import br.com.fiap.logitech.pedidos.faturamento.FaturaRecusadaException;
import br.com.fiap.logitech.pedidos.faturamento.FaturamentoIndisponivelException;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import java.io.IOException;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;

/**
 * Cliente HTTP do serviço de Faturamento: {@code POST /api/v1/faturas}.
 */
@Component
public class ClienteFaturamentoHttp implements ClienteFaturamento {

    private final HttpClient http;
    private final ObjectMapper json = new ObjectMapper();
    private final String urlBase;
    private final Duration tempoLimite;

    public ClienteFaturamentoHttp(@Value("${logitech.faturamento.url}") String urlBase,
                                  @Value("${logitech.faturamento.timeout-ms}") long timeoutMs) {
        this.urlBase = urlBase.endsWith("/") ? urlBase.substring(0, urlBase.length() - 1) : urlBase;
        this.tempoLimite = Duration.ofMillis(timeoutMs);
        this.http = HttpClient.newBuilder().connectTimeout(this.tempoLimite).build();
    }

    @Override
    public String emitir(SolicitacaoFatura solicitacao) {
        HttpResponse<String> resposta = enviar(solicitacao);
        int status = resposta.statusCode();
        if (status >= 400 && status < 500) {
            throw new FaturaRecusadaException(solicitacao.pedidoId(), status, resposta.body());
        }
        if (status < 200 || status >= 300) {
            throw new FaturamentoIndisponivelException(
                    "faturamento respondeu HTTP " + status + ": " + resposta.body());
        }
        return numeroDaNota(resposta.body());
    }

    private HttpResponse<String> enviar(SolicitacaoFatura solicitacao) {
        try {
            HttpRequest requisicao = HttpRequest.newBuilder()
                    .uri(URI.create(urlBase + "/api/v1/faturas"))
                    .timeout(tempoLimite)
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(json.writeValueAsString(solicitacao)))
                    .build();
            return http.send(requisicao, HttpResponse.BodyHandlers.ofString());
        } catch (InterruptedException erro) {
            Thread.currentThread().interrupt();
            throw new FaturamentoIndisponivelException("chamada ao faturamento interrompida", erro);
        } catch (IOException erro) {
            throw new FaturamentoIndisponivelException(
                    "não foi possível falar com o faturamento em " + urlBase, erro);
        }
    }

    private String numeroDaNota(String corpo) {
        try {
            JsonNode no = json.readTree(corpo);
            String numero = no.path("numeroNotaFiscal").asText(null);
            if (numero == null || numero.isBlank()) {
                throw new FaturamentoIndisponivelException(
                        "faturamento respondeu sem numeroNotaFiscal: " + corpo);
            }
            return numero;
        } catch (IOException erro) {
            throw new FaturamentoIndisponivelException("resposta ilegível do faturamento: " + corpo, erro);
        }
    }
}
