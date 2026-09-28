"""API HTTP do serviço de frete da LogiTech (FastAPI, porta 8000).

Para subir:

    uvicorn app.main:app --host 0.0.0.0 --port 8000

Documentação OpenAPI gerada pelo Pydantic: http://localhost:8000/docs
"""

from fastapi import FastAPI, HTTPException

from .distancias import distancia_km
from .modelos import (
    PedidoCotacao,
    RespostaCotacao,
    RespostaRestricoes,
    RespostaSaude,
)
from .pedagio import restricoes_circulacao, tarifa_pedagio
from .registro import modalidades, obter
from .taxas import taxa_meio_pagamento

app = FastAPI(
    title="LogiTech Frete",
    version="1.3.0",
    description="Motor de cálculo de frete da LogiTech Enterprise AI Platform.",
)


@app.get("/health", response_model=RespostaSaude, tags=["infraestrutura"])
def saude() -> RespostaSaude:
    """Sonda de saúde exigida pela ADR-006, sem dependência externa."""
    return RespostaSaude(status="ok")


@app.get("/api/v1/frete/modalidades", tags=["frete"])
def listar_modalidades() -> dict[str, list[str]]:
    """As modalidades que o registro conhece."""
    return {"modalidades": modalidades()}


@app.get("/api/v1/frete/restricoes/{cidade}", response_model=RespostaRestricoes,
         tags=["frete"])
def consultar_restricoes(cidade: str) -> RespostaRestricoes:
    """Restrições de circulação de caminhão na cidade de destino."""
    janelas = restricoes_circulacao(cidade)
    return RespostaRestricoes(cidade=cidade.upper(), janelas=janelas)


@app.post("/api/v1/frete/cotacao", response_model=RespostaCotacao, tags=["frete"])
async def cotar(pedido: PedidoCotacao) -> RespostaCotacao:
    """Calcula o frete de uma carga entre dois centros de distribuição."""
    try:
        estrategia = obter(pedido.modalidade)
    except KeyError as ausente:
        raise HTTPException(status_code=422, detail=str(ausente)) from None

    cotacao = estrategia.cotar(
        distancia_km(pedido.origem, pedido.destino), pedido.pesoKg)
    pedagio = tarifa_pedagio(pedido.origem, pedido.destino)
    subtotal = round(cotacao.valor + pedagio, 2)
    taxa = taxa_meio_pagamento(subtotal, pedido.meioPagamento)

    return RespostaCotacao(
        valor=cotacao.valor,
        pedagio=pedagio,
        taxaPagamento=taxa,
        total=round(subtotal + taxa, 2),
        prazoDias=cotacao.prazo_dias,
        modalidade=cotacao.modalidade,
    )
