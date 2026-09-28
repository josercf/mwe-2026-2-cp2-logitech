# PR #57: Strategy para a taxa de meio de pagamento do frete

- **Autor:** Rafael Nunes (time de frete)
- **Base:** `main` · **Estado:** aberto, aguardando revisão
- **Arquivos:** `servicos/frete/app/taxas.py`, `servicos/frete/app/taxas_pagamento.py` (novo), `servicos/frete/tests/test_taxas_pagamento.py` (novo)

## Descrição

A taxa de adquirência do cartão corporativo hoje é decidida por um `if` em
`app/taxas.py`. Este PR leva a taxa para o mesmo desenho que as modalidades de
frete já usam: um `Protocol` (`TaxaPagamento`), uma classe por meio de pagamento
(`TaxaDebito`, `TaxaCredito`) e um registro (`REGISTRO_TAXAS`) consultado por
`obter_taxa`.

Motivação:

1. Deixa a taxa aberta para extensão e fechada para modificação, como pede o
   Open/Closed.
2. Mantém um só estilo no serviço: quem já conhece o registro de modalidades
   entende o registro de taxas sem ler nada novo.
3. Cada taxa passa a ser testável isoladamente.

Para débito e crédito, os dois meios aceitos pela API, o comportamento não muda:
`taxa_meio_pagamento` continua com a mesma assinatura,
e a suíte existente passa sem alteração (35 testes verdes, 30 antigos e 5 novos).

## Diff

```diff
diff --git a/servicos/frete/app/taxas.py b/servicos/frete/app/taxas.py
index 9179bc6..0972ea8 100644
--- a/servicos/frete/app/taxas.py
+++ b/servicos/frete/app/taxas.py
@@ -5,12 +5,9 @@ os clientes: uma para a função débito e outra para a função crédito do
 cartão. Pagamento por outro meio não passa por esta função.
 """
 
-TAXA_DEBITO = 0.009
-TAXA_CREDITO = 0.029
+from .taxas_pagamento import obter_taxa
 
 
 def taxa_meio_pagamento(valor: float, meio: str) -> float:
     """Taxa em reais sobre o valor cobrado, arredondada em duas casas."""
-    if meio == "debito":
-        return round(valor * TAXA_DEBITO, 2)
-    return round(valor * TAXA_CREDITO, 2)
+    return obter_taxa(meio).calcular(valor)
diff --git a/servicos/frete/app/taxas_pagamento.py b/servicos/frete/app/taxas_pagamento.py
new file mode 100644
index 0000000..6a9185a
--- /dev/null
+++ b/servicos/frete/app/taxas_pagamento.py
@@ -0,0 +1,45 @@
+"""Taxa de adquirência como Strategy, no mesmo desenho das modalidades de frete."""
+
+from typing import Protocol, runtime_checkable
+
+
+@runtime_checkable
+class TaxaPagamento(Protocol):
+    """Contrato que toda taxa de meio de pagamento precisa cumprir."""
+
+    meio: str
+
+    def calcular(self, valor: float) -> float: ...
+
+
+class TaxaDebito:
+    meio = "debito"
+    percentual = 0.009
+
+    def calcular(self, valor: float) -> float:
+        return round(valor * self.percentual, 2)
+
+
+class TaxaCredito:
+    meio = "credito"
+    percentual = 0.029
+
+    def calcular(self, valor: float) -> float:
+        return round(valor * self.percentual, 2)
+
+
+REGISTRO_TAXAS: dict[str, TaxaPagamento] = {}
+
+
+def registrar_taxa(taxa: TaxaPagamento) -> None:
+    if taxa.meio in REGISTRO_TAXAS:
+        raise ValueError("meio de pagamento já registrado: %s" % taxa.meio)
+    REGISTRO_TAXAS[taxa.meio] = taxa
+
+
+def obter_taxa(meio: str) -> TaxaPagamento:
+    return REGISTRO_TAXAS[meio]
+
+
+registrar_taxa(TaxaDebito())
+registrar_taxa(TaxaCredito())
diff --git a/servicos/frete/tests/test_taxas_pagamento.py b/servicos/frete/tests/test_taxas_pagamento.py
new file mode 100644
index 0000000..7387560
--- /dev/null
+++ b/servicos/frete/tests/test_taxas_pagamento.py
@@ -0,0 +1,30 @@
+"""O protocolo e o registro das taxas de meio de pagamento."""
+
+import pytest
+
+from app.taxas_pagamento import (
+    REGISTRO_TAXAS,
+    TaxaCredito,
+    TaxaDebito,
+    TaxaPagamento,
+    obter_taxa,
+    registrar_taxa,
+)
+
+
+@pytest.mark.parametrize("classe", [TaxaDebito, TaxaCredito])
+def test_cada_taxa_cumpre_o_protocolo(classe):
+    assert isinstance(classe(), TaxaPagamento)
+
+
+def test_registro_tem_debito_e_credito():
+    assert set(REGISTRO_TAXAS) == {"debito", "credito"}
+
+
+def test_obter_taxa_devolve_a_instancia_registrada():
+    assert obter_taxa("debito").calcular(1000.0) == 9.00
+
+
+def test_registrar_recusa_meio_repetido():
+    with pytest.raises(ValueError):
+        registrar_taxa(TaxaCredito())
```
