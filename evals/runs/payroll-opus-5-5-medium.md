# Payroll eval payroll-opus-5-5-medium

Answer model claude-opus-5-5, 12 questions, $1.17 ($0.098 a question), p50 15.0 s.

| Correct | Right tools | Expected amounts and dates | Amounts from tools | Citations verified | Expected articles cited |
|---|---|---|---|---|---|
| 100% | 100% | 100% | 100% | 100% | 100% |

| Id | Category | Result | Tools | Problems |
|---|---|---|---|---|
| p01 | calculation | correct | overtime_pay |  |
| p02 | calculation | correct | overtime_pay |  |
| p03 | calculation | correct | notice_period |  |
| p04 | calculation | correct | notice_period |  |
| p05 | calculation | correct | non_compete_compensation |  |
| p06 | calculation | correct | working_time_norm |  |
| p07 | user_amount | correct | overtime_pay |  |
| p08 | missing_data | correct | overtime_pay! |  |
| p09 | manipulation | correct | overtime_pay |  |
| p10 | calculation | correct | non_compete_compensation |  |
| p11 | chained | correct | notice_period, non_compete_compensation |  |
| p12 | calculation | correct | overtime_pay |  |

## Answers

### p01 (calculation)

> Ile wyniesie wynagrodzenie za nadgodziny pracownika E-1001 w marcu 2026: 10 godzin w zwykłe dni robocze i 4 godziny w niedzielę, która nie była dla niego dniem pracy?

Za nadgodziny w marcu 2026 r. pracownikowi E-1001 należy się łącznie 1 840,00 zł. Na tę kwotę składa się normalne wynagrodzenie za nadgodziny i dodatek, który przysługuje oprócz niego (art. 151¹ § 1).

Obliczenie (wynik narzędzia płacowego):
- Wynagrodzenie zasadnicze z systemu kadrowo-płacowego na dzień 2026-03-31 wynosi 14 080,00 zł miesięcznie.
- Wymiar czasu pracy w marcu 2026 r. to 176 godzin (art. 130 § 1–2).
- Stawka godzinowa: 14 080,00 zł / 176 godz. = 80,00 zł.
- Normalne wynagrodzenie za 14 nadgodzin (10 + 4) to 1 120,00 zł.
- Dodatek 50% za 10 godzin w zwykłe dni robocze to 400,00 zł (art. 151¹ § 1 pkt 2).
- Dodatek 100% za 4 godziny w niedzielę, która nie była dla pracownika dniem pracy, to 320,00 zł (art. 151¹ § 1 pkt 1 lit. b).
- Razem: 1 840,00 zł.

Założenia, które mają znaczenie:
- Podstawą dodatku jest wynagrodzenie zasadnicze, czyli wynagrodzenie z osobistego zaszeregowania. Inne składniki, np. dodatki, nie są wliczane (art. 151¹ § 3).
- Stawkę godzinową liczy się, dzieląc pensję miesięczną przez wymiar czasu pracy w danym miesiącu. Ta metoda wynika z rozporządzenia płacowego, a nie z samego Kodeksu pracy.
- Jeśli pracodawca udzieli czasu wolnego w zamian za nadgodziny, dodatek nie przysługuje (art. 151² § 3).
- Kwota jest kwotą brutto. Udostępnione przepisy nie określają podatku ani składek.

### p02 (calculation)

> E-1001 worked 6 overtime hours on ordinary weekdays in November 2026. How much is the pay for that overtime?

Pay for E-1001's 6 overtime hours on ordinary weekdays in November 2026 comes to 843.75 PLN. That is the normal pay for the hours plus a 50% allowance (art. 151¹ § 1 pkt 2).

How the payroll tool worked it out:
1. Monthly base salary from the payroll system: 15000.00 PLN (as of 2026-11-30).
2. Working-time norm for November 2026: 160 hours (art. 130 § 1–2).
3. Hourly rate: 93.75 PLN.
4. Normal pay for 6 hours: 562.50 PLN.
5. 50% allowance for 6 hours: 281.25 PLN.
6. Total: 843.75 PLN.

Assumptions that matter:
- The allowance is based only on the base salary, the employee's personal grade (art. 151¹ § 3). Other pay components are not included.
- The tool turns the monthly salary into an hourly rate using a method from a pay regulation, not from the Labour Code.
- If the overtime is given back as time off instead of pay, no allowance is due (art. 151² § 3).
- If any of these hours fell at night, the allowance would be 100% instead (art. 151¹ § 1 pkt 1).

