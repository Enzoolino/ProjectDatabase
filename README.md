# ProjectDatabase
Aplikacja stworzona jako projekt semestralny do przedmiotu "**Programowanie Obiektowe**". Jej funkcjonalność sprowadza się do zarządzania lokalną bazą danych, w tym przypadku dla całego koncernu/firmy **Dealera samochodów**, którego działalność rozszerza się na większym obszarze.

## OGÓLNE INFORMACJE
### SQL INSERT
Kod bazy SQL, którą trzeba samodzielnie 'zinsertować' znajduje się bezpośrednio wewnątrz pobranego projektu środowiska programowania, w folderze 'SQL'. </br></br>
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
Okno logowania jest obrazem witającym nas po włączeniu aplikacji. Jak wyżej wspomniano, aby zalogować się do pełnej aplikacji, trzeba podać prawidłowe dane logowania.</br>
W prawym górnym rogu znajduje się mała ikonka, która po kliknięciu ukaże 'Dropdown menu', z którego mamy 2 opcje do wyboru:
- **Help** jest na ten moment wyłącznie elementem wizualnym i nie implementuje żadnej funkcjonalności
- **Exit** to tak zwany 'shutdown', który zamyka całą aplikacje

https://github.com/Enzoolino/ProjectDatabase/assets/31781455/dacefb98-2430-4dba-a865-f753edb3526c

### GŁÓWNE OKNO I PANEL NAWIGACJI
Po zalogowaniu z lewej strony można zauważyć Panel boczny, służący do nawigacji po aplikacji. Pierwsze jego 7 przycisków służy do zmiany widoku, który oglądamy, a ostatnia z opcji to Wylogowanie (Logout), która cofa nas do ekranu logowania, skąd możemy zalogować się ponownie na wybrane konto lub zamknąć aplikację.**Ciągnąc za Panel boczny możemy kontrolować pozycję aplikacji.**

https://github.com/Enzoolino/ProjectDatabase/assets/31781455/a50bbc20-cc95-433a-975e-8f0651941012

Dodatkowo w lewym górnym rogu mamy element z imieniem i nazwiskiem, który aktualizuje się automatycznie, bazując na zalogowanym obecnie użytkowniku.

### STRONA HOME
Strona Home jest pierwszą stroną, która wita użytkownika i ma nastawienie bardziej na wizualność niż funkcjonalność. W prawym górnym rogu widzimy datę, która automatycznie synchronizuje się z obecną lokalną datą.
Zaraz pod nią znajdują się trzy 'karty' z różnymi informacjami.
1. **Sold Vehicles** - Pobiera informacje z bazy danych i wyświetla ile firma sprzedała do tej pory samochodów</br>

![Card1](https://github.com/Enzoolino/ProjectDatabase/assets/31781455/6ce30891-7b6d-45c0-b29b-c8a3784cf095)

2. **Opened Dealerships** - Pobiera informacje z bazy danych i wyświetla ile firma otworzyła do tej pory salonów samochodowych</br>

![Card2](https://github.com/Enzoolino/ProjectDatabase/assets/31781455/f873889c-9511-42c0-a832-4a2e3822c943)

3. **Employee of the Month** - Wyświetla nazwisko pracownika miesiąca, które jest jasno wpisane w kodzie</br>

![card3](https://github.com/Enzoolino/ProjectDatabase/assets/31781455/6aa6cca4-0c39-4abd-a287-9d494c45a9b2)

Pod spodem, w centralnej części okna znajduje się animacja oparta na 'Frame'ach', aby zapewnić jej płynność. Trwa ona 18 sekund i w rzeczywistości jest zwykłym loopem, który będzie resetował się w nieskończoność.

https://github.com/Enzoolino/ProjectDatabase/assets/31781455/10b74d2d-df77-411e-9e00-5666bd2840bb

### SEKCJA CUSTOMERS
Pierwszą z opcji jest sekcja Customers (klienci), która zawiera w sobie 2 zmienne widoki.
#### Customers
Okno pozwala na dodawanie, usuwanie, edytowanie klientów. Klienci to osoby, które wykonały jakikolwiek zakup od firmy... Należy jednak zwrócić uwagę, że to klient jest przypisany do zakupu, a nie na odwrót, przez co możemy uzyskać kilku takich samych klientów, ale przypisanych do innego zakupu. Poniżej podane jest wytłumaczenie wszystkich kolumn tabeli Custoemers:
- **Sin** - To główny identyfikator klienta oraz zakupu, który zapożyczony jest z tabeli 'Purchases' (Zakupy) - Composite Primary Key
- **Vin** - To numer Vin samochodu, który zakupił klient, również zapożyczony z tabeli 'Purchases' (Zakupy) - Composite Primary key
- **Name** - To imię danego klienta
- **Surname** - To nazwisko danego klienta
- **Phone** - To numer telefonu danego klienta
- **Add Time** - To czas w którym klient został dodany, a więc też czas zakupu
- **Operations** - Znajdują się tutaj przyciski, które pozwalają nam edytować obecne wiersze (Usuń, Edytuj)

Aplikacja pozwoli nam dodać tylko Siny, które nie mają jeszcze przypisanych klientów (oraz powiązane z nimi Vin, Czas) i tak samo dla edycji, pozwoli nam zamienić Sin tylko na taki, który nie został przypisany jeszcze do innego klienta.</br>
Usuwanie jest mniej restrykcyjne, ponieważ klient nie jest zapożyczany przez żadną inną tabelę, jednak tego typu działanie wskazane jest tylko w momencie np. Anulacji zakupu.</br>

Dodatkowo znajduje się tutaj dynamiczna funkcja Search (Szukaj), która wraz z wpisywaniem do okna tekstu, przeszukuje tabelę i filtruje po wszystkich możliwych rekordach, które w jakiejś kolumnie zawierają wpisany tekst.
#### Purchase
TBC...
