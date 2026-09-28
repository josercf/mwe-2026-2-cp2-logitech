"""Registro de modalidades de frete.

A rota HTTP não sabe quais modalidades existem: pergunta ao registro qual
estratégia atende aquele nome, e executa. Modalidade nova entra com uma
classe em `estrategias.py` e uma linha aqui, sem tocar em `app/main.py`.
"""

from .estrategias import (
    EstrategiaFrete,
    FreteEconomico,
    FreteExpresso,
    FretePadrao,
    FreteRefrigerado,
)

REGISTRO: dict[str, EstrategiaFrete] = {}
"""Mapa `nome da modalidade -> instância da estratégia`, montado na importação."""


def registrar(estrategia: EstrategiaFrete) -> None:
    """Põe uma estratégia no registro, indexada pelo próprio `modalidade`.

    Recusa nome repetido: duas estratégias disputando a mesma modalidade é
    erro de programação que aparece na importação do módulo.
    """
    nome = estrategia.modalidade
    if nome in REGISTRO:
        raise ValueError("modalidade já registrada: %s" % nome)
    REGISTRO[nome] = estrategia


def obter(modalidade: str) -> EstrategiaFrete:
    """Devolve a estratégia de uma modalidade. Levanta `KeyError` se não houver."""
    try:
        return REGISTRO[modalidade]
    except KeyError:
        raise KeyError(
            "modalidade não suportada: %r. Disponíveis: %s"
            % (modalidade, ", ".join(modalidades()))) from None


def modalidades() -> list[str]:
    """Os nomes registrados, em ordem alfabética."""
    return sorted(REGISTRO)


registrar(FreteExpresso())
registrar(FreteEconomico())
registrar(FretePadrao())
registrar(FreteRefrigerado())
