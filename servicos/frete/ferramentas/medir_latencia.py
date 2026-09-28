"""Dispara N requisições ao mesmo tempo e mede cada uma e o conjunto.

Uso:

    python -m ferramentas.medir_latencia --n 10
    python -m ferramentas.medir_latencia --n 10 --url http://localhost:8010/api/v1/tarifas?origem=SAO&destino=LDB

Sem `--url`, envia `POST /api/v1/frete/cotacao` para localhost:8000 com a
carga de referência (SAO -> LDB, 100 kg, expresso).
"""

import argparse
import asyncio
import statistics
import time

import httpx

URL_COTACAO = "http://localhost:8000/api/v1/frete/cotacao"
CARGA = {"origem": "SAO", "destino": "LDB", "pesoKg": 100.0,
         "modalidade": "expresso", "meioPagamento": "credito"}


async def uma(cliente: httpx.AsyncClient, indice: int, url: str,
              t0: float) -> tuple[int, float, float, int]:
    inicio = time.perf_counter()
    if url == URL_COTACAO:
        resposta = await cliente.post(url, json=CARGA)
    else:
        resposta = await cliente.get(url)
    fim = time.perf_counter()
    return indice, inicio - t0, fim - t0, resposta.status_code


async def principal(n: int, url: str) -> None:
    limites = httpx.Limits(max_connections=n, max_keepalive_connections=n)
    async with httpx.AsyncClient(timeout=30.0, limits=limites) as cliente:
        t0 = time.perf_counter()
        resultados = await asyncio.gather(
            *(uma(cliente, i + 1, url, t0) for i in range(n)))
        total = time.perf_counter() - t0

    duracoes = []
    for indice, inicio, fim, status in sorted(resultados, key=lambda r: r[2]):
        duracoes.append(fim - inicio)
        print("req %02d  inicio +%.3fs  fim +%.3fs  duracao %.3fs  status %d"
              % (indice, inicio, fim, fim - inicio, status))
    print("requisicoes %d  total %.3fs  menor %.3fs  mediana %.3fs  maior %.3fs"
          % (n, total, min(duracoes), statistics.median(duracoes),
             max(duracoes)))


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--n", type=int, default=10)
    parser.add_argument("--url", default=URL_COTACAO)
    argumentos = parser.parse_args()
    asyncio.run(principal(argumentos.n, argumentos.url))
