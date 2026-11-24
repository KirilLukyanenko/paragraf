# Eval run `holdout-retrieval-default`

retrieval vector+bm25x0.5+named+rewrite/k8, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 40 questions, 2026-11-24 11:09 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 91.4% |
| All expected articles retrieved | 85.7% |
| MRR of the first expected article | 0.816 |
| Cost | $0.01 ($0.0002 per question) |
| Latency p50 / p95 | 2.1 s / 2.8 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 2 | 41.7% | – | – | – |
| injection | 1 | 100.0% | – | – | – |
| lookup | 16 | 93.8% | – | – | – |
| multi_article | 10 | 91.7% | – | – | – |
| not_in_code | 3 | – | – | – | – |
| numeric | 6 | 100.0% | – | – | – |
| out_of_domain | 2 | – | – | – | – |

## Misses

- **h014** (lookup, en): retrieved 188, 53, 185, 182^3, 180, 186, 189, 183; expected 148^1
- **h017** (multi_article, pl): retrieved 119, 122, 116, 115, 117, 124, 121, 118; expected 114, 115, 119
- **h018** (multi_article, en): retrieved 53, 39, 36, 55, 48, 36^2, 201, 51; expected 41, 36
- **h039** (ambiguous_or_false_premise, en): retrieved 30, 55, 36^1, 29^4, 48, 41, 34, 25; expected 36, 30
- **h040** (ambiguous_or_false_premise, pl): retrieved 186, 186^7, 183, 182^1f, 186^8, 186^5, 182^3, 182^1c; expected 180, 182^3, 182^1a
