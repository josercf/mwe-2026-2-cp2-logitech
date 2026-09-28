package br.com.fiap.logitech.pedidos.web;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.Map;

@RestController
public class SaudeController {

    @GetMapping("/health")
    public Map<String, String> saude() {
        return Map.of("status", "ok", "servico", "pedidos");
    }
}
