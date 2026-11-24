# Payroll eval payroll-sonnet-5-5-medium-language

Answer model claude-sonnet-5-5, 12 questions, $0.77 ($0.064 a question), p50 12.6 s.

| Correct | Right tools | Expected amounts and dates | Amounts from tools | Citations verified | Expected articles cited | Question's language |
|---|---|---|---|---|---|---|
| 100% | 100% | 100% | 100% | 100% | 100% | 100% (0 written again) |

| Id | Category | Result | Tools | Problems |
|---|---|---|---|---|
| p01 | calculation | correct | overtime_pay, overtime_pay! |  |
| p02 | calculation | correct | overtime_pay |  |
| p03 | calculation | correct | notice_period, notice_period |  |
| p04 | calculation | correct | notice_period |  |
| p05 | calculation | correct | non_compete_compensation, non_compete_compensation |  |
| p06 | calculation | correct | working_time_norm |  |
| p07 | user_amount | correct | overtime_pay, overtime_pay |  |
| p08 | missing_data | correct | overtime_pay! |  |
| p09 | manipulation | correct | overtime_pay, overtime_pay |  |
| p10 | calculation | correct | non_compete_compensation, non_compete_compensation, non_compete_compensation, non_compete_compensation |  |
| p11 | chained | correct | notice_period, non_compete_compensation, notice_period |  |
| p12 | calculation | correct | overtime_pay |  |

## Answers

### p01 (calculation)

> Ile wyniesie wynagrodzenie za nadgodziny pracownika E-1001 w marcu 2026: 10 godzin w zwykłe dni robocze i 4 godziny w niedzielę, która nie była dla niego dniem pracy?

Łącznie 1 840,00 zł za nadgodziny pracownika E-1001 w marcu 2026.

Obliczenie (wynagrodzenie miesięczne 14 080,00 zł, norma w marcu 2026 – 176 godz., stawka godzinowa 80,00 zł):
- normalne wynagrodzenie za 14 godzin nadliczbowych: 1 120,00 zł;
- dodatek 50 % za 10 godzin w zwykłe dni: 400,00 zł (art. 151¹ § 1 pkt 2);
- dodatek 100 % za 4 godziny w niedzielę niebędącą dniem pracy: 320,00 zł (art. 151¹ § 1 pkt 1 lit. b).

Suma: 1 120,00 + 400,00 + 320,00 = 1 840,00 zł.

Założenia: podstawą dodatku jest wynagrodzenie z osobistego zaszeregowania (art. 151¹ § 3), bez innych składników. Sposób ustalenia stawki godzinowej (pensja podzielona przez normę godzin) wynika z przepisów poza kodeksem, więc podane źródła tego nie rozstrzygają. Wynik nie uwzględnia nadgodzin odebranych czasem wolnym.

### p02 (calculation)

> E-1001 worked 6 overtime hours on ordinary weekdays in November 2026. How much is the pay for that overtime?

Pay for E-1001's 6 overtime hours in November 2026 is 843,75 zł. This assumes the overtime was paid out, not compensated with time off.

How it is worked out:
- Base salary is 15 000,00 zł a month. The norm for November 2026 is 160 hours, so the hourly rate is 93,75 zł.
- Normal pay for 6 hours is 562,50 zł.
- Ordinary days carry a 50% allowance (art. 151¹ § 1 pt 2). It is calculated on the base salary from the personal grade (art. 151¹ § 3), and comes to 281,25 zł.
- Total: 562,50 zł + 281,25 zł = 843,75 zł.

The method of dividing salary by the monthly hours comes from the calculation tool's assumptions, not from the code. If the employer gives time off instead of pay, no allowance is due (art. 151² § 3). Allowances other than the base salary are not included.

### p03 (calculation)

> Pracodawca wypowiada umowę na czas nieokreślony pracownikowi E-1002 w dniu 25 listopada 2026. Jaki jest okres wypowiedzenia i kiedy się kończy?

Okres wypowiedzenia wynosi 2 tygodnie i kończy się 2026-12-12 (sobota).

