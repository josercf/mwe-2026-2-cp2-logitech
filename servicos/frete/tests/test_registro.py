"""O registro de modalidades."""

import pytest

from app.estrategias import Cotacao, EstrategiaFrete, FreteExpresso
from app.registro import REGISTRO, modalidades, obter, registrar


def test_as_quatro_modalidades_estao_registradas():
    assert set(REGISTRO) == {"expresso", "economico", "padrao", "refrigerado"}


def test_obter_devolve_uma_estrategia_de_verdade():
    estrategia = obter("expresso")
    assert isinstance(estrategia, EstrategiaFrete)
    assert isinstance(estrategia.cotar(500.0, 100.0), Cotacao)


def test_obter_recusa_modalidade_desconhecida():
    with pytest.raises(KeyError):
        obter("teletransporte")


def test_registrar_recusa_nome_repetido():
    with pytest.raises(ValueError):
        registrar(FreteExpresso())


def test_modalidades_vem_ordenado_e_sem_repeticao():
    lista = modalidades()
    assert lista == sorted(lista)
    assert len(lista) == len(set(lista))
