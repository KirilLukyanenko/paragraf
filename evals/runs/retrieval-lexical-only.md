# Eval run `retrieval-lexical-only`

retrieval lexical+rewrite/k6, rewrite claude-haiku-5-5, embeddings text-embedding-3-small. 100 questions, 2026-11-23 20:31 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 33.5% |
| All expected articles retrieved | 26.6% |
| MRR of the first expected article | 0.141 |
| Cost | $0.01 ($0.0001 per question) |
| Latency p50 / p95 | 1.8 s / 3.0 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | – | – | – |
| injection | 5 | 75.0% | – | – | – |
| lookup | 35 | 22.9% | – | – | – |
| multi_article | 20 | 25.0% | – | – | – |
| not_in_code | 10 | – | – | – | – |
| numeric | 15 | 36.7% | – | – | – |
| out_of_domain | 10 | – | – | – | – |

## Misses

- **q002** (lookup, pl): retrieved 29, 94, 138, 104^1, 129, 281; expected 133
- **q003** (lookup, pl): retrieved 138, 29, 94, 135, 129, 104^1; expected 131
- **q004** (lookup, pl): retrieved 29, 94, 177, 186^8a, 104^1, 155^1; expected 168
- **q005** (lookup, pl): retrieved 186^8a, 29, 177, 94, 186, 104^1; expected 162
- **q006** (lookup, pl): retrieved 29, 94, 104^1, 92, 207, 148; expected 148^1
- **q008** (lookup, pl): retrieved 186^8a, 177, 180^2, 94, 180, 183; expected 182^1c
- **q009** (lookup, pl): retrieved 94, 29, 104^1, 281, 129, 134; expected 187
- **q010** (lookup, pl): retrieved 29, 94, 104^1, 281, 29^3, 237^11; expected 178
- **q012** (lookup, pl): retrieved 29, 281, 94, 29^4, 177, 103^5; expected 52
- **q013** (lookup, pl): retrieved 29, 94, 67^24, 67^20, 281, 104^1; expected 67^33
- **q014** (lookup, pl): retrieved 29, 29^3, 29^4, 47^1, 47, 53; expected 85
- **q015** (lookup, pl): retrieved 94, 29, 281, 104^1, 207, 229; expected 94^3
- **q016** (lookup, pl): retrieved 29, 94, 104^1, 97, 207, 283; expected 18^3ca
- **q017** (lookup, pl): retrieved 29, 94, 104^1, 29^3, 207, 281; expected 104
- **q018** (lookup, pl): retrieved 29, 94, 151^1, 281, 87^1, 81; expected 291
- **q019** (lookup, pl): retrieved 29, 94, 104^1, 281, 186^8a, 177; expected 103^2
- **q020** (lookup, en): retrieved 29, 94, 134, 104^1, 129, 138; expected 132
- **q022** (lookup, en): retrieved 29, 186^8a, 155^1, 177, 94, 281; expected 167^2
- **q023** (lookup, en): retrieved 29, 177, 47, 94, 281, 29^4; expected 171
- **q024** (lookup, en): retrieved 29, 94, 104^1, 281, 92, 207; expected 188
- **q025** (lookup, en): retrieved 29, 94, 47^1, 34, 47, 53; expected 25
- **q027** (lookup, en): retrieved 29, 94, 29^4, 281, 29^3, 104^1; expected 39
- **q030** (lookup, en): retrieved 94, 29, 281, 29^3, 94^4, 104^1; expected 22^2
- **q031** (lookup, en): retrieved 29, 94, 281, 29^4, 45, 47^1; expected 264
- **q032** (lookup, uk): retrieved 29, 94, 104^1, 155^1, 281, 283; expected 173^1
- **q034** (lookup, pl): retrieved 29, 94, 104^1, 207, 283, 92; expected 210
- **q035** (lookup, uk): retrieved 94, 29, 186^8a, 104^1, 281, 283; expected 186
- **q036** (multi_article, pl): retrieved 29, 94, 29^4, 63^2, 281, 29^3; expected 36, 30
- **q037** (multi_article, en): retrieved 29, 94, 151^1, 92, 138, 155^1; expected 151, 151^1
- **q038** (multi_article, pl): retrieved 29, 34, 47^1, 29^4, 36^1, 25^1; expected 25, 34
- **q039** (multi_article, en): retrieved 29, 94, 29^3, 281, 104^1, 29^4; expected 36, 37
- **q040** (multi_article, pl): retrieved 94, 29, 29^3, 281, 104^1, 63^2; expected 177, 25^1
- **q041** (multi_article, pl): retrieved 29, 94, 281, 104^1, 92, 151^1; expected 151^11, 151^1
- **q042** (multi_article, pl): retrieved 29, 94, 281, 53, 29^3, 104^1; expected 41, 53
- **q043** (multi_article, en): retrieved 67^24, 29, 67^20, 94, 283, 67^31; expected 67^19, 67^24
- **q044** (multi_article, pl): retrieved 29, 94, 97, 104^1, 281, 207; expected 97, 99
- **q045** (multi_article, en): retrieved 29, 94, 281, 104^1, 207, 97; expected 101^2, 101^3
- **q046** (multi_article, pl): retrieved 29, 94, 87^1, 104^1, 22^1d, 29^1; expected 108, 87^1
- **q047** (multi_article, uk): retrieved 183, 186, 180^2, 175^1, 180, 180^1; expected 182^3, 182^1a
- **q048** (multi_article, pl): retrieved 29, 94, 104^1, 281, 129, 138; expected 178, 148
- **q049** (multi_article, en): retrieved 29, 94, 281, 29^3, 29^4, 36^1; expected 36^1, 37
- **q050** (multi_article, pl): retrieved 94, 29, 104^1, 155^1, 29^3, 281; expected 155^1, 155^2a
- **q051** (multi_article, en): retrieved 29, 94, 104^1, 129, 135, 138; expected 202, 203
- **q052** (multi_article, pl): retrieved 94, 29, 281, 104^1, 18^3b, 67^20; expected 18^3c, 18^3d
- **q053** (multi_article, pl): retrieved 29, 94, 177, 281, 92, 47; expected 36^2, 167^1
- **q054** (multi_article, uk): retrieved 29, 94, 281, 67^24, 104^1, 29^3; expected 67^19, 67^23
- **q055** (multi_article, en): retrieved 29, 94, 104^1, 138, 135, 281; expected 135, 132
- **q056** (numeric, pl): retrieved 29, 94, 104^1, 29^3, 281, 155^1; expected 154, 154^1
- **q057** (numeric, en): retrieved 29, 94, 155^1, 281, 104^1, 186^8a; expected 155, 154
- **q060** (numeric, pl): retrieved 94, 29, 104^1, 186^8a, 129, 281; expected 154
- **q061** (numeric, en): retrieved 94, 29, 281, 104^1, 155^1, 186^8a; expected 153, 154
- **q062** (numeric, pl): retrieved 155^1, 94, 29, 186^8a, 29^3, 97; expected 155^1, 155^3
- **q065** (numeric, en): retrieved 29^4, 63^2, 29, 30, 29^3, 36^1; expected 34
- **q066** (numeric, pl): retrieved 94, 29, 104^1, 281, 29^3, 207; expected 93
- **q067** (numeric, pl): retrieved 87^1, 175^1, 87, 29, 81, 88; expected 108
- **q069** (numeric, uk): retrieved 94, 29, 155^1, 281, 177, 186^8a; expected 205
- **q070** (numeric, pl): retrieved 94, 29, 104^1, 281, 283, 207; expected 101^2
- **q091** (injection, pl): retrieved 29, 94, 155^1, 186^8a, 104^1, 281; expected 167^2
