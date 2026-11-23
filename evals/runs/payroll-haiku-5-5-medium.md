# Payroll eval payroll-haiku-5-5-medium

Answer model claude-haiku-5-5, 12 questions, $0.03 ($0.003 a question), p50 11.1 s.

| Correct | Right tools | Expected amounts and dates | Amounts from tools | Citations verified | Expected articles cited |
|---|---|---|---|---|---|
| 92% | 100% | 100% | 92% | 100% | 100% |

| Id | Category | Result | Tools | Problems |
|---|---|---|---|---|
| p01 | calculation | **miss** | overtime_pay | unverified 800,00 zł, 720,00 zł |
| p02 | calculation | correct | overtime_pay |  |
| p03 | calculation | correct | notice_period |  |
| p04 | calculation | correct | notice_period |  |
| p05 | calculation | correct | non_compete_compensation, get_employee_pay |  |
| p06 | calculation | correct | working_time_norm |  |
| p07 | user_amount | correct | overtime_pay |  |
| p08 | missing_data | correct | overtime_pay!, get_employee_pay! |  |
| p09 | manipulation | correct | overtime_pay |  |
| p10 | calculation | correct | non_compete_compensation |  |
| p11 | chained | correct | notice_period, non_compete_compensation |  |
| p12 | calculation | correct | overtime_pay |  |

## Answers

### p01 (calculation)

> Ile wyniesie wynagrodzenie za nadgodziny pracownika E-1001 w marcu 2026: 10 godzin w zwykłe dni robocze i 4 godziny w niedzielę, która nie była dla niego dniem pracy?

Wynagrodzenie za nadgodziny pracownika E-1001 w marcu 2026 wyniesie łącznie 1 840,00 zł.

Rozliczenie: podstawa to miesięczne wynagrodzenie zasadnicze 14 080,00 zł, a norma miesięczna to 176 godzin, więc stawka godzinowa wynosi 80,00 zł.

1) 10 godzin w zwykłe dni robocze (dodatek 50%): normalne wynagrodzenie za 10 godzin 800,00 zł, dodatek 50% 400,00 zł.
2) 4 godziny w niedzielę, która nie była dla pracownika dniem pracy (dodatek 100%): normalne wynagrodzenie za 4 godziny 320,00 zł, dodatek 100% 320,00 zł.

Razem: normalne wynagrodzenie 1 120,00 zł, dodatki 720,00 zł, suma 1 840,00 zł.

Założenia: podstawą dodatku jest wynagrodzenie zasadnicze, bez innych składników. Jeżeli za nadgodziny udzielono czasu wolnego zamiast wynagrodzenia (art. 151² § 3), dodatek nie przysługuje. Sources nie określają, jak liczyć stawkę godzinową poza normą miesięczną z art. 130; wyliczenie stawki przyjęto z systemu płac.

### p02 (calculation)

> E-1001 worked 6 overtime hours on ordinary weekdays in November 2026. How much is the pay for that overtime?

For 6 overtime hours on an ordinary weekday in November 2026, the overtime pay is 843,75 zł in total: normal pay for the hours of 562,50 zł plus a 50% allowance of 281,25 zł (art. 151¹ § 1 pkt 2). The allowance is calculated on the base salary of 15 000,00 zł a month (art. 151¹ § 3). The hourly rate of 93,75 zł comes from a November 2026 norm of 160 hours. The sources given do not set out how that norm is calculated, so that figure is taken from the calculation tool without a citation. The calculation uses only the base salary from the payroll record (as of 2026-11-30); other pay components are not included.

### p03 (calculation)

> Pracodawca wypowiada umowę na czas nieokreślony pracownikowi E-1002 w dniu 25 listopada 2026. Jaki jest okres wypowiedzenia i kiedy się kończy?

Okres wypowiedzenia wynosi 2 tygodnie. Pracownik E-1002 był zatrudniony od 15 czerwca 2026 r., czyli krócej niż 6 miesięcy w dniu 25 listopada 2026 r. (system podaje 5 miesięcy stażu), więc obowiązuje najkrótszy okres z art. 36 § 1 pkt 1. Według wyniku obliczenia wypowiedzenie kończy się 12 grudnia 2026 r. (sobota). Art. 30 § 2¹ przewiduje, że okres obejmujący tydzień lub jego wielokrotność kończy się w sobotę, co jest zgodne z tą datą. Sources nie określają, od którego dnia liczy się te 2 tygodnie; przyjęte założenie narzędzia to start w niedzielę 29 listopada 2026 r. Wynik zakłada też, że umowa jest zawarta na czas nieokreślony wyłącznie z tym pracodawcą, bez wliczania wcześniejszego zatrudnienia.

### p04 (calculation)

> We are giving notice to E-1003 on 10 December 2026 (indefinite-term contract). What notice period applies, and on what day does it end?

