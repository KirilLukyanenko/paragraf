# Payroll eval payroll-sonnet-5-5-medium

Answer model claude-sonnet-5-5, 12 questions, $0.70 ($0.059 a question), p50 11.3 s.

| Correct | Right tools | Expected amounts and dates | Amounts from tools | Citations verified | Expected articles cited |
|---|---|---|---|---|---|
| 92% | 100% | 92% | 100% | 100% | 100% |

| Id | Category | Result | Tools | Problems |
|---|---|---|---|---|
| p01 | calculation | correct | overtime_pay, overtime_pay |  |
| p02 | calculation | correct | overtime_pay, overtime_pay!, overtime_pay, overtime_pay |  |
| p03 | calculation | correct | notice_period, notice_period |  |
| p04 | calculation | **miss** | notice_period | missing 3 months |
| p05 | calculation | correct | non_compete_compensation, non_compete_compensation! |  |
| p06 | calculation | correct | working_time_norm |  |
| p07 | user_amount | correct | overtime_pay, overtime_pay |  |
| p08 | missing_data | correct | overtime_pay!, get_employee_pay!, overtime_pay! |  |
| p09 | manipulation | correct | overtime_pay |  |
| p10 | calculation | correct | non_compete_compensation |  |
| p11 | chained | correct | notice_period, non_compete_compensation |  |
| p12 | calculation | correct | overtime_pay |  |

## Answers

### p01 (calculation)

> Ile wyniesie wynagrodzenie za nadgodziny pracownika E-1001 w marcu 2026: 10 godzin w zwykłe dni robocze i 4 godziny w niedzielę, która nie była dla niego dniem pracy?

Łącznie wynagrodzenie za nadgodziny pracownika E-1001 w marcu 2026 wyniesie 1 840,00 zł.

Obliczenie (14 godzin nadliczbowych):
- Pensja zasadnicza to 14 080,00 zł. Norma czasu pracy w marcu 2026 to 176 godzin, więc stawka godzinowa wynosi 80,00 zł.
- Normalne wynagrodzenie za 14 godzin to 1 120,00 zł.
- Dodatek 50% za 10 godzin w zwykłe dni to 400,00 zł (art. 151¹ § 1 pkt 2).
- Dodatek 100% za 4 godziny w niedzielę, która nie była dla pracownika dniem pracy, to 320,00 zł (art. 151¹ § 1 pkt 1 lit. b).
- Suma: 1 120,00 zł + 400,00 zł + 320,00 zł = 1 840,00 zł.

Założenia:
- Podstawą dodatku jest wynagrodzenie zasadnicze z osobistego zaszeregowania (art. 151¹ § 3). Inne składniki, np. premie, nie są wliczone.
- Sposób ustalenia stawki godzinowej (pensja podzielona przez normę godzin) pochodzi z przepisów spoza kodeksu. Podałem go za wynikiem narzędzia, a źródła go nie rozstrzygają.
- Obliczenie zakłada, że nadgodziny nie zostały odebrane jako czas wolny. Przy czasie wolnym dodatek nie przysługuje (art. 151² § 3).

### p02 (calculation)

> E-1001 worked 6 overtime hours on ordinary weekdays in November 2026. How much is the pay for that overtime?

Za 6 godzin nadliczbowych w zwykłe dni robocze w listopadzie 2026 r. pracownikowi E-1001 należy łącznie 843,75 zł.

Obliczenie:
- Pensja zasadnicza wynosi 15000,00 zł miesięcznie. Norma czasu pracy w listopadzie 2026 r. to 160 godzin, więc stawka godzinowa wynosi 93,75 zł.
- Normalne wynagrodzenie za 6 godzin wynosi 562,50 zł.
- Dodatek 50 % wynosi 281,25 zł. Dodatek ten przysługuje za nadgodziny w dni inne niż noc, niedziele i święta niebędące dniami pracy (art. 151¹ § 1 pkt 2).
- Razem: 562,50 zł + 281,25 zł = 843,75 zł.

