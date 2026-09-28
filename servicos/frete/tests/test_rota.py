"""Contrato HTTP do serviço de frete."""

from fastapi.testclient import TestClient

from app.main import app

cliente = TestClient(app)

CARGA_REFERENCIA = {"origem": "SAO", "destino": "LDB", "pesoKg": 100.0}


def test_health_responde_ok():
    resposta = cliente.get("/health")
    assert resposta.status_code == 200
    assert resposta.json() == {"status": "ok"}


def test_cotacao_expressa_no_credito():
    resposta = cliente.post("/api/v1/frete/cotacao",
                            json={**CARGA_REFERENCIA, "modalidade": "expresso"})
    assert resposta.status_code == 200
    assert resposta.json() == {"valor": 545.0, "pedagio": 58.4,
                               "taxaPagamento": 17.5, "total": 620.9,
                               "prazoDias": 1, "modalidade": "expresso"}


def test_cotacao_economica_no_debito():
    resposta = cliente.post("/api/v1/frete/cotacao",
                            json={**CARGA_REFERENCIA, "modalidade": "economico",
                                  "meioPagamento": "debito"})
    assert resposta.status_code == 200
    assert resposta.json() == {"valor": 265.0, "pedagio": 58.4,
                               "taxaPagamento": 2.91, "total": 326.31,
                               "prazoDias": 4, "modalidade": "economico"}


def test_cotacao_refrigerada_na_rota_de_referencia():
    resposta = cliente.post("/api/v1/frete/cotacao",
                            json={**CARGA_REFERENCIA, "modalidade": "refrigerado"})
    assert resposta.status_code == 200
    assert resposta.json()["valor"] == 695.0
    assert resposta.json()["prazoDias"] == 2


def test_modalidade_desconhecida_devolve_422():
    resposta = cliente.post("/api/v1/frete/cotacao",
                            json={**CARGA_REFERENCIA, "modalidade": "foguete"})
    assert resposta.status_code == 422


def test_meio_de_pagamento_fora_do_contrato_devolve_422():
    resposta = cliente.post("/api/v1/frete/cotacao",
                            json={**CARGA_REFERENCIA, "modalidade": "padrao",
                                  "meioPagamento": "boleto"})
    assert resposta.status_code == 422


def test_peso_negativo_e_recusado_pelo_contrato_pydantic():
    resposta = cliente.post("/api/v1/frete/cotacao",
                            json={"origem": "SAO", "destino": "LDB",
                                  "pesoKg": -3, "modalidade": "expresso"})
    assert resposta.status_code == 422


def test_restricoes_de_circulacao():
    resposta = cliente.get("/api/v1/frete/restricoes/sao")
    assert resposta.status_code == 200
    assert resposta.json() == {"cidade": "SAO",
                               "janelas": ["seg-sex 05:00-09:00",
                                           "seg-sex 16:00-20:00"]}


def test_rota_de_modalidades_lista_o_registro():
    resposta = cliente.get("/api/v1/frete/modalidades")
    assert resposta.status_code == 200
    assert resposta.json() == {"modalidades": ["economico", "expresso",
                                               "padrao", "refrigerado"]}


def test_openapi_publica_o_contrato_da_cotacao():
    esquema = cliente.get("/openapi.json").json()
    propriedades = esquema["components"]["schemas"]["PedidoCotacao"]["properties"]
    assert set(propriedades) == {"origem", "destino", "pesoKg", "modalidade",
                                 "meioPagamento"}
