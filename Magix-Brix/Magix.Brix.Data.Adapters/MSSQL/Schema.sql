/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

/****** Object:  Table [dbo].[ViewStateStorage]    Script Date: 07/18/2009 21:46:22 ******/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ViewStateStorage]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ViewStateStorage](
	[ID] [nvarchar](4000) NOT NULL,
	[Content] [text] NOT NULL,
	[Created] [datetime] NOT NULL
)
END;
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ViewStateStorage]') AND name = N'IDX_Key')
CREATE NONCLUSTERED INDEX [IDX_Key] ON [dbo].[ViewStateStorage] 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ViewStateStorage]') AND name = N'IDX_CreatedKey')
CREATE NONCLUSTERED INDEX [IDX_CreatedKey] ON [dbo].[ViewStateStorage] 
(
	[Created] ASC
)WITH (IGNORE_DUP_KEY = OFF);



/****** Object:  Table [dbo].[Documents]    Script Date: 07/18/2009 21:46:22 ******/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Documents](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TypeName] [nvarchar](255),
	[Created] [datetime] NULL,
	[Modified] [datetime] NULL,
	[Parent] [int] NULL,
	[ParentPropertyName] [nvarchar](255) NULL
 CONSTRAINT [PK_Documents] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
END;
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND name = N'IDX_TypeName')
CREATE NONCLUSTERED INDEX [IDX_TypeName] ON [dbo].[Documents] 
(
	[TypeName] ASC
)WITH (IGNORE_DUP_KEY = OFF);


/****** Object:  Table [dbo].[Documents2Documents]    Script Date: 07/18/2009 21:46:22 ******/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Documents2Documents]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Documents2Documents](
	[Document1ID] [int] NOT NULL,
	[Document2ID] [int] NOT NULL,
	[PropertyName] [nvarchar](255) NOT NULL
 CONSTRAINT [PK_Documents2Documents] PRIMARY KEY CLUSTERED 
(
	[Document1ID] ASC,
	[Document2ID] ASC,
	[PropertyName] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
END;


/****** Object:  Table [dbo].[PropertyDates]    Script Date: 07/18/2009 21:46:22 ******/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PropertyDates]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PropertyDates](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255),
	[Value] [datetime] NULL,
	[FK_Document] [int] NULL,
 CONSTRAINT [PK_PropertyDates] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
END;


/****** Object:  Table [dbo].[PropertyDecimals]    Script Date: 07/18/2009 21:46:22 ******/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PropertyDecimals]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PropertyDecimals](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255),
	[Value] [decimal](16, 8) NULL,
	[FK_Document] [int] NULL,
 CONSTRAINT [PK_PropertyDecimals] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
END;


/****** Object:  Table [dbo].[PropertyBools]    Script Date: 07/18/2009 21:46:22 ******/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PropertyBools]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PropertyBools](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255),
	[Value] [bit] NULL,
	[FK_Document] [int] NULL,
 CONSTRAINT [PK_PropertyBools] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
END;


/****** Object:  Table [dbo].[PropertyBLOBS]    Script Date: 07/18/2009 21:46:22 ******/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PropertyBLOBS]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PropertyBLOBS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255),
	[Value] [varbinary](max) NULL,
	[FK_Document] [int] NULL,
 CONSTRAINT [PK_PropertyBLOBS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
END;


/****** Object:  Table [dbo].[PropertyStrings]    Script Date: 07/18/2009 21:46:22 ******/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PropertyStrings]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PropertyStrings](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255),
	[Value] [nvarchar](MAX),
	[FK_Document] [int] NULL,
 CONSTRAINT [PK_PropertyStrings] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
END;


/****** Object:  Table [dbo].[PropertyLongStrings]    Script Date: 07/18/2009 21:46:22 ******/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PropertyLongStrings]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PropertyLongStrings](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255),
	[Value] [text],
	[FK_Document] [int] NULL,
 CONSTRAINT [PK_PropertyLongStrings] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
END;


/****** Object:  Table [dbo].[PropertyInts]    Script Date: 07/18/2009 21:46:22 ******/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PropertyInts]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PropertyInts](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255),
	[Value] [int] NULL,
	[FK_Document] [int] NULL,
 CONSTRAINT [PK_PropertyInts] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
END;


/****** Object:  Table [dbo].[PropertyGuids] */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PropertyGuids]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PropertyGuids](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255),
	[Value] [uniqueidentifier] NULL,
	[FK_Document] [int] NULL,
 CONSTRAINT [PK_PropertyGuids] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
END;


