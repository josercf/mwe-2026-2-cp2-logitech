"""Taxa de adquirência do cartão corporativo usado para pagar o frete.

O contrato da LogiTech com a adquirente fixa duas taxas, iguais para todos
os clientes: uma para a função débito e outra para a função crédito do
cartão. Pagamento por outro meio não passa por esta função.
"""

TAXA_DEBITO = 0.009
TAXA_CREDITO = 0.029


def taxa_meio_pagamento(valor: float, meio: str) -> float:
    """Taxa em reais sobre o valor cobrado, arredondada em duas casas."""
    if meio == "debito":
        return round(valor * TAXA_DEBITO, 2)
    return round(valor * TAXA_CREDITO, 2)
