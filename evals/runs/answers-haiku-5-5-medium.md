# Eval run `answers-haiku-5-5-medium`

retrieval vector+bm25x0.5+named+rewrite/k8, answer claude-haiku-5-5 effort Medium, judge claude-sonnet-5-5. 100 questions, 2026-11-23 20:50 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 96.2% |
| All expected articles retrieved | 93.7% |
| MRR of the first expected article | 0.907 |
| Status correct (answers / says not in the code / out of scope) | 99.0% |
| Citation precision (citations verified by code) | 99.1% |
| Citation recall (expected articles cited and verified) | 95.6% |
| Grounded answers (every citation verified, at least one) | 98.0% |
| Expected points made (judge) | 93.5% |
| Answers contradicting the sources (judge) | 5.0% |
| Answers adding law from outside the sources (judge) | 9.0% |
| Cost | $1.61 ($0.0161 per question) |
| Latency p50 / p95 | 6.5 s / 16.4 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | 100.0% | 93.3% | 100.0% |
| injection | 5 | 100.0% | 100.0% | 92.9% | 100.0% |
| lookup | 35 | 97.1% | 100.0% | 95.2% | 97.1% |
| multi_article | 20 | 95.0% | 100.0% | 91.8% | 95.0% |
| not_in_code | 10 | – | 90.0% | 100.0% | 100.0% |
| numeric | 15 | 93.3% | 100.0% | 86.8% | 100.0% |
| out_of_domain | 10 | – | 100.0% | 100.0% | 100.0% |

## Misses

- **q008** (lookup, pl): retrieved 182^1f, 186^7, 182^1d, 183, 182^1a, 180, 182^1e, 182^3; expected 182^1c
- **q016** (lookup, pl): citations: art. 18³ᶜᵃ § 1: QuoteNotFound
- **q027** (lookup, en): outside: Art. 39 applies only to termination with notice.
- **q033** (lookup, uk): CONTRADICTS: The answer says the amount of the employer's compensation claim under Art. 61¹ is not set in the sources, but Art. 61² § 1 sets it at pay for the notice period (for a fixed-term contract, pay for the time the contract was to last, capped at the notice period).
- **q035** (lookup, uk): CONTRADICTS: The application is filed "не раніше ніж за 21 день до початку відпустки" (no earlier than 21 days before). Art. 186 § 7 says no later than 21 days before, i.e. not shorter than 21 days.
- **q037** (multi_article, en): outside: The sources do not say whether the employer can instead give you time off in lieu; that is covered by art. 151² (the sources do actually cover this in art. 151², so this is a mild misstatement, but not a legal claim).
- **q041** (multi_article, pl): retrieved 151^11, 151^4, 151^9, 147, 151^3, 151^9b, 130, 133; expected 151^11, 151^1
- **q042** (multi_article, pl): retrieved 53, 229, 92, 55, 231, 36, 201, 48; expected 41, 53
- **q045** (multi_article, en): outside: an oral or unsigned agreement is void (the 'unsigned' part goes beyond the sources, which only require written form)
- **q047** (multi_article, uk): citations: 182^1^a: Malformed; 182^1^a: Malformed. CONTRADICTS: Статтю 182³ «не називає право невід'ємним, тому її можна вважати правом без явного обмеження передачі» — urlop ojcowski is granted only to the father (pracownik – ojciec), so it is not transferable. | Ліміт 65 тижнів «якщо дитина має відповідне застереження» is vague, but the 65 weeks applies only to the 'Za życiem' certificate for one child; this is broadly consistent.
- **q048** (multi_article, pl): CONTRADICTS: System z art. 139 nie jest w tym przepisie wymieniony, więc ten limit nie obejmuje systemu równoważnego (art. 148 expressly covers the working time systems and the 8-hour limit for a child under 4 applies without consent; the answer concludes the limit does not apply).. outside: System z art. 139 jest systemem równoważnym (the sources do not say art. 139 is the equivalent system).
- **q052** (multi_article, pl): outside: Wybór między zadośćuczynieniem a odszkodowaniem należy do Ciebie (the choice between the two belongs to the employee).
- **q055** (multi_article, en): CONTRADICTS: A shift longer than 6 hours gives a 15-minute break (art. 134 § 1 says at least 6 hours, not longer than 6).
- **q056** (numeric, pl): retrieved 154, 155, 155^1, 155^2, 167^3, 162, 167^1, 158; expected 154, 154^1
- **q062** (numeric, pl): retrieved 155^1, 155^2, 171, 158, 167^1, 168, 153, 173; expected 155^1, 155^3
- **q071** (not_in_code, pl): outside: PIT rules are usually found in the Personal Income Tax Act (ustawa o podatku dochodowym od osób fizycznych).
- **q072** (not_in_code, en): outside: Contribution rates are set by the ustawa o systemie ubezpieczeń społecznych.
- **q074** (not_in_code, en): status OutOfScope
- **q077** (not_in_code, pl): outside: Listę świąt zwykle znajdziesz w ustawie o dniach wolnych od pracy.
- **q079** (not_in_code, uk): outside: Rules on work permits for foreigners and exceptions for Ukrainian citizens are usually in separate acts on conditions of admitting foreigners to work.
