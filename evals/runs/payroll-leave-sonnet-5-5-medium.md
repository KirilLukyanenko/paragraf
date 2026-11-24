# Payroll eval payroll-leave-sonnet-5-5-medium

Answer model claude-sonnet-5-5, 4 questions, $0.33 ($0.083 a question), p50 15.3 s.

| Correct | Right tools | Expected amounts and dates | Amounts from tools | Citations verified | Expected articles cited | Question's language |
|---|---|---|---|---|---|---|
| 100% | 100% | 100% | 100% | 100% | 75% | 100% (0 written again) |

| Id | Category | Result | Tools | Problems |
|---|---|---|---|---|
| p13 | calculation | correct | annual_leave, notice_period!, annual_leave |  |
| p14 | calculation | correct | annual_leave | not cited: art. 154^1 |
| p15 | calculation | correct | annual_leave, annual_leave, annual_leave |  |
| p16 | calculation | correct | annual_leave, annual_leave, annual_leave, annual_leave, annual_leave |  |

## Answers

### p13 (calculation)

> Pracuję od 7 lat i skończyłam studia magisterskie. Ile dni urlopu wypoczynkowego mi przysługuje w roku?

Przysługuje Ci 26 dni urlopu wypoczynkowego w roku (208 godzin, przy pełnym etacie).

Obliczenie:
- Do okresu pracy, od którego zależy wymiar urlopu, wlicza się ukończenie szkoły wyższej – 8 lat (art. 155 § 1 pkt 6).
- 7 lat pracy + 8 lat studiów = 15 lat.
- 15 lat to co najmniej 10 lat, więc wymiar urlopu wynosi 26 dni (art. 154 § 1 pkt 2). Gdybyś miała mniej niż 10 lat, miałabyś 20 dni.

Uwagi: Okresy poprzedniego zatrudnienia wlicza się bez względu na przerwy (art. 154¹ § 1). Jeśli studiowałaś w czasie zatrudnienia, liczy się albo okres zatrudnienia, albo okres nauki, zależnie od tego, co jest korzystniejsze (art. 155 § 2). Tego kalkulacja nie sprawdzała. Przy niepełnym etacie wymiar zmniejsza się proporcjonalnie (art. 154 § 2). Jeśli to Twoja pierwsza praca, urlop w pierwszych latach nabywa się stopniowo, tego powyższe wyliczenie nie obejmuje. To ogólna informacja, nie porada prawna.

### p14 (calculation)

> I've worked 11 years in total, but only 2 of them with my current employer. Do I get 20 or 26 days of annual leave?

You get 26 days (208 hours), assuming full-time work. The 20 or 26 days depends on your total period of employment, not on your time with the current employer. Art. 154 § 1 gives 26 days if you have been employed at least 10 years. Your 11 years meet that threshold. The calculation tool applied art. 154¹ § 1, which counts earlier employment, whatever the breaks or the way the job ended. That article is not in the sources I was given, so I'm relying on the tool's quotation of it.

Assumptions and limits:
- School years can count toward the 10 years under art. 155, but you already pass the threshold without them.
- If you work part-time, the leave is proportional to your working time, and a part-day is rounded up to a full day (art. 154 § 2).
- The annual total cannot exceed 26 days (art. 154 § 3).
- If you changed employers during the year, the leave for that year may be proportional (art. 155¹). This is general information, not advice for your specific case.

### p15 (calculation)

> Mam 12 lat stażu pracy i odchodzę z firmy 15 maja 2026. Ile dni urlopu przysługuje mi u obecnego pracodawcy w tym roku?

Przysługuje Ci 11 dni urlopu u obecnego pracodawcy w 2026 r. (88 godzin przy pełnym etacie nie jest w wyniku narzędzia, więc podaję tylko dni).

Obliczenie:
1) Przy stażu 12 lat pełny wymiar to 26 dni, bo staż wynosi co najmniej 10 lat (art. 154 § 1 pkt 2).
2) W roku ustania stosunku pracy urlop jest proporcjonalny do okresu przepracowanego u dotychczasowego pracodawcy w tym roku (art. 155¹ § 1 pkt 1).
3) Każdy miesiąc pracy to 1/12 wymiaru urlopu, a niepełny miesiąc zaokrągla się w górę do pełnego (art. 155²ᵃ § 1 i 2). Od 1 stycznia do 15 maja to 5 miesięcy: styczeń–kwiecień oraz niepełny maj.
4) 5/12 × 26 dni daje po zaokrągleniu w górę do pełnego dnia 11 dni (art. 155³ § 1).

Założenia: zatrudnienie trwa u tego pracodawcy od początku roku, pełny etat, a urlop z tego roku nie został jeszcze wykorzystany. Jeśli już go wykorzystałeś w przysługującym lub wyższym wymiarze, nie przysługuje Ci więcej (art. 155¹ § 1 pkt 1). Niewykorzystany urlop zostanie wypłacony jako ekwiwalent (art. 171 § 1).

### p16 (calculation)

> Я працюю на пів ставки, маю 4 роки стажу і загальну середню освіту. Скільки днів відпустки мені належить на рік?

Вам належить 10 днів відпустки на рік (80 годин).

Розрахунок:
1) Стаж: 4 роки роботи + 4 роки за закінчення загальноосвітньої середньої школи (art. 155 § 1 pkt 4) = 8 років. Це менше 10 років, тож базовий розмір відпустки — 20 днів (art. 154 § 1 pkt 1).
2) Для роботи на пів ставки розмір відпустки встановлюється пропорційно до обсягу робочого часу (art. 154 § 2): 20 × 0,5 = 10 днів. Неповний день округлюється вгору, але тут виходить рівно 10 днів.

Примітки: період навчання й роботи сумуються за вашими даними. Якщо ви вчилися під час роботи, зараховується або період роботи, або період навчання — що вигідніше (art. 155 § 2). Це загальна інформація, а не юридична консультація.