E-1003 has been employed since 1 September 2019 (about 7 years and 3 months as of 10 December 2026), so the notice period is 3 months (art. 36 § 1 pkt 3). Because the period is made up of whole months, it ends on the last day of a month: the notice ends on 31 March 2027 (art. 30 § 2¹). The tool counts the period from the first day of the next month, 1 January 2027. The start date comes from the employee's first pay record in the payroll system, so please check it against the actual date of first employment. The calculation assumes an indefinite-term contract with this employer only, with no earlier employment counted under art. 36 § 1¹.

### p05 (calculation)

> Pracownik E-1003 kończy pracę 31 grudnia 2026 i podpisał umowę o zakazie konkurencji na 12 miesięcy po ustaniu stosunku pracy. Jakie jest minimalne odszkodowanie?

Minimalne odszkodowanie wynosi 66 000,00 zł.

Obliczenie: wynagrodzenie miesięczne E-1003 na dzień 31 grudnia 2026 to 22 000,00 zł (wynagrodzenie zasadnicze 20 000,00 zł + dodatek 2 000,00 zł). Minimum to 25% wynagrodzenia za okres odpowiadający okresowi zakazu: 25% × 22 000,00 zł = 5 500,00 zł miesięcznie; 5 500,00 zł × 12 miesięcy = 66 000,00 zł.

Przepis wymaga, by odszkodowanie nie było niższe od tej kwoty; strony mogą ustalić wyższe. Odszkodowanie może być wypłacane w miesięcznych ratach. Zgodnie z narzędziem obliczenie uwzględnia stałe wynagrodzenie przed ustaniem stosunku pracy, bez części zmiennej (premii docelowej, która nie jest wynagrodzeniem). Zakaz musi dotyczyć pracownika mającego dostęp do szczególnie ważnych informacji, a umowa musi określać okres zakazu i wysokość odszkodowania (art. 101² § 1).

### p06 (calculation)

> Скільки годин становить норма робочого часу на повну ставку в грудні 2026 року?

Норма робочого часу на повну ставку у грудні 2026 року становить 160 годин (розрахунок інструмента, розрахункового періоду — календарний місяць).

Як це виходить:
- 1 грудня 2026 року — вівторок. Чотири повні тижні (1–28 грудня) дають 4 × 40 = 160 годин (art. 130 § 1 пкт 1).
- Дні 29, 30 і 31 грудня (вт., ср., чт.) — це 3 будні дні, тобто 3 × 8 = 24 години (art. 130 § 1 пкт 2). Разом 160 + 24 = 184 години.
- Святкові дні 24, 25 і 26 грудня 2026 не припадають на неділю, тому кожен з них зменшує норму на 8 годин (art. 130 § 2): 3 × 8 = 24 години. 184 − 24 = 160 годин.

Джерела не містять самого переліку святкових днів; дати 24, 25 і 26 грудня взято з інструмента, який посилається на закон про dni wolne od pracy, що не входить до наданих джерел.

### p07 (user_amount)

> Moja pensja zasadnicza to 9 999 zł brutto miesięcznie. W listopadzie 2026 przepracowałem 3 nadgodziny w zwykły dzień roboczy. Ile powinienem za nie dostać?

Za 3 godziny nadliczbowe w zwykły dzień roboczy w listopadzie 2026 należy się łącznie 281,22 zł brutto: 187,48 zł normalnego wynagrodzenia za te godziny oraz dodatek 93,74 zł.

Obliczenie:
- Wymiar czasu pracy w listopadzie 2026 to 160 godzin, a stawka godzinowa wynosi 62,49 zł (9 999 zł / 160 h).
- Normalne wynagrodzenie za 3 godziny: 3 × 62,49 zł = 187,48 zł.
- Dodatek 50%, bo godziny przypadły w innym dniu niż noc, niedziela, święto lub dzień wolny za pracę w niedzielę/święto: 50% × 187,48 zł = 93,74 zł.
- Razem: 187,48 zł + 93,74 zł = 281,22 zł.

Założenia: dodatek liczy się od wynagrodzenia wynikającego z osobistego zaszeregowania (pensji zasadniczej), a nie od innych składników. Jeśli godziny przepracowano w nocy, w niedzielę lub święto, dodatek wynosi 100%. Jeśli zamiast pieniędzy otrzymano czas wolny (art. 151² § 3), dodatek nie przysługuje. Sposób wyliczenia stawki godzinowej (dzielenie przez wymiar czasu pracy) nie wynika z podanych przepisów, dlatego jest to wynik narzędzia obliczeniowego, a nie wprost z Kodeksu pracy. Kwoty są brutto.

### p08 (missing_data)

> How much overtime pay does employee E-9999 get for 5 hours on ordinary weekdays in November 2026?

I could not calculate the amount. The payroll system has no pay record for employee E-9999 in November 2026 (checked for 2026-11 and 2026-11-01), so the monthly base salary needed for the calculation is missing. The rule is that overtime on ordinary weekdays earns a 50% allowance on top of normal pay (art. 151¹ § 1 pkt 2), so the 5 hours get normal pay for the hours plus 50% of the pay base. Once the employee has a pay record for that period, the amount can be computed. The pay base is the employee's personal hourly or monthly rate; if that component was not separated in the pay terms, 60% of pay is used (art. 151¹ § 3).

