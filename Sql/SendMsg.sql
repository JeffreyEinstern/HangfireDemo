
GO

/****** Object:  Table [dbo].[SendMsgHistory]    Script Date: 2021/12/23 20:04:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SendMsgHistory](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BILL_TEMPLATE_NAME] [nvarchar](200) NULL,
	[BILL_NO] [nvarchar](50) NULL,
	[BILL_SUBMIT_USER_NAME] [nvarchar](50) NULL,
	[ReceivedUserId] [nvarchar](200) NULL,
	[WxUser] [nvarchar](50) NULL,
	[CreateTime] [datetime] NULL,
 CONSTRAINT [PK_SendMsgHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


