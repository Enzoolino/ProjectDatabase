CREATE DATABASE Dealership
GO
USE Dealership
GO

-- branch_office ----------------------------------------------START

-- Structure for table "branch_office"

CREATE TABLE [branch_office] (
  [BranchID] Int IDENTITY(1000, 1) NOT NULL,
  [Location] Varchar(45) NOT NULL,
  [Phone] Varchar(11) DEFAULT NULL,
  CONSTRAINT [CHK_CorrectNumberBranch] CHECK (Phone NOT LIKE '%[^0-9]%')
);
GO

-- Keys for table "branch_office"

ALTER TABLE [branch_office] ADD CONSTRAINT [PK_Branch] PRIMARY KEY ([BranchID]);
GO
ALTER TABLE [branch_office] ADD CONSTRAINT [UQ_PhoneBranch] UNIQUE ([Phone]);
GO

-- branch_office ----------------------------------------------END



-- customer ----------------------------------------------START

-- Structure for table "customer"

CREATE TABLE [customer] (
  [Sin] Bigint NOT NULL,
  [Vin] Int NOT NULL,
  [Name] Varchar(45) DEFAULT NULL,
  [Surname] Varchar(45) DEFAULT NULL,
  [Phone] Varchar(11) DEFAULT NULL,
  [AddTime] Datetime DEFAULT GETDATE(),
  CONSTRAINT [CHK_CorrectNumberCustomer] CHECK (Phone NOT LIKE '%[^0-9]%')
);
GO

-- Keys for table "customer"

ALTER TABLE [customer] ADD CONSTRAINT [PK_Customer] PRIMARY KEY ([Sin], [Vin]);
GO
ALTER TABLE [customer] ADD CONSTRAINT [UQ_PhoneCustomer] UNIQUE ([Phone]);
GO

-- customer ----------------------------------------------END



-- dealership ----------------------------------------------START

-- Structure for table "dealership"

CREATE TABLE [dealership] (
  [Name] Varchar(45) NOT NULL,
  [Location] Varchar(45) DEFAULT NULL,
  [BranchID] Int DEFAULT NULL,
  [Phone] Varchar(11) DEFAULT NULL,
  CONSTRAINT [CHK_CorrectNumberDealership] CHECK (Phone NOT LIKE '%[^0-9]%')
);
GO

-- Keys for table "dealership"

ALTER TABLE [dealership] ADD CONSTRAINT [PK_DealerName] PRIMARY KEY ([Name]);
GO

-- dealership ----------------------------------------------END



-- warehouse ----------------------------------------------START

-- Structure for table "warehouse"

CREATE TABLE [warehouse] (
  [Name] Varchar(45) NOT NULL,
  [Location] Varchar(45) DEFAULT NULL,
  [BranchID] Int DEFAULT NULL,
  [Phone] Varchar(11) DEFAULT NULL,
  CONSTRAINT [CHK_CorrectNumberWarehouse] CHECK (Phone NOT LIKE '%[^0-9]%')
);
GO

-- Keys for table "warehouse"

ALTER TABLE [warehouse] ADD CONSTRAINT [PK_WarehouseName] PRIMARY KEY ([Name]);
GO

-- warehouse ----------------------------------------------END



-- employee ----------------------------------------------START

-- Structure for table "employee"

CREATE TABLE [employee] (
  [EmpID] Bigint IDENTITY(1,1) NOT NULL,
  [Name] Varchar(45) DEFAULT NULL,
  [Surname] Varchar(45) DEFAULT NULL,
  [SuperiorID] Bigint DEFAULT NULL,
  [BranchID] Int DEFAULT NULL,
  [WorkLocation] Varchar(45) DEFAULT NULL,
  [Position] Varchar(45) DEFAULT NULL,
  [EmployedDate] Datetime DEFAULT GETDATE()
);
GO

-- Keys for table "employee"

ALTER TABLE [employee] ADD CONSTRAINT [PK_Employee] PRIMARY KEY ([EmpID]);
GO

-- employee ----------------------------------------------END



-- position ----------------------------------------------START

-- Structure for table "position"

CREATE TABLE [position] (
  [Name] Varchar(45) NOT NULL
);
GO

-- Keys for table "position"

ALTER TABLE [position] ADD CONSTRAINT [PK_PositionName] PRIMARY KEY ([Name]);
GO

-- position ----------------------------------------------END



-- purchase ----------------------------------------------START


-- Structure for table "purchase"

CREATE TABLE [purchase] (
  [Sin] Bigint IDENTITY(100000000, 1) NOT NULL,
  [Vin] Int NOT NULL,
  [Dealership] Varchar(45) NOT NULL,
  [PurchaseTime] Datetime DEFAULT GETDATE()
);
GO

-- Keys for table "purchase"

ALTER TABLE [purchase] ADD CONSTRAINT [PK_Purchase] PRIMARY KEY ([Sin],[Vin]);
GO

-- purchase ----------------------------------------------END



-- sell ----------------------------------------------START

-- Structure for table "sell"

