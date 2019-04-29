use MO
go

Alter procedure [dbo].[Email_subscription]

as
SET NOCOUNT ON


declare		@Table1				nvarchar(50)
		,	@Sql_Table1			nvarchar(Max)


declare		@TableHead		nvarchar(max)
		,	@TableBody		nvarchar(max)
		,	@TableTail		nvarchar(max)
		,	@Body			nvarchar(max)
		,	@Body2			nvarchar(max)
		,	@Sub			nvarchar(250)

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
										<tr>
											<td align=left>
												<div class=contentEditableContainer contentTextEditable>
													<div class=contentEditable align=center>
														<h2 >Greetings !</h2>
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





set @Table1='#Account'
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'#Account') AND type in (N'U'))
set @Sql_Table1='DROP TABLE #Account'
print @Sql_Table1
exec (@Sql_Table1)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'#Account') AND type in (N'U'))
BEGIN

	CREATE TABLE #Account
	(		
			id						int identity primary key 
		,	[AccountId]				NVARCHAR(50)
		,	[UserId]				NVARCHAR(50)
		,	[Email]					NVARCHAR(50)
		,	[Username]				NVARCHAR(50)
		,	[Password]				NVARCHAR(max)
		,	[NoHp]					NVARCHAR(30)
		,	[DatabasePathMo]		NVARCHAR(255)	
		,   [DatabasePathErasoft]	NVARCHAR(255)
		,   [Status]				bit
		,   [PhotoKtpUrl]			NVARCHAR(255)
		,   [NamaTokoOnline]		NVARCHAR(255)
		,   [KODE_SUBSCRIPTION]		NVARCHAR(100)
		,   [TGL_SUBSCRIPTION]		date
		,	[email_Tempo1]			date
		,	[email_Tempo2]			date
		,   [VCode]					NVARCHAR(max)
		,   [PhotoKtpBase64]		NVARCHAR(max)
		,	[TOKEN_CC]				NVARCHAR(100)
		,	[KODE_REFERRAL]			NVARCHAR(100)
		,	[TGL_DAFTAR]			datetime
		,	[jumlahUser]			NVARCHAR(50)
		,	[tgl_approve]			datetime
		,	[tgl_email1]			datetime
		,	[tgl_email2]			datetime
	)

END
declare @sql	nvarchar(Max)
set @sql='	
INSERT INTO #Account
SELECT	
		[AccountId]
    ,	[UserId]
    ,	[Email]
    ,	[Username]
    ,	[Password]
    ,	[NoHp]
    ,	[DatabasePathMo]
    ,	[DatabasePathErasoft]
    ,	[Status]
    ,	[PhotoKtpUrl]
    ,	[NamaTokoOnline]
    ,	[KODE_SUBSCRIPTION]
    ,	[TGL_SUBSCRIPTION]
	,	[email_Tempo1]=case 
			when [KODE_SUBSCRIPTION]=''01'' then
				dateadd(week,-1,[TGL_SUBSCRIPTION])
			else
				dateadd(month,-1,[TGL_SUBSCRIPTION])
			end
	,	[email_Tempo2]=case 
			when [KODE_SUBSCRIPTION]=''01'' then
				dateadd(dd,-3,[TGL_SUBSCRIPTION])
			else
				dateadd(week,-1,[TGL_SUBSCRIPTION])
			end
	
    ,	[VCode]
    ,	[PhotoKtpBase64]
    ,	[TOKEN_CC]
    ,	[KODE_REFERRAL]
    ,	[TGL_DAFTAR]
    ,	[jumlahUser]
    ,	[tgl_approve]
    ,	[tgl_email1]
    ,	[tgl_email2]
FROM [MO].[dbo].[Account]
where [TGL_SUBSCRIPTION]>=dateadd(month,-1,getdate())
and ([tgl_email1] is null or [tgl_email2] is null)'

print(@sql)
exec(@sql)


