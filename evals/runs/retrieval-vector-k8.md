# Eval run `retrieval-vector-k8`

retrieval vector+named+rewrite/k8, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:37 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 89.9% |
| All expected articles retrieved | 83.5% |
| MRR of the first expected article | 0.831 |
| Cost | $0.02 ($0.0002 per question) |
| Latency p50 / p95 | 2.0 s / 3.7 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 100.0% | – | – | – |
| lookup | 35 | 91.4% | – | – | – |
| multi_article | 20 | 82.5% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 90.0% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q006** (lookup, pl): retrieved 188, 53, 180, 181, 183, 185, 186, 182^3; expected 148^1
- **q008** (lookup, pl): retrieved 182^1f, 182^1e, 183, 182^3, 186^8a, 182^1a, 186^7, 186; expected 182^1c
- **q010** (lookup, pl): retrieved 178^1, 151^8, 151^7, 151^5, 151, 203, 151^4, 149; expected 178
- **q036** (multi_article, pl): retrieved 36, 36^1, 42, 49, 23^1, 32, 34, 25^1; expected 36, 30
- **q042** (multi_article, pl): retrieved 53, 229, 55, 92, 230, 201, 231, 148^1; expected 41, 53
- **q043** (multi_article, en): retrieved 67^24, 67^18, 67^20, 67^25, 67^31, 67^29, 67^30, 67^27; expected 67^19, 67^24
- **q046** (multi_article, pl): retrieved 108, 87, 92, 22^1d, 22^1f, 22^1c, 22^1e, 47; expected 108, 87^1
- **q053** (multi_article, pl): retrieved 171, 36^2, 148^1, 155^2, 37, 47, 41, 173; expected 36^2, 167^1
- **q054** (multi_article, uk): retrieved 67^19, 67^18, 18^3a, 32, 188^1, 178, 304^5, 186; expected 67^19, 67^23
- **q055** (multi_article, en): retrieved 133, 146, 136, 134, 154^2, 144, 132, 148; expected 135, 132
- **q056** (numeric, pl): retrieved 154, 155^3, 155^2, 155^1, 154^2, 167^3, 162, 172; expected 154, 154^1
- **q061** (numeric, en): retrieved 153, 161, 155^1, 158, 155^2, 155^2a, 155^3, 172; expected 153, 154
- **q063** (numeric, uk): retrieved 180, 183, 183^1, 180^2, 180^1, 182, 182^1f, 182^3; expected 180, 182^1a