CREATE TABLE [sell] (
  [Sin] Bigint NOT NULL,
  [Vin] Int NOT NULL,
  [EmpID] Bigint NOT NULL,
  [SellTime] Datetime DEFAULT GETDATE()
);
GO

-- Keys for table "sell"

ALTER TABLE [sell] ADD CONSTRAINT [PK_Sell] PRIMARY KEY ([Sin],[Vin]);
GO

-- sell ----------------------------------------------END



-- users ----------------------------------------------START

-- Structure for table "users"

CREATE TABLE [users] (
  [ID] Bigint IDENTITY(1,1) NOT NULL,
  [Username] varchar(50) NOT NULL,
  [Password] varchar(50) NOT NULL,
  [AccessLevel] Tinyint NOT NULL,
  [Owner] Bigint,
  [AddTime] Datetime DEFAULT GETDATE()
);
GO

-- Keys for table "users"

ALTER TABLE [users] ADD CONSTRAINT [PK_User] PRIMARY KEY ([ID]);
GO
ALTER TABLE [users] ADD CONSTRAINT [UQ_Username] UNIQUE ([Username]);


-- users ----------------------------------------------END



-- magazine_vehicles ----------------------------------------------START

-- Structure for table "warehouse_vehicles"

CREATE TABLE [warehouse_vehicles] (
  [Vin] Int DEFAULT '0' NOT NULL,
  [Brand] Varchar(50) DEFAULT 'CUSTOM',
  [Color] Varchar(30) DEFAULT 'Black',
  [Year] Int DEFAULT YEAR(GETDATE()),
  [Model] Varchar(50) DEFAULT 'CUSTOM',
  [Door] Tinyint DEFAULT '4',
  [Warehouse] Varchar(45) NOT NULL,
  [DeliveryTime] Datetime DEFAULT GETDATE()
);
GO

-- Keys for table "warehouse_vehicles"

ALTER TABLE [warehouse_vehicles] ADD CONSTRAINT [PK_WarehouseVehicleVin] PRIMARY KEY ([Vin]);
GO


-- magazine_vehicles ----------------------------------------------END



-- vehicles ----------------------------------------------START

-- Structure for table "vehicles"

CREATE TABLE [vehicles] (
  [Vin] Int DEFAULT '0' NOT NULL,
  [Brand] Varchar(50) DEFAULT 'CUSTOM',
  [Color] Varchar(30) DEFAULT 'Black',
  [Year] Int DEFAULT YEAR(GETDATE()),
  [Model] Varchar(50) DEFAULT 'CUSTOM',
  [Door] Tinyint DEFAULT '4',
  [Price] Decimal DEFAULT '0',
  [Dealership] Varchar(45) NOT NULL,
  [DeliveryTime] Datetime DEFAULT GETDATE(),
  CONSTRAINT [CHK_CorrectPrice] CHECK (Price >= 0)
);
GO

-- Keys for table "vehicles"

ALTER TABLE [vehicles] ADD CONSTRAINT [PK_VehicleVin] PRIMARY KEY ([Vin]);
GO

-- vehicles ----------------------------------------------END



-- sold_vehicles ----------------------------------------------START

-- Structure for table "sold_vehicles"

CREATE TABLE [sold_vehicles] (
  [Vin] Int NOT NULL,
  [Brand] Varchar(50),
  [Color] Varchar(30),
  [Year] Int DEFAULT NULL,
  [Model] Varchar(50) DEFAULT NULL,
  [Door] Tinyint DEFAULT NULL,
  [Price] Decimal DEFAULT NULL,
  [Dealership] Varchar(45) DEFAULT NULL,
  [SellTime] Datetime DEFAULT GETDATE()
);
GO

-- Keys for table "sold_vehicles"

ALTER TABLE [sold_vehicles] ADD CONSTRAINT [PK_SoldVehicleVin] PRIMARY KEY ([Vin]);
GO

-- sold_vehicles ----------------------------------------------END



-- Brands ----------------------------------------------START

-- Structure for table "brands"

CREATE TABLE [brands] (
  [Name] Varchar(50) NOT NULL
);
GO

-- Keys for table "brands"

ALTER TABLE [brands] ADD CONSTRAINT [PK_BrandName] PRIMARY KEY ([Name]);
GO

-- Brands ----------------------------------------------END


-- Create foreign keys (relationship) section ------------------------------------------------

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
[dealership] ([Name]) ON UPDATE CASCADE ON DELETE CASCADE
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




-- Data insertion section ------------------------------------------------------------------------------

-- Data for table "branch_office"

INSERT INTO branch_office (Location, Phone)
VALUES 
('Kraków',48474836470),
('Warszawa',48474836471),
('£ódŸ',48474836472),
('Poznañ',48474836473)
GO

-- Data for table "dealership"

INSERT INTO dealership 
VALUES 
('Hoden','Warszawa, ul.Buforowa 32', 1001, 48888777666),
('Harrison','Kraków, ul.Marsza³kowska 15', 1000, 48666777888)
GO

-- Data for table "warehouse"

