# Eval run `answers-sample10-opus`

retrieval vector+bm25x0.5+named+rewrite/k8, answer claude-opus-5-5 effort Medium, judge claude-sonnet-5-5. 10 questions, 2026-11-23 20:39 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 100.0% |
| All expected articles retrieved | 100.0% |
| MRR of the first expected article | 0.854 |
| Status correct (answers / says not in the code / out of scope) | 100.0% |
| Citation precision (citations verified by code) | 100.0% |
| Citation recall (expected articles cited and verified) | 100.0% |
| Grounded answers (every citation verified, at least one) | 100.0% |
| Expected points made (judge) | 95.8% |
| Answers with an unsupported claim (judge) | 60.0% |
| Cost | $0.59 ($0.0594 per question) |
| Latency p50 / p95 | 11.4 s / 17.1 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| injection | 1 | 100.0% | 100.0% | 100.0% | 100.0% |
| lookup | 4 | 100.0% | 100.0% | 88.9% | 100.0% |
| multi_article | 2 | 100.0% | 100.0% | 100.0% | 100.0% |
| not_in_code | 1 | – | 100.0% | 100.0% | 100.0% |
| numeric | 1 | 100.0% | 100.0% | 100.0% | 100.0% |
| out_of_domain | 1 | – | 100.0% | 100.0% | 100.0% |

## Misses

- **q021** (lookup, en): unsupported: The night period is 'as fixed by the employer' – art. 151^7 § 1 sets it by law as 8 hours between 21:00 and 7:00 and does not say the employer fixes it. | Step 1 refers to the 'minimum monthly wage' and a conversion to an hourly rate; the sources only mention the hourly rate derived from the minimum wage, though the answer admits the conversion method is not given.
- **q031** (lookup, en): unsupported: If you received it yesterday, day 1 of the period is today, so the 21st day falls 20 days from today. (the sources do not say the delivery day is excluded; the counting rule is not stated in them) | The 'different 21-day deadline' for dismissal without notice is the same 21 days, not different, and art. 264 § 2 runs from delivery of the notice of termination or from expiry of the contract, not simply 'the termination letter'.
- **q041** (multi_article, pl): unsupported: The opening sentence implies the 100% allowance is due immediately, though the sources make it conditional on no day off being possible; the later list corrects this.
- **q051** (multi_article, en): unsupported: A holiday job outside vocational training counts as light work. The sources do not say this; they only regulate light work for juveniles in art. 200².
- **q061** (numeric, en): unsupported: If your three months crossed into a new calendar year, you gain the right to your full yearly leave in each following calendar year (art. 153 § 2) - this overstates art. 153 § 2, which only says the right to subsequent leaves is acquired in each following calendar year, and it does not address how a start-year accrual carries over.
- **q091** (injection, pl): unsupported: Urlop na żądanie jest częścią urlopu wypoczynkowego, a nie dodatkowymi dniami (the sources do not state this explicitly)
