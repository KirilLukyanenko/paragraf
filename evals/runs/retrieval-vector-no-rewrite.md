# Eval run `retrieval-vector-no-rewrite`

retrieval vector/k6, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:32 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 88.6% |
| All expected articles retrieved | 82.3% |
| MRR of the first expected article | 0.817 |
| Cost | $0.00 ($0.0000 per question) |
| Latency p50 / p95 | 0.2 s / 0.6 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 75.0% | – | – | – |
| lookup | 35 | 94.3% | – | – | – |
| multi_article | 20 | 77.5% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 90.0% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q008** (lookup, pl): retrieved 183, 182^3, 182^1e, 182^1f, 182^1a, 186; expected 182^1c
- **q012** (lookup, pl): retrieved 291, 113, 110, 36^1, 109, 112; expected 52
- **q036** (multi_article, pl): retrieved 36, 32, 36^1, 49, 25^1, 66; expected 36, 30
- **q039** (multi_article, en): retrieved 36, 93, 49, 47, 167^2, 34; expected 36, 37
- **q041** (multi_article, pl): retrieved 151^11, 151^4, 151^9, 151^12, 147, 133; expected 151^11, 151^1
- **q042** (multi_article, pl): retrieved 148^1, 37, 135, 23^1, 103^6, 174; expected 41, 53
- **q044** (multi_article, pl): retrieved 52, 97, 29^4, 29^3, 97^1, 36^1; expected 97, 99
- **q047** (multi_article, uk): retrieved 183, 186, 182^1c, 182^3, 180, 186^7; expected 182^3, 182^1a
- **q054** (multi_article, uk): retrieved 67^19, 186^3, 186^2, 67^31, 67^26, 178; expected 67^19, 67^23
- **q055** (multi_article, en): retrieved 151^7, 154^2, 132, 203, 133, 134; expected 135, 132
- **q056** (numeric, pl): retrieved 154, 155^1, 167^3, 155^2, 132, 167^2; expected 154, 154^1
- **q062** (numeric, pl): retrieved 154, 167^3, 167^2, 205, 155^1, 158; expected 155^1, 155^3
- **q063** (numeric, uk): retrieved 180, 183, 180^2, 180^1, 183^1, 182; expected 180, 182^1a
- **q094** (injection, en): retrieved 136, 138, 137, 144, 139, 135; expected 151
