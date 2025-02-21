CREATE DATABASE CalculatorDB;
GO

USE CalculatorDB;
GO

CREATE TABLE CalculationHistory (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Type VARCHAR(50) NOT NULL, -- Standard, Interest, Currency
    Expression VARCHAR(255) NOT NULL,
    Result VARCHAR(255) NOT NULL,
    Timestamp DATETIME DEFAULT GETDATE()
);

-- Index for faster queries
CREATE INDEX IDX_CalculationHistory_Type ON CalculationHistory(Type);
