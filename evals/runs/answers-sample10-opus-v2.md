# Eval run `answers-sample10-opus-v2`

retrieval vector+bm25x0.5+named+rewrite/k8, answer claude-opus-5-5 effort Medium, judge claude-sonnet-5-5. 10 questions, 2026-11-23 20:40 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 93.8% |
| All expected articles retrieved | 87.5% |
| MRR of the first expected article | 0.792 |
| Status correct (answers / says not in the code / out of scope) | 100.0% |
| Citation precision (citations verified by code) | 100.0% |
| Citation recall (expected articles cited and verified) | 93.8% |
| Grounded answers (every citation verified, at least one) | 100.0% |
| Expected points made (judge) | 91.7% |
| Answers contradicting the sources (judge) | 0.0% |
| Answers adding law from outside the sources (judge) | 10.0% |
| Cost | $0.60 ($0.0604 per question) |
| Latency p50 / p95 | 13.8 s / 16.9 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| injection | 1 | 100.0% | 100.0% | 100.0% | 100.0% |
| lookup | 4 | 100.0% | 100.0% | 88.9% | 100.0% |
| multi_article | 2 | 75.0% | 100.0% | 83.3% | 100.0% |
| not_in_code | 1 | – | 100.0% | 100.0% | 100.0% |
| numeric | 1 | 100.0% | 100.0% | 100.0% | 100.0% |
| out_of_domain | 1 | – | 100.0% | 100.0% | 100.0% |

## Misses

- **q041** (multi_article, pl): retrieved 151^11, 151^4, 151^9, 151^3, 147, 151^9b, 151^2, 130; expected 151^11, 151^1
- **q051** (multi_article, en): outside: The 7 hours/35 hours holiday cap applies to light work 'for a purpose other than vocational training' (the sources only say 'light work' in art. 200² and do not mention this distinction).
