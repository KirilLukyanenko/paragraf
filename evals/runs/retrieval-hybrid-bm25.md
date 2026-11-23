# Eval run `retrieval-hybrid-bm25`

retrieval vector+bm25+named+rewrite/k6, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:35 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 94.3% |
| All expected articles retrieved | 89.9% |
| MRR of the first expected article | 0.863 |
| Cost | $0.01 ($0.0001 per question) |
| Latency p50 / p95 | 1.9 s / 3.1 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 100.0% | – | – | – |
| lookup | 35 | 97.1% | – | – | – |
| multi_article | 20 | 92.5% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 86.7% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q008** (lookup, pl): retrieved 182^1f, 186^7, 183, 182^1d, 186, 182^3; expected 182^1c
- **q041** (multi_article, pl): retrieved 151^11, 151^4, 151^9, 151^3, 147, 130; expected 151^11, 151^1
- **q042** (multi_article, pl): retrieved 53, 36, 39, 229, 55, 92; expected 41, 53
- **q055** (multi_article, en): retrieved 134, 138, 144, 136, 133, 135; expected 135, 132
- **q056** (numeric, pl): retrieved 154, 155^1, 158, 155^2, 167^3, 162; expected 154, 154^1
- **q061** (numeric, en): retrieved 153, 155^2, 155^1, 158, 161, 155^2a; expected 153, 154
- **q062** (numeric, pl): retrieved 155^1, 158, 171, 154, 155^2, 168; expected 155^1, 155^3
- **q063** (numeric, uk): retrieved 180, 180^2, 183^1, 180^1, 183, 182; expected 180, 182^1a