INSERT INTO warehouse
VALUES
('Holan', 'Warszawa, ul.Majteczna 70', 1001, 48555777999),
('Hondersen', 'Kraków, ul.Bia³a 32', 1000, 48555888444)
GO

-- Data for table "brands"

INSERT INTO brands
VALUES 
('TOYOTA'),
('BMW'),
('MERCEDES'),
('SUBARU'),
('CUSTOM')
GO

-- Data for table "warehouse_vehicles"

INSERT INTO warehouse_vehicles (Vin, Brand, Color, Year, Model, Door, Warehouse)
VALUES
(10011, 'BMW', 'White', 2023, 'X2', 4, 'Holan'),
(10012, 'BMW', 'Black', 2023, '128ti', 4, 'Holan')
GO

-- Data for table "vehicles"

INSERT INTO vehicles (Vin, Brand, Color, Year, Model, Door, Price, Dealership)
VALUES 
(10009, 'TOYOTA', 'Black', 2016, 'Camry CE', 4, 23570, 'Hoden'),
(10010, 'TOYOTA', 'Black', 2015, 'Camry CE', 4, 20150, 'Hoden')
GO

-- Data for table sold_vehicles

INSERT INTO sold_vehicles (Vin, Brand, Color, Year, Model, Door, Price, Dealership)
VALUES
(10001, 'TOYOTA', 'White', 2016, 'Camry CE\n', 4, 25690, 'Hoden'),
(10002, 'TOYOTA', 'Black', 2015, 'Camry CE', 4, 22680, 'Hoden'),
(10003, 'TOYOTA', 'Red', 2015, 'Camry CE', 4, 22680, 'Hoden'),
(10004, 'TOYOTA', 'White', 2016, 'Camry CE', 4, 27680, 'Harrison'),
(10005, 'TOYOTA', 'Red', 2016, 'Camry CE', 2, 27680, 'Harrison'),
(10006, 'TOYOTA', 'White', 2014, 'Camry LE\n', 2, 22680, 'Harrison'),
(10007, 'TOYOTA', 'White', 2014, 'Camry LE', 4, 22680, 'Harrison'),
(10008, 'TOYOTA', 'White', 2017, 'Camry LE', 4, 26780, 'Harrison')
GO

-- Data for table "position"

INSERT INTO position
VALUES
('Dealer'),
('Transport'),
('Manager'),
('Branch Chief')
GO

-- Data for table "employee"

INSERT INTO employee (Name, Surname, SuperiorID, BranchID, WorkLocation, Position)
VALUES 
('Jack', 'Ali', 2, 1000, 'Harrison', 'Dealer'),
('Jason', 'Comfort', 3, 1000,'Harrison', 'Manager'),
('Mary', 'Aral', NULL, 1000, NULL, 'Branch Chief'),
('Kevin', 'Berlak', 6, 1001, 'Hoden', 'Dealer'),
('Paul', 'Delima', 6, 1001, 'Hoden', 'Dealer'),
('Linda', 'Farah', 7, 1001,'Hoden', 'Manager'),
('Devid', 'Geist',NULL, 1001,'Hoden', 'Branch Chief'),
('Daniel', 'Farbane', 7, 1001, NULL, 'Transport')
GO

-- Data for table "purchase"

INSERT INTO purchase (Vin, Dealership)
VALUES 
(10001, 'Hoden'),
(10002, 'Hoden'),
(10003, 'Hoden'),
(10004, 'Harrison'),
(10005, 'Harrison'),
(10006, 'Harrison'),
(10007, 'Harrison'),
(10008, 'Harrison')
GO

-- Data for table "sell"

INSERT INTO sell (Sin, Vin, EmpID)
VALUES 
(100000000, 10001, 4),
(100000001, 10002, 4),
(100000002, 10003, 5),
(100000003, 10004, 1),
(100000004, 10005, 1),
(100000005, 10006, 1),
(100000006, 10007, 1),
(100000007, 10008, 1)
GO

-- Data for table "users"

INSERT INTO users (Username, Password, AccessLevel, Owner)
VALUES 
--Superadmin
('admin','admin', 4, 3), 
('admino', 'badmino', 4, 7), 
-- Admin
('Hala','Bala', 3, 2), 
('Mukake', 'Busake', 3, 6),
-- Worker
('Work','Work', 2, 1), 
('Maci', 'Paci', 2, 4),
('Kubuœ', 'Puchatek', 2, 5),
-- Other
('Trans','Trans', 1, 8) 
GO

-- Data for table "customer"

INSERT INTO customer (Sin, Vin, Name, Surname, Phone)
VALUES 
(100000000, 10001, 'Joyce', 'Kwan', 48474836473),
(100000001, 10002, 'Samuel', 'Chan', 48474836472),
(100000002, 10003, 'Kelly', 'Luu', 48474836471),
(100000003, 10004, 'Andrew', 'Lata', 48474836470),
(100000004, 10005, 'Lori', 'Fichtel', 48474836474),
(100000005, 10006, 'Emily', 'German', 48474836475),
(100000006, 10007, 'Tony', 'Hoey', 48474836476),
(100000007, 10008, 'Allen', 'Zhang', 48474836477)
GO



