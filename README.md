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

## OBSŁUGA APLIKACJI
### OKNO LOGOWANIA
Okno logowania jest obrazem witającym nas po włączeniu aplikacji. Jak wyżej wspomniano, aby zalogować się do pełnej aplikacji trzeba podać prawidłowe dane logowania.</br>
W prawym górnym rogu znajduje się mała ikonka, która po kliknięciu ukaże nam 'Dropdown menu', z którego mamy 2 opcje do wyboru:
- **Help** jest na ten moment wyłącznie elementem wizualnym i nie implementuje żadnej funkcjonalności
- **Exit** to tak zwany 'shutdown', który zamyka całą aplikacje



https://github.com/Enzoolino/ProjectDatabase/assets/31781455/dacefb98-2430-4dba-a865-f753edb3526c

### GŁÓWNE OKNO I PANEL NAWIGACJI
   