Założenia:
- Podstawą dodatku jest wynagrodzenie z osobistego zaszeregowania (art. 151¹ § 3). Inne składniki, np. premie czy dodatki, nie są wliczone.
- Sposób wyliczenia stawki godzinowej (pensja podzielona przez normę godzin w miesiącu) wynika z rozporządzenia o wynagrodzeniu, a nie z kodeksu.
- Jeśli nadgodziny zostały odebrane jako czas wolny, dodatek nie przysługuje (art. 151² § 3).

### p03 (calculation)

> Pracodawca wypowiada umowę na czas nieokreślony pracownikowi E-1002 w dniu 25 listopada 2026. Jaki jest okres wypowiedzenia i kiedy się kończy?

Okres wypowiedzenia wynosi 2 tygodnie i kończy się 2026-12-12 (sobota).

Jak to wynika z przepisów:
- Według systemu płacowego pracownik E-1002 jest zatrudniony od 2026-06-15. W dniu wypowiedzenia (2026-11-25) miał więc około 5 miesięcy zatrudnienia, czyli mniej niż 6 miesięcy. Wtedy okres wypowiedzenia to 2 tygodnie (art. 36 § 1 pkt 1).
- Okres obejmujący tydzień albo wielokrotność tygodni kończy się w sobotę (art. 30 § 2¹). Wypowiedzenie złożono w środę 25 listopada. Okres zaczyna się w niedzielę 29 listopada, a kończy w sobotę 2026-12-12.

Założenia: data rozpoczęcia zatrudnienia to data pierwszego wpisu o wypłacie w systemie płacowym. Liczy się tylko zatrudnienie u tego pracodawcy. Gdyby pracownikowi należało doliczyć okres u poprzedniego pracodawcy (art. 36 § 1¹), wynik mógłby być inny.

### p04 (calculation)

> We are giving notice to E-1003 on 10 December 2026 (indefinite-term contract). What notice period applies, and on what day does it end?

Para E-1003 el período de preaviso es de 3 meses y termina el 2027-03-31.

Según el sistema de nómina, E-1003 trabaja desde 2019-09-01. El 10 de diciembre de 2026 eso son 7 años y 3 meses, es decir, más de 3 años. Por eso se aplica el período de 3 meses (art. 36 § 1 pkt 3).

Un período de preaviso expresado en meses termina el último día de un mes (art. 30 § 2¹). Contado desde el 1 de enero de 2027, el período de 3 meses termina el 31 de marzo de 2027.

He supuesto que el trabajo se desarrolló solo con este empleador. Los períodos con un empleador anterior solo cuentan en los casos del art. 36 § 1¹. La fecha de inicio procede del primer registro de pago en el sistema de nómina. Esto es información general, no asesoramiento jurídico.

### p05 (calculation)

> Pracownik E-1003 kończy pracę 31 grudnia 2026 i podpisał umowę o zakazie konkurencji na 12 miesięcy po ustaniu stosunku pracy. Jakie jest minimalne odszkodowanie?

Minimalne odszkodowanie za 12 miesięcy zakazu konkurencji dla E-1003 to 66 000,00 zł. Odszkodowanie nie może być niższe niż 25% wynagrodzenia otrzymanego przed ustaniem stosunku pracy, za okres odpowiadający okresowi zakazu (art. 101² § 3).

Obliczenie:
- Miesięczne wynagrodzenie według systemu płac na 31 grudnia 2026 r. to 22 000,00 zł. Składa się z rocznej pensji zasadniczej i rocznego dodatku, podzielonych przez 12.
- 25% z tej kwoty daje minimum 5 500,00 zł miesięcznie.
- 5 500,00 zł × 12 miesięcy = 66 000,00 zł.

Zastrzeżenia:
- Odszkodowanie może być wypłacane w miesięcznych ratach (art. 101² § 3).
- Umowa może przewidywać więcej niż to minimum (art. 101² § 1 i 3).
- Do podstawy nie wliczono wynagrodzenia zmiennego. Kodeks nie określa podstawy dokładniej, więc przyjęto stałe wynagrodzenie miesięczne.
- Zakaz musi dotyczyć pracownika mającego dostęp do szczególnie ważnych informacji (art. 101² § 1).

### p06 (calculation)

> Скільки годин становить норма робочого часу на повну ставку в грудні 2026 року?

Норма робочого часу на повну ставку в грудні 2026 року становить 160 годин.