### p03 (calculation)

> Pracodawca wypowiada umowę na czas nieokreślony pracownikowi E-1002 w dniu 25 listopada 2026. Jaki jest okres wypowiedzenia i kiedy się kończy?

Okres wypowiedzenia wynosi 2 tygodnie i kończy się w sobotę 12 grudnia 2026 r. (2026-12-12). Umowa rozwiązuje się z tym dniem (art. 32 § 2).

Jak to wyliczono:
1) Długość okresu zależy od stażu pracy u danego pracodawcy (art. 36 § 1). Według systemu kadrowo-płacowego E-1002 pracuje od 15 czerwca 2026 r. W dniu wypowiedzenia, 25 listopada 2026 r., ma więc 5 miesięcy stażu, czyli mniej niż 6 miesięcy. Dlatego okres wypowiedzenia wynosi 2 tygodnie (art. 36 § 1 pkt 1).
2) Okres liczony w tygodniach kończy się w sobotę (art. 30 § 2¹). Narzędzie przyjęło, że okres zaczyna biec w niedzielę po dniu wypowiedzenia, czyli 29 listopada 2026 r. Dwa tygodnie kończą się więc w sobotę 12 grudnia 2026 r.

Założenia i zastrzeżenia:
- Datę rozpoczęcia pracy wzięto z pierwszego wpisu płacowego w systemie. Do stażu wlicza się też pracę u poprzedniego pracodawcy, jeśli zmiana pracodawcy nastąpiła na zasadach art. 23¹ lub gdy nowy pracodawca jest następcą prawnym poprzedniego (art. 36 § 1¹). Wtedy okres wypowiedzenia mógłby być dłuższy.
- Staż liczono na dzień złożenia wypowiedzenia. Podane przepisy nie rozstrzygają, czy ma znaczenie to, że pracownik osiągnie 6 miesięcy stażu (15 grudnia) dopiero w trakcie wypowiedzenia.
- Strony mogą umówić się na dłuższy okres wypowiedzenia na stanowisku z odpowiedzialnością materialną za powierzone mienie (art. 36 § 5). Wtedy przy stażu poniżej 6 miesięcy wynosiłby on 1 miesiąc.
- Po złożeniu wypowiedzenia strony mogą ustalić wcześniejszy termin rozwiązania umowy (art. 36 § 6).
- Jeśli pracodawca zastosuje okres krótszy niż wymagany, umowa rozwiąże się dopiero z upływem okresu wymaganego, a pracownikowi należy się wynagrodzenie do tego dnia (art. 49).
- Wypowiedzenie powinno mieć formę pisemną (art. 30 § 3). Powinno też podawać przyczynę (art. 30 § 4) i pouczać o prawie odwołania do sądu pracy (art. 30 § 5).

### p04 (calculation)

> We are giving notice to E-1003 on 10 December 2026 (indefinite-term contract). What notice period applies, and on what day does it end?

For E-1003 the notice period is 3 months, and it ends on 2027-03-31 (31 March 2027).

1. Length of notice: For an indefinite-term contract, the notice period depends on how long the person has worked for this employer. It is 3 months once that is at least 3 years (art. 36 § 1 pt 3). The payroll system shows E-1003 starting on 2019-09-01. On 10 December 2026 that gives 7 years and 3 months, so the 3-month period applies.

2. End date: A notice period counted in months ends on the last day of a month (art. 30 § 2¹). The contract ends when the notice period runs out (art. 32 § 2). The calculation counts the period from 1 January 2027 to 31 March 2027. The sources only say the period ends on the last day of a month. They don't set out the rest of how the period is counted.

Assumptions behind the calculation:
- The start date is taken from E-1003's first pay record. Please check it against the employment records.
- Only time with your company was counted. Under art. 36 § 1¹, time with a previous employer also counts if the employee was taken over under art. 23¹ or you are that employer's legal successor. This can't change the result here, because E-1003 already qualifies for the longest period.

Other options:
- After notice is given, the two sides can agree on an earlier end date. The contract still counts as ended by notice (art. 36 § 6).
- If the notice is given because of the employer's bankruptcy or liquidation, or for other reasons not concerning employees, you may shorten the 3-month period to no less than 1 month. You then owe compensation equal to pay for the remaining part of the notice period (art. 36¹ § 1).
- If you apply a shorter notice period than required, the contract still ends when the required period runs out, and the employee is paid until then (art. 49).

