# Eval run `retrieval-hybrid-k10`

retrieval vector+lexical+named+rewrite/k10, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:33 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 91.8% |
| All expected articles retrieved | 87.3% |
| MRR of the first expected article | 0.697 |
| Cost | $0.01 ($0.0001 per question) |
| Latency p50 / p95 | 1.9 s / 3.1 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 100.0% | – | – | – |
| lookup | 35 | 94.3% | – | – | – |
| multi_article | 20 | 85.0% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 90.0% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q006** (lookup, pl): retrieved 188^1, 53, 180, 29, 94, 186, 207, 104^1, 92, 182^3; expected 148^1
- **q008** (lookup, pl): retrieved 186^8a, 182^1f, 186^4, 182^1d, 182^1a, 180, 182^1e, 29, 175^1, 183; expected 182^1c
- **q041** (multi_article, pl): retrieved 151^11, 147, 151^3, 29, 94, 104^1, 151^4, 129, 151^9, 281; expected 151^11, 151^1
- **q048** (multi_article, pl): retrieved 150, 151^8, 151^9, 148, 151, 138, 142^1, 135, 179, 29; expected 178, 148
- **q049** (multi_article, en): retrieved 36^1, 36, 47^1, 29^4, 53, 34, 48, 47, 55, 29; expected 36^1, 37
- **q053** (multi_article, pl): retrieved 47, 36^2, 36^1, 47^1, 29, 148^1, 94, 37, 155^2, 281; expected 36^2, 167^1
- **q054** (multi_article, uk): retrieved 67^19, 67^18, 18^3a, 304^5, 29, 94, 188^1, 281, 178, 67^24; expected 67^19, 67^23
- **q055** (multi_article, en): retrieved 134, 139, 138, 136, 144, 135, 143, 29, 133, 94; expected 135, 132
- **q056** (numeric, pl): retrieved 155^1, 154, 29, 155^2, 94, 104^1, 155^3, 29^3, 281, 186^8a; expected 154, 154^1
- **q067** (numeric, pl): retrieved 87, 88, 91, 87^1, 85, 81, 78, 77^3, 129, 175^1; expected 108