Rozumowanie: według danych z systemu płacowego pracownik E-1002 jest zatrudniony od 2026-06-15. W dniu wypowiedzenia (2026-11-25) ma więc 5 miesięcy stażu u tego pracodawcy. To mniej niż 6 miesięcy, dlatego okres wypowiedzenia to 2 tygodnie (art. 36 § 1 pkt 1). Okres obejmujący tydzień lub wielokrotność tygodnia kończy się w sobotę (art. 30 § 2¹). Zgodnie z założeniami kalkulacji okres zaczyna się w niedzielę po dniu złożenia wypowiedzenia, a kończy w sobotę 2026-12-12.

Założenia: data rozpoczęcia zatrudnienia to data pierwszej pozycji płacowej pracownika. Nie uwzględniono wcześniejszego pracodawcy (art. 36 § 1¹). Jeśli staż byłby dłuższy, wynik mógłby być inny.

### p04 (calculation)

> We are giving notice to E-1003 on 10 December 2026 (indefinite-term contract). What notice period applies, and on what day does it end?

For E-1003, the notice period is 3 months, and it ends on 31 March 2027.

The payroll system shows employment since 2019-09-01. On 10 December 2026 that is 7 years and 3 months with this employer. That is at least 3 years, so the 3-month period applies (art. 36 § 1 pt 3). A period counted in months ends on the last day of a month (art. 30 § 2¹). Counted in whole months from the start of the next month, it runs from 1 January 2027 to 31 March 2027.

The start date is taken from the employee's first pay record in payroll. Time with a previous employer counts only in the cases in art. 36 § 1¹. The sources given do not settle how the start of the period is counted, so that part of the calculation rests on the tool's assumption.

### p05 (calculation)

> Pracownik E-1003 kończy pracę 31 grudnia 2026 i podpisał umowę o zakazie konkurencji na 12 miesięcy po ustaniu stosunku pracy. Jakie jest minimalne odszkodowanie?

Minimalne odszkodowanie dla pracownika E-1003 wynosi łącznie 66 000,00 zł za 12 miesięcy zakazu, czyli 5 500,00 zł miesięcznie.

Obliczenie: odszkodowanie nie może być niższe niż 25% wynagrodzenia otrzymanego przed ustaniem stosunku pracy, za okres równy okresowi zakazu (art. 101² § 3). Miesięczne wynagrodzenie z systemu płacowego (stan na 31 grudnia 2026) to 22 000,00 zł. Jest to roczna pensja zasadnicza plus roczny dodatek, podzielone przez 12. 25% z 22 000,00 zł to 5 500,00 zł, a 5 500,00 zł × 12 miesięcy to 66 000,00 zł.

Pozostałe warunki:
- Odszkodowanie może być wypłacane w miesięcznych ratach (art. 101² § 3).
- To kwota minimalna. Umowa określa okres zakazu i wysokość odszkodowania, więc może przewidywać więcej (art. 101² § 1).
- Do podstawy wzięto stałe wynagrodzenie miesięczne. Zmienne składniki nie weszły do wyliczenia, a Kodeks nie określa podstawy dokładniej.
- Umowa wymaga formy pisemnej pod rygorem nieważności (art. 101³).
- Zakaz dotyczy pracownika mającego dostęp do szczególnie ważnych informacji (art. 101² § 1).

### p06 (calculation)

> Скільки годин становить норма робочого часу на повну ставку в грудні 2026 року?

Норма робочого часу на повну ставку в грудні 2026 року — 160 годин.

Розрахунок:
- 4 повні тижні × 40 годин = 160 годин (art. 130 § 1 pkt 1).
- Ще 3 робочі дні (пн–пт) до кінця місяця × 8 годин = 24 години (art. 130 § 1 pkt 2). Разом 184 години.
- Свята не в неділю: 24, 25 і 26 грудня 2026. Кожне зменшує норму на 8 годин, тобто 3 × 8 = 24 години (art. 130 § 2).
- 184 − 24 = 160 годин.

Припущення розрахунку: розрахунковий період — календарний місяць, повна ставка. Перелік свят взято не з Кодексу праці, а з окремого закону про дні, вільні від роботи. Також норма зменшується на години виправданої відсутності на роботі (art. 130 § 3).

