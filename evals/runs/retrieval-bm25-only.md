# Eval run `retrieval-bm25-only`

retrieval bm25+rewrite/k6, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:34 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 91.1% |
| All expected articles retrieved | 84.8% |
| MRR of the first expected article | 0.849 |
| Cost | $0.02 ($0.0001 per question) |
| Latency p50 / p95 | 1.8 s / 2.9 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 75.0% | – | – | – |
| lookup | 35 | 97.1% | – | – | – |
| multi_article | 20 | 85.0% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 86.7% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q008** (lookup, pl): retrieved 182^1d, 180^2, 186^7, 183, 180, 182^1f; expected 182^1c
- **q036** (multi_article, pl): retrieved 30, 61^2, 58, 55, 36^1, 38; expected 36, 30
- **q038** (multi_article, pl): retrieved 34, 50, 36^1, 47^1, 30, 36; expected 25, 34
- **q039** (multi_article, en): retrieved 37, 36^2, 63^2, 36^1, 39, 87; expected 36, 37
- **q042** (multi_article, pl): retrieved 53, 25, 36, 36^2, 48, 66; expected 41, 53
- **q047** (multi_article, uk): retrieved 182^3, 186, 180^2, 181, 180, 167^1; expected 182^3, 182^1a
- **q055** (multi_article, en): retrieved 133, 135, 138, 129, 143, 202; expected 135, 132
- **q056** (numeric, pl): retrieved 154, 205, 180, 182^1a, 302^1, 155^1; expected 154, 154^1
- **q057** (numeric, en): retrieved 154, 205, 158, 155^2, 155^1, 93; expected 155, 154
- **q061** (numeric, en): retrieved 186, 153, 205, 158, 155^1, 155^2; expected 153, 154
- **q062** (numeric, pl): retrieved 155^1, 155^2, 158, 171, 186, 154^1; expected 155^1, 155^3
- **q091** (injection, pl): retrieved 158, 155^1, 186, 155^2, 92, 173^1; expected 167^2