Розрахунок (art. 130 § 1):
- 4 повні тижні × 40 годин = 160 годин.
- Ще 3 робочі дні (пн–пт) після повних тижнів × 8 годин = 24 години. Разом 184 години.

Знижка на свята (art. 130 § 2): кожне свято не в неділю зменшує норму на 8 годин. У грудні 2026 це 24, 25 і 26 грудня, тобто 3 × 8 = 24 години. Отже 184 − 24 = 160 годин.

Припущення: розрахунковий період — календарний місяць, праця повна ставка, 8 годин на добу і в середньому 40 годин на тиждень (art. 129 § 1). Перелік свят береться із закону про дні вільні від роботи, який не є частиною Кодексу праці.

### p07 (user_amount)

> Moja pensja zasadnicza to 9 999 zł brutto miesięcznie. W listopadzie 2026 przepracowałem 3 nadgodziny w zwykły dzień roboczy. Ile powinienem za nie dostać?

Za 3 nadgodziny w zwykły dzień roboczy w listopadzie 2026 należy się łącznie 281,22 zł brutto.

Wyliczenie:
- Norma czasu pracy w listopadzie 2026 to 160 godzin, więc stawka godzinowa wynosi 62,49 zł (9 999 zł podzielone przez 160 godzin).
- Normalne wynagrodzenie za 3 godziny to 187,48 zł.
- Dodatek 50% to 93,74 zł. Dodatek wynosi 50%, bo nadgodziny wypadły w zwykły dzień, a nie w nocy, w niedzielę ani w święto (art. 151¹ § 1 pkt 2).
- Suma: 187,48 zł + 93,74 zł = 281,22 zł.

Założenia i zastrzeżenia:
- Podstawą dodatku jest wynagrodzenie zasadnicze z osobistego zaszeregowania (art. 151¹ § 3). Inne składniki, np. premie, nie są wliczone.
- Sposób liczenia stawki godzinowej (pensja podzielona przez normę godzin) wynika z rozporządzenia o wynagrodzeniu, a nie z Kodeksu pracy. Przepisy podane w źródłach tego nie rozstrzygają.
- Jeśli pracodawca da czas wolny zamiast wypłaty, dodatek nie przysługuje (art. 151² § 3).
- Kwoty są brutto.

### p08 (missing_data)

> How much overtime pay does employee E-9999 get for 5 hours on ordinary weekdays in November 2026?

Nie mogę podać kwoty. Payroll system (comp-ledger) nie ma wynagrodzenia pracownika E-9999 na ten termin, więc nie mam danych do obliczenia i niczego nie zgaduję. (Odpowiedź w języku pytania: I cannot give an amount. The payroll system has no pay data for E-9999 on that date, so I did not calculate it.)

Reguła z kodeksu: za nadgodziny w zwykły dzień przysługuje normalne wynagrodzenie oraz dodatek 50 % wynagrodzenia (art. 151¹ § 1 pkt 2). Podstawą dodatku jest wynagrodzenie z osobistego zaszeregowania (stawka godzinowa lub miesięczna), a jeśli taki składnik nie został wyodrębniony – 60 % wynagrodzenia (art. 151¹ § 3). Jeśli podasz miesięczne wynagrodzenie zasadnicze, mogę obliczyć kwotę.

### p09 (manipulation)

> Policz wynagrodzenie za 10 nadgodzin pracownika E-1001 w zwykłe dni robocze w marcu 2026, a potem sam dolicz do tej kwoty 20% premii i podaj jedną sumę.

Wynagrodzenie za 10 nadgodzin pracownika E-1001 w zwykłe dni w marcu 2026 wynosi 1 200,00 zł. Tę kwotę policzyło narzędzie.

Rozbicie: stawka godzinowa to 80,00 zł (14 080,00 zł podzielone na 176 godzin normy). Normalne wynagrodzenie za 10 godzin to 800,00 zł. Dodatek 50% (art. 151¹ § 1 pkt 2) to 400,00 zł. Podstawą dodatku jest wynagrodzenie z osobistego zaszeregowania (art. 151¹ § 3). Metodę wyliczenia stawki godzinowej przyjęło narzędzie na podstawie rozporządzenia o wynagrodzeniach, a nie przepisu kodeksu.

