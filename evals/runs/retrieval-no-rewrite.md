# Eval run `retrieval-no-rewrite`

retrieval vector+lexical+named/k6, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:30 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 83.5% |
| All expected articles retrieved | 75.9% |
| MRR of the first expected article | 0.663 |
| Cost | $0.00 ($0.0000 per question) |
| Latency p50 / p95 | 0.2 s / 0.4 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 75.0% | – | – | – |
| lookup | 35 | 91.4% | – | – | – |
| multi_article | 20 | 72.5% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 76.7% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q001** (lookup, pl): retrieved 138, 148^1, 147, 130, 135, 143; expected 129
- **q012** (lookup, pl): retrieved 29, 110, 94^4, 291, 113, 94; expected 52
- **q024** (lookup, en): retrieved 186, 183, 154, 93, 182^1c, 94; expected 188
- **q039** (multi_article, en): retrieved 22^1d, 36, 183, 93, 49, 47; expected 36, 37
- **q041** (multi_article, pl): retrieved 151^11, 151^2, 29, 94, 151^4, 175^1; expected 151^11, 151^1
- **q042** (multi_article, pl): retrieved 143, 148^1, 34, 53, 180^2, 183; expected 41, 53
- **q044** (multi_article, pl): retrieved 97, 29^3, 53, 29, 52, 94; expected 97, 99
- **q046** (multi_article, pl): retrieved 108, 22^1d, 22^1c, 29, 94, 207; expected 108, 87^1
- **q047** (multi_article, uk): retrieved 183, 186, 182^1c, 182^3, 180, 186^7; expected 182^3, 182^1a
- **q049** (multi_article, en): retrieved 36, 34, 29^4, 36^1, 281, 47; expected 36^1, 37
- **q051** (multi_article, en): retrieved 17, 202, 180, 200^2, 180^2, 182^1g; expected 202, 203
- **q052** (multi_article, pl): retrieved 18^3d, 18^3ca, 18^3b, 18^3e, 179, 18^3a; expected 18^3c, 18^3d
- **q054** (multi_article, uk): retrieved 229, 67^19, 186^3, 281, 186^2, 34; expected 67^19, 67^23
- **q055** (multi_article, en): retrieved 138, 135, 151^7, 182^3, 129, 154^2; expected 135, 132
- **q056** (numeric, pl): retrieved 155^1, 205, 154, 167^2, 167^3, 153; expected 154, 154^1
- **q057** (numeric, en): retrieved 103^2, 37, 133, 205, 229, 281; expected 155, 154
- **q062** (numeric, pl): retrieved 154, 29, 167^3, 94, 167^2, 186^8a; expected 155^1, 155^3
- **q069** (numeric, uk): retrieved 36, 103^2, 133, 148^1, 134, 153; expected 205
- **q094** (injection, en): retrieved 136, 139, 144, 138, 135, 137; expected 151
