# Eval run `retrieval-hybrid-k8`

retrieval vector+lexical+named+rewrite/k8, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:32 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 92.4% |
| All expected articles retrieved | 87.3% |
| MRR of the first expected article | 0.682 |
| Cost | $0.02 ($0.0002 per question) |
| Latency p50 / p95 | 1.9 s / 3.0 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 100.0% | – | – | – |
| lookup | 35 | 94.3% | – | – | – |
| multi_article | 20 | 85.0% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 93.3% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q008** (lookup, pl): retrieved 186^8a, 182^1f, 182^1a, 186^4, 183, 182^1d, 180, 177; expected 182^1c
- **q010** (lookup, pl): retrieved 178^1, 151, 151^1, 149, 151^2, 29, 151^8, 94; expected 178
- **q039** (multi_article, en): retrieved 29^4, 29^3, 36^1, 36, 48, 51, 47, 30; expected 36, 37
- **q042** (multi_article, pl): retrieved 53, 36, 92, 47, 36^1, 177, 48, 23^1; expected 41, 53
- **q049** (multi_article, en): retrieved 36^1, 36, 47^1, 29^4, 53, 34, 47, 48; expected 36^1, 37
- **q051** (multi_article, en): retrieved 178^1, 200^2, 151, 203, 29, 94, 104^1, 129; expected 202, 203
- **q054** (multi_article, uk): retrieved 67^19, 29, 188^1, 94, 178, 281, 304^5, 67^24; expected 67^19, 67^23
- **q055** (multi_article, en): retrieved 148, 138, 134, 135, 144, 139, 136, 143; expected 135, 132
- **q056** (numeric, pl): retrieved 154, 155^1, 155^2, 94, 155^3, 29, 177, 29^3; expected 154, 154^1
- **q057** (numeric, en): retrieved 154, 155^2, 155^1, 154^2, 29, 155^3, 94, 104^1; expected 155, 154