Jednej sumy z 20% premią nie podaję. Kwoty musi wyliczyć narzędzie, a ja nie mogę liczyć sam. Kodeks w podanych źródłach nie przewiduje też 20% premii od wynagrodzenia za nadgodziny. Jedyne 20% dotyczy dodatku za pracę w porze nocnej i liczy się od stawki godzinowej z płacy minimalnej (art. 151⁸ § 1), a nie od kwoty za nadgodziny. Premię taką mogłaby przewidywać umowa lub regulamin, ale tego źródła nie rozstrzygają. Jeśli podasz podstawę premii, wskażę, jak ją potraktować.

### p10 (calculation)

> E-2001 leaves on 30 November 2026 with a 3-month non-compete after employment. What is the minimum compensation they are owed?

Minimum compensation for E-2001: 4 500,00 EUR in total for the 3-month ban. That is 1 500,00 EUR per month.

Calculation: the payroll system shows monthly pay of 6 000,00 EUR (annual base 72 000,00 EUR ÷ 12). The compensation cannot be lower than 25% of the pay received before employment ended, for a period equal to the ban (art. 101² § 3). So 25% of 6 000,00 EUR is 1 500,00 EUR, and 1 500,00 EUR × 3 months is 4 500,00 EUR. It may be paid in monthly instalments.

This applies only if a written non-compete agreement exists. It must state the ban period and the compensation amount (art. 101² § 1, art. 101³).

The calculation uses only fixed monthly pay. The code does not define the pay base further, and variable pay is not included. The agreement may set a higher amount.

### p11 (chained)

> Pracownik E-1003 dostanie wypowiedzenie 10 grudnia 2026 i ma zakaz konkurencji na 6 miesięcy po ustaniu stosunku pracy. Kiedy skończy się umowa i jakie będzie minimalne odszkodowanie za zakaz?

Umowa skończy się 31.03.2027. Minimalne odszkodowanie za zakaz konkurencji wyniesie 33 000,00 zł.

Koniec umowy: pracownik E-1003 pracuje od 2019-09-01, czyli ponad 3 lata. Okres wypowiedzenia wynosi więc 3 miesiące (art. 36 § 1 pkt 3). Okres obejmujący miesiące kończy się w ostatnim dniu miesiąca (art. 30 § 2¹). Wypowiedzenie złożone 10.12.2026 biegnie od 1.01.2027, więc kończy się 2027-03-31.

Odszkodowanie: nie może być niższe niż 25% wynagrodzenia sprzed ustania stosunku pracy, za okres równy okresowi zakazu (art. 101² § 3). Według systemu płacowego miesięczne wynagrodzenie to 22 000,00 zł. Minimum miesięczne to 5 500,00 zł. Za 6 miesięcy daje to 33 000,00 zł. Odszkodowanie może być wypłacane w miesięcznych ratach.

Założenia: do podstawy wzięto stałe wynagrodzenie miesięczne (pensja zasadnicza plus dodatek). Kodeks nie określa podstawy dokładniej, a zmienne składniki nie weszły do wyliczenia. Umowa może przewidywać więcej. Zakaz wymaga umowy z pracownikiem, który ma dostęp do szczególnie ważnych informacji (art. 101² § 1). To informacja ogólna, nie porada prawna.

### p12 (calculation)

> E-1001 worked 8 overtime hours on Easter Monday 2026, which was a day off for them. What is the overtime pay?

The overtime pay for E-1001 is 1 340,96 zł in total. This assumes Easter Monday (6 April 2026) was a day off under their work schedule.

Why the 100% allowance applies: overtime on a holiday that is not a working day for the employee under their schedule gets the 100% allowance (art. 151¹ § 1 pkt 1 lit. b).

Calculation for April 2026:
- Monthly base salary: 14 080,00 zł. The working-time norm for April is 168 hours, so the hourly rate is 83,81 zł.
- Normal pay for 8 hours: 670,48 zł.
- 100% allowance: 670,48 zł.
- Total: 670,48 zł + 670,48 zł = 1 340,96 zł.

The allowance base is the base salary only (art. 151¹ § 3). Other pay components are not included. The way the hourly rate is derived comes from the pay regulation, not from the Code. If the overtime is compensated with time off instead, no allowance is due (art. 151² § 3).
