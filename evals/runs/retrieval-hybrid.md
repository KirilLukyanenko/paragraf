# Eval run `retrieval-hybrid`

retrieval vector+lexical+named+rewrite/k6, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:30 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 82.9% |
| All expected articles retrieved | 74.7% |
| MRR of the first expected article | 0.682 |
| Cost | $0.01 ($0.0001 per question) |
| Latency p50 / p95 | 1.9 s / 3.0 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 75.0% | – | – | – |
| lookup | 35 | 85.7% | – | – | – |
| multi_article | 20 | 75.0% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 83.3% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q008** (lookup, pl): retrieved 182^1f, 186^8a, 182^1a, 186^4, 183, 182^1d; expected 182^1c
- **q010** (lookup, pl): retrieved 178^1, 151, 151^1, 151^2, 151^8, 29; expected 178
- **q020** (lookup, en): retrieved 134, 133, 136, 148, 154^2, 139; expected 132
- **q024** (lookup, en): retrieved 180, 186, 29, 183, 94, 104^1; expected 188
- **q031** (lookup, en): retrieved 44, 45, 29^4, 48, 56, 36^1; expected 264
- **q039** (multi_article, en): retrieved 36^1, 29^3, 29^4, 36, 51, 36^2; expected 36, 37
- **q042** (multi_article, pl): retrieved 53, 45, 41^1, 48, 92, 36^1; expected 41, 53
- **q044** (multi_article, pl): retrieved 97, 94^6, 29^4, 29, 94, 104^1; expected 97, 99
- **q045** (multi_article, en): retrieved 101^2, 29, 94, 101^1, 104^1, 281; expected 101^2, 101^3
- **q049** (multi_article, en): retrieved 36^1, 36, 47^1, 29^4, 53, 34; expected 36^1, 37
- **q051** (multi_article, en): retrieved 200^2, 151^7, 151^1, 151^8, 203, 29; expected 202, 203
- **q052** (multi_article, pl): retrieved 18^3d, 18^3f, 18^3b, 18^3g, 18^3a, 18^3e; expected 18^3c, 18^3d
- **q053** (multi_article, pl): retrieved 171, 152, 47, 167^1, 29, 172; expected 36^2, 167^1
- **q054** (multi_article, uk): retrieved 67^19, 67^18, 45, 18^3a, 304^5, 29; expected 67^19, 67^23
- **q055** (multi_article, en): retrieved 135, 134, 138, 144, 136, 143; expected 135, 132
- **q056** (numeric, pl): retrieved 155^1, 154, 29, 155^2, 94, 104^1; expected 154, 154^1
- **q057** (numeric, en): retrieved 155^1, 154, 155^2, 154^2, 29, 155^3; expected 155, 154
- **q062** (numeric, pl): retrieved 155^1, 155^2, 205, 154, 29, 94; expected 155^1, 155^3
- **q070** (numeric, pl): retrieved 101^3, 101^4, 47^1, 61^2, 55, 29; expected 101^2
- **q091** (injection, pl): retrieved 29^3, 29^1, 29, 155^1, 29^2, 36; expected 167^2
