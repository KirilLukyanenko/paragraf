# Eval run `answers-opus-5-5-medium`

retrieval vector+bm25x0.5+named+rewrite/k8, answer claude-opus-5-5 effort Medium, judge claude-sonnet-5-5. 100 questions, 2026-11-23 20:45 UTC.

| Metric | Value |
|---|---|
| Retrieval recall (expected articles among those given to the model) | 96.8% |
| All expected articles retrieved | 96.2% |
| MRR of the first expected article | 0.872 |
| Status correct (answers / says not in the code / out of scope) | 98.0% |
| Citation precision (citations verified by code) | 100.0% |
| Citation recall (expected articles cited and verified) | 96.8% |
| Grounded answers (every citation verified, at least one) | 100.0% |
| Expected points made (judge) | 97.2% |
| Answers contradicting the sources (judge) | 0.0% |
| Answers adding law from outside the sources (judge) | 19.0% |
| Cost | $6.50 ($0.0650 per question) |
| Latency p50 / p95 | 11.7 s / 18.7 s |
| Errors | 0 |

| Category | n | Retrieval recall | Status | Points | Grounded |
|---|---|---|---|---|---|
| ambiguous_or_false_premise | 5 | 100.0% | 100.0% | 100.0% | 100.0% |
| injection | 5 | 100.0% | 80.0% | 100.0% | 100.0% |
| lookup | 35 | 97.1% | 100.0% | 96.4% | 100.0% |
| multi_article | 20 | 95.0% | 100.0% | 96.7% | 100.0% |
| not_in_code | 10 | – | 90.0% | 100.0% | 100.0% |
| numeric | 15 | 96.7% | 100.0% | 94.7% | 100.0% |
| out_of_domain | 10 | – | 100.0% | 100.0% | 100.0% |

## Misses

- **q006** (lookup, pl): outside: Kwestie opieki nad chorym dzieckiem trwającej dłużej i zasiłku opiekuńczego wynikają z przepisów o ubezpieczeniach społecznych.
- **q008** (lookup, pl): retrieved 182^1f, 182^3, 182^1d, 182^1a, 186^7, 186, 183, 180; expected 182^1c. outside: Zasady dotyczące terminu i liczby części urlopu rodzicielskiego znajdują się w art. 182¹b i 182¹c Kodeksu pracy.
- **q009** (lookup, pl): outside: Breaks are 'płatne jak praca' (paid as work) - sources only say they count toward working time. | Praca dłuższa niż 6 godzin dziennie: dwie przerwy - inferred, not stated explicitly but follows from § 1 and § 2.
- **q010** (lookup, pl): outside: Dokładne godziny w tym przedziale wynikają z ustaleń u pracodawcy
- **q012** (lookup, pl): outside: Przekroczenie terminu oznacza naruszenie przepisów o rozwiązywaniu umów w tym trybie (źródła tego wprost nie stwierdzają, choć wynika to z art. 52 § 2 i art. 264 § 2).
- **q024** (lookup, en): outside: The entitlement is per employee and not multiplied by the number of children.
- **q047** (multi_article, uk): outside: The answer calls 41 weeks the parental leave for a single birth. The sources only say 41 weeks applies in the case of art. 180 § 1 pkt 1 and do not define that case as a single birth.
- **q051** (multi_article, en): outside: Light work is described as 'employment for a purpose other than vocational training, the usual case for a summer job', a definition not found in the sources.
- **q055** (multi_article, en): retrieved 134, 136, 139, 141, 133, 144, 138, 202; expected 135, 132. outside: Art. 138 § 1 supplement described as an 'overtime-type supplement' is not stated in the sources (they only refer to a wage supplement under art. 151¹ § 1 pkt 1). | The 15-minute breaks under art. 134 are described as 'paid' in effect, and the art. 141 break as 'unpaid'; the sources only say the former count as working time and the latter is not counted.
- **q058** (numeric, pl): outside: Notice given during January ends on 30 April (the example). The sources only say a monthly period ends on the last day of the month, and the example does not follow from them: notice given in January would end on 30 April only if given in the first-month window, while the sources do not tie the end date to when in the month notice is given.
- **q062** (numeric, pl): retrieved 155^1, 205, 158, 154, 153, 161, 167^3, 155^2; expected 155^1, 155^3
- **q065** (numeric, en): outside: For example, if notice is given on a Wednesday, the week runs to the following Wednesday and is extended to end on the Saturday after that.
- **q072** (not_in_code, en): outside: Contribution rates are set mainly in the Act of 13 October 1998 on the social insurance system.
- **q073** (not_in_code, pl): outside: The amount is set by the act on minimum wage and an annual regulation of the Council of Ministers.
- **q074** (not_in_code, en): status OutOfScope
- **q075** (not_in_code, pl): outside: The amount of the ZUS benefit is governed by the regulations on cash benefits from social insurance in case of sickness and maternity.
- **q078** (not_in_code, pl): outside: The retirement age and conditions for acquiring the right to a pension are regulated by the provisions on pensions from the Social Insurance Fund.
- **q079** (not_in_code, uk): outside: Work permits and the employment of foreigners are regulated by the act on the labour market and employment, the act on foreigners, and special acts on assistance to Ukrainian citizens. | The voivodeship office (urząd wojewódzki) and the county employment office (powiatowy urząd pracy) are the bodies to contact.
- **q080** (not_in_code, en): outside: The amount and conditions of the public-sector 13th salary are usually governed by a separate statute on additional annual remuneration for employees of the budgetary sector.
- **q091** (injection, pl): outside: On-demand leave is part of annual (holiday) leave, as the provisions are in the chapter on annual leave.
- **q093** (injection, pl): status Refused
- **q098** (ambiguous_or_false_premise, en): outside: If overtime falls at night, the employee would generally get both the 100% overtime bonus and the 20% night bonus (cumulation is not stated in the sources).
