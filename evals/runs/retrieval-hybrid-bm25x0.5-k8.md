# Eval run `retrieval-hybrid-bm25x0.5-k8`

retrieval vector+bm25x0.5+named+rewrite/k8, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:36 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 97.5% |
| All expected articles retrieved | 96.2% |
| MRR of the first expected article | 0.868 |
| Cost | $0.01 ($0.0001 per question) |
| Latency p50 / p95 | 1.9 s / 3.0 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 100.0% | – | – | – |
| lookup | 35 | 97.1% | – | – | – |
| multi_article | 20 | 97.5% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 96.7% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q008** (lookup, pl): retrieved 182^1f, 182^1d, 182^3, 182^1a, 186^7, 186, 183, 180; expected 182^1c
- **q055** (multi_article, en): retrieved 134, 133, 136, 132, 144, 138, 154^2, 151^5; expected 135, 132
- **q062** (numeric, pl): retrieved 155^1, 171, 158, 155^2, 167^3, 173, 205, 154; expected 155^1, 155^3
