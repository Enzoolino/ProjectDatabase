# ProjectDatabase
Aplikacja stworzona jako projekt semestralny do przedmiotu "**Programowanie Obiektowe**". Jej funkcjonalność sprowadza się do zarządzania lokalną bazą danych, w tym przypadku dla całego koncernu/firmy **Dealera samochodów**, którego działalność rozszerza się na większym obszarze.

## OGÓLNE INFORMACJE
### SQL INSERT
Kod bazy SQL, którą trzeba samodzielnie 'zinsertować' znajduje się bezpośrednio wewnątrz pobranego projektu środowiska programowania, wewnątrz folderu 'SQL'. </br></br>
[SQL file](/WorkshopDataModifier/SQL/Dealership.sql)

### LOGIN
Aplikacja obsługuje system logowania, z czego każde konto jest przypisane do konkretnego pracownika, a każdy pracownik może mieć inny poziom dostępu:
   - Aplikacja obsługuje na ten moment 2 poziomy dostępu:
      - Administrator ( > 2 )
      - Pracownik ( < 2 )
       
   - Przykładowy dane logowanie, które znajdują się od razu w pliku SQL to:
      - **Dla administratora:** Username: admin , Password: admin 
      - **Dla pracownika:** Username: work , Password: work

### KLUCZE OBCE
Podczas korzystania z aplikacji trzeba zwracać uwagę na klucze obce, zwłaszcza jeśli jesteśmy zalogowani jako administrator. Nie dojdzie do żadnych poważnych błędów, gdyż aplikacja ma zaimplementowaną obsługę różnych przypadków, ale brak znajomości tych kluczy może utrudnić korzystanie z aplikacji:</br>
```
ALTER TABLE [users] ADD CONSTRAINT [AppAccount] FOREIGN KEY ([Owner]) REFERENCES
[employee] ([EmpID]) ON UPDATE CASCADE ON DELETE CASCADE
GO

ALTER TABLE [employee] ADD CONSTRAINT [EmpBranch] FOREIGN KEY ([BranchID]) REFERENCES
[branch_office] ([BranchID]) ON UPDATE CASCADE ON DELETE NO ACTION
GO

ALTER TABLE [employee] ADD CONSTRAINT [Superior] FOREIGN KEY ([SuperiorID]) REFERENCES
[employee] ([EmpID]) ON UPDATE NO ACTION ON DELETE NO ACTION
GO

ALTER TABLE [employee] ADD CONSTRAINT [WorkerLocation] FOREIGN KEY ([WorkLocation]) REFERENCES
[dealership] ([Name]) ON UPDATE CASCADE ON DELETE CASCADE --
GO

ALTER TABLE [employee] ADD CONSTRAINT [PositionName] FOREIGN KEY ([Position]) REFERENCES
[position] ([Name]) ON UPDATE CASCADE ON DELETE NO ACTION
GO

ALTER TABLE [purchase] ADD CONSTRAINT [PurchasedVin] FOREIGN KEY ([Vin]) REFERENCES
[sold_vehicles] ([Vin]) ON UPDATE CASCADE ON DELETE CASCADE
GO

ALTER TABLE [purchase] ADD CONSTRAINT [PurchasedDealership] FOREIGN KEY ([Dealership]) REFERENCES
[dealership] ([Name]) ON UPDATE CASCADE ON DELETE CASCADE
GO

ALTER TABLE [sell] ADD CONSTRAINT [SellingEmp] FOREIGN KEY ([EmpID]) REFERENCES
[employee] ([EmpID]) ON UPDATE CASCADE ON DELETE CASCADE
GO

ALTER TABLE [sell] ADD CONSTRAINT [TransactionIdentity] FOREIGN KEY ([Sin], [Vin]) REFERENCES
[purchase] ([Sin], [Vin]) ON UPDATE NO ACTION ON DELETE NO ACTION
GO

ALTER TABLE [customer] ADD CONSTRAINT [CustomerIdentity] FOREIGN KEY ([Sin], [Vin]) REFERENCES
[purchase] ([Sin], [Vin]) ON UPDATE CASCADE ON DELETE NO ACTION
GO

ALTER TABLE [dealership] ADD CONSTRAINT [DealershipBranch] FOREIGN KEY ([BranchID]) REFERENCES
[branch_office] ([BranchID]) ON UPDATE NO ACTION ON DELETE NO ACTION
GO

ALTER TABLE [warehouse] ADD CONSTRAINT [WarehouseBranch] FOREIGN KEY ([BranchID]) REFERENCES
[branch_office] ([BranchID]) ON UPDATE NO ACTION ON DELETE NO ACTION
GO

ALTER TABLE [warehouse_vehicles] ADD CONSTRAINT [BrandWarehouseIdentity] FOREIGN KEY ([Brand]) REFERENCES
[brands] ([Name]) ON UPDATE CASCADE ON DELETE NO ACTION
GO

ALTER TABLE [warehouse_vehicles] ADD CONSTRAINT [CarWarehouse] FOREIGN KEY ([Warehouse]) REFERENCES
[warehouse] ([Name]) ON UPDATE CASCADE ON DELETE NO ACTION
GO

ALTER TABLE [vehicles] ADD CONSTRAINT [BrandIdentity] FOREIGN KEY ([Brand]) REFERENCES
[brands] ([Name]) ON UPDATE CASCADE ON DELETE NO ACTION
GO

ALTER TABLE [vehicles] ADD CONSTRAINT [CarDealership] FOREIGN KEY ([Dealership]) REFERENCES
[dealership] ([Name]) ON UPDATE CASCADE ON DELETE NO ACTION
GO
```

