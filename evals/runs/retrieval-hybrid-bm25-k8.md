# Eval run `retrieval-hybrid-bm25-k8`

retrieval vector+bm25+named+rewrite/k8, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:36 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 96.2% |
| All expected articles retrieved | 93.7% |
| MRR of the first expected article | 0.838 |
| Cost | $0.01 ($0.0001 per question) |
| Latency p50 / p95 | 1.9 s / 3.0 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 100.0% | – | – | – |
| lookup | 35 | 97.1% | – | – | – |
| multi_article | 20 | 92.5% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 96.7% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q008** (lookup, pl): retrieved 182, 182^1d, 182^3, 182^1f, 186, 186^7, 182^1a, 183; expected 182^1c
- **q042** (multi_article, pl): retrieved 53, 92, 55, 229, 47, 93, 36, 231; expected 41, 53
- **q054** (multi_article, uk): retrieved 67^19, 188^1, 183, 186, 186^8, 186^7, 180, 188; expected 67^19, 67^23
- **q055** (multi_article, en): retrieved 134, 138, 144, 136, 133, 132, 154^2, 143; expected 135, 132
- **q062** (numeric, pl): retrieved 155^1, 158, 205, 155^2, 154, 162, 153, 171; expected 155^1, 155^3
