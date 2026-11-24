# Payroll eval payroll-leave-haiku-5-5-medium

Answer model claude-haiku-5-5, 4 questions, $0.01 ($0.003 a question), p50 14.2 s.

| Correct | Right tools | Expected amounts and dates | Amounts from tools | Citations verified | Expected articles cited | Question's language |
|---|---|---|---|---|---|---|
| 100% | 100% | 100% | 100% | 100% | 75% | 100% (0 written again) |

| Id | Category | Result | Tools | Problems |
|---|---|---|---|---|
| p13 | calculation | correct | annual_leave |  |
| p14 | calculation | correct | annual_leave | not cited: art. 154^1 |
| p15 | calculation | correct | annual_leave |  |
| p16 | calculation | correct | annual_leave |  |

## Answers

### p13 (calculation)

> Pracuję od 7 lat i skończyłam studia magisterskie. Ile dni urlopu wypoczynkowego mi przysługuje w roku?

Przysługuje Pani 26 dni urlopu wypoczynkowego w roku (przy pełnym etacie, co daje 208 godzin według obliczenia narzędzia).

Obliczenie:
- Zgodnie z art. 154 § 1 wymiar zależy od okresu zatrudnienia: 20 dni, jeśli pracownik jest zatrudniony krócej niż 10 lat, albo 26 dni, jeśli co najmniej 10 lat.
- Do okresu zatrudnienia wlicza się okresy poprzedniego zatrudnienia (art. 154¹ § 1).
- Do okresu pracy, od którego zależy wymiar urlopu, wlicza się z tytułu ukończenia szkoły wyższej 8 lat (art. 155 § 1 pkt 6).
- 7 lat pracy + 8 lat za szkołę wyższą = 15 lat, czyli co najmniej 10 lat, więc wymiar wynosi 26 dni.

Założenia: liczę tylko ukończone studia wyższe i 7 pełnych lat pracy. Jeśli część studiów odbywała Pani podczas pracy, art. 155 § 2 może wpływać na zaliczenie okresów. Tej reguły nie mam w źródłach, więc tego punktu nie rozstrzygam. Wynik nie zmieni się, jeśli nawet bez zaliczenia studiów staż wynosi 7 lat, bo wtedy wymiar byłby 20 dni. Dlatego warto ustalić, czy zaliczenie studiów zostało uwzględnione.

### p14 (calculation)

> I've worked 11 years in total, but only 2 of them with my current employer. Do I get 20 or 26 days of annual leave?

You get 26 days of annual leave. Art. 154 § 1 sets 26 days for an employee employed at least 10 years, and 20 days for one employed less than 10 years. The test is total length of employment, not time with your current employer. The leave calculation counted your 11 years in full, which gives 26 days (208 hours). It assumes all 11 years count, including those with previous employers, and that you have no school time to add. The rule that previous employment counts is in art. 154¹, which is not in the sources I have, so I cannot quote it. Art. 155 § 1 and § 2 would only matter if you need school time added, and you already qualify without it.

### p15 (calculation)

> Mam 12 lat stażu pracy i odchodzę z firmy 15 maja 2026. Ile dni urlopu przysługuje mi u obecnego pracodawcy w tym roku?

Przy 12 latach zatrudnienia pełny wymiar urlopu wynosi 26 dni w roku (art. 154 § 1 pkt 2). U obecnego pracodawcy urlop w roku 2026 przysługuje w wymiarze proporcjonalnym do okresu przepracowanego u niego w tym roku, czyli od 1 stycznia do 15 maja 2026 r. (art. 155¹ § 1 pkt 1), chyba że wcześniej wykorzystasz urlop w wymiarze równym lub wyższym.

Nie podaję konkretnej liczby dni proporcjonalnego urlopu. Narzędzie obliczeniowe wskazało okres 5 miesięcy i 11 dni, ale w polu liczby dni podało 26, czyli wymiar roczny, a nie wynik proporcjonalny. Zasada obliczania części za miesiąc, na której opiera się wynik, nie znajduje się w przepisach, którymi dysponuję. Niepełny dzień urlopu zaokrągla się w górę do pełnego dnia (art. 155³ § 1).

Niewykorzystany urlop przy ustaniu stosunku pracy przechodzi na ekwiwalent pieniężny (art. 171 § 1). Wyliczenie zależy też od tego, ile dni urlopu już wykorzystałeś w 2026 r.

### p16 (calculation)

> Я працюю на пів ставки, маю 4 роки стажу і загальну середню освіту. Скільки днів відпустки мені належить на рік?

Вам належить 10 днів відпустки на рік. Розрахунок: за стажем менше 10 років базова відпустка становить 20 днів (art. 154 § 1 pkt 1). До стажу зараховується 4 роки навчання за програмою середньої загальноосвітньої школи (art. 155 § 1 pkt 4), тож загальний період для відпустки становить 4 + 4 = 8 років, що все одно менше 10 років, тому базовий розмір залишається 20 днів. Оскільки ви працюєте на пів ставки, розмір пропорційно зменшується: 20 × 0,5 = 10 днів (art. 154 § 2). Це дорівнює 80 годинам. Припущення: усі 4 роки стажу були у роботодавців, навчання не поєднувалося з роботою (інакше діє правило про більш вигідний період, art. 155 § 2, яке тут не перевірялося). Якщо у вас є інші періоди роботи, варто їх перевірити, бо від загального стажу залежить, чи дійдете ви до 10 років і 26 днів.