## OBSŁUGA / DZIAŁALNOŚĆ APLIKACJI
### OKNO LOGOWANIA
Okno logowania jest obrazem witającym nas po włączeniu aplikacji. Jak wyżej wspomniano, aby zalogować się do pełnej aplikacji trzeba podać prawidłowe dane logowania.</br>
W prawym górnym rogu znajduje się mała ikonka, która po kliknięciu ukaże nam 'Dropdown menu', z którego mamy 2 opcje do wyboru:
- **Help** jest na ten moment wyłącznie elementem wizualnym i nie implementuje żadnej funkcjonalności
- **Exit** to tak zwany 'shutdown', który zamyka całą aplikacje

https://github.com/Enzoolino/ProjectDatabase/assets/31781455/dacefb98-2430-4dba-a865-f753edb3526c

### GŁÓWNE OKNO I PANEL NAWIGACJI
Po zalogowaniu można zauważyć tzw. Stronę główną czyli HomePage oraz z lewej strony Panel boczny służący do nawigacji po aplikacji. Ostatnia z opcji to Wylogowanie (Logout), która cofa nas do ekranu logowania skąd możemy zalogować się znowu na inne konto lub zamknąć aplikację.

https://github.com/Enzoolino/ProjectDatabase/assets/31781455/a50bbc20-cc95-433a-975e-8f0651941012

Dodatkowo w lewym górnym rogu mamy element z imieniem i nazwiskiem, który automatycznie się aktualizuje bazując na zalogowanym obecnie użytkowniku.

### STRONA HOME
Strona Home jest pierwszą stroną, która wita użytkownika i ma nastawienie bardziej na wizualność niż funkcjonalność. W prawym górnym rogu widzimy datę, która automatycznie synchronizuje się z obecną lokalną datą.
Zaraz pod nią widzimu trzy 'karty' z różnymi informacjami.
1. **Sold Vehicles** - Pobiera informacje z bazy danych i wyświetla ile firma sprzedała do tej pory samochodów</br>

![Card1](https://github.com/Enzoolino/ProjectDatabase/assets/31781455/6ce30891-7b6d-45c0-b29b-c8a3784cf095)

2. **Opened Dealerships** - Pobiera informacje z bazy danych i wyświetla ile firma otworzyła do tej pory salonów samochodowych</br>

![Card2](https://github.com/Enzoolino/ProjectDatabase/assets/31781455/f873889c-9511-42c0-a832-4a2e3822c943)

3. **Employee of the Month** - Wyświetla nazwisko pracownika miesiąca, które jest jasno wpisane w kodzie</br>

![card3](https://github.com/Enzoolino/ProjectDatabase/assets/31781455/6aa6cca4-0c39-4abd-a287-9d494c45a9b2)

Pod spodem, w centralnej części okna znajduje się animacja oparta na 'Frame'ach', aby zapewnić jej płynność. Trwa ona 18 sekund i jest zwykłym loopem z obrazkami.

https://github.com/Enzoolino/ProjectDatabase/assets/31781455/10b74d2d-df77-411e-9e00-5666bd2840bb

TBC...