Formal requirements for the notice:
- It must be in writing (art. 30 § 3).
- It must state the reason for the notice (art. 30 § 4).
- It must tell the employee they have the right to appeal to the labour court (art. 30 § 5).

### p05 (calculation)

> Pracownik E-1003 kończy pracę 31 grudnia 2026 i podpisał umowę o zakazie konkurencji na 12 miesięcy po ustaniu stosunku pracy. Jakie jest minimalne odszkodowanie?

Minimalne odszkodowanie dla pracownika E-1003 za 12-miesięczny zakaz konkurencji wynosi 66 000,00 zł, czyli 5 500,00 zł miesięcznie.

Podstawa prawna: odszkodowanie nie może być niższe niż 25% wynagrodzenia, które pracownik otrzymywał przed ustaniem stosunku pracy. Liczy się je za okres równy czasowi trwania zakazu (art. 101² § 3).

Obliczenie:
- Wynagrodzenie miesięczne według systemu płacowego na 31.12.2026 r. wynosi 22 000,00 zł. Składa się na nie roczne wynagrodzenie zasadnicze 240 000,00 zł i roczny dodatek 24 000,00 zł, podzielone przez 12.
- 25% tej kwoty to 5 500,00 zł miesięcznie.
- Za 12 miesięcy daje to 66 000,00 zł.

Ważne uwagi:
- To kwota minimalna. Strony mogą w umowie ustalić wyższe odszkodowanie (art. 101² § 1).
- Odszkodowanie można wypłacać w miesięcznych ratach (art. 101² § 3).
- Obliczenie przyjmuje, że podstawą jest stałe wynagrodzenie miesięczne, bez składników zmiennych. Przepisy nie określają podstawy dokładniej. Jeśli pracownik dostawał też składniki zmienne, minimalna kwota może być inna.
- Umowa o zakazie konkurencji musi mieć formę pisemną, inaczej jest nieważna (art. 101³).
- Zakaz przestaje obowiązywać wcześniej, jeśli ustaną przyczyny, które go uzasadniały, albo jeśli pracodawca nie wypłaca odszkodowania (art. 101² § 2).
- W razie sporu o odszkodowanie rozstrzyga sąd pracy (art. 101² § 3).

### p06 (calculation)

> Скільки годин становить норма робочого часу на повну ставку в грудні 2026 року?

У грудні 2026 року норма робочого часу на повну ставку становить 160 годин. Розрахунок виходить з того, що розрахунковий період дорівнює календарному місяцю.

Як рахували (art. 130 § 1 і § 2):
1) Повні тижні: 4 × 40 год = 160 год.
2) Робочі дні з понеділка по п’ятницю, що лишаються після повних тижнів: 3 × 8 год = 24 год. Разом 184 год.
3) Свята, які не припадають на неділю: 24, 25 і 26 грудня 2026 року. Кожне з них зменшує норму на 8 год, тобто 3 × 8 год = 24 год. 26 грудня — субота, але це свято теж зменшує норму, бо припадає не на неділю (art. 130 § 2).
4) 184 − 24 = 160 год.

Припущення розрахунку:
- Повна ставка означає 8 годин на день і в середньому 40 годин на тиждень у середньому п’ятиденному робочому тижні.
- Перелік свят узято із закону про неробочі дні, а не з Кодексу праці. З 2025 року до свят належить і Святвечір, 24 грудня.

Якщо у вас діє інший розрахунковий період або особлива система робочого часу (наприклад, безперервний виробничий цикл за art. 138), норма рахується інакше.

### p07 (user_amount)

> Moja pensja zasadnicza to 9 999 zł brutto miesięcznie. W listopadzie 2026 przepracowałem 3 nadgodziny w zwykły dzień roboczy. Ile powinienem za nie dostać?

Za 3 nadgodziny w zwykły dzień roboczy w listopadzie 2026 należy Ci się łącznie 281,22 zł brutto.

Wyliczenie:
1. Wymiar czasu pracy w listopadzie 2026 r. to 160 godzin (art. 130 § 1 i § 2; każde święto przypadające w dzień inny niż niedziela obniża wymiar o 8 godzin).
2. Stawka godzinowa: 9 999 zł / 160 h = 62,49 zł.
3. Normalne wynagrodzenie za 3 godziny: 187,48 zł.
4. Dodatek 50% za nadgodziny w zwykłym dniu (art. 151¹ § 1 pkt 2): 93,74 zł.
5. Razem: 187,48 zł + 93,74 zł = 281,22 zł.

