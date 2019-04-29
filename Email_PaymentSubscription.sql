


declare		@Table1				nvarchar(50)
		,	@Sql_Table1			nvarchar(Max)


declare		@TableHead		nvarchar(max)
		,	@TableBody		nvarchar(max)
		,	@TableTail		nvarchar(max)
		,	@Body			nvarchar(max)
		,	@Body2			nvarchar(max)
		,	@Sub			nvarchar(max)


SET @TableHead = '<html><head>' + '<style>'
    + '<style type="text/css">
		  body {
		   padding-top: 0 !important;
		   padding-bottom: 0 !important;
		   padding-top: 0 !important;
		   padding-bottom: 0 !important;
		   margin:0 !important;
		   width: 100% !important;
		   -webkit-text-size-adjust: 100% !important;
		   -ms-text-size-adjust: 100% !important;
		   -webkit-font-smoothing: antialiased !important;
		 }
		 .tableContent img {
		   border: 0 !important;
		   display: block !important;
		   outline: none !important;
		 }
		 a{
		  color:#382F2E;
		 }

		p, h1{
		  color:#382F2E;
		  margin:0;
		}
		 p{
			  text-align:left;
			  color:#999999;
			  font-size:15px;
			  font-weight:bolder;
			  line-height:19px;
			}

		

		h2{
		  text-align:left;
		   color:#222222; 
		   font-size:19px;
		  font-weight:normal;
		}
		div,p,ul,h1{
		  margin:0;
		}

		.bgBody{
		  background: #ffffff;
		}
		.bgItem{
		  background: #ffffff;
		}

     </style> </head> <body>
     <table width="100%" border="0" cellspacing="0" 
	 cellpadding="0" class="tableContent bgBody" align="center"  style=font-family:Helvetica, Arial,serif;>
     <tr><td height=35></td></tr>
      <tr>
        <td>
          <table width=600 border=0 cellspacing=0 cellpadding=0 align=center class=bgItem>
            <tr>
              <td width=40></td>
               <td width=520>
					<table width=520 border=0 cellspacing=0 cellpadding=0 align=cente">
						<tr>
							<td height=75></td>
						</tr>
						<tr>
							<td class=movableContentContainer valign=top>
								<div lass=movableContent>
									<table width=520 border=0 cellspacing=0 cellpadding=0 align=center>
										<tr>
											<td valign=top align=center>
												<div class=contentEditableContainer contentTextEditable>
													<div class=contentEditable>														
														<p style=text-align:center;margin:0;font-family:Georgia,Time,sans-serif;font-size:26px;color:#222222;>
														<span style=color:#cc0000;>MASTER ONLINE</span></p>
													</div>
												</div>
											</td>
										</tr>										
									</table>
								</div>							
					'
									
SET @TableTail = '
						
                      </div>
                    </td>
                  </tr>

                </table>
              </td>
              <td width=40></td>
            </tr>
          </table>
        </td>
      </tr>

      <tr><td height=88></td></tr>

    </table>
</table></body></html>' 



set @Table1='#Subscription'
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'#Subscription') AND type in (N'U'))
set @Sql_Table1='DROP TABLE #Subscription'
print @Sql_Table1
exec (@Sql_Table1)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'#Subscription') AND type in (N'U'))
BEGIN
	CREATE TABLE #Subscription
	(		
			id						int identity primary key 
		,	[Account]				NVARCHAR(50)
		,	[Email]					NVARCHAR(50)
		,	[TipeSubs]				NVARCHAR(2)	
		,	[Subscription]			NVARCHAR(50)
		,   [TanggalBayar]			date		
		,	[Nilai]					float
		,   [TipePembayaran]		NVARCHAR(100)
		,	[DrTGL]					date
		,	[SdTGL]					date	
		,	[jumlahUser]			int
		,	[tgl_email]				datetime
	)

END



declare @sql	nvarchar(Max)
set @sql='	
INSERT INTO #Subscription
SELECT	
		[Account]
      ,	[Email]
      ,	[TipeSubs]
	  ,	[Subscription]= case
		when [TipeSubs]=''01'' then
			''BASIC''
		when [TipeSubs]=''02'' then
			''SILVER''
		when [TipeSubs]=''03'' then
			''GOLD''
		end
      ,	[TanggalBayar]
      ,	[Nilai]=isnull([Nilai],0)
      ,	[TipePembayaran]
      ,	[DrTGL]=isnull([DrTGL],''1900-01-01'')
      ,	[SdTGL]=isnull([SdTGL],''1900-01-01'')
      ,	[jumlahUser]=isnull([jumlahUser],0)
	  ,	[tgl_email]
  FROM [MO].[dbo].[AktivitasSubscription]
  where [tgl_email] is null
