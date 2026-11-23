# Eval run `retrieval-vector-only`

retrieval vector+rewrite/k6, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:31 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 88.6% |
| All expected articles retrieved | 82.3% |
| MRR of the first expected article | 0.855 |
| Cost | $0.02 ($0.0002 per question) |
| Latency p50 / p95 | 2.0 s / 2.8 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 100.0% | – | – | – |
| lookup | 35 | 91.4% | – | – | – |
| multi_article | 20 | 77.5% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 90.0% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q006** (lookup, pl): retrieved 53, 182^3, 186, 185, 180, 182; expected 148^1
- **q008** (lookup, pl): retrieved 182^1f, 182^1e, 183, 182^3, 186^8a, 182^1a; expected 182^1c
- **q010** (lookup, pl): retrieved 178^1, 151^8, 151^7, 151, 151^5, 203; expected 178
- **q036** (multi_article, pl): retrieved 36, 49, 36^1, 34, 25^1, 42; expected 36, 30
- **q041** (multi_article, pl): retrieved 151^4, 151^11, 151^9, 147, 151^12, 151^10; expected 151^11, 151^1
- **q042** (multi_article, pl): retrieved 53, 229, 55, 92, 230, 148^1; expected 41, 53
- **q043** (multi_article, en): retrieved 67^24, 67^20, 67^18, 67^25, 67^27, 67^31; expected 67^19, 67^24
- **q046** (multi_article, pl): retrieved 108, 282, 283, 22^1d, 119, 121; expected 108, 87^1
- **q053** (multi_article, pl): retrieved 172, 173, 155^2, 47, 171, 167^1; expected 36^2, 167^1
- **q054** (multi_article, uk): retrieved 67^19, 304^5, 188^1, 178, 186, 67^31; expected 67^19, 67^23
- **q055** (multi_article, en): retrieved 134, 136, 139, 133, 141, 144; expected 135, 132
- **q056** (numeric, pl): retrieved 154, 155^3, 155^1, 155^2, 167^3, 162; expected 154, 154^1
- **q062** (numeric, pl): retrieved 155^1, 154, 161, 167^2, 167^3, 158; expected 155^1, 155^3
- **q063** (numeric, uk): retrieved 180, 183, 183^1, 182, 180^2, 180^1; expected 180, 182^1a
