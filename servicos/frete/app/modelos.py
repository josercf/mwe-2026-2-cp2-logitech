"""Contrato HTTP do serviço de frete, descrito com Pydantic.

Os nomes dos campos seguem a ADR-006. A mesma classe dá a validação de
entrada, a serialização de saída e o `/docs` do FastAPI.
"""

from typing import Literal

from pydantic import BaseModel, Field


class PedidoCotacao(BaseModel):
    """O que o cliente da API envia para pedir uma cotação."""

    origem: str = Field(min_length=3, max_length=3,
                        description="Código do centro de distribuição de origem, por exemplo SAO")
    destino: str = Field(min_length=3, max_length=3,
                         description="Código do centro de distribuição de destino, por exemplo LDB")
    pesoKg: float = Field(gt=0, le=30000,
                          description="Peso da carga em quilogramas")
    modalidade: str = Field(min_length=3, max_length=40,
                            description="Nome da modalidade de frete registrada")
    meioPagamento: Literal["debito", "credito"] = Field(
        default="credito",
        description="Função do cartão corporativo usada no pagamento")


class RespostaCotacao(BaseModel):
    """O que o serviço devolve. Valores em reais, prazo em dias corridos."""

    valor: float
    pedagio: float
    taxaPagamento: float
    total: float
    prazoDias: int
    modalidade: str


class RespostaRestricoes(BaseModel):
    """Janelas de restrição de circulação de caminhão numa cidade."""

    cidade: str
    janelas: list[str]


class RespostaSaude(BaseModel):
    """Corpo de `GET /health`, o mesmo em todos os serviços da plataforma."""

    status: str = "ok"