'
print(@sql)
exec(@sql)


declare 
		@Account				NVARCHAR(50)
	,	@Email					NVARCHAR(50)
	,	@TipeSubs				NVARCHAR(2)	
	,	@Subscription			NVARCHAR(50)	
	,   @TanggalBayar			date
	,   @TipePembayaran			NVARCHAR(100)
	,	@Nilai					float
	,	@DrTGL					date
	,	@SdTGL					date		
	,	@jumlahUser				int
	,	@tgl_email				datetime
	,   @isemail1				nvarchar(1)	
	,	@cmdWhere				nvarchar(1000)
	,	@i						int
	,	@count					int

set @count=0
SELECT @count = COUNT(*) FROM #Subscription
print @count

SET @i = 1
WHILE @i <= 1 --@count
BEGIN
	set @isemail1=''
	select 
				@Account=[Account]
			,	@Email=[Email]
			,	@TipeSubs=[TipeSubs]
			,	@Subscription=[Subscription]
			,	@TanggalBayar=[TanggalBayar]
			,	@TipePembayaran=[TipePembayaran]
			,	@Nilai=[Nilai]
			,	@DrTGL=[DrTGL]
			,	@SdTGL=[SdTGL]
			,	@jumlahUser=[jumlahUser]
	from #Subscription
	WHERE id = @i
	
	
	set @Sub='Email Payment Subscription'
	set @TableBody='	
			<tr>
				<td align=left>
					<div class=contentEditableContainer contentTextEditable>
						<div class=contentEditable align=center>
							<p  style=text-align:left;color:#382F2E;font-size:14px;font-weight:normal;line-height:19px;>
								Jakarta, '+ (CONVERT(VARCHAR(20), getdate(), 103)) +'
								<br />
								Dear '+ @Account +' ,
								<br />
								<br />
								<br />
									Kami telah menerima pembayaran Subscription <span style=color:#cc0000;> Master Online </span>
									anda. 
								<br />
									Jika ada pertanyaan hubungi customer service kami di 021-6349318 pada jam kerja.
								<br />
								<br />																 
										Tanggal&nbsp;&nbsp;		'+ (CONVERT(VARCHAR(20), @TanggalBayar, 103)) +'
								<br />																 
										Tagihan&nbsp;&nbsp;		'+ @Account +'('+ @Email +')

								<br />
										Plan MO&nbsp;&nbsp; 	'+ @Subscription +'
								<br />
										Total&nbsp;&nbsp;		Rp. '+ (CONVERT(VARCHAR, @Nilai))  +'
								<br />
										User&nbsp;&nbsp;		'+ (CONVERT(VARCHAR, @jumlahUser))  +'
								<br />
										Periode&nbsp;&nbsp; '+ (CONVERT(VARCHAR(20), @DrTGL, 103)) +' s/d '+ (CONVERT(VARCHAR(20), @SdTGL, 103)) +'																							
								<br />
								<br />
								<br />
								<br />
									Terima Kasih
								<br />
								<br />
								<br />
									Admin Master Online
								<br />
									021-6349318 
								</p>
						</div>
					</div>
				</td>
			</tr>'
						

		set  @Body = @TableHead +  @TableBody +@TableTail
				
		
		print(len(@Body))
		--print('=============================')
		--print (@TableHead)
		--print (@TableBody)
		--print (@TableTail)
		print('=============================')
		print(@Body)

		set @Email='ir.dharmawan@gmail.com'
		if len(ltrim(rtrim(@Body)))>0 
		begin
			-------------SENT DB EMAIL---------------
			exec msdb.dbo.sp_send_dbmail
			  @profile_name='AutoEmail_CS',
			  @recipients=@Email,
			  @subject=@Sub,
			  @body=@Body ,
			  @body_format = 'HTML'  	
			-----------------------------------------


			---------------UPDATE EMAIL DI AKTIVITAS SUBSCRIPTION--------------
			print('update Email')
			--update [MO].[dbo].[AktivitasSubscription]
			--	set [tgl_email]=getdate()
			--where [Account]=@Account
			--and	[Email]=@Email
			--and [tgl_email] is null


		end

		SET @i = @i + 1
end

--SELECT * FROM #Subscription
DROP TABLE #Subscription