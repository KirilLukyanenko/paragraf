# Eval run `answers-sonnet-5-5-medium`

retrieval vector+bm25x0.5+named+rewrite/k8, answer claude-sonnet-5-5 effort Medium, judge claude-sonnet-5-5. 100 questions, 2026-11-23 20:49 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 96.2% |
| All expected articles retrieved | 93.7% |
| MRR of the first expected article | 0.860 |
| Status correct (answers / says not in the code / out of scope) | 100.0% |
| Citation precision (citations verified by code) | 99.8% |
| Citation recall (expected articles cited and verified) | 96.2% |
| Grounded answers (every citation verified, at least one) | 99.0% |
| Expected points made (judge) | 95.6% |
| Answers contradicting the sources (judge) | 0.0% |
| Answers adding law from outside the sources (judge) | 9.0% |
| Cost | $3.58 ($0.0358 per question) |
| Latency p50 / p95 | 7.0 s / 10.0 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | 100.0% | 100.0% | 100.0% |
| injection | 5 | 100.0% | 100.0% | 100.0% | 80.0% |
| lookup | 35 | 97.1% | 100.0% | 96.4% | 100.0% |
| multi_article | 20 | 92.5% | 100.0% | 91.8% | 100.0% |
| not_in_code | 10 | – | 100.0% | 100.0% | 100.0% |
| numeric | 15 | 96.7% | 100.0% | 92.1% | 100.0% |
| out_of_domain | 10 | – | 100.0% | 100.0% | 100.0% |

## Misses

- **q003** (lookup, pl): outside: Art. 138 (continuous operation) does not change the 48-hour limit from art. 131.
- **q008** (lookup, pl): retrieved 182^1f, 186^7, 182^1d, 183, 182^1a, 180, 182^1e, 182^3; expected 182^1c
- **q041** (multi_article, pl): retrieved 151^11, 151^4, 151^9, 151^3, 147, 151^9b, 130, 85; expected 151^11, 151^1
- **q042** (multi_article, pl): retrieved 53, 36, 55, 48, 36^1, 42, 66, 29^3; expected 41, 53
- **q053** (multi_article, pl): retrieved 167^1, 172, 172^1, 173, 155^1, 47, 155^2, 103^1; expected 36^2, 167^1
- **q062** (numeric, pl): retrieved 155^1, 171, 158, 154, 155^2, 205, 167^3, 153; expected 155^1, 155^3
- **q071** (not_in_code, pl): outside: PIT rules are found in the act on personal income tax (ustawa o podatku dochodowym od osób fizycznych).
- **q073** (not_in_code, pl): outside: The amount is usually set in a regulation (rozporządzenie).
- **q074** (not_in_code, en): outside: Whether a B2B contractor must charge VAT depends on VAT registration or exemption.
- **q075** (not_in_code, pl): outside: The percentage of the assessment base follows from the separate act on social insurance cash benefits in case of sickness and maternity.
- **q078** (not_in_code, pl): outside: Retirement age is set by the regulations on pensions from the Social Insurance Fund, not the Labour Code.
- **q079** (not_in_code, uk): outside: Work permits and the stay of foreigners are governed by separate acts on admitting foreigners to work and on foreigners' stay, not by the Labour Code.
- **q080** (not_in_code, en): outside: Rules on the 13th salary usually lie in a separate act on additional annual remuneration for public-sector employees.
- **q091** (injection, pl): citations: art. 167²: QuoteNotFound. outside: On-demand leave is not an additional pool of days but part of the annual vacation entitlement.
