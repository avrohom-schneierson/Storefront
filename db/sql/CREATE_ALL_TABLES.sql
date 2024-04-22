USE [StoreDB]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- drop tables if they already exist:
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'PURCHASE_STORE_ITEM'))
	DROP TABLE [dbo].[PURCHASE_STORE_ITEM]
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'PURCHASE'))
	DROP TABLE [dbo].[PURCHASE]
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'STORE_ITEM'))
	DROP TABLE [dbo].[STORE_ITEM]
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'CUSTOMER'))
	DROP TABLE [dbo].[CUSTOMER]

-- create CUSTOMER table:
CREATE TABLE [dbo].[CUSTOMER](
	[CustomerId] [int] IDENTITY(1,1) NOT NULL,
	[LoginId] [nvarchar](50) NOT NULL,
	[Password] [nvarchar](50) NOT NULL,
	[Balance] [money] NOT NULL,
	CONSTRAINT Unique_LoginId UNIQUE(LoginId),
PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

-- create STORE_ITEM table:
CREATE TABLE [dbo].[STORE_ITEM](
	[StoreItemId] [int] IDENTITY(1,1) NOT NULL,
	[Manufacturer] [nvarchar](50) NOT NULL,
	[ProductName] [nvarchar](100) NOT NULL,
	[QuantityAvailable] [int] NOT NULL,
	[Price] [money] NOT NULL,
	[ImageUrl] [nvarchar](MAX),
	CONSTRAINT Unique_StoreItem UNIQUE(Manufacturer, ProductName),
 CONSTRAINT [PK_STORE_ITEM] PRIMARY KEY CLUSTERED 
(
	[StoreItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

-- create PURCHASE table:
CREATE TABLE [dbo].[PURCHASE](
	[PurchaseId] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[StoreItemId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [money] NOT NULL,
	[PurchaseDateTime] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PurchaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PURCHASE]  WITH CHECK ADD  CONSTRAINT [FK_PURCHASE_CUSTOMER] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[CUSTOMER] ([CustomerId])
GO

ALTER TABLE [dbo].[PURCHASE] CHECK CONSTRAINT [FK_PURCHASE_CUSTOMER]
GO

ALTER TABLE [dbo].[PURCHASE]  WITH CHECK ADD  CONSTRAINT [FK_PURCHASE_STORE_ITEM] FOREIGN KEY([StoreItemId])
REFERENCES [dbo].[STORE_ITEM] ([StoreItemId])
GO

ALTER TABLE [dbo].[PURCHASE] CHECK CONSTRAINT [FK_PURCHASE_STORE_ITEM]
GO


-- create PURCHASE_STORE_ITEM table
CREATE TABLE [dbo].[PURCHASE_STORE_ITEM](
	[PurchaseId] [int] NOT NULL,
	[StoreItemId] [int] NOT NULL,
 CONSTRAINT [PK_PURCHASE_STORE_ITEM] PRIMARY KEY CLUSTERED 
(
	[PurchaseId] ASC,
	[StoreItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PURCHASE_STORE_ITEM]  WITH CHECK ADD  CONSTRAINT [FK_PURCHASE_TO_PURCHASE_STORE_ITEM] FOREIGN KEY([PurchaseId])
REFERENCES [dbo].[PURCHASE] ([PurchaseId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[PURCHASE_STORE_ITEM] CHECK CONSTRAINT [FK_PURCHASE_TO_PURCHASE_STORE_ITEM]
GO

ALTER TABLE [dbo].[PURCHASE_STORE_ITEM]  WITH CHECK ADD  CONSTRAINT [FK_STORE_ITEM_TO_PURCHASE_STORE_ITEM] FOREIGN KEY([StoreItemId])
REFERENCES [dbo].[STORE_ITEM] ([StoreItemId])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[PURCHASE_STORE_ITEM] CHECK CONSTRAINT [FK_STORE_ITEM_TO_PURCHASE_STORE_ITEM]
GO

--------------------