/****** Object:  ForeignKey Script Date: 07/18/2009 21:46:22 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Document_Document]') AND parent_object_id = OBJECT_ID(N'[dbo].[Documents]'))
ALTER TABLE [dbo].[Documents]  WITH CHECK ADD  CONSTRAINT [FK_Document_Document] FOREIGN KEY([Parent])
REFERENCES [dbo].[Documents] ([ID]);


/****** Object:  ForeignKey Script Date: 07/18/2009 21:46:22 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Documents2Documents_Document]') AND parent_object_id = OBJECT_ID(N'[dbo].[Documents2Documents]'))
ALTER TABLE [dbo].[Documents2Documents]  WITH CHECK ADD  CONSTRAINT [FK_Documents2Documents_Document] FOREIGN KEY([Document1ID])
REFERENCES [dbo].[Documents] ([ID]);


/****** Object:  ForeignKey Script Date: 07/18/2009 21:46:22 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Documents2Documents_Document2]') AND parent_object_id = OBJECT_ID(N'[dbo].[Documents2Documents]'))
ALTER TABLE [dbo].[Documents2Documents]  WITH CHECK ADD  CONSTRAINT [FK_Documents2Documents_Document2] FOREIGN KEY([Document2ID])
REFERENCES [dbo].[Documents] ([ID])
ON DELETE CASCADE;


/****** Object:  ForeignKey Script Date: 07/18/2009 21:46:22 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Documents_PropertyBLOBS]') AND parent_object_id = OBJECT_ID(N'[dbo].[PropertyBLOBS]'))
ALTER TABLE [dbo].[PropertyBLOBS]  WITH CHECK ADD  CONSTRAINT [FK_Documents_PropertyBLOBS] FOREIGN KEY([FK_Document])
REFERENCES [dbo].[Documents] ([ID])
ON DELETE CASCADE;


/****** Object:  ForeignKey Script Date: 07/18/2009 21:46:22 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Documents_PropertyBools]') AND parent_object_id = OBJECT_ID(N'[dbo].[PropertyBools]'))
ALTER TABLE [dbo].[PropertyBools]  WITH CHECK ADD  CONSTRAINT [FK_Documents_PropertyBools] FOREIGN KEY([FK_Document])
REFERENCES [dbo].[Documents] ([ID])
ON DELETE CASCADE;


/****** Object:  ForeignKey [FKBD7C7EF3F615DD14]    Script Date: 07/18/2009 21:46:22 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Documents_PropertyDates]') AND parent_object_id = OBJECT_ID(N'[dbo].[PropertyDates]'))
ALTER TABLE [dbo].[PropertyDates]  WITH CHECK ADD  CONSTRAINT [FK_Documents_PropertyDates] FOREIGN KEY([FK_Document])
REFERENCES [dbo].[Documents] ([ID])
ON DELETE CASCADE;


/****** Object:  ForeignKey [FKBD7C7EF3F615DD14]    Script Date: 07/18/2009 21:46:22 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Documents_PropertyGuids]') AND parent_object_id = OBJECT_ID(N'[dbo].[PropertyGuids]'))
ALTER TABLE [dbo].[PropertyGuids]  WITH CHECK ADD  CONSTRAINT [FK_Documents_PropertyGuids] FOREIGN KEY([FK_Document])
REFERENCES [dbo].[Documents] ([ID])
ON DELETE CASCADE;


/****** Object:  ForeignKey [FK37E88A2CF615DD14]    Script Date: 07/18/2009 21:46:22 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Documents_PropertyDecimals]') AND parent_object_id = OBJECT_ID(N'[dbo].[PropertyDecimals]'))
ALTER TABLE [dbo].[PropertyDecimals]  WITH CHECK ADD  CONSTRAINT [FK_Documents_PropertyDecimals] FOREIGN KEY([FK_Document])
REFERENCES [dbo].[Documents] ([ID])
ON DELETE CASCADE;


/****** Object:  ForeignKey [FKA2A0409BF615DD14]    Script Date: 07/18/2009 21:46:22 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Documents_PropertyInts]') AND parent_object_id = OBJECT_ID(N'[dbo].[PropertyInts]'))
ALTER TABLE [dbo].[PropertyInts]  WITH CHECK ADD  CONSTRAINT [FK_Documents_PropertyInts] FOREIGN KEY([FK_Document])
REFERENCES [dbo].[Documents] ([ID])
ON DELETE CASCADE;


/****** Object:  ForeignKey [FKDCD8C370F615DD14]    Script Date: 07/18/2009 21:46:22 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Documents_PropertyLongStrings]') AND parent_object_id = OBJECT_ID(N'[dbo].[PropertyLongStrings]'))
ALTER TABLE [dbo].[PropertyLongStrings]  WITH CHECK ADD  CONSTRAINT [FK_Documents_PropertyLongStrings] FOREIGN KEY([FK_Document])
REFERENCES [dbo].[Documents] ([ID])
ON DELETE CASCADE;


/****** Object:  ForeignKey [FK3125AE06F615DD14]    Script Date: 07/18/2009 21:46:22 ******/
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Documents_PropertyStrings]') AND parent_object_id = OBJECT_ID(N'[dbo].[PropertyStrings]'))
ALTER TABLE [dbo].[PropertyStrings]  WITH CHECK ADD  CONSTRAINT [FK_Documents_PropertyStrings] FOREIGN KEY([FK_Document])
REFERENCES [dbo].[Documents] ([ID])
ON DELETE CASCADE;
