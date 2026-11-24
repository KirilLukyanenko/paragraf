# Eval run `holdout-retrieval-vector-only`

retrieval vector/k8, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 40 questions, 2026-11-24 11:09 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 84.8% |
| All expected articles retrieved | 80.0% |
| MRR of the first expected article | 0.673 |
| Cost | $0.00 ($0.0000 per question) |
| Latency p50 / p95 | 0.2 s / 0.6 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 2 | 58.3% | – | – | – |
| injection | 1 | 100.0% | – | – | – |
| lookup | 16 | 87.5% | – | – | – |
| multi_article | 10 | 75.0% | – | – | – |
| not_in_code | 3 | – | – | – | – |
| numeric | 6 | 100.0% | – | – | – |
| out_of_domain | 2 | – | – | – | – |

## Misses

- **h010** (lookup, pl): retrieved 22^1c, 56, 60, 61^2, 61^1, 50, 57, 36^1; expected 291
- **h016** (lookup, pl): retrieved 151^4, 151, 67^20, 56, 151^5, 57, 26^1, 22^2; expected 22
- **h017** (multi_article, pl): retrieved 87^1, 282, 87, 82, 91, 237^1, 281, 60; expected 114, 115, 119
- **h018** (multi_article, en): retrieved 53, 229, 92, 93, 148^1, 167^2, 135, 173^1; expected 41, 36
- **h023** (multi_article, pl): retrieved 36^1, 29^4, 254, 112, 97, 66, 264, 256; expected 52, 264
- **h039** (ambiguous_or_false_premise, en): retrieved 36, 42, 23^1, 70, 34, 29^1, 37, 68^3; expected 36, 30
- **h040** (ambiguous_or_false_premise, pl): retrieved 183, 182^3, 186, 182^1f, 182^1c, 180, 180^2, 186^7; expected 180, 182^3, 182^1a
