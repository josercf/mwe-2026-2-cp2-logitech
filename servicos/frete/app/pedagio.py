"""Cliente da API de tarifas e restrições da concessionária de rodovias.

A concessionária publica a tarifa de pedágio por trecho e as restrições de
circulação de caminhão por cidade. O endereço vem de `PEDAGIO_URL`; em
desenvolvimento, `ferramentas/pedagio_simulado.py` responde no lugar dela.
"""

import os

import requests

PEDAGIO_URL = os.environ.get("PEDAGIO_URL", "http://pedagio:8010")
TIMEOUT_S = 2.0


def tarifa_pedagio(origem: str, destino: str) -> float:
    """Soma das praças de pedágio do trecho, em reais, para caminhão de 2 eixos."""
    resposta = requests.get(
        f"{PEDAGIO_URL}/api/v1/tarifas",
        params={"origem": origem.upper(), "destino": destino.upper()},
        timeout=TIMEOUT_S,
    )
    resposta.raise_for_status()
    return float(resposta.json()["valor"])


def restricoes_circulacao(cidade: str) -> list[str]:
    """Janelas de restrição de circulação de caminhão vigentes na cidade."""
    resposta = requests.get(
        f"{PEDAGIO_URL}/api/v1/restricoes/{cidade.upper()}",
        timeout=TIMEOUT_S,
    )
    resposta.raise_for_status()
    return list(resposta.json()["janelas"])
