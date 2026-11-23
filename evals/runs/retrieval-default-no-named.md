# Eval run `retrieval-default-no-named`

retrieval vector+bm25x0.5+rewrite/k8, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:38 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 96.2% |
| All expected articles retrieved | 93.7% |
| MRR of the first expected article | 0.894 |
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

- **q024** (lookup, en): retrieved 186, 182^3, 183, 182^1d, 180, 53, 186^2, 186^3; expected 188
- **q036** (multi_article, pl): retrieved 36, 36^1, 25^1, 25, 42, 34, 49, 39; expected 36, 30
- **q042** (multi_article, pl): retrieved 53, 36, 229, 55, 92, 42, 36^1, 36^2; expected 41, 53
- **q055** (multi_article, en): retrieved 134, 136, 138, 144, 133, 135, 139, 154^2; expected 135, 132
- **q057** (numeric, en): retrieved 154, 155^1, 158, 155^3, 155^2, 167^3, 162, 167^2; expected 155, 154
