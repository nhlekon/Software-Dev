CREATE DATABASE CarTradingDB;
GO

USE CarTradingDB;
GO

-- Users Table (Stores administrators and managers)
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) CHECK (Role IN ('Admin', 'Manager')) NOT NULL,
    Department VARCHAR(50) NULL -- Only applies to Admin role
);

-- Cars Table (Stores car listings)
CREATE TABLE Cars (
    CarID INT IDENTITY(1,1) PRIMARY KEY,
    Make VARCHAR(50) NOT NULL,
    Model VARCHAR(50) NOT NULL,
    Year INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    Status VARCHAR(20) CHECK (Status IN ('Available', 'Sold')) NOT NULL,
    AddedBy INT FOREIGN KEY REFERENCES Users(UserID) ON DELETE SET NULL
);

-- Transactions Table (Stores sales/purchases)
CREATE TABLE Transactions (
    TransactionID INT IDENTITY(1,1) PRIMARY KEY,
    CarID INT FOREIGN KEY REFERENCES Cars(CarID) ON DELETE CASCADE,
    BuyerName VARCHAR(100) NOT NULL,
    SalePrice DECIMAL(10,2) NOT NULL,
    TransactionDate DATETIME DEFAULT GETDATE(),
    ProcessedBy INT FOREIGN KEY REFERENCES Users(UserID) ON DELETE SET NULL
);

-- Indexes for faster lookup
CREATE INDEX IDX_Cars_Status ON Cars(Status);
CREATE INDEX IDX_Transactions_Date ON Transactions(TransactionDate);
