"""Taxa de adquirência do cartão corporativo."""

from app.taxas import taxa_meio_pagamento


def test_taxa_no_debito():
    assert taxa_meio_pagamento(1000.0, "debito") == 9.00


def test_taxa_no_credito():
    assert taxa_meio_pagamento(1000.0, "credito") == 29.00


def test_taxa_arredonda_em_duas_casas():
    assert taxa_meio_pagamento(603.40, "credito") == 17.50
