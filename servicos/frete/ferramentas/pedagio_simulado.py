"""Simulador local da API da concessionária de rodovias.

Responde no lugar da concessionária em desenvolvimento e nas medições, com
os mesmos caminhos e o mesmo formato de resposta. A latência de cada
resposta vem de `PEDAGIO_LATENCIA_MS` (padrão 300, a mediana observada na
API real em agosto de 2026).

Para subir:

    uvicorn ferramentas.pedagio_simulado:app --port 8010
"""

import asyncio
import os

from fastapi import FastAPI

LATENCIA_S = int(os.environ.get("PEDAGIO_LATENCIA_MS", "300")) / 1000

TARIFAS = {
    ("SAO", "LDB"): 58.40,
    ("SAO", "RIO"): 71.90,
    ("SAO", "CWB"): 43.20,
    ("SAO", "BHZ"): 39.60,
    ("SAO", "POA"): 97.30,
    ("SAO", "SSA"): 112.80,
    ("RIO", "BHZ"): 48.70,
    ("RIO", "VIX"): 36.10,
    ("CWB", "POA"): 54.50,
    ("BHZ", "SSA"): 66.00,
}
TARIFA_PADRAO = 60.00

RESTRICOES = {
    "SAO": ["seg-sex 05:00-09:00", "seg-sex 16:00-20:00"],
    "RIO": ["seg-sex 06:00-10:00", "seg-sex 17:00-20:00"],
    "BHZ": ["seg-sex 07:00-09:00"],
}

app = FastAPI(title="Concessionária (simulador)")


@app.get("/api/v1/tarifas")
async def tarifas(origem: str, destino: str) -> dict:
    await asyncio.sleep(LATENCIA_S)
    valor = TARIFAS.get((origem, destino)) or TARIFAS.get((destino, origem))
    return {"origem": origem, "destino": destino,
            "valor": valor if valor is not None else TARIFA_PADRAO}


@app.get("/api/v1/restricoes/{cidade}")
async def restricoes(cidade: str) -> dict:
    await asyncio.sleep(LATENCIA_S)
    return {"cidade": cidade, "janelas": RESTRICOES.get(cidade, [])}