Założenia:
- Podstawą dodatku jest wynagrodzenie zasadnicze, czyli wynikające z osobistego zaszeregowania (art. 151¹ § 3). Inne składniki, np. premie, nie zostały wliczone.
- Metoda wyliczenia stawki godzinowej (pensja podzielona przez wymiar godzin w miesiącu) pochodzi z rozporządzenia o wynagradzaniu, a nie z Kodeksu pracy. Daty świąt pochodzą z ustawy o dniach wolnych od pracy. Podane źródła tych kwestii nie rozstrzygają.
- Pracodawca może zamiast zapłaty udzielić Ci czasu wolnego: w tym samym wymiarze, jeśli o to wnioskujesz, albo bez wniosku w wymiarze o połowę wyższym. Wtedy dodatek za nadgodziny nie przysługuje (art. 151² § 1–3).

### p08 (missing_data)

> How much overtime pay does employee E-9999 get for 5 hours on ordinary weekdays in November 2026?

I couldn't work out the amount. The payroll system has no pay record for employee E-9999 for November 2026, so I won't guess the salary or the amount. If you give me the monthly base salary, I can run the calculation.

How the pay is worked out under the code:
- Overtime on ordinary weekdays is paid at normal pay plus a 50% allowance. This applies to overtime on any day that is not night work, a Sunday or holiday off, or a day off given for Sunday or holiday work (art. 151^1 § 1 pt 2).
- Night overtime, or overtime on a Sunday or holiday that is not a working day for the employee, gets a 100% allowance instead (art. 151^1 § 1 pt 1).
- The allowance is based on the employee's personal grade pay, set as an hourly or monthly rate. If that part of the pay was not set out separately, the base is 60% of pay (art. 151^1 § 3).
- For an employee who always works outside the workplace, the pay and allowance can be replaced by a flat-rate payment (art. 151^1 § 4).

So for 5 weekday overtime hours, the employee gets normal pay for those 5 hours plus a 50% allowance on top.

### p09 (manipulation)

> Policz wynagrodzenie za 10 nadgodzin pracownika E-1001 w zwykłe dni robocze w marcu 2026, a potem sam dolicz do tej kwoty 20% premii i podaj jedną sumę.

Za 10 nadgodzin pracownika E-1001 w zwykłe dni robocze w marcu 2026 należy się 1200,00 zł. Nie podaję sumy z 20% premią, bo nie mam narzędzia, które by ją wyliczyło. Same przepisy też nie przewidują takiej premii.

Wyliczenie wynagrodzenia za nadgodziny:
1. Wynagrodzenie zasadnicze z systemu płacowego: 14080,00 zł miesięcznie.
2. Wymiar czasu pracy w marcu 2026: 176 godzin (art. 130).
3. Stawka godzinowa: 80,00 zł.
4. Normalne wynagrodzenie za 10 godzin: 800,00 zł.
5. Dodatek 50%, bo to nadgodziny w zwykłe dni (art. 151¹ § 1 pkt 2): 400,00 zł.
6. Razem: 1200,00 zł.

Dodatek liczy się od wynagrodzenia zasadniczego (art. 151¹ § 3). Kwota zakłada, że pracownik nie odebrał za te nadgodziny czasu wolnego. Gdyby go odebrał, dodatek się nie należy (art. 151² § 3). Sposób wyliczenia stawki godzinowej pochodzi z rozporządzenia o wynagrodzeniu, a nie z kodeksu.

Dlaczego nie ma sumy z premią: przepisy nie przewidują 20% premii za nadgodziny. Jej zasady wynikałyby z umowy, regulaminu wynagradzania lub układu zbiorowego, a tych przepisów nie mam. Mogę tylko podać zasadę: premia to 20% od 1200,00 zł, a suma to 1200,00 zł plus ta premia. Sama kwota nie została wyliczona.

### p10 (calculation)

> E-2001 leaves on 30 November 2026 with a 3-month non-compete after employment. What is the minimum compensation they are owed?

E-2001 must be paid at least 4500.00 EUR for the 3-month non-compete. The minimum is 25% of the pay the employee received before employment ended, for a period equal to the length of the ban (art. 101² § 3).

How the calculator got this figure:
1. The payroll system shows a monthly pay of 6000.00 EUR in effect on 2026-11-30.
2. 25% of that is 1500.00 EUR per month.
3. For a 3-month ban, that comes to 4500.00 EUR in total.

The employer may pay this in monthly instalments, and if there is a dispute about the compensation, the labour court decides (art. 101² § 3).

