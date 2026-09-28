"""Fixtures comuns: a concessionária é substituída por um dublê em memória.

Nenhum teste fala com a rede. As tarifas do dublê são as do simulador.
"""

import pytest

import app.main as main

TARIFAS_DUBLE = {("SAO", "LDB"): 58.40, ("SAO", "RIO"): 71.90}
RESTRICOES_DUBLE = {"SAO": ["seg-sex 05:00-09:00", "seg-sex 16:00-20:00"]}


@pytest.fixture(autouse=True)
def concessionaria_em_memoria(monkeypatch):
    monkeypatch.setattr(
        main, "tarifa_pedagio",
        lambda origem, destino: TARIFAS_DUBLE.get((origem, destino), 60.00))
    monkeypatch.setattr(
        main, "restricoes_circulacao",
        lambda cidade: RESTRICOES_DUBLE.get(cidade.upper(), []))