declare 
		@AccountId				NVARCHAR(50)
	,	@UserId					NVARCHAR(50)
	,	@Email					NVARCHAR(50)
	,	@Username				NVARCHAR(50)	
	,   @Status					bit
	,   @KODE_SUBSCRIPTION		NVARCHAR(100)
	,   @TGL_SUBSCRIPTION		date
	,	@email_Tempo1			date
	,	@email_Tempo2			date
	,   @tgl_email1				date
	,   @tgl_email2				date	
	,   @isemail1				nvarchar(1)
	,   @sendemail				nvarchar(1)
	,	@cmdWhere				nvarchar(1000)
	,	@i						int
	,	@count					int


set @count=0
SELECT @count = COUNT(*) FROM #Account
print @count

SET @i = 1
WHILE @i <= @count
BEGIN
	set @isemail1=''
	set @sendemail=''
	select 
				@AccountId=[AccountId]
			,	@UserId=[UserId]
			,	@Email=[Email]
			,	@Username=[Username]
			,	@Status=[Status]
			,	@KODE_SUBSCRIPTION=[KODE_SUBSCRIPTION]
			,	@TGL_SUBSCRIPTION=[TGL_SUBSCRIPTION]
			,	@email_Tempo1=[email_Tempo1]
			,	@email_Tempo2=[email_Tempo2]
			,	@tgl_email1=[tgl_email1]
			,	@tgl_email2=[tgl_email2]
	from #Account
	WHERE id = @i

	--print(@KODE_SUBSCRIPTION)
	--print(@tgl_email1)
	if @KODE_SUBSCRIPTION='01' 
	begin
		
		if @tgl_email1 is null
		begin
			--print('tgl 1 is null')
			--print(@TGL_SUBSCRIPTION)
			--print (dateadd(week,-1,@TGL_SUBSCRIPTION))
			if dateadd(week,-1,@TGL_SUBSCRIPTION)<=getdate()
			begin				
				set @Sub='Email subscribe ke-1'	
				set @sendemail='Y'	
				set @isemail1='Y'	
				set @TableBody='	
						<tr>
							<td align=left>
								<div class=contentEditableContainer contentTextEditable>
									<div class=contentEditable align=center>
										<p  style=text-align:left;color:#382F2E;font-size:14px;font-weight:normal;line-height:19px;>
											Jakarta, '+ (CONVERT(VARCHAR(20), getdate(), 103)) +'
											<br />
											Dear ' + @Username +',
											<br />
											<br />
												Terima kasih telah bergabung di <span style=color:#cc0000;> Master Online </span>, 
											<br />
												akun free trial anda akan expired pada tgl '+ (CONVERT(VARCHAR(20), @TGL_SUBSCRIPTION, 103)) +' , silahkan melakukan subscribe untuk tetap dapat
												menggunakan master online , 
											<br />
												atau hubungi customer service kami di 021-6349318 pada jam kerja. 
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
				--set  @Body2 =@Body+ @TableTail
			end			
		end
		else
		begin
			if @tgl_email2 is null
			begin
				--if @TGL_SUBSCRIPTION<=dateadd(dd,-3,@TGL_SUBSCRIPTION)
				if dateadd(dd,-3,@TGL_SUBSCRIPTION)<=getdate()
				begin
					set @Sub='Email subscribe ke-2'
					set @sendemail='Y'
					set @isemail1='N'
					set @TableBody='	
							<tr>
								<td align=left>
									<div class=contentEditableContainer contentTextEditable>
										<div class=contentEditable align=center>
											<p  style=text-align:left;color:#382F2E;font-size:14px;font-weight:normal;line-height:19px;>
												Jakarta, '+ (CONVERT(VARCHAR(20), getdate(), 103)) +'
												<br />
												Dear ' + @Username +',
												<br />
												<br />
													Terima kasih telah bergabung di <span style=color:#cc0000;> Master Online </span>, 
												<br />
													akun free trial anda akan expired pada tgl '+ (CONVERT(VARCHAR(20), @TGL_SUBSCRIPTION, 103)) +' , silahkan melakukan subscribe untuk tetap dapat
													menggunakan master online , 
												<br />
													atau hubungi customer service kami di 021-6349318 pada jam kerja. 
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
					--set  @Body2 =@Body+ @TableTail
				end		
			end
		end		
	end

	if @KODE_SUBSCRIPTION<>'01' 
	begin
		if @tgl_email1 is null
		begin
			if @TGL_SUBSCRIPTION<=dateadd(month,-1,@TGL_SUBSCRIPTION)
			begin
				set @Sub='Email subscribe ke-1'
				set @sendemail='Y'
				set @isemail1='Y'
				set @TableBody='	
						<tr>
							<td align=left>
								<div class=contentEditableContainer contentTextEditable>
									<div class=contentEditable align=center>
										<p  style=text-align:left;color:#382F2E;font-size:14px;font-weight:normal;line-height:19px;>
											Jakarta, '+ (CONVERT(VARCHAR(20), getdate(), 103)) +'
											<br />
											Dear ' + @Username +',
											<br />
											<br />
												Terima kasih telah menggunakan<span style=color:#cc0000;> Master Online </span>, 
											<br />
												akun anda akan expired pada tgl '+ (CONVERT(VARCHAR(20), @TGL_SUBSCRIPTION, 103)) +' , silahkan melakukan pembayaran agar akun anda tetap aktif ,
											<br />
												di menu pengaturan  , subscription .  
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
				set  @Body = @TableHead +  @TableBody + @TableTail
				--set  @Body2 =@Body+ @TableTail
				
			end			
		end
		else
		begin
			if @tgl_email2 is null
			begin
				--if @TGL_SUBSCRIPTION<=dateadd(wk,-1,@TGL_SUBSCRIPTION)
				if dateadd(wk,-1,@TGL_SUBSCRIPTION)<=getdate()
				begin
					set @Sub='Email subscribe ke-2'
					set @sendemail='Y'
					set @isemail1='N'
					set @TableBody='	
						<tr>
							<td align=left>
								<div class=contentEditableContainer contentTextEditable>
									<div class=contentEditable align=center>
										<p  style=text-align:left;color:#382F2E;font-size:14px;font-weight:normal;line-height:19px;>
											Jakarta, '+ (CONVERT(VARCHAR(20), getdate(), 103)) +'
											<br />
											Dear ' + @Username +',
											<br />
											<br />
												Terima kasih telah menggunakan<span style=color:#cc0000;> Master Online </span>, 
											<br />
												akun anda akan expired pada tgl '+ (CONVERT(VARCHAR(20), @TGL_SUBSCRIPTION, 103)) +' , silahkan melakukan pembayaran agar akun anda tetap aktif ,
											<br />
												di menu pengaturan  , subscription . 
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
					--set  @Body2 =@Body+ @TableTail
				end		
			end
		end	
	end

	print(@Sub)
	print(len(@Body))
	print('===============')
	print(@Body)
	print(@TableTail)
	print('===============')
	--set @Email='ir.dharmawan@gmail.com'

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

		print(@isemail1)

		---------------UPDATE EMAIL DI AKUN---------------
		if @sendemail='Y'
		begin
			if @isemail1='Y'
			begin
				print('update Email 1')
				update [MO].[dbo].[Account]
					set [tgl_email1]=getdate()
				where [AccountId]=@AccountId
				and	[Username]=@Username
				and [tgl_email1] is null


			end
			if @isemail1='N'
			begin
				print('update Email 2')
				update [MO].[dbo].[Account]
					set [tgl_email2]=getdate()
				where [AccountId]=@AccountId
				and	[Username]=@Username
				and [tgl_email2] is null
			end	
		end
		---------------------------------------------
	end
	

	--select * from [MO].[dbo].[Account] where [AccountId]='1'
	--update [MO].[dbo].[Account]
	--	set [tgl_email1]=null
	--	,	[tgl_email2]=null
	--where [AccountId]='1'
			
	SET @i = @i + 1
END

--SELECT * FROM #Account
DROP TABLE #Account

--EXEC xp_dirtree 'C:\', 10, 1

--select * from msdb.dbo.sysmail_sentitems 
--select * from msdb.dbo.sysmail_unsentitems 
--select * from msdb.dbo.sysmail_faileditems 