"""O protocolo e as quatro estratégias, na rota de referência (500 km, 100 kg)."""

import pytest

from app.estrategias import (
    Cotacao,
    EstrategiaFrete,
    FreteEconomico,
    FreteExpresso,
    FretePadrao,
    FreteRefrigerado,
    valor_base,
)

CLASSES = [FreteExpresso, FreteEconomico, FretePadrao, FreteRefrigerado]


def test_protocolo_declara_cotar_e_modalidade():
    assert hasattr(EstrategiaFrete, "cotar")
    assert "modalidade" in EstrategiaFrete.__annotations__


@pytest.mark.parametrize("classe", CLASSES)
def test_cada_estrategia_cumpre_o_protocolo(classe):
    estrategia = classe()
    assert isinstance(estrategia, EstrategiaFrete)
    assert isinstance(estrategia.modalidade, str) and estrategia.modalidade


@pytest.mark.parametrize("classe,valor,prazo", [
    (FreteExpresso, 545.00, 1),
    (FreteEconomico, 265.00, 4),
    (FretePadrao, 380.00, 2),
    (FreteRefrigerado, 695.00, 2),
])
def test_valor_e_prazo_na_rota_de_referencia(classe, valor, prazo):
    cotacao = classe().cotar(500.0, 100.0)
    assert isinstance(cotacao, Cotacao)
    assert cotacao.valor == pytest.approx(valor, abs=0.01)
    assert cotacao.prazo_dias == prazo
    assert cotacao.modalidade == classe.modalidade


def test_expresso_e_sempre_mais_caro_e_mais_rapido_que_economico():
    for distancia in (120.0, 500.0, 1960.0):
        for peso in (5.0, 100.0, 2000.0):
            rapido = FreteExpresso().cotar(distancia, peso)
            barato = FreteEconomico().cotar(distancia, peso)
            assert rapido.valor > barato.valor
            assert rapido.prazo_dias < barato.prazo_dias


def test_prazo_minimo_do_expresso_e_de_um_dia():
    assert FreteExpresso().cotar(12.0, 1.0).prazo_dias == 1


def test_valor_base_arredonda_em_duas_casas():
    assert valor_base(333.0, 7.0, 0.37, 0.19) == 124.54