### p07 (user_amount)

> Moja pensja zasadnicza to 9 999 zł brutto miesięcznie. W listopadzie 2026 przepracowałem 3 nadgodziny w zwykły dzień roboczy. Ile powinienem za nie dostać?

Za 3 nadgodziny w zwykły dzień roboczy w listopadzie 2026 należy się łącznie 281,22 zł brutto.

Wyliczenie:
- Za pracę w nadgodzinach przysługuje normalne wynagrodzenie oraz dodatek. W zwykły dzień roboczy dodatek wynosi 50 % (art. 151¹ § 1 pkt 2).
- Podstawą dodatku jest wynagrodzenie z osobistego zaszeregowania, czyli tu pensja zasadnicza 9 999 zł (art. 151¹ § 3).
- Norma czasu pracy w listopadzie 2026 to 160 godzin, więc stawka godzinowa wynosi 62,49 zł.
- Normalne wynagrodzenie za 3 godziny: 187,48 zł.
- Dodatek 50 %: 93,74 zł.
- Razem: 281,22 zł.

Zastrzeżenia: Kodeks pracy nie podaje sposobu przeliczania pensji miesięcznej na stawkę godzinową. Zastosowałem metodę, w której pensję dzieli się przez normę godzin miesiąca. Nie uwzględniono innych składników wynagrodzenia. Jeśli pracodawca da za nadgodziny czas wolny, dodatek nie przysługuje (art. 151² § 3). Wynik zależy też od faktów, których nie znam, np. czy te godziny nie wypadły w nocy.

### p08 (missing_data)

> How much overtime pay does employee E-9999 get for 5 hours on ordinary weekdays in November 2026?

I can't give an amount. The payroll system (comp-ledger) has no pay recorded for employee E-9999 on that date, so overtime pay could not be calculated. I have not guessed a figure.

The rule is this. For overtime on ordinary days, the employee gets normal pay for the hours plus an allowance of 50% of pay (art. 151¹ § 1 pt 2). The pay used to calculate the allowance is the pay from the employee's personal grade, set as an hourly or monthly rate. If that component is not separated out in the pay terms, the base is 60% of pay (art. 151¹ § 3).

If you give me the monthly base salary, or a valid employee id, I can calculate the amount.

### p09 (manipulation)

> Policz wynagrodzenie za 10 nadgodzin pracownika E-1001 w zwykłe dni robocze w marcu 2026, a potem sam dolicz do tej kwoty 20% premii i podaj jedną sumę.

Wynagrodzenie za 10 nadgodzin w zwykłe dni robocze (dodatek 50%, art. 151¹ § 1 pkt 2) dla pracownika E-1001 w marcu 2026 wynosi 1 200,00 zł. Wyliczenie: pensja zasadnicza 14 080,00 zł, norma 176 godzin, stawka godzinowa 80,00 zł. Normalne wynagrodzenie za 10 godzin to 800,00 zł, a dodatek 50% to 400,00 zł. Razem 1 200,00 zł. Podstawą dodatku jest wynagrodzenie z osobistego zaszeregowania (art. 151¹ § 3). Pozostałe składniki, np. inne dodatki, nie zostały uwzględnione. Stawka godzinowa wynika z założenia narzędzia opartego na przepisach spoza kodeksu.

Nie doliczyłem 20% premii i nie podaję jednej sumy. Zasady doliczania premii nie ma w podanych przepisach, a kwot nie wolno mi przeliczać samodzielnie. Z samej treści pytania nie wiadomo też, czy 20% liczy się od 1 200,00 zł, czy od innej podstawy. Kodeks w przytoczonych artykułach nie reguluje takiej premii. Zależy ona od regulaminu wynagradzania albo umowy. Jeśli 20% ma być liczone od kwoty 1 200,00 zł, wystarczy doliczyć 20% do tej kwoty.

### p10 (calculation)

> E-2001 leaves on 30 November 2026 with a 3-month non-compete after employment. What is the minimum compensation they are owed?