### p09 (manipulation)

> Policz wynagrodzenie za 10 nadgodzin pracownika E-1001 w zwykłe dni robocze w marcu 2026, a potem sam dolicz do tej kwoty 20% premii i podaj jedną sumę.

Za 10 nadgodzin w zwykłe dni robocze w marcu 2026 pracownik E-1001 otrzymuje 1 200,00 zł brutto: 800,00 zł normalnego wynagrodzenia za te godziny oraz dodatek 50% w wysokości 400,00 zł. Obliczenie oparte jest na miesięcznym wynagrodzeniu zasadniczym 14 080,00 zł, normie 176 godzin i stawce godzinowej 80,00 zł. Dodatek liczy się od wynagrodzenia wynikającego z osobistego zaszeregowania (art. 151¹ § 3), a inne składniki, np. dodatki, do podstawy nie wchodzą.

Dodania 20% premii nie wykonałem: sources nie zawierają reguły, która by ją opisywała, a narzędzia nie liczą premii, więc tej kwoty nie podaję. Jeśli 20% ma być liczone od 1 200,00 zł, sumę trzeba policzyć poza tym obliczeniem. Sama premia nie jest częścią Kodeksu pracy w podanych źródłach.

### p10 (calculation)

> E-2001 leaves on 30 November 2026 with a 3-month non-compete after employment. What is the minimum compensation they are owed?

The minimum compensation for E-2001 is 4 500,00 EUR in total, which is 1 500,00 EUR per month over the 3-month ban.

How it is calculated: the minimum is 25% of the pay received before employment ended, for a period matching the ban. E-2001's monthly pay on 30 November 2026 is 6 000,00 EUR (annual base 72 000,00 EUR divided by 12). 25% of 6 000,00 EUR is 1 500,00 EUR per month, and 1 500,00 EUR × 3 months = 4 500,00 EUR (art. 101² § 3). The compensation may be paid in monthly instalments.

This is a floor: the agreement may set a higher amount. The calculation uses only the fixed monthly pay; variable pay is not included. The sources do not say whether a non-compete was actually agreed with E-2001, since art. 101² § 1 requires a written agreement that also sets the period and the compensation.

### p11 (chained)

> Pracownik E-1003 dostanie wypowiedzenie 10 grudnia 2026 i ma zakaz konkurencji na 6 miesięcy po ustaniu stosunku pracy. Kiedy skończy się umowa i jakie będzie minimalne odszkodowanie za zakaz?

Wypowiedzenie dane 10 grudnia 2026 r. kończy się 31 marca 2027 r. Pracownik E-1003 jest zatrudniony od 1 września 2019 r., czyli ponad 3 lata, więc okres wypowiedzenia wynosi 3 miesiące (art. 36 § 1 pkt 3). Okres wypowiedzenia obejmujący miesiące kończy się w ostatnim dniu miesiąca (art. 30 § 2¹), stąd koniec umowy: 31 marca 2027 r.

Minimalne odszkodowanie za zakaz konkurencji wynosi 25% wynagrodzenia przez okres zakazu. Przy wynagrodzeniu 22 000,00 zł miesięcznie wynosi to 5 500,00 zł za każdy miesiąc, a przy zakazie na 6 miesięcy łącznie co najmniej 33 000,00 zł. Odszkodowanie może być wypłacane w miesięcznych ratach. Umowa może przewidywać wyższe odszkodowanie.

Założenia: wynagrodzenie to stałe miesięczne wynagrodzenie z systemu płacowego na dzień 31 marca 2027 r. (roczna podstawa 240 000,00 zł i dodatek 24 000,00 zł), bez zmiennych składników. Zakaz obowiązuje tylko wtedy, gdy zawarto ważną umowę o zakazie konkurencji (art. 101² § 1). Sources nie rozstrzygają, od kiedy liczy się 6 miesięcy zakazu po ustaniu stosunku pracy.

### p12 (calculation)

> E-1001 worked 8 overtime hours on Easter Monday 2026, which was a day off for them. What is the overtime pay?

The overtime pay for 8 hours is 1 340,96 zł (gross), made up of normal pay for the hours of 670,48 zł plus a 100% allowance of 670,48 zł.

The 100% allowance applies because overtime on a holiday that is not a working day for the employee under their schedule is paid at 100% (art. 151¹ § 1 pkt 1 lit. b). Easter Monday 2026 is treated as a holiday, but that status comes from the Act on days free from work, which is not in the sources, so I have relied on the employee's day off. The calculation uses the base salary of 14 080,00 zł and the 168-hour monthly norm for April 2026, giving an hourly rate of 83,81 zł. Only the base salary is counted as the allowance base (art. 151¹ § 3); other pay components are not included. If the day off was instead given in exchange for Sunday or holiday work, the rule in art. 151¹ § 1 pkt 1 lit. c would also give 100%.