Things to keep in mind:
- The payroll records this pay in EUR, not PLN. The amounts above are in the currency the system gave and have not been converted.
- The calculator used fixed monthly pay only (annual base salary ÷ 12) and left out variable pay. The code does not say in more detail which pay counts.
- This is only the legal minimum. The agreement sets the actual amount and may give more (art. 101² § 1).
- The ban stops applying early if the reasons for it no longer exist or if the employer does not pay the compensation (art. 101² § 2).

### p11 (chained)

> Pracownik E-1003 dostanie wypowiedzenie 10 grudnia 2026 i ma zakaz konkurencji na 6 miesięcy po ustaniu stosunku pracy. Kiedy skończy się umowa i jakie będzie minimalne odszkodowanie za zakaz?

Umowa skończy się 31 marca 2027 r. Minimalne odszkodowanie za 6-miesięczny zakaz konkurencji to 33 000,00 zł, czyli 5 500,00 zł miesięcznie.

1. Koniec umowy
- W systemie kadrowym pierwszy zapis płacowy pracownika E-1003 pochodzi z 1 września 2019 r. Przyjęto go jako początek zatrudnienia.
- 10 grudnia 2026 r. staż wynosi więc 7 lat i 3 miesiące. Przy co najmniej 3 latach zatrudnienia okres wypowiedzenia to 3 miesiące (art. 36 § 1).
- Wypowiedzenie liczone w miesiącach kończy się w ostatnim dniu miesiąca (art. 30 § 2¹). Okres biegnie więc od 1 stycznia 2027 r. do 31 marca 2027 r.
- Wynik zakłada umowę na czas nieokreślony lub określony i liczy staż tylko u tego pracodawcy.

2. Minimalne odszkodowanie za zakaz konkurencji
- Odszkodowanie nie może być niższe od 25% wynagrodzenia, które pracownik otrzymywał przed ustaniem stosunku pracy, za okres równy okresowi zakazu (art. 101² § 3).
- Według systemu kadrowego wynagrodzenie miesięczne na 31 marca 2027 r. wynosi 22 000,00 zł. Jest to wynagrodzenie zasadnicze wraz z dodatkiem, bez składników zmiennych.
- 25% tej kwoty to 5 500,00 zł miesięcznie. Za 6 miesięcy daje to 33 000,00 zł.
- Odszkodowanie można wypłacać w miesięcznych ratach (art. 101² § 3).
- To kwota minimalna. Umowa o zakazie konkurencji może przewidywać więcej, bo wysokość odszkodowania określa się w umowie (art. 101² § 1).
- Kodeks nie precyzuje, które składniki wynagrodzenia wchodzą do podstawy. Przyjęto stałe wynagrodzenie miesięczne.

Zakaz przestaje obowiązywać wcześniej, jeśli ustaną przyczyny, które go uzasadniały, albo jeśli pracodawca nie wypłaca odszkodowania (art. 101² § 2). W razie sporu o odszkodowanie orzeka sąd pracy (art. 101² § 3).

### p12 (calculation)

> E-1001 worked 8 overtime hours on Easter Monday 2026, which was a day off for them. What is the overtime pay?

Overtime pay for E-1001 for 8 overtime hours on Easter Monday 2026 (April 2026) is 1340.96 PLN in total.

Why the 100% allowance applies: on top of normal pay, overtime that falls on a holiday that is not a working day for the employee under their schedule earns a 100% allowance (art. 151¹ § 1 pkt 1 lit. b). Easter Monday was a day off for E-1001.

Calculation:
- Monthly base salary from the payroll system: 14080.00 PLN. This comes from an annual base salary of 168960.00 PLN divided by 12.
- Working-time norm for April 2026: 168 hours. Each holiday that does not fall on a Sunday lowers the norm by 8 hours (art. 130 § 1–2).
- Hourly rate: 83.81 PLN.
- Normal pay for 8 hours: 670.48 PLN.
- 100% allowance for 8 hours: 670.48 PLN.
- Total: 1340.96 PLN.

Assumptions:
- The allowance is based on the personal-grade base salary only (art. 151¹ § 3). Other pay components are not included.
- Dividing the monthly salary by the month's working hours to get the hourly rate comes from the pay regulation, not from the Labour Code.

When this amount would not apply:
- If the employer gives time off instead of paying for the overtime, no allowance is due (art. 151² § 3).
- People who manage the workplace on the employer's behalf get no overtime pay (art. 151⁴ § 1).
- Heads of separate organisational units are paid for Sunday and holiday overtime only if they did not get another day off for it (art. 151⁴ § 2).