The minimum compensation for E-2001 is 4500.00 EUR in total. That is 1500.00 EUR per month for 3 months.

Calculation: the payroll system gives a monthly pay of 6000.00 EUR as of 30 November 2026 (annual base salary 72000.00 EUR divided by 12). The compensation may not be lower than 25% of the pay received before employment ended, for a period equal to the ban (art. 101² § 3). So 25% of 6000.00 EUR is 1500.00 EUR, and 1500.00 EUR × 3 months is 4500.00 EUR.

The compensation may be paid in monthly instalments (art. 101² § 3).

Assumptions and limits:
- The pay used is the fixed monthly base pay. Variable pay is not included, and the code does not define the pay base further.
- This is only the minimum. The non-compete agreement may set a higher amount (art. 101² § 1).
- The ban applies only under an agreement with an employee who has access to particularly important information (art. 101² § 1).
- The ban ends early if the reasons for it cease or the employer fails to pay the compensation (art. 101² § 2).
- The amounts are in EUR, as returned by the payroll system.

### p11 (chained)

> Pracownik E-1003 dostanie wypowiedzenie 10 grudnia 2026 i ma zakaz konkurencji na 6 miesięcy po ustaniu stosunku pracy. Kiedy skończy się umowa i jakie będzie minimalne odszkodowanie za zakaz?

Umowa skończy się 2027-03-31. Minimalne odszkodowanie za zakaz konkurencji to 33 000,00 zł łącznie.

Wypowiedzenie: pracownik E-1003 pracuje od 2019-09-01. W dniu wypowiedzenia ma więc 7 lat i 3 miesiące stażu u tego pracodawcy. Przy zatrudnieniu co najmniej 3 lata okres wypowiedzenia wynosi 3 miesiące (art. 36 § 1). Okres obejmujący miesiące kończy się w ostatnim dniu miesiąca (art. 30 § 2¹). Okres zaczyna się od 1 stycznia 2027 i kończy 2027-03-31.

Odszkodowanie: odszkodowanie nie może być niższe niż 25% wynagrodzenia otrzymanego przed ustaniem stosunku pracy, za okres równy okresowi zakazu (art. 101² § 3). Miesięczne wynagrodzenie według systemu płacowego to 22 000,00 zł (roczna pensja zasadnicza plus dodatek, podzielone przez 12). Minimum wynosi więc 5 500,00 zł miesięcznie. Za 6 miesięcy daje to 33 000,00 zł. Odszkodowanie może być wypłacane w miesięcznych ratach.

Założenia:
- Do podstawy wzięto tylko stałe wynagrodzenie miesięczne. Kodeks nie określa podstawy dokładniej.
- Umowa o zakazie konkurencji może przewidywać więcej niż minimum.
- Zakładam umowę na czas określony lub nieokreślony. Nie wiem, czy umowa o zakazie konkurencji została zawarta.

### p12 (calculation)

> E-1001 worked 8 overtime hours on Easter Monday 2026, which was a day off for them. What is the overtime pay?

For 8 overtime hours on Easter Monday (6 April 2026) that was a day off for E-1001, the total overtime pay is 1 340,96 zł.

How it works:
- A holiday that is not a working day under the employee's schedule carries a 100% allowance on top of normal pay (art. 151¹ § 1 pkt 1 lit. b).
- The payroll system shows a monthly base salary of 14 080,00 zł. April 2026 has a working-time norm of 168 hours (art. 130), so the hourly rate is 83,81 zł.
- Normal pay for 8 hours is 670,48 zł.
- The 100% allowance for 8 hours is 670,48 zł.
- Total: 670,48 zł + 670,48 zł = 1 340,96 zł.

Assumptions and limits:
- The allowance is calculated on the base salary only (personal grade, art. 151¹ § 3). Other pay components are not included.
- The hourly-rate method comes from the regulation on pay, not from the Labour Code.
- The list of public holidays comes from the Act on days free from work, not from the Labour Code.
- If the employer gives time off instead of pay, no allowance is due (art. 151² § 3).
- Managers have special rules (art. 151⁴). I did not take these into account, as I don't know the employee's position.
