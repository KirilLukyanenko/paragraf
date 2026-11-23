# Eval run `retrieval-default-no-rewrite`

retrieval vector+bm25x0.5+named/k8, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:38 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 92.4% |
| All expected articles retrieved | 89.9% |
| MRR of the first expected article | 0.790 |
| Cost | $0.00 ($0.0000 per question) |
| Latency p50 / p95 | 0.2 s / 0.6 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 75.0% | – | – | – |
| lookup | 35 | 97.1% | – | – | – |
| multi_article | 20 | 87.5% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 90.0% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q012** (lookup, pl): retrieved 148^1, 291, 113, 110, 36^1, 109, 112, 66; expected 52
- **q041** (multi_article, pl): retrieved 151^11, 151^4, 147, 151^3, 151^9, 151^2, 151^12, 133; expected 151^11, 151^1
- **q042** (multi_article, pl): retrieved 148^1, 143, 36^2, 36, 34, 37, 135, 23^1; expected 41, 53
- **q047** (multi_article, uk): retrieved 183, 186, 182^1c, 182^3, 180, 186^7, 188, 180^2; expected 182^3, 182^1a
- **q054** (multi_article, uk): retrieved 67^19, 186^3, 186^2, 67^31, 67^26, 178, 53, 182^1d; expected 67^19, 67^23
- **q056** (numeric, pl): retrieved 154, 155^1, 167^3, 205, 167^2, 153, 161, 158; expected 154, 154^1
- **q062** (numeric, pl): retrieved 154, 205, 167^3, 158, 167^2, 167^1, 154^2, 168; expected 155^1, 155^3
- **q094** (injection, en): retrieved 136, 139, 144, 146, 137, 135, 148^1, 149; expected 151
