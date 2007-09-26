<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:VBScript="urn:schemas-sqlxml-org:vbs">
	<xsl:output method="xml" indent="no" omit-xml-declaration="yes" standalone="yes" media-type="text/html" />
	<xsl:decimal-format name="ru" decimal-separator="," grouping-separator=" " />
	<xsl:preserve-space elements="" />
	<xsl:strip-space elements="*" />

	<msxsl:script language="VBScript" implements-prefix="VBScript">
		<![CDATA[
			Function LPad(Value, Char, Length)
				If Length > Len(Value) Then
					LPad = String(Length - Len(Value), Char) & Value
				Else
					LPad = Value
				End If
			End Function
			
			Function YYYYMMDD(DateTime)
				YYYYMMDD = LPad(Year(DateTime), "0", 2) & "-" & LPad(Month(DateTime), "0", 2) & "-" & LPad(Day(DateTime), "0", 2)
				
				' Доплняем дату временем, если оно указано
				If Hour(DateTime) > 0 Or Minute(DateTime) > 0 Or Second(DateTime) > 0 Then
					YYYYMMDD = YYYYMMDD & "T" & LPad(Hour(DateTime), "0", 2) & ":" & LPad(Minute(DateTime), "0", 2) & ":" & LPad(Second(DateTime), "0", 2)
				End If
			End Function
			
			Function CDate2(DateTime)
				CDate2 = DateSerial(Mid(DateTime, 1, 4), Mid(DateTime, 6, 2), Mid(DateTime, 9, 2))
				
				If Len(DateTime) = 20 Then
					CDate2 = DateAdd("h", Mid(DateTime, 12, 2), CDate2)
					CDate2 = DateAdd("n", Mid(DateTime, 15, 2), CDate2)
					CDate2 = DateAdd("s", Mid(DateTime, 18, 2), CDate2)
				End If
			End Function
			
			Function DateAdd2(Interval, Value, Date)
				DateAdd2 = YYYYMMDD(DateAdd(Interval, Value, CDate2(Date)))
			End Function
			
			Function Weekday2(Date)
				Weekday2 = Weekday(CDate2(Date))
			End Function
		]]>
	</msxsl:script>
	
	<xsl:template match="/">
		<HTML>
			<HEAD>
				<META HTTP-EQUIV="Content-Type" CONTENT="text/html; charset=Windows-1251" />
				<META NAME="Description" CONTENT="KMindex.ru — статистика сайтов, счетчик посещений" />
				<META NAME="Keywords" CONTENT="счетчик посещений, счетчик, рейтинг, рейтинг сайта, статистика, интернет-статистика, каталог, система статистики" />
				<META HTTP-EQUIV="Content-Language" CONTENT="ru" />
				<TITLE>KMindex.ru — рейтинг и статистика сайтов, счетчик посещений</TITLE>
				<LINK REL="stylesheet" TYPE="text/css" HREF="/Common Files/Sept.CSS" />
			</HEAD>
			<BODY TOPMARGIN="10" LEFTMARGIN="15" RIGHTMARGIN="15" BOTTOMMARGIN="0" SCROLL="YES" BGCOLOR="#FFFFFF">
				<table cellpadding="0" cellspacing="0" width="100%" height="100%">
					<tr>
						<td align="center">
							<script type="text/javascript">
								<xsl:text disable-output-escaping="yes">
									<![CDATA[
											document.write('<a href="http://click.kmindex.ru/?uid=118561"><img '+
											'src="http://counting.kmindex.ru/?uid=118561&r='+escape(document.referrer)+
											((typeof(screen)=='undefined')?'':'&s='+screen.width+screen.height+
											(screen.colorDepth?screen.colorDepth:screen.pixelDepth))+
											'&'+Math.random()+'" width=1 height=1 border=0 alt=KMindex></a>')
									]]>
								</xsl:text>
							</script>
							<!--begin of Rambler's Top100 code -->
							<a href="http://top100.rambler.ru/top100/">
							<img src="http://counter.rambler.ru/top100.cnt?893675" alt="" width="1" height="1" border="0" /></a> 
							<!--end of Top100 code-->
							<iframe src="http://banner.lbs.km.ru/18087979type=2rnd=0" frameborder="0" vspace="0" hspace="0" width="100%" height="90" marginwidth="0" marginheight="0" scrolling="no">
								<a href="http://www.km.ru"  target="_parent">
									<img src="http://banner.lbs.km.ru/18087979type=3rnd=0" border="0" width="100%" height="90" />
								</a>
							</iframe>
						</td>
					</tr>
					<tr height="25">
						<td></td>
					</tr>
					<tr height="1" bgcolor="#C0113C">
						<td colspan="3"></td>
					</tr>
					<tr>
						<td>
							<table cellpadding="0" cellspacing="0" width="100%">
								<col width="226" />
								<col width="10" />
								<col width="*" />
								<tr>
									<td>
										<img src="/images/hlg.gif" alt="" width="226" height="29" />
									</td>
									<td></td>
									<td valign="middle">
										<table cellpadding="3" cellspacing="0" width="100%">
											<tr>
												<td nowrap="true">
													<a href="/about-k.asp" class="normal s">О проекте</a>
												</td>
												<td>
													<div class="normal s">|</div>
												</td>
												<td nowrap="true">
													<a href="/page-k.asp" class="normal s">Рейтинг сайтов</a>
												</td>
												<td>
													<div class="normal s">|</div>
												</td>
												<td nowrap="true">
													<a href="/reg-k.asp" class="normal s">Регистрация</a>
												</td>
												<td>
													<div class="normal s">|</div>
												</td>
												<td nowrap="true">
													<a class="normal s">
														<xsl:attribute name="href">
															<xsl:text>/Pill?</xsl:text>
															<xsl:for-each select="Root">
																<xsl:value-of select="concat('q', '=', @User_ID)" />
															</xsl:for-each>
														</xsl:attribute>
														<xsl:text>Код счетчика</xsl:text>
													</a>
												</td>
												<td>
													<div class="normal s">|</div>
												</td>
												<td nowrap="true">
													<a href="/login-k.asp" class="normal s">Вход для участников</a>
												</td>
												<td width="100%"></td>
												<td nowrap="true">
													<a href="/page-k.asp" class="normal s bold">Выход</a>
												</td>
											</tr>
										</table>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr height="5">
						<td></td>
					</tr>
					<tr>
						<td>
							<table cellpadding="2" cellspacing="0">
								<tr>
									<td>
										<a href="http://km.ru" class="normal s">KM.RU (главная)</a>
									</td>
									<td>
										<div class="normal s">»</div>
									</td>
									<td>
										<a href="/" class="normal s">KMindex</a>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr height="5">
						<td></td>
					</tr>
					<tr height="100%">
						<td>
							<!-- Основной блок -->
							<xsl:apply-templates />
							<!-- /Основной блок -->
						</td>
					</tr>
					<tr height="15">
						<td></td>
					</tr>
					<tr>
						<td>
							<!-- Футер -->
							<table cellpadding="5" cellspacing="0" width="100%">
								<tr bgcolor="#C0113C" height="10">
									<td>
										<table cellpadding="0" cellspacing="0" width="100%">
											<col width="50%" />
											<col width="50%" />
											<tr>
												<td>
													<div class="normal s white">© «КМ Онлайн», 2002-2007  © «Кирилл и Мефодий», 1998-2001</div>
												</td>
												<td align="right">
													<a href="http://www.km.ru/aboutkm/" class="normal s white">«КМ Онлайн»</a>
													<span class="normal s white"> | </span>
													<a href="http://www.km.ru/presentation/" class="normal s white">Реклама</a>
												</td>
											</tr>
										</table>
									</td>
								</tr>
								<tr>
									<td align="center">
										<table cellpadding="5" cellspacing="0">
											<tr>
												<td>
													<!--begin of Top100 logo--> 
													<a href="http://top100.rambler.ru/top100/">
													<img src="http://top100-images.rambler.ru/top100/banner-88x31-rambler-blue.gif" alt="Rambler's Top100" width="88" height="31" border="0" /></a> 
													<!--end of Top100 logo -->
												</td>
												<td>
													<!--Rating@Mail.ru COUNTER-->
													<a target="_top" href="http://top.mail.ru/jump?from=914732">
													<img src="http://top.list.ru/counter?id=914732;t=79" border="0" height="31" width="38"
													alt="Рейтинг@Mail.ru" /></a><!--/COUNTER-->
												</td>
												<td>
													<!-- Кнопка KMiNDEX -->
													<a href="http://click.kmindex.ru/"><img src="http://counter.kmindex.ru/1.gif" border="0" width="88" height="31" alt="KMindex" /></a>
													<!-- /Кнопка KMiNDEX -->
												</td>
											</tr>
										</table>
									</td>
								</tr>
							</table>
							<!-- /Футер -->
						</td>
					</tr>
				</table>
			</BODY>
		</HTML>
	</xsl:template>
	
	<xsl:template match="Root">
		<xsl:variable name="Root" select="current()" />
		<xsl:variable name="User_ID" select="@User_ID" />
		<xsl:variable name="Server-Name" select="@Server-Name" />
		<xsl:variable name="Q" select="@Q" />
		<xsl:variable name="Report" select="@Report" />
		<xsl:variable name="R" select="@R" />
		<xsl:variable name="UID" select="@UID" />
		<xsl:variable name="Date" select="substring(@Date, 1, 10)" />
		<xsl:variable name="Now" select="@Now" />
		<xsl:variable name="F" select="@F" />
		<xsl:variable name="Total-Hosts" select="sum(Node/@hosts)" />
		<xsl:variable name="Total-Hits" select="sum(Node/@hits)" />
		<xsl:variable name="Weekday" select="VBScript:Weekday2(string($Date))" />
		<table cellpadding="0" cellspacing="5" width="100%" height="100%">
			<tr>
				<td>
					<!-- Информация о сайте -->
					<xsl:for-each select="document(concat('http://', $Server-Name, '/Services/SYTED_USR_RES/', '?', 'user_id', '=', $User_ID))/Root/Node">
						<table cellpadding="5" cellspacing="1" bgcolor="#CCCCCC" width="100%">
							<tr bgcolor="#FFFFFF">
								<td>
									<table cellpadding="2" cellspacing="0" width="100%">
										<col width="*" />
										<col width="300" />
										<tr>
											<td>
												<table cellpadding="2" cellspacing="0" width="100%">
													<tr>
														<td>
															<span class="normal m bold">
																<xsl:value-of select="@rs_name" />
															</span>
															<span class="normal m">
																<xsl:value-of select="concat(' (', @rs_url, ')')" />
															</span>
														</td>
													</tr>
													<tr>
														<td>
															<div class="normal s">
																<xsl:value-of select="@rs_descr" />
															</div>
														</td>
													</tr>
													<tr>
														<td>
															<div class="normal s" style="color: #666666">
																<xsl:choose>
																	<xsl:when test="contains(@rubric, 'E144BFF7-28B8-4359-A29C-91E71E614304')">Авто, мото</xsl:when>
																	<xsl:when test="contains(@rubric, '7BC7DF6F-1F5D-4EA0-B7FF-BBCC1FDFDD7D')">Администрации</xsl:when>
																	<xsl:when test="contains(@rubric, '654C9143-32A8-4296-9D84-567E5B0D8800')">Банки</xsl:when>
																	<xsl:when test="contains(@rubric, 'AD82AE05-6A7C-42B0-8F2E-9736A203B4B7')">Безопасность</xsl:when>
																	<xsl:when test="contains(@rubric, 'E315599A-E594-49E3-A43A-CAC2681E0F4D')">Бизнес, финансы</xsl:when>
																	<xsl:when test="contains(@rubric, '31C3D004-7D7C-49EB-82B7-19BA94FB9565')">Власть, закон</xsl:when>
																	<xsl:when test="contains(@rubric, '745EEA32-35D8-4E71-8B59-BE68195674E8')">Города, регионы</xsl:when>
																	<xsl:when test="contains(@rubric, 'EE16B620-9A49-43F4-8461-B3CD74951450')">Для детей</xsl:when>
																	<xsl:when test="contains(@rubric, 'FC42B49B-BFF3-47B7-BF3A-E048A2794DDC')">Досуг, хобби</xsl:when>
																	<xsl:when test="contains(@rubric, 'E72E60C5-2275-4A17-810A-826935B79783')">Игры и конкурсы</xsl:when>
																	<xsl:when test="contains(@rubric, 'A48CEF8C-A1BA-4986-8E2B-E9B8DA727ED8')">Интернет</xsl:when>
																	<xsl:when test="contains(@rubric, '7FF7C060-586B-424B-818A-EE46219CAC71')">Искусство, культура</xsl:when>
																	<xsl:when test="contains(@rubric, 'CCA0CDFC-B2AC-4C1C-818E-59D97F6A0D51')">История</xsl:when>
																	<xsl:when test="contains(@rubric, 'D16BCC3B-4B01-4940-AFB2-F8FCE59CCA2D')">Кино</xsl:when>
																	<xsl:when test="contains(@rubric, 'DA036459-AF54-47C7-AE06-218D9D2AF248')">Компании</xsl:when>
																	<xsl:when test="contains(@rubric, '4537C898-8C2D-4564-B24F-B9BB27986691')">Компьютеры, программы</xsl:when>
																	<xsl:when test="contains(@rubric, 'F19F4E5E-8F5A-4136-815F-630B1D45A7BD')">Консалтинг, аудит, аналитика</xsl:when>
																	<xsl:when test="contains(@rubric, 'D9B2C4EE-9A01-4A3A-852C-BCEAF3963B83')">Кулинария, питание</xsl:when>
																	<xsl:when test="contains(@rubric, 'AD4DDB46-5663-4E45-BE49-616E6CCEB699')">Литература</xsl:when>
																	<xsl:when test="contains(@rubric, '4FFB3274-0D52-4BF3-B3E0-17B5682B688D')">Медицина</xsl:when>
																	<xsl:when test="contains(@rubric, 'BB1AD8F2-D8D5-4F87-9F14-4D47FBC88D63')">Музыка, МP3</xsl:when>
																	<xsl:when test="contains(@rubric, '576D1EE6-3252-40A8-A337-676F71BE21D9')">Наука</xsl:when>
																	<xsl:when test="contains(@rubric, '0EF95CB5-5FB2-43CC-AE06-2643B79D1F7F')">Недвижимость</xsl:when>
																	<xsl:when test="contains(@rubric, '55C5858F-CB63-4CB5-859A-73D510EB2E23')">Образование</xsl:when>
																	<xsl:when test="contains(@rubric, 'EAB503D2-838E-4C1A-849D-6D54B9AE43DA')">Партии, движения, политика</xsl:when>
																	<xsl:when test="contains(@rubric, 'C3E78C8B-4E55-4DF9-819E-345698269641')">Персоналии</xsl:when>
																	<xsl:when test="contains(@rubric, '6B65C0A2-39F0-4857-A1BA-B829BD1B0A31')">Природа, животные</xsl:when>
																	<xsl:when test="contains(@rubric, 'DEAB06FA-A369-4CEC-B31F-665FFC5D2CD3')">Прочее</xsl:when>
																	<xsl:when test="contains(@rubric, 'BE719219-23BC-4B39-8BE8-992B4A0D5E97')">Путешествие, туризм</xsl:when>
																	<xsl:when test="contains(@rubric, 'FB5CBAB0-3B9C-42B2-B0F6-746F038B489B')">Работа, карьера</xsl:when>
																	<xsl:when test="contains(@rubric, '008F5432-9D1A-4C8A-BA6A-55F099140C7B')">Радио, TV</xsl:when>
																	<xsl:when test="contains(@rubric, '167C3039-40A1-4F4D-9BAB-90915DBAD9E1')">Развлечения</xsl:when>
																	<xsl:when test="contains(@rubric, '39BCB19C-48A3-4714-9F1F-93184269FE47')">Реклама, маркетинг</xsl:when>
																	<xsl:when test="contains(@rubric, 'C95D79AB-86F6-4D05-B55D-D8FB27C64003')">Религия, вера</xsl:when>
																	<xsl:when test="contains(@rubric, '96C9B6FD-1519-4B12-B58B-76DAC9A4D306')">Связь, коммуникации</xsl:when>
																	<xsl:when test="contains(@rubric, 'DB2ED4E2-079B-4676-8DC8-2E91D37EE9A9')">Сервисы</xsl:when>
																	<xsl:when test="contains(@rubric, '5DD38C1B-CC44-4601-BB2B-DF31BB748431')">СМИ, периодика</xsl:when>
																	<xsl:when test="contains(@rubric, 'AC41B8B0-8186-450D-8E85-4157FBEA0CEB')">Спорт</xsl:when>
																	<xsl:when test="contains(@rubric, 'A390E888-CA3F-47C2-A57F-A9358B494FE2')">Справочники</xsl:when>
																	<xsl:when test="contains(@rubric, '10362C30-738F-4D61-A7B8-ED58E1D4E8D4')">Строительство и ремонт</xsl:when>
																	<xsl:when test="contains(@rubric, 'F6C240FC-7983-434F-A076-4FF05EE1BB71')">Техника, технологии</xsl:when>
																	<xsl:when test="contains(@rubric, '61D84F75-D588-47B9-8B8C-18988707BA58')">Товары, услуги</xsl:when>
																	<xsl:when test="contains(@rubric, 'ABEF7336-9C76-4409-9849-2BEE7C67FBA9')">Торговля</xsl:when>
																	<xsl:when test="contains(@rubric, '6E3638AD-EC0A-47E8-B235-3169A61D1367')">Транспорт</xsl:when>
																	<xsl:when test="contains(@rubric, '387474CA-EA17-4839-B583-702F878C6523')">Фото, графика, дизайн</xsl:when>
																	<xsl:when test="contains(@rubric, '82723805-03DE-41D5-A8A3-CB84BE8C33E8')">Юмор, анекдоты</xsl:when>
																	<xsl:when test="contains(@rubric, '3D46349F-82A7-46ED-B21B-9D4FB3E6038B')">Электронная коммерция</xsl:when>
																	<xsl:when test="contains(@rubric, '7DDFB939-D384-4F79-AF44-A5CB00534B1E')">Блоги</xsl:when>
																	<xsl:when test="contains(@rubric, '8C2A931A-67BE-4574-8578-1C066C43DD9D')">Знакомства</xsl:when>
																	<xsl:when test="contains(@rubric, '2E0FA882-7543-4B11-8077-174D95A7E208')">Доски объявлений</xsl:when>
																	<xsl:when test="contains(@rubric, '5631FE2E-1D5F-4C60-A8B6-599936856F38')">Каталоги, поисковики</xsl:when>
																	<xsl:when test="contains(@rubric, '3285961A-E603-4F2A-A57E-DB0A510A510D')">Конференции, чаты, форумы</xsl:when>
																	<xsl:when test="contains(@rubric, 'C14B1718-466C-409B-A062-7CAF2DBE5862')">Новости</xsl:when>
																	<xsl:when test="contains(@rubric, 'C533BD68-579E-4D64-8513-679C5BA2C8F3')">Магазины</xsl:when>
																	<xsl:when test="contains(@rubric, 'F75F4033-35D9-49F9-84B3-15B3A6BE1A2A')">Порталы, универсальные ресурсы</xsl:when>
																	<xsl:when test="contains(@rubric, 'F4607F4A-35DF-4FB0-A3D3-35C5AE6BECD4')">Почтовые сервисы и  хостинг</xsl:when>
																	<xsl:when test="contains(@rubric, '8CB9D79C-942E-445C-B785-C6EDC83D895E')">Провайдеры</xsl:when>
																	<xsl:when test="contains(@rubric, '2188E81F-5163-4DE1-B029-CEF037953280')">Белоруссия</xsl:when>
																	<xsl:when test="contains(@rubric, '9350A6F1-75CB-439E-8877-778EFF80E80A')">Казахстан</xsl:when>
																	<xsl:when test="contains(@rubric, 'E2C74EDC-B733-45C6-BFF8-75A6E9E05123')">Прибалтика</xsl:when>
																	<xsl:when test="contains(@rubric, '95E773D3-4C7A-417C-9331-EACF11C0D4CB')">Украина</xsl:when>
																	<xsl:when test="contains(@rubric, '1427A84D-2710-4E78-A47A-72D237EA65ED')">Германия</xsl:when>
																	<xsl:when test="contains(@rubric, '4F84D508-BAAB-4BCE-A5C0-1FDC37C09FFA')">Израиль</xsl:when>
																	<xsl:when test="contains(@rubric, '169EE42B-88B6-43EB-A018-957BE5F23DA4')">Канада</xsl:when>
																	<xsl:when test="contains(@rubric, '92E850D3-6B09-42AB-95CA-D2BE43DAF3E6')">США</xsl:when>
																	<xsl:when test="contains(@rubric, '3DB3C1DE-653A-4D63-88AA-2820ECC526D4')">Другие страны</xsl:when>
																	<xsl:otherwise>Не участвует в рейтинге</xsl:otherwise>
																</xsl:choose>
															</div>
														</td>
													</tr>
												</table>
											</td>
											<td align="right" valign="top">
												<a class="normal s">
													<xsl:attribute name="href">
														<xsl:text>/change.asp?</xsl:text>
														<xsl:value-of select="concat('idn', '=', $User_ID)" />
													</xsl:attribute>
													<xsl:text>Изменить регистрационные данные</xsl:text>
												</a>
											</td>
										</tr>
									</table>
								</td>
							</tr>
						</table>
					</xsl:for-each>
					<!-- /Информация о сайте -->
				</td>
			</tr>
			<tr>
				<td>
					<a name="1" />
					<xsl:apply-templates select="$Root" mode="Period" />
				</td>
			</tr>
			<tr>
				<td>
					<xsl:apply-templates select="$Root" mode="Year" />
				</td>
			</tr>
			<tr>
				<td>
					<xsl:apply-templates select="$Root" mode="Month" />
				</td>
			</tr>
			<tr>
				<td>
					<xsl:apply-templates select="$Root" mode="Date" />
				</td>
			</tr>
			<!--
			<xsl:choose>
				<xsl:when test="$Report = 4">
					<tr>
						<td>
							<table cellpadding="5" cellspacing="0" width="100%">
								<col width="100" />
								<col width="*" />
								<tr>
									<td valign="top" rowspan="2">
										<img src="/Common Files/Soon.gif" alt="" />
									</td>
									<td valign="top">
										<span class="normal">Точное определение браузеров — самый полный список, включая новые версии Firefox, Opera, браузеры для КПК, коммуникаторов и смартфонов.</span>
									</td>
								</tr>
								<tr>
									<td valign="top">
										<div class="normal">Следите за изменениями!</div>
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr height="10">
						<td></td>
					</tr>
				</xsl:when>
				<xsl:when test="$Report = 5">
					<xsl:choose>
						<xsl:when test="substring($Now, 18, 2) mod 10 = 1">
							<tr>
								<td>
									<table cellpadding="5" cellspacing="0" width="100%">
										<col width="100" />
										<col width="*" />
										<tr>
											<td valign="top" rowspan="2">
												<img src="/Common Files/Soon.gif" alt="" />
											</td>
											<td valign="top">
												<span class="normal">По статистике всех сайтов, зарегистрированных в KMindex, процент посетителей, использующих Windows Vista дома, больше, чем на работе.</span>
											</td>
										</tr>
									</table>
								</td>
							</tr>
						</xsl:when>
					</xsl:choose>
					<tr height="10">
						<td></td>
					</tr>
				</xsl:when>
			</xsl:choose>
			-->
			<tr>
				<td>
					<xsl:choose>
						<xsl:when test="count(Node) != 0">
							<!-- Таблица -->
							<table cellpadding="0" cellspacing="0" width="100%">
								<col width="0" />
								<col width="*" />
								<col width="0" />
								<tr>
									<td></td>
									<td>
										<table cellpadding="7" cellspacing="0" width="100%">
											<col width="10" />
											<col width="*" />
											<col width="150" />
											<col width="150" />
											<col width="10" />
											<tr bgcolor="#EEEEEE">
												<td></td>
												<td>
													<div class="normal bold">
														<xsl:choose>
															<xsl:when test="$Report = 4">Браузер</xsl:when>
															<xsl:when test="$Report = 5">Платформа</xsl:when>
															<xsl:when test="$Report = 6">Страна</xsl:when>
															<xsl:when test="$Report = 7">Город</xsl:when>
														</xsl:choose>
													</div>
												</td>
												<td align="right">
													<xsl:choose>
														<xsl:when test="$Q = 0">
															<span class="normal bold">Хосты</span>
															<span class="normal">
																<xsl:text disable-output-escaping="yes">&#160;&amp;#8595;</xsl:text>
															</span>
														</xsl:when>
														<xsl:otherwise>
															<a class="normal bold">
																<xsl:attribute name="href">
																	<xsl:text>?</xsl:text>
																	<xsl:value-of select="concat('idn', '=', $User_ID)" />
																	<xsl:text>&amp;</xsl:text>
																	<xsl:value-of select="concat('date', '=', $Date)" />
																	<xsl:text>&amp;</xsl:text>
																	<xsl:value-of select="concat('report', '=', $Report)" />
																	<xsl:text>#1</xsl:text>
																</xsl:attribute>
																<xsl:text>Хосты</xsl:text>
															</a>
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td align="right">
													<xsl:choose>
														<xsl:when test="$Q = 1">
															<span class="normal bold">Хиты</span>
															<span class="normal">
																<xsl:text disable-output-escaping="yes">&#160;&amp;#8595;</xsl:text>
															</span>
														</xsl:when>
														<xsl:otherwise>
															<a class="normal bold">
																<xsl:attribute name="href">
																	<xsl:text>?</xsl:text>
																	<xsl:value-of select="concat('idn', '=', $User_ID)" />
																	<xsl:text>&amp;</xsl:text>
																	<xsl:value-of select="concat('q', '=', 1)" />
																	<xsl:text>&amp;</xsl:text>
																	<xsl:value-of select="concat('date', '=', $Date)" />
																	<xsl:text>&amp;</xsl:text>
																	<xsl:value-of select="concat('report', '=', $Report)" />
																	<xsl:text>#1</xsl:text>
																</xsl:attribute>
																<xsl:text>Хиты</xsl:text>
															</a>
														</xsl:otherwise>
													</xsl:choose>
												</td>
												<td></td>
											</tr>
											<tr height="1" bgcolor="#CCCCCC">
												<td colspan="5"></td>
											</tr>
											<xsl:for-each select="Node[position() &lt;= 5 or $F = 1]">
												<tr>
													<td></td>
													<td>
														<div class="normal">
															<xsl:choose>
																<xsl:when test="$Report = 5">
																	<xsl:choose>
																		<xsl:when test="@name = 1">Windows версии 3.11 или более ранней</xsl:when>
																		<xsl:when test="@name = 3">Windows NT</xsl:when>
																		<xsl:when test="@name = 4">Windows 95</xsl:when>
																		<xsl:when test="@name = 5">Windows 98</xsl:when>
																		<xsl:when test="@name = 6">Windows ME</xsl:when>
																		<xsl:when test="@name = 7">Windows 2000</xsl:when>
																		<xsl:when test="@name = 9">Windows CE</xsl:when>
																		<xsl:when test="@name = 10">Windows XP</xsl:when>
																		<xsl:when test="@name = 11">Windows 2003</xsl:when>
																		<xsl:when test="@name = 12">Windows Vista</xsl:when>
																		<xsl:when test="@name = 20">Linux</xsl:when>
																		<xsl:when test="@name = 30">FreeBSD</xsl:when>
																		<xsl:when test="@name = 170">Apple MacOS</xsl:when>
																		<xsl:when test="@name = 180">WebTV</xsl:when>
																		<xsl:when test="@name = 120">X11</xsl:when>
																		<xsl:when test="@name = 130">OS/2</xsl:when>
																		<xsl:when test="@name = 200">CP/M</xsl:when>
																		<xsl:when test="@name = 240">Смартфон, телефон с поддержкой J2ME</xsl:when>
																		<xsl:otherwise>
																			<span class="bold">Платформа не определена</span>
																			<div class="xs">Такая ситуация возможна, если у посетителя установлена специализированная операционная система или прокси-сервер пропускает не всю информацию, передаваемую браузером.</div>
																		</xsl:otherwise>
																	</xsl:choose>
																</xsl:when>
																<xsl:when test="$Report = 4">
																	<xsl:choose>
																		<xsl:when test="@name = 1">Opera</xsl:when>
																		<xsl:when test="@name = 9">Opera 9</xsl:when>
																		<xsl:when test="@name = 8">Opera 8</xsl:when>
																		<xsl:when test="@name = 7">Opera 7</xsl:when>
																		<xsl:when test="@name = 6">Opera 6</xsl:when>
																		<xsl:when test="@name = 90">Браузер на ядре Mozilla</xsl:when>
																		<xsl:when test="@name = 190">Браузер на ядре Gecko</xsl:when>
																		<xsl:when test="@name = 191">Firebird</xsl:when>
																		<xsl:when test="@name = 192">Firefox</xsl:when>
																		<xsl:when test="@name = 193">Safari</xsl:when>
																		<xsl:when test="@name = 194">Galeon</xsl:when>
																		<xsl:when test="@name = 195">MultiZilla</xsl:when>
																		<xsl:when test="@name = 149">Netscape 7</xsl:when>
																		<xsl:when test="@name = 150">Netscape 6</xsl:when>
																		<xsl:when test="@name = 56">Konqueror</xsl:when>
																		<xsl:when test="@name = 20">Internet Explorer</xsl:when>
																		<xsl:when test="@name = 21">Internet Explorer 5.5</xsl:when>
																		<xsl:when test="@name = 22">Internet Explorer 5</xsl:when>
																		<xsl:when test="@name = 23">Internet Explorer 6</xsl:when>
																		<xsl:when test="@name = 24">Internet Explorer 4</xsl:when>
																		<xsl:when test="@name = 25">Internet Explorer 3</xsl:when>
																		<xsl:when test="@name = 27">Internet Explorer 7</xsl:when>
																		<xsl:when test="@name = 150">Netscape 6</xsl:when>
																		<xsl:when test="@name = 151">Netscape 5</xsl:when>
																		<xsl:when test="@name = 152">Netscape 4</xsl:when>
																		<xsl:when test="@name = 153">Netscape 3</xsl:when>
																		<xsl:when test="@name = 161">Portable Internet Explorer</xsl:when>
																		<xsl:when test="@name = 162">Squid</xsl:when>
																		<xsl:when test="@name = 170">Nutscrape</xsl:when>
																		<xsl:when test="@name = 180">PowerBrowser</xsl:when>
																		<xsl:when test="@name = 122">WebTV</xsl:when>
																		<xsl:when test="@name = 101">Google</xsl:when>
																		<xsl:otherwise>
																			<span class="bold">Неизвестный браузер</span>
																			<div class="xs">Возможно, эти посетители заходили на сайт браузером, отсутствующим в нашей базе данных. В ближайшее время мы собираемся значительно пополнить список определяемых браузеров, включив в него наиболее распростаненные браузеры для КПК и смартфонов.</div>
																		</xsl:otherwise>
																	</xsl:choose>
																</xsl:when>
																<xsl:when test="$Report = 6">
																	<xsl:choose>
																		<xsl:when test="@name = 'AD'">Андорра</xsl:when>
																		<xsl:when test="@name = 'AE'">ОАЭ</xsl:when>
																		<xsl:when test="@name = 'AF'">Афганистан</xsl:when>
																		<xsl:when test="@name = 'AG'">Антигуа и Барбуда</xsl:when>
																		<xsl:when test="@name = 'AI'">Ангилья</xsl:when>
																		<xsl:when test="@name = 'AL'">Албания</xsl:when>
																		<xsl:when test="@name = 'AM'">Армения</xsl:when>
																		<xsl:when test="@name = 'AN'">Антильские острова</xsl:when>
																		<xsl:when test="@name = 'AO'">Ангола</xsl:when>
																		<xsl:when test="@name = 'AQ'">Антарктика</xsl:when>
																		<xsl:when test="@name = 'AR'">Аргентина</xsl:when>
																		<xsl:when test="@name = 'AS'">Американское Самоа</xsl:when>
																		<xsl:when test="@name = 'AT'">Австрия</xsl:when>
																		<xsl:when test="@name = 'AU'">Австралия</xsl:when>
																		<xsl:when test="@name = 'AW'">Аруба</xsl:when>
																		<xsl:when test="@name = 'AZ'">Азербайджан</xsl:when>
																		<xsl:when test="@name = 'BA'">Босния и Герцеговина</xsl:when>
																		<xsl:when test="@name = 'BB'">Барбадос</xsl:when>
																		<xsl:when test="@name = 'BD'">Бангладеш</xsl:when>
																		<xsl:when test="@name = 'BE'">Бельгия</xsl:when>
																		<xsl:when test="@name = 'BF'">Буркина-Фасо</xsl:when>
																		<xsl:when test="@name = 'BG'">Болгария</xsl:when>
																		<xsl:when test="@name = 'BH'">Бахрейн</xsl:when>
																		<xsl:when test="@name = 'BI'">Бурунди</xsl:when>
																		<xsl:when test="@name = 'BJ'">Бенин</xsl:when>
																		<xsl:when test="@name = 'BM'">Бермудские острова</xsl:when>
																		<xsl:when test="@name = 'BN'">Бруней</xsl:when>
																		<xsl:when test="@name = 'BO'">Боливия</xsl:when>
																		<xsl:when test="@name = 'BR'">Бразилия</xsl:when>
																		<xsl:when test="@name = 'BS'">Багамские Острова</xsl:when>
																		<xsl:when test="@name = 'BT'">Бутан</xsl:when>
																		<xsl:when test="@name = 'BW'">Ботсвана</xsl:when>
																		<xsl:when test="@name = 'BY'">Белоруссия</xsl:when>
																		<xsl:when test="@name = 'BZ'">Белиз</xsl:when>
																		<xsl:when test="@name = 'CA'">Канада</xsl:when>
																		<xsl:when test="@name = 'CD'">Демократическая республика Конго</xsl:when>
																		<xsl:when test="@name = 'CF'">Центральноафриканская республика</xsl:when>
																		<xsl:when test="@name = 'CG'">Республика Конго</xsl:when>
																		<xsl:when test="@name = 'CH'">Швейцария</xsl:when>
																		<xsl:when test="@name = 'CI'">Кот-ДИвуар</xsl:when>
																		<xsl:when test="@name = 'CK'">Кука, Острова</xsl:when>
																		<xsl:when test="@name = 'CL'">Чили</xsl:when>
																		<xsl:when test="@name = 'CM'">Камерун</xsl:when>
																		<xsl:when test="@name = 'CN'">Китай</xsl:when>
																		<xsl:when test="@name = 'CO'">Колумбия</xsl:when>
																		<xsl:when test="@name = 'CR'">Коста-Рика</xsl:when>
																		<xsl:when test="@name = 'CS'">Сербия и Черногория</xsl:when>
																		<xsl:when test="@name = 'CU'">Куба</xsl:when>
																		<xsl:when test="@name = 'CV'">Кабо Верди</xsl:when>
																		<xsl:when test="@name = 'CY'">Кипр</xsl:when>
																		<xsl:when test="@name = 'CZ'">Чехия</xsl:when>
																		<xsl:when test="@name = 'DE'">Германия</xsl:when>
																		<xsl:when test="@name = 'DJ'">Джибути</xsl:when>
																		<xsl:when test="@name = 'DK'">Дания</xsl:when>
																		<xsl:when test="@name = 'DM'">Доминика</xsl:when>
																		<xsl:when test="@name = 'DO'">Доминиканская республика</xsl:when>
																		<xsl:when test="@name = 'DZ'">Алжир</xsl:when>
																		<xsl:when test="@name = 'EC'">Эквадор</xsl:when>
																		<xsl:when test="@name = 'EE'">Эстония</xsl:when>
																		<xsl:when test="@name = 'EG'">Египет</xsl:when>
																		<xsl:when test="@name = 'ER'">Эритрея</xsl:when>
																		<xsl:when test="@name = 'ES'">Испания</xsl:when>
																		<xsl:when test="@name = 'ET'">Эфиопия</xsl:when>
																		<xsl:when test="@name = 'EU'">Европейский Союз</xsl:when>
																		<xsl:when test="@name = 'FI'">Финляндия</xsl:when>
																		<xsl:when test="@name = 'FJ'">Фиджи</xsl:when>
																		<xsl:when test="@name = 'FK'">Фолклендские острова</xsl:when>
																		<xsl:when test="@name = 'FM'">Микронезия</xsl:when>
																		<xsl:when test="@name = 'FO'">Фареры</xsl:when>
																		<xsl:when test="@name = 'FR'">Франция</xsl:when>
																		<xsl:when test="@name = 'GA'">Габон</xsl:when>
																		<xsl:when test="@name = 'GB'">Великобритания</xsl:when>
																		<xsl:when test="@name = 'GD'">Гренада</xsl:when>
																		<xsl:when test="@name = 'GE'">Грузия</xsl:when>
																		<xsl:when test="@name = 'GF'">Гвиана Французская</xsl:when>
																		<xsl:when test="@name = 'GH'">Гана</xsl:when>
																		<xsl:when test="@name = 'GI'">Гибралтар</xsl:when>
																		<xsl:when test="@name = 'GL'">Гренландия</xsl:when>
																		<xsl:when test="@name = 'GM'">Гамбия</xsl:when>
																		<xsl:when test="@name = 'GN'">Гвинея</xsl:when>
																		<xsl:when test="@name = 'GP'">Гваделупа</xsl:when>
																		<xsl:when test="@name = 'GQ'">Экваториальная Гвинея</xsl:when>
																		<xsl:when test="@name = 'GR'">Греция</xsl:when>
																		<xsl:when test="@name = 'GT'">Гватемала</xsl:when>
																		<xsl:when test="@name = 'GU'">Гуам</xsl:when>
																		<xsl:when test="@name = 'GW'">Гвинея-Бисау</xsl:when>
																		<xsl:when test="@name = 'GY'">Гайана</xsl:when>
																		<xsl:when test="@name = 'HK'">Гонг-Конг</xsl:when>
																		<xsl:when test="@name = 'HN'">Гондурас</xsl:when>
																		<xsl:when test="@name = 'HR'">Хорватия</xsl:when>
																		<xsl:when test="@name = 'HT'">Гаити</xsl:when>
																		<xsl:when test="@name = 'HU'">Венгрия</xsl:when>
																		<xsl:when test="@name = 'ID'">Индонезия</xsl:when>
																		<xsl:when test="@name = 'IE'">Ирландия</xsl:when>
																		<xsl:when test="@name = 'IL'">Израиль</xsl:when>
																		<xsl:when test="@name = 'IN'">Индия</xsl:when>
																		<xsl:when test="@name = 'IO'">Британская территория в Индийском океане</xsl:when>
																		<xsl:when test="@name = 'IQ'">Ирак</xsl:when>
																		<xsl:when test="@name = 'IR'">Иран</xsl:when>
																		<xsl:when test="@name = 'IS'">Исландия</xsl:when>
																		<xsl:when test="@name = 'IT'">Италия</xsl:when>
																		<xsl:when test="@name = 'JM'">Ямайка</xsl:when>
																		<xsl:when test="@name = 'JO'">Иордания</xsl:when>
																		<xsl:when test="@name = 'JP'">Япония</xsl:when>
																		<xsl:when test="@name = 'KE'">Кения</xsl:when>
																		<xsl:when test="@name = 'KG'">Киргизстан</xsl:when>
																		<xsl:when test="@name = 'KH'">Камбоджа</xsl:when>
																		<xsl:when test="@name = 'KI'">Кирибати</xsl:when>
																		<xsl:when test="@name = 'KM'">Коморос</xsl:when>
																		<xsl:when test="@name = 'KN'">Сент-Китс и Невис</xsl:when>
																		<xsl:when test="@name = 'KR'">Южная Корея</xsl:when>
																		<xsl:when test="@name = 'KW'">Кувейт</xsl:when>
																		<xsl:when test="@name = 'KY'">Кайман, Острова</xsl:when>
																		<xsl:when test="@name = 'KZ'">Казахстан</xsl:when>
																		<xsl:when test="@name = 'LA'">Лаос</xsl:when>
																		<xsl:when test="@name = 'LB'">Ливан</xsl:when>
																		<xsl:when test="@name = 'LC'">Сент-Люсия</xsl:when>
																		<xsl:when test="@name = 'LI'">Лихтенштейн</xsl:when>
																		<xsl:when test="@name = 'LK'">Шри-Ланка</xsl:when>
																		<xsl:when test="@name = 'LR'">Либерия</xsl:when>
																		<xsl:when test="@name = 'LS'">Лесото</xsl:when>
																		<xsl:when test="@name = 'LT'">Литва</xsl:when>
																		<xsl:when test="@name = 'LU'">Люксембург</xsl:when>
																		<xsl:when test="@name = 'LV'">Латвия</xsl:when>
																		<xsl:when test="@name = 'LY'">Ливия</xsl:when>
																		<xsl:when test="@name = 'MA'">Марокко</xsl:when>
																		<xsl:when test="@name = 'MC'">Монако</xsl:when>
																		<xsl:when test="@name = 'MD'">Молдавия</xsl:when>
																		<xsl:when test="@name = 'MG'">Мадагаскар</xsl:when>
																		<xsl:when test="@name = 'MH'">Маршалловы Острова</xsl:when>
																		<xsl:when test="@name = 'MK'">Македония</xsl:when>
																		<xsl:when test="@name = 'ML'">Мали</xsl:when>
																		<xsl:when test="@name = 'MM'">Мьянма</xsl:when>
																		<xsl:when test="@name = 'MN'">Монголия</xsl:when>
																		<xsl:when test="@name = 'MO'">Макао</xsl:when>
																		<xsl:when test="@name = 'MP'">Северные Марианские Острова</xsl:when>
																		<xsl:when test="@name = 'MQ'">Мартиника</xsl:when>
																		<xsl:when test="@name = 'MR'">Мавритания</xsl:when>
																		<xsl:when test="@name = 'MT'">Мальта</xsl:when>
																		<xsl:when test="@name = 'MU'">Маврикий</xsl:when>
																		<xsl:when test="@name = 'MV'">Мальдивы</xsl:when>
																		<xsl:when test="@name = 'MW'">Малави</xsl:when>
																		<xsl:when test="@name = 'MX'">Мексика</xsl:when>
																		<xsl:when test="@name = 'MY'">Малайзия</xsl:when>
																		<xsl:when test="@name = 'MZ'">Мозамбик</xsl:when>
																		<xsl:when test="@name = 'NA'">Намибия</xsl:when>
																		<xsl:when test="@name = 'NC'">Новая Каледония</xsl:when>
																		<xsl:when test="@name = 'NE'">Нигер</xsl:when>
																		<xsl:when test="@name = 'NF'">Норфолк</xsl:when>
																		<xsl:when test="@name = 'NG'">Нигерия</xsl:when>
																		<xsl:when test="@name = 'NI'">Никарагуа</xsl:when>
																		<xsl:when test="@name = 'NL'">Нидерланды</xsl:when>
																		<xsl:when test="@name = 'NO'">Норвегия</xsl:when>
																		<xsl:when test="@name = 'NP'">Непал</xsl:when>
																		<xsl:when test="@name = 'NR'">Науру</xsl:when>
																		<xsl:when test="@name = 'NU'">Ниуэ</xsl:when>
																		<xsl:when test="@name = 'NZ'">Новая Зеландия</xsl:when>
																		<xsl:when test="@name = 'OM'">Оман</xsl:when>
																		<xsl:when test="@name = 'PA'">Панама</xsl:when>
																		<xsl:when test="@name = 'PE'">Перу</xsl:when>
																		<xsl:when test="@name = 'PF'">Французская Полинезия</xsl:when>
																		<xsl:when test="@name = 'PG'">Папуа-Новая Гвинея</xsl:when>
																		<xsl:when test="@name = 'PH'">Филиппины</xsl:when>
																		<xsl:when test="@name = 'PK'">Пакистан</xsl:when>
																		<xsl:when test="@name = 'PL'">Польша</xsl:when>
																		<xsl:when test="@name = 'PR'">Пуэрто-Рико</xsl:when>
																		<xsl:when test="@name = 'PS'">Палестинская автономия</xsl:when>
																		<xsl:when test="@name = 'PT'">Португалия</xsl:when>
																		<xsl:when test="@name = 'PW'">PALAU</xsl:when>
																		<xsl:when test="@name = 'PY'">Парагвай</xsl:when>
																		<xsl:when test="@name = 'QA'">Катар</xsl:when>
																		<xsl:when test="@name = 'RE'">Реюньон</xsl:when>
																		<xsl:when test="@name = 'RO'">Румыния</xsl:when>
																		<xsl:when test="@name = 'RS'">Сербия</xsl:when>
																		<xsl:when test="@name = 'RU'">Россия</xsl:when>
																		<xsl:when test="@name = 'RW'">Руанда</xsl:when>
																		<xsl:when test="@name = 'SA'">Саудовская Аравия</xsl:when>
																		<xsl:when test="@name = 'SB'">Соломоновы Острова</xsl:when>
																		<xsl:when test="@name = 'SC'">Сейшельские Острова</xsl:when>
																		<xsl:when test="@name = 'SD'">Судан</xsl:when>
																		<xsl:when test="@name = 'SE'">Швеция</xsl:when>
																		<xsl:when test="@name = 'SG'">Сингапур</xsl:when>
																		<xsl:when test="@name = 'SI'">Словения</xsl:when>
																		<xsl:when test="@name = 'SK'">Словакия</xsl:when>
																		<xsl:when test="@name = 'SL'">Сьерра-Леоне</xsl:when>
																		<xsl:when test="@name = 'SM'">Сан-Марино</xsl:when>
																		<xsl:when test="@name = 'SN'">Сенегал</xsl:when>
																		<xsl:when test="@name = 'SO'">Сомали</xsl:when>
																		<xsl:when test="@name = 'SR'">Суринам</xsl:when>
																		<xsl:when test="@name = 'ST'">Сан-Томе и Принсипи</xsl:when>
																		<xsl:when test="@name = 'SV'">Сальвадор</xsl:when>
																		<xsl:when test="@name = 'SY'">Сирия</xsl:when>
																		<xsl:when test="@name = 'SZ'">Свазиленд</xsl:when>
																		<xsl:when test="@name = 'TD'">Чад</xsl:when>
																		<xsl:when test="@name = 'TF'">Французские Южные Территории</xsl:when>
																		<xsl:when test="@name = 'TG'">Того</xsl:when>
																		<xsl:when test="@name = 'TH'">Таиланд</xsl:when>
																		<xsl:when test="@name = 'TJ'">Таджикистан</xsl:when>
																		<xsl:when test="@name = 'TK'">Токелау</xsl:when>
																		<xsl:when test="@name = 'TL'">Тимор</xsl:when>
																		<xsl:when test="@name = 'TM'">Туркменистан</xsl:when>
																		<xsl:when test="@name = 'TN'">Тунис</xsl:when>
																		<xsl:when test="@name = 'TO'">Тонга</xsl:when>
																		<xsl:when test="@name = 'TR'">Турция</xsl:when>
																		<xsl:when test="@name = 'TT'">Тринидад и Тобаго</xsl:when>
																		<xsl:when test="@name = 'TV'">Тувалу</xsl:when>
																		<xsl:when test="@name = 'TW'">Тайвань</xsl:when>
																		<xsl:when test="@name = 'TZ'">Танзания</xsl:when>
																		<xsl:when test="@name = 'UA'">Украина</xsl:when>
																		<xsl:when test="@name = 'UG'">Уганда</xsl:when>
																		<xsl:when test="@name = 'US'">США</xsl:when>
																		<xsl:when test="@name = 'UY'">Уругвай</xsl:when>
																		<xsl:when test="@name = 'UZ'">Узбекистан</xsl:when>
																		<xsl:when test="@name = 'VA'">Ватикан</xsl:when>
																		<xsl:when test="@name = 'VC'">Сент-Винсент и Гренадины</xsl:when>
																		<xsl:when test="@name = 'VE'">Венесуэла</xsl:when>
																		<xsl:when test="@name = 'VG'">Виргинские острова (Британские)</xsl:when>
																		<xsl:when test="@name = 'VI'">Виргинские Острова (США)</xsl:when>
																		<xsl:when test="@name = 'VN'">Вьетнам</xsl:when>
																		<xsl:when test="@name = 'VU'">Вануату</xsl:when>
																		<xsl:when test="@name = 'WS'">Самоа</xsl:when>
																		<xsl:when test="@name = 'YE'">Йемен</xsl:when>
																		<xsl:when test="@name = 'YT'">Майотта</xsl:when>
																		<xsl:when test="@name = 'YU'">Югославия</xsl:when>
																		<xsl:when test="@name = 'ZA'">Южная Африка</xsl:when>
																		<xsl:when test="@name = 'ZM'">Замбия</xsl:when>
																		<xsl:when test="@name = 'ZW'">Зимбабве</xsl:when>
																		<xsl:otherwise>
																			<span class="bold">Страна не определена</span>
																			<div class="xs">Для этих посетителей нет точного соответствия IP-адреса той или иной стране, поэтому мы вынесли их в отдельную строку. Обновление базы данных стран и городов производится регулярно, чтобы вы могли получать достоверную информацию.</div>
																		</xsl:otherwise>
																	</xsl:choose>
																</xsl:when>
																<xsl:when test="$Report = 7">
																	<xsl:choose>
																		<xsl:when test="@name = 1">Москва</xsl:when>
																		<xsl:when test="@name = 2">Санкт-Петербург</xsl:when>
																		<xsl:when test="@name = 3">Новосибирск</xsl:when>
																		<xsl:when test="@name = 4">Владивосток</xsl:when>
																		<xsl:when test="@name = 5">Омск</xsl:when>
																		<xsl:when test="@name = 6">Томск</xsl:when>
																		<xsl:when test="@name = 7">Ярославль</xsl:when>
																		<xsl:when test="@name = 8">Ростов-на-Дону</xsl:when>
																		<xsl:when test="@name = 9">Екатеринбург</xsl:when>
																		<xsl:when test="@name = 10">Новороссийск</xsl:when>
																		<xsl:when test="@name = 11">Кострома</xsl:when>
																		<xsl:when test="@name = 12">Псков</xsl:when>
																		<xsl:when test="@name = 13">Архангельск</xsl:when>
																		<xsl:when test="@name = 14">Иваново</xsl:when>
																		<xsl:when test="@name = 15">Пенза</xsl:when>
																		<xsl:when test="@name = 16">Самара</xsl:when>
																		<xsl:when test="@name = 17">Обнинск</xsl:when>
																		<xsl:when test="@name = 18">Великий Новгород</xsl:when>
																		<xsl:when test="@name = 19">Комсомольск-на-Амуре</xsl:when>
																		<xsl:when test="@name = 20">Пермь</xsl:when>
																		<xsl:when test="@name = 21">Надым</xsl:when>
																		<xsl:when test="@name = 22">Норильск</xsl:when>
																		<xsl:when test="@name = 24">Тверь</xsl:when>
																		<xsl:when test="@name = 25">Хабаровск</xsl:when>
																		<xsl:when test="@name = 26">Тюмень</xsl:when>
																		<xsl:when test="@name = 27">Сахалин</xsl:when>
																		<xsl:when test="@name = 28">Петрозаводск</xsl:when>
																		<xsl:when test="@name = 29">Воронеж</xsl:when>
																		<xsl:when test="@name = 30">Вологда</xsl:when>
																		<xsl:when test="@name = 31">Саранск</xsl:when>
																		<xsl:when test="@name = 32">Ижевск</xsl:when>
																		<xsl:when test="@name = 33">Нижнекамск</xsl:when>
																		<xsl:when test="@name = 34">Иркутск</xsl:when>
																		<xsl:when test="@name = 35">Саратов</xsl:when>
																		<xsl:when test="@name = 36">Кемерово</xsl:when>
																		<xsl:when test="@name = 37">Тольятти</xsl:when>
																		<xsl:when test="@name = 38">Владимир</xsl:when>
																		<xsl:when test="@name = 39">Красноярск</xsl:when>
																		<xsl:when test="@name = 40">Ставрополь</xsl:when>
																		<xsl:when test="@name = 41">Краснодар</xsl:when>
																		<xsl:when test="@name = 42">Казань</xsl:when>
																		<xsl:when test="@name = 43">Астрахань</xsl:when>
																		<xsl:when test="@name = 44">Таганрог</xsl:when>
																		<xsl:when test="@name = 45">Нижний Новгород</xsl:when>
																		<xsl:when test="@name = 46">Нижний Тагил</xsl:when>
																		<xsl:when test="@name = 47">Сочи</xsl:when>
																		<xsl:when test="@name = 48">Калуга</xsl:when>
																		<xsl:when test="@name = 49">Набережные Челны</xsl:when>
																		<xsl:when test="@name = 50">Назрань</xsl:when>
																		<xsl:when test="@name = 51">Барнаул</xsl:when>
																		<xsl:when test="@name = 52">Якутск</xsl:when>
																		<xsl:when test="@name = 53">Магадан</xsl:when>
																		<xsl:when test="@name = 54">Челябинск</xsl:when>
																		<xsl:when test="@name = 55">Сургут</xsl:when>
																		<xsl:when test="@name = 56">Волгоград</xsl:when>
																		<xsl:when test="@name = 57">Ханты-Мансийск</xsl:when>
																		<xsl:when test="@name = 58">Магнитогорск</xsl:when>
																		<xsl:when test="@name = 59">Майкоп</xsl:when>
																		<xsl:when test="@name = 60">Петропавловск-Камчатский</xsl:when>
																		<xsl:when test="@name = 61">Калининград</xsl:when>
																		<xsl:when test="@name = 62">Тула</xsl:when>
																		<xsl:when test="@name = 63">Находка</xsl:when>
																		<xsl:when test="@name = 64">Бийск</xsl:when>
																		<xsl:when test="@name = 65">Подольск</xsl:when>
																		<xsl:when test="@name = 66">Зеленоград</xsl:when>
																		<xsl:when test="@name = 67">Брянск</xsl:when>
																		<xsl:when test="@name = 68">Тамбов</xsl:when>
																		<xsl:when test="@name = 69">Рязань</xsl:when>
																		<xsl:when test="@name = 70">Смоленск</xsl:when>
																		<xsl:when test="@name = 71">Орел</xsl:when>
																		<xsl:when test="@name = 72">Липецк</xsl:when>
																		<xsl:when test="@name = 73">Белгород</xsl:when>
																		<xsl:when test="@name = 74">Курск</xsl:when>
																		<xsl:when test="@name = 75">Чебоксары</xsl:when>
																		<xsl:when test="@name = 76">Череповец</xsl:when>
																		<xsl:when test="@name = 77">Альметьевск</xsl:when>
																		<xsl:when test="@name = 78">Ессентуки</xsl:when>
																		<xsl:when test="@name = 79">Ульяновск</xsl:when>
																		<xsl:when test="@name = 80">Черноголовка</xsl:when>
																		<xsl:when test="@name = 81">Уфа</xsl:when>
																		<xsl:when test="@name = 82">Стерлитамак</xsl:when>
																		<xsl:when test="@name = 83">Салават</xsl:when>
																		<xsl:when test="@name = 84">Новый Уренгой</xsl:when>
																		<xsl:when test="@name = 85">Салехард</xsl:when>
																		<xsl:when test="@name = 86">Нефтеюганск</xsl:when>
																		<xsl:when test="@name = 87">Ноябрьск</xsl:when>
																		<xsl:when test="@name = 88">Первоуральск</xsl:when>
																		<xsl:when test="@name = 89">Снежинск</xsl:when>
																		<xsl:when test="@name = 90">Киров</xsl:when>
																		<xsl:when test="@name = 91">Пятигорск</xsl:when>
																		<xsl:when test="@name = 92">Когалым</xsl:when>
																		<xsl:when test="@name = 93">Мурманск</xsl:when>
																		<xsl:when test="@name = 94">Оренбург</xsl:when>
																		<xsl:when test="@name = 95">Сыктывкар</xsl:when>
																		<xsl:when test="@name = 97">Чита</xsl:when>
																		<xsl:when test="@name = 98">Нижневартовск</xsl:when>
																		<xsl:when test="@name = 99">Абакан</xsl:when>
																		<xsl:when test="@name = 100">Курган</xsl:when>
																		<xsl:when test="@name = 101">Орск</xsl:when>
																		<xsl:when test="@name = 102">Братск</xsl:when>
																		<xsl:when test="@name = 103">Улан-Удэ</xsl:when>
																		<xsl:when test="@name = 104">Уссурийск</xsl:when>
																		<xsl:when test="@name = 105">Биробиджан</xsl:when>
																		<xsl:when test="@name = 106">Лучегорск</xsl:when>
																		<xsl:when test="@name = 107">Белогорск</xsl:when>
																		<xsl:when test="@name = 108">Саяногорск</xsl:when>
																		<xsl:when test="@name = 109">Махачкала</xsl:when>
																		<xsl:when test="@name = 110">Сызрань</xsl:when>
																		<xsl:when test="@name = 111">Жуковский</xsl:when>
																		<xsl:when test="@name = 116">Нальчик</xsl:when>
																		<xsl:when test="@name = 117">Новокузнецк</xsl:when>
																		<xsl:when test="@name = 118">Великие Луки</xsl:when>
																		<xsl:when test="@name = 119">Южно-Сахалинск</xsl:when>
																		<xsl:when test="@name = 120">Владикавказ</xsl:when>
																		<xsl:when test="@name = 121">Элиста</xsl:when>
																		<xsl:when test="@name = 122">Новочеркасск</xsl:when>
																		<xsl:when test="@name = 123">Красноуральск</xsl:when>
																		<xsl:when test="@name = 124">Дальнегорск</xsl:when>
																		<xsl:when test="@name = 125">Мончегорск</xsl:when>
																		<xsl:when test="@name = 126">Красный Сулин</xsl:when>
																		<xsl:when test="@name = 127">Дубна</xsl:when>
																		<xsl:when test="@name = 128">Озерск</xsl:when>
																		<xsl:when test="@name = 129">Серпухов</xsl:when>
																		<xsl:when test="@name = 130">Благовещенск</xsl:when>
																		<xsl:when test="@name = 131">Кисловодск</xsl:when>
																		<xsl:when test="@name = 132">Кирово-Чепецк</xsl:when>
																		<xsl:when test="@name = 133">Новокуйбышевск</xsl:when>
																		<xsl:when test="@name = 134">Костомукша</xsl:when>
																		<xsl:when test="@name = 135">Волгодонск</xsl:when>
																		<xsl:when test="@name = 136">Балабаново</xsl:when>
																		<xsl:when test="@name = 137">Кушва</xsl:when>
																		<xsl:when test="@name = 138">Апатиты</xsl:when>
																		<xsl:when test="@name = 139">Чистополь</xsl:when>
																		<xsl:when test="@name = 140">Новодвинск</xsl:when>
																		<xsl:when test="@name = 141">Нягань</xsl:when>
																		<xsl:when test="@name = 142">Нижний Архыз</xsl:when>
																		<xsl:when test="@name = 143">Балашов</xsl:when>
																		<xsl:when test="@name = 144">Елизово</xsl:when>
																		<xsl:when test="@name = 145">Шахты</xsl:when>
																		<xsl:when test="@name = 146">Каменск</xsl:when>
																		<xsl:when test="@name = 147">Семикаракорск</xsl:when>
																		<xsl:when test="@name = 148">Димитровград</xsl:when>
																		<xsl:when test="@name = 149">Миасс</xsl:when>
																		<xsl:when test="@name = 150">Верхняя Пышма</xsl:when>
																		<xsl:when test="@name = 151">Серов</xsl:when>
																		<xsl:when test="@name = 152">Каменск-Уральский</xsl:when>
																		<xsl:when test="@name = 153">Богданович</xsl:when>
																		<xsl:when test="@name = 154">Междуреченск</xsl:when>
																		<xsl:when test="@name = 155">Сосновый Бор</xsl:when>
																		<xsl:when test="@name = 156">Минусинск</xsl:when>
																		<xsl:when test="@name = 157">Кызыл</xsl:when>
																		<xsl:when test="@name = 158">Воркута</xsl:when>
																		<xsl:when test="@name = 159">Шадринск</xsl:when>
																		<xsl:when test="@name = 160">Йошкар-Ола</xsl:when>
																		<xsl:when test="@name = 161">Среднеуральск</xsl:when>
																		<xsl:when test="@name = 162">Горно-Алтайск</xsl:when>
																		<xsl:when test="@name = 163">Муром</xsl:when>
																		<xsl:when test="@name = 164">Рубцовск</xsl:when>
																		<xsl:when test="@name = 165">Угорск</xsl:when>
																		<xsl:when test="@name = 166">Переславль-Залесский</xsl:when>
																		<xsl:when test="@name = 167">Ухта</xsl:when>
																		<xsl:when test="@name = 168">Кировск</xsl:when>
																		<xsl:when test="@name = 169">Ковдор</xsl:when>
																		<xsl:when test="@name = 170">Черкесск</xsl:when>
																		<xsl:when test="@name = 171">Энгельс</xsl:when>
																		<xsl:when test="@name = 172">Луга</xsl:when>
																		<xsl:when test="@name = 173">Печора</xsl:when>
																		<xsl:when test="@name = 174">Ковров</xsl:when>
																		<xsl:when test="@name = 176">Анжеро-Судженск</xsl:when>
																		<xsl:when test="@name = 177">Реж</xsl:when>
																		<xsl:when test="@name = 178">Новоуральск</xsl:when>
																		<xsl:when test="@name = 179">Воскресенск</xsl:when>
																		<xsl:when test="@name = 180">Рыбинск</xsl:when>
																		<xsl:when test="@name = 181">Дмитров</xsl:when>
																		<xsl:when test="@name = 182">Королев</xsl:when>
																		<xsl:when test="@name = 183">Троицк</xsl:when>
																		<xsl:when test="@name = 184">Пущино</xsl:when>
																		<xsl:when test="@name = 185">Юбилейный</xsl:when>
																		<xsl:when test="@name = 186">Одинцово</xsl:when>
																		<xsl:when test="@name = 187">Химки</xsl:when>
																		<xsl:when test="@name = 188">Реутов</xsl:when>
																		<xsl:when test="@name = 189">Люберцы</xsl:when>
																		<xsl:when test="@name = 190">Томилино</xsl:when>
																		<xsl:when test="@name = 191">Красногорск</xsl:when>
																		<xsl:when test="@name = 192">Пушкин</xsl:when>
																		<xsl:when test="@name = 193">Кронштадт</xsl:when>
																		<xsl:when test="@name = 194">Удомля</xsl:when>
																		<xsl:when test="@name = 195">Сергиев Посад</xsl:when>
																		<xsl:when test="@name = 196">Заринск</xsl:when>
																		<xsl:when test="@name = 197">Можайск</xsl:when>
																		<xsl:when test="@name = 198">Северск</xsl:when>
																		<xsl:when test="@name = 199">Тында</xsl:when>
																		<xsl:when test="@name = 200">Славянка</xsl:when>
																		<xsl:when test="@name = 201">Артем</xsl:when>
																		<xsl:when test="@name = 202">Мариинск</xsl:when>
																		<xsl:when test="@name = 203">Кандалакша</xsl:when>
																		<xsl:when test="@name = 204">Елабуга</xsl:when>
																		<xsl:when test="@name = 205">Асбест</xsl:when>
																		<xsl:when test="@name = 206">не определен</xsl:when>
																		<xsl:when test="@name = 207">Корсаков</xsl:when>
																		<xsl:when test="@name = 208">Нефтекамск</xsl:when>
																		<xsl:when test="@name = 209">Лобня</xsl:when>
																		<xsl:when test="@name = 210">Шатура</xsl:when>
																		<xsl:when test="@name = 212">Мыски</xsl:when>
																		<xsl:when test="@name = 213">Кумертау</xsl:when>
																		<xsl:when test="@name = 214">Фрязино</xsl:when>
																		<xsl:when test="@name = 215">Тайга</xsl:when>
																		<xsl:when test="@name = 216">Мирный</xsl:when>
																		<xsl:when test="@name = 217">Пушкино</xsl:when>
																		<xsl:when test="@name = 218">Усинск</xsl:when>
																		<xsl:when test="@name = 219">Дзержинск</xsl:when>
																		<xsl:when test="@name = 220">Заволжье</xsl:when>
																		<xsl:when test="@name = 221">Лыткарино</xsl:when>
																		<xsl:when test="@name = 222">Балашиха</xsl:when>
																		<xsl:when test="@name = 223">Мытищи</xsl:when>
																		<xsl:when test="@name = 225">Далматово</xsl:when>
																		<xsl:when test="@name = 226">Щербинка</xsl:when>
																		<xsl:when test="@name = 227">Истра</xsl:when>
																		<xsl:when test="@name = 229">Грозный</xsl:when>
																		<xsl:when test="@name = 230">Менделеевск</xsl:when>
																		<xsl:when test="@name = 231">Ангарск</xsl:when>
																		<xsl:when test="@name = 232">Россошь</xsl:when>
																		<xsl:when test="@name = 233">Анадырь</xsl:when>
																		<xsl:when test="@name = 234">Зима</xsl:when>
																		<xsl:when test="@name = 235">Беслан</xsl:when>
																		<xsl:when test="@name = 236">Минеральные Воды</xsl:when>
																		<xsl:when test="@name = 237">Раменское</xsl:when>
																		<xsl:when test="@name = 238">Пыть-Ях</xsl:when>
																		<xsl:when test="@name = 239">Железногорск(Курская обл.)</xsl:when>
																		<xsl:when test="@name = 240">Клин</xsl:when>
																		<xsl:when test="@name = 241">Долгопрудный</xsl:when>
																		<xsl:when test="@name = 242">Ейск</xsl:when>
																		<xsl:when test="@name = 243">Новошахтинск</xsl:when>
																		<xsl:when test="@name = 244">Березовский(Свердл. обл.)</xsl:when>
																		<xsl:when test="@name = 245">Туапсе</xsl:when>
																		<xsl:when test="@name = 246">Ногинск</xsl:when>
																		<xsl:when test="@name = 247">Старая Купавна</xsl:when>
																		<xsl:when test="@name = 248">Ревда</xsl:when>
																		<xsl:when test="@name = 249">Невинномысск</xsl:when>
																		<xsl:when test="@name = 250">Качканар</xsl:when>
																		<xsl:when test="@name = 251">Гатчина</xsl:when>
																		<xsl:when test="@name = 252">Электросталь</xsl:when>
																		<xsl:when test="@name = 253">Бронницы</xsl:when>
																		<xsl:when test="@name = 254">Соликамск</xsl:when>
																		<xsl:when test="@name = 255">Арсеньев</xsl:when>
																		<xsl:when test="@name = 256">Бор</xsl:when>
																		<xsl:when test="@name = 257">Наро-Фоминск</xsl:when>
																		<xsl:when test="@name = 258">Лысьва</xsl:when>
																		<xsl:when test="@name = 259">Кудымкар</xsl:when>
																		<xsl:when test="@name = 260">Выборг</xsl:when>
																		<xsl:when test="@name = 261">Березники</xsl:when>
																		<xsl:when test="@name = 262">Кстово</xsl:when>
																		<xsl:when test="@name = 263">Очер</xsl:when>
																		<xsl:when test="@name = 264">Арзамас</xsl:when>
																		<xsl:when test="@name = 265">Нарьян-Мар</xsl:when>
																		<xsl:when test="@name = 266">Аксай</xsl:when>
																		<xsl:when test="@name = 267">Домодедово</xsl:when>
																		<xsl:when test="@name = 268">Балаково</xsl:when>
																		<xsl:when test="@name = 269">Георгиевск</xsl:when>
																		<xsl:when test="@name = 270">Городец</xsl:when>
																		<xsl:when test="@name = 271">Железнодорожный</xsl:when>
																		<xsl:when test="@name = 272">Щелково</xsl:when>
																		<xsl:when test="@name = 273">Мосальск</xsl:when>
																		<xsl:when test="@name = 274">Вольск</xsl:when>
																		<xsl:when test="@name = 275">Павловский Посад</xsl:when>
																		<xsl:when test="@name = 276">Волоколамск</xsl:when>
																		<xsl:when test="@name = 277">Коломна</xsl:when>
																		<xsl:when test="@name = 278">Назарово</xsl:when>
																		<xsl:when test="@name = 279">Ачинск</xsl:when>
																		<xsl:when test="@name = 280">Богородск</xsl:when>
																		<xsl:when test="@name = 281">Белорецк</xsl:when>
																		<xsl:when test="@name = 282">Протвино</xsl:when>
																		<xsl:when test="@name = 283">Канск</xsl:when>
																		<xsl:when test="@name = 284">Гуково</xsl:when>
																		<xsl:when test="@name = 285">Лениногорск</xsl:when>
																		<xsl:when test="@name = 286">Краснотурьинск</xsl:when>
																		<xsl:when test="@name = 287">Краснокамск</xsl:when>
																		<xsl:when test="@name = 288">Мегион</xsl:when>
																		<xsl:when test="@name = 289">Краснообск</xsl:when>
																		<xsl:when test="@name = 290">Малаховка</xsl:when>
																		<xsl:when test="@name = 291">Михнево</xsl:when>
																		<xsl:when test="@name = 292">Давыдово</xsl:when>
																		<xsl:when test="@name = 293">Серебряные Пруды</xsl:when>
																		<xsl:when test="@name = 294">Луховицы</xsl:when>
																		<xsl:when test="@name = 295">Лесозаводск</xsl:when>
																		<xsl:when test="@name = 296">Навашино</xsl:when>
																		<xsl:when test="@name = 297">Сараны</xsl:when>
																		<xsl:when test="@name = 298">Котельники</xsl:when>
																		<xsl:when test="@name = 299">Кавалерово</xsl:when>
																		<xsl:when test="@name = 300">Тобольск</xsl:when>
																		<xsl:when test="@name = 301">Ишим</xsl:when>
																		<xsl:when test="@name = 302">Климовск</xsl:when>
																		<xsl:when test="@name = 303">Вольно-Hадеждинское</xsl:when>
																		<xsl:when test="@name = 304">Ольга</xsl:when>
																		<xsl:when test="@name = 305">Орехово-Зуево</xsl:when>
																		<xsl:when test="@name = 306">Карталы</xsl:when>
																		<xsl:when test="@name = 307">Аткарск</xsl:when>
																		<xsl:when test="@name = 308">Углич</xsl:when>
																		<xsl:when test="@name = 309">Красноуфимск</xsl:when>
																		<xsl:when test="@name = 310">Светогорск</xsl:when>
																		<xsl:when test="@name = 311">Борисоглебск</xsl:when>
																		<xsl:when test="@name = 312">Бердск</xsl:when>
																		<xsl:when test="@name = 313">Болотное</xsl:when>
																		<xsl:when test="@name = 314">Маслянино</xsl:when>
																		<xsl:when test="@name = 315">Копейск</xsl:when>
																		<xsl:when test="@name = 316">Дзержинский</xsl:when>
																		<xsl:when test="@name = 317">Солнечногорск</xsl:when>
																		<xsl:when test="@name = 318">Монино</xsl:when>
																		<xsl:when test="@name = 319">Менделеево</xsl:when>
																		<xsl:when test="@name = 320">Дедовск</xsl:when>
																		<xsl:when test="@name = 321">Железногорск(Красноярск-26)</xsl:when>
																		<xsl:when test="@name = 322">Тахтамукай</xsl:when>
																		<xsl:when test="@name = 323">Дальнереченск</xsl:when>
																		<xsl:when test="@name = 324">Руза</xsl:when>
																		<xsl:when test="@name = 325">Буденновск</xsl:when>
																		<xsl:otherwise>
																			<span class="bold">Город не определен</span>
																			<div class="xs">В нашей базе данных присутствуют наиболее крупные города и населенные пункты, но в некоторых случаях посетители заходят на сайт и из других мест, которые мы не можем точно определить. Такие посетители сгруппированы в этой строке.</div>
																		</xsl:otherwise>
																	</xsl:choose>
																</xsl:when>
															</xsl:choose>
														</div>
													</td>
													<td align="right" valign="top">
														<span class="normal s bold">
															<xsl:value-of select="format-number(@hosts, '### ### ##0', 'ru')" />
														</span>
														<span class="normal s">
															<xsl:text> (</xsl:text>
															<xsl:value-of select="format-number((@hosts div $Total-Hosts) * 100, '#0,00', 'ru')" />
															<xsl:text>%)</xsl:text>
														</span>
													</td>
													<td align="right" valign="top">
														<span class="normal s bold">
															<xsl:value-of select="format-number(@hits, '### ### ##0', 'ru')" />
														</span>
														<span class="normal s">
															<xsl:text> (</xsl:text>
															<xsl:value-of select="format-number((@hits div $Total-Hits) * 100, '#0,00', 'ru')" />
															<xsl:text>%)</xsl:text>
														</span>
													</td>
												</tr>
												<tr height="1" bgcolor="#EEEEEE">
													<td colspan="5"></td>
												</tr>
											</xsl:for-each>
											<xsl:choose>
												<xsl:when test="$F = 0">
													<tr bgcolor="#FFFFFF">
														<td></td>
														<td colspan="3">
															<table cellpadding="0" cellspacing="0" width="100%">
																<col width="17" />
																<col width="5" />
																<col width="*" />
																<tr>
																	<td>
																		<a class="normal s">
																			<xsl:attribute name="href">
																				<xsl:text>?</xsl:text>
																				<xsl:value-of select="concat('idn', '=', $User_ID)" />
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('date', '=', $Date)" />
																				<xsl:if test="$Q != 0">
																					<xsl:text>&amp;</xsl:text>
																					<xsl:value-of select="concat('q', '=', $Q)" />
																				</xsl:if>
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('report', '=', $Report)" />
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('f', '=', 1)" />
																				<xsl:text>#1</xsl:text>
																			</xsl:attribute>
																			<img src="/Common Files/down-down.gif" alt="" width="17" height="17" border="0" />
																		</a>
																	</td>
																	<td></td>
																	<td>
																		<a class="normal s">
																			<xsl:attribute name="href">
																				<xsl:text>?</xsl:text>
																				<xsl:value-of select="concat('idn', '=', $User_ID)" />
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('date', '=', $Date)" />
																				<xsl:if test="$Q != 0">
																					<xsl:text>&amp;</xsl:text>
																					<xsl:value-of select="concat('q', '=', $Q)" />
																				</xsl:if>
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('report', '=', $Report)" />
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('f', '=', 1)" />
																				<xsl:text>#1</xsl:text>
																			</xsl:attribute>
																			<xsl:text>Отобразить весь список</xsl:text>
																		</a>
																	</td>
																</tr>
															</table>
														</td>
														<td></td>
													</tr>
												</xsl:when>
												<xsl:otherwise>
													<tr bgcolor="#FFFFFF">
														<td></td>
														<td colspan="3">
															<table cellpadding="0" cellspacing="0" width="100%">
																<col width="17" />
																<col width="5" />
																<col width="*" />
																<tr>
																	<td>
																		<a class="normal s">
																			<xsl:attribute name="href">
																				<xsl:text>?</xsl:text>
																				<xsl:value-of select="concat('idn', '=', $User_ID)" />
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('date', '=', $Date)" />
																				<xsl:if test="$Q != 0">
																					<xsl:text>&amp;</xsl:text>
																					<xsl:value-of select="concat('q', '=', $Q)" />
																				</xsl:if>
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('report', '=', $Report)" />
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('f', '=', 0)" />
																				<xsl:text>#1</xsl:text>
																			</xsl:attribute>
																			<img src="/Common Files/up-up.gif" alt="" width="17" height="17" border="0" />
																		</a>
																	</td>
																	<td></td>
																	<td>
																		<a class="normal s">
																			<xsl:attribute name="href">
																				<xsl:text>?</xsl:text>
																				<xsl:value-of select="concat('idn', '=', $User_ID)" />
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('date', '=', $Date)" />
																				<xsl:if test="$Q != 0">
																					<xsl:text>&amp;</xsl:text>
																					<xsl:value-of select="concat('q', '=', $Q)" />
																				</xsl:if>
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('report', '=', $Report)" />
																				<xsl:text>&amp;</xsl:text>
																				<xsl:value-of select="concat('f', '=', 0)" />
																				<xsl:text>#1</xsl:text>
																			</xsl:attribute>
																			<xsl:text>Отобразить первые пять строк</xsl:text>
																		</a>
																	</td>
																</tr>
															</table>
														</td>
														<td></td>
													</tr>
												</xsl:otherwise>
											</xsl:choose>
										</table>
									</td>
									<td></td>
								</tr>
							</table>
							<!-- /Таблица -->
						</xsl:when>
						<xsl:otherwise>
							<div class="normal bold">Нет данных за выбраный день.</div>
						</xsl:otherwise>
					</xsl:choose>
				</td>
			</tr>
			<tr height="20">
				<td></td>
			</tr>
			<tr>
				<td>
					<!-- Навигация по отчетам -->
					<table cellpadding="5" cellspacing="0" width="100%">
						<col width="50" />
						<col width="*" />
						<col width="50" />
						<tr>
							<td></td>
							<td align="center">
								<table cellpadding="0" cellspacing="0" width="90%">
									<col width="25%" />
									<col width="25%" />
									<col width="25%" />
									<col width="25%" />
									<tr>
										<td>
											<div class="normal s red bold pad5">Посещаемость</div>
										</td>
										<td>
											<div class="normal s red bold pad5">Рефереры</div>
										</td>
										<td>
											<div class="normal s red bold pad5">Посетители</div>
										</td>
										<td>
											<div class="normal s red bold pad5">География</div>
										</td>
									</tr>
									<tr>
										<td valign="top">
											<table cellpadding="1" cellspacing="0" width="100%">
												<tr>
													<td>
														<a class="normal s pad5">
															<xsl:attribute name="href">
																<xsl:text>/h.asp?</xsl:text>
																<xsl:value-of select="concat('idn', '=', $User_ID)" />
																<xsl:text>&amp;</xsl:text>
																<xsl:value-of select="concat('date', '=', $Date)" />
																<xsl:text>#1</xsl:text>
															</xsl:attribute>
															<xsl:text>Динамика посещений</xsl:text>
														</a>
													</td>
												</tr>
												<tr>
													<td>
														<a class="normal s pad5">
															<xsl:attribute name="href">
																<xsl:text>/bm.asp?</xsl:text>
																<xsl:value-of select="concat('idn', '=', $User_ID)" />
																<xsl:text>&amp;</xsl:text>
																<xsl:value-of select="concat('date', '=', $Date)" />
																<xsl:text>#1</xsl:text>
															</xsl:attribute>
															<xsl:text>Ядро аудитории</xsl:text>
														</a>
													</td>
												</tr>
												<tr>
													<td>
														<a class="normal s pad5">
															<xsl:attribute name="href">
																<xsl:text>/pg-x.asp?</xsl:text>
																<xsl:value-of select="concat('idn', '=', $User_ID)" />
																<xsl:text>&amp;</xsl:text>
																<xsl:value-of select="concat('date', '=', $Date)" />
																<xsl:text>#1</xsl:text>
															</xsl:attribute>
															<xsl:text>Популярность страниц</xsl:text>
														</a>
													</td>
												</tr>
											</table>
										</td>
										<td valign="top">
											<table cellpadding="1" cellspacing="0" width="100%">
												<tr>
													<td>
														<a class="normal s pad5">
															<xsl:attribute name="href">
																<xsl:text>/ref-x.asp?</xsl:text>
																<xsl:value-of select="concat('idn', '=', $User_ID)" />
																<xsl:text>&amp;</xsl:text>
																<xsl:value-of select="concat('date', '=', $Date)" />
																<xsl:text>#1</xsl:text>
															</xsl:attribute>
															<xsl:text>Все рефереры</xsl:text>
														</a>
													</td>
												</tr>
												<tr>
													<td>
														<a class="normal s pad5">
															<xsl:attribute name="href">
																<xsl:text>/transfer-x.asp?</xsl:text>
																<xsl:value-of select="concat('idn', '=', $User_ID)" />
																<xsl:text>&amp;</xsl:text>
																<xsl:value-of select="concat('date', '=', $Date)" />
																<xsl:text>#1</xsl:text>
															</xsl:attribute>
															<xsl:text>Поисковые системы</xsl:text>
														</a>
													</td>
												</tr>
												<tr>
													<td>
														<a class="normal s pad5">
															<xsl:attribute name="href">
																<xsl:text>/words-x.asp?</xsl:text>
																<xsl:value-of select="concat('idn', '=', $User_ID)" />
																<xsl:text>&amp;</xsl:text>
																<xsl:value-of select="concat('date', '=', $Date)" />
																<xsl:text>#1</xsl:text>
															</xsl:attribute>
															<xsl:text>Поисковые фразы</xsl:text>
														</a>
													</td>
												</tr>
												<tr>
													<td>
														<a class="normal s pad5">
															<xsl:attribute name="href">
																<xsl:text>/mail.asp?</xsl:text>
																<xsl:value-of select="concat('idn', '=', $User_ID)" />
																<xsl:text>&amp;</xsl:text>
																<xsl:value-of select="concat('date', '=', $Date)" />
																<xsl:text>#1</xsl:text>
															</xsl:attribute>
															<xsl:text>Почтовые системы</xsl:text>
														</a>
													</td>
												</tr>
											</table>
										</td>
										<td valign="top">
											<table cellpadding="1" cellspacing="0" width="100%">
												<tr>
													<td>
														<xsl:choose>
															<xsl:when test="$Report = 5">
																<div class="normal s bold white selected pad5">Операционные системы</div>
															</xsl:when>
															<xsl:otherwise>
																<a class="normal s pad5">
																	<xsl:attribute name="href">
																		<xsl:text>/platform.asp?</xsl:text>
																		<xsl:value-of select="concat('idn', '=', $User_ID)" />
																		<xsl:text>&amp;</xsl:text>
																		<xsl:value-of select="concat('date', '=', $Date)" />
																		<xsl:text>#1</xsl:text>
																	</xsl:attribute>
																	<xsl:text>Операционные системы</xsl:text>
																</a>
															</xsl:otherwise>
														</xsl:choose>
													</td>
												</tr>
												<tr>
													<td>
														<xsl:choose>
															<xsl:when test="$Report = 4">
																<div class="normal s bold white selected pad5">Браузеры</div>
															</xsl:when>
															<xsl:otherwise>
																<a class="normal s pad5">
																	<xsl:attribute name="href">
																		<xsl:text>/browser.asp?</xsl:text>
																		<xsl:value-of select="concat('idn', '=', $User_ID)" />
																		<xsl:text>&amp;</xsl:text>
																		<xsl:value-of select="concat('date', '=', $Date)" />
																		<xsl:text>#1</xsl:text>
																	</xsl:attribute>
																	<xsl:text>Браузеры</xsl:text>
																</a>
															</xsl:otherwise>
														</xsl:choose>
													</td>
												</tr>
											</table>
										</td>
										<td valign="top">
											<table cellpadding="1" cellspacing="0" width="100%">
												<tr>
													<td>
														<xsl:choose>
															<xsl:when test="$Report = 6">
																<div class="normal s bold white selected pad5">Страны</div>
															</xsl:when>
															<xsl:otherwise>
																<a class="normal s pad5">
																	<xsl:attribute name="href">
																		<xsl:text>/country.asp?</xsl:text>
																		<xsl:value-of select="concat('idn', '=', $User_ID)" />
																		<xsl:text>&amp;</xsl:text>
																		<xsl:value-of select="concat('date', '=', $Date)" />
																		<xsl:text>#1</xsl:text>
																	</xsl:attribute>
																	<xsl:text>Страны</xsl:text>
																</a>
															</xsl:otherwise>
														</xsl:choose>
													</td>
												</tr>
												<tr>
													<td>
														<xsl:choose>
															<xsl:when test="$Report = 7">
																<div class="normal s bold white selected pad5">Города</div>
															</xsl:when>
															<xsl:otherwise>
																<a class="normal s pad5">
																	<xsl:attribute name="href">
																		<xsl:text>/city.asp?</xsl:text>
																		<xsl:value-of select="concat('idn', '=', $User_ID)" />
																		<xsl:text>&amp;</xsl:text>
																		<xsl:value-of select="concat('date', '=', $Date)" />
																		<xsl:text>#1</xsl:text>
																	</xsl:attribute>
																	<xsl:text>Города</xsl:text>
																</a>
															</xsl:otherwise>
														</xsl:choose>
													</td>
												</tr>
											</table>
										</td>
									</tr>
								</table>
							</td>
							<td></td>
						</tr>
					</table>
					<!-- /Навигация по отчетам -->
				</td>
			</tr>
			<tr height="100%">
				<td></td>
			</tr>
		</table>
	</xsl:template>
		
	<xsl:template match="Root" mode="Date">
		<xsl:variable name="Date" select="@Date" />
		<xsl:variable name="Now" select="@Now" />
		<xsl:variable name="User_ID" select="@User_ID" />
		<xsl:variable name="Count" select="30 + number(substring($Date, 6, 2) - number(boolean(number(substring($Date, 6, 2)) > 7))) mod 2 - number(boolean(number(substring($Date, 6, 2)) = 2)) * 2 + number(boolean(number(substring($Date, 6, 2)) = 2)) * (number(not(boolean(substring($Date, 1, 4) mod 4))) * 2)" />
		<xsl:variable name="Q" select="@Q" />
		<xsl:variable name="Columns">
			<Column>1</Column>
			<Column>2</Column>
			<Column>3</Column>
			<Column>4</Column>
			<Column>5</Column>
			<Column>6</Column>
			<Column>7</Column>
			<Column>8</Column>
			<Column>9</Column>
			<Column>10</Column>
			<Column>11</Column>
			<Column>12</Column>
			<Column>13</Column>
			<Column>14</Column>
			<Column>15</Column>
			<Column>16</Column>
			<Column>17</Column>
			<Column>18</Column>
			<Column>19</Column>
			<Column>20</Column>
			<Column>21</Column>
			<Column>22</Column>
			<Column>23</Column>
			<Column>24</Column>
			<Column>25</Column>
			<Column>26</Column>
			<Column>27</Column>
			<Column>28</Column>
			<xsl:if test="$Count >= 29">
				<Column>29</Column>
			</xsl:if>
			<xsl:if test="$Count >= 30">
				<Column>30</Column>
			</xsl:if>
			<xsl:if test="$Count >= 31">
				<Column>31</Column>
			</xsl:if>
		</xsl:variable>
		<div class="nowrap">
			<span class="normal s bold pad5 100px">Число:</span>
			<xsl:for-each select="msxsl:node-set($Columns)/Column">
				<xsl:variable name="Column" select="text()" />
				<xsl:choose>
					<xsl:when test="number(substring($Date, 9, 2)) = $Column">
						<span class="normal s bold white selected pad5">
							<xsl:value-of select="$Column" />
						</span>
					</xsl:when>
					<xsl:when test="concat(substring($Date, 1, 4), substring($Date, 6, 2), format-number($Column, '00')) > translate(substring($Now, 1, 10), '-', '') or translate(VBScript:DateAdd2('m', -2, substring($Now, 1, 10)), '-', '') > concat(substring($Date, 1, 4), substring($Date, 6, 2), format-number($Column, '00'))">
						<span class="normal s disabled pad5">
							<xsl:value-of select="$Column" />
						</span>
					</xsl:when>
					<xsl:otherwise>
						<a class="normal s pad5">
							<xsl:attribute name="href">
								<xsl:text>?</xsl:text>
								<xsl:value-of select="concat('idn', '=', $User_ID)" />
								<xsl:text>&amp;</xsl:text>
								<xsl:value-of select="concat('date', '=', substring($Date, 1, 4), '-', substring($Date, 6, 2), '-', format-number($Column, '00'))" />
								<xsl:if test="$Q != 0">
									<xsl:text>&amp;</xsl:text>
									<xsl:value-of select="concat('q', '=', $Q)" />
								</xsl:if>
								<xsl:text>#1</xsl:text>
							</xsl:attribute>
							<xsl:value-of select="$Column" />
						</a>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
		</div>
	</xsl:template>

	<xsl:template match="Root" mode="Month">
		<xsl:variable name="Year" select="substring(@Date, 1, 4)" />
		<xsl:variable name="Month" select="substring(@Date, 6, 2)" />
		<xsl:variable name="Date" select="@Date" />
		<xsl:variable name="Now" select="@Now" />
		<xsl:variable name="User_ID" select="@User_ID" />
		<xsl:variable name="Q" select="@Q" />
		<xsl:variable name="Columns">
			<Column>1</Column>
			<Column>2</Column>
			<Column>3</Column>
			<Column>4</Column>
			<Column>5</Column>
			<Column>6</Column>
			<Column>7</Column>
			<Column>8</Column>
			<Column>9</Column>
			<Column>10</Column>
			<Column>11</Column>
			<Column>12</Column>
		</xsl:variable>
		<div class="nowrap">
			<span class="normal s bold pad5 100px">Месяц:</span>
			<xsl:for-each select="msxsl:node-set($Columns)/Column">
				<xsl:variable name="Column" select="text()" />
				<xsl:choose>
					<xsl:when test="number(substring($Date, 6, 2)) = $Column">
						<span class="normal s bold white selected pad5">
							<xsl:choose>
								<xsl:when test="$Column = 1">Январь</xsl:when>
								<xsl:when test="$Column = 2">Февраль</xsl:when>
								<xsl:when test="$Column = 3">Март</xsl:when>
								<xsl:when test="$Column = 4">Апрель</xsl:when>
								<xsl:when test="$Column = 5">Май</xsl:when>
								<xsl:when test="$Column = 6">Июнь</xsl:when>
								<xsl:when test="$Column = 7">Июль</xsl:when>
								<xsl:when test="$Column = 8">Август</xsl:when>
								<xsl:when test="$Column = 9">Сентябрь</xsl:when>
								<xsl:when test="$Column = 10">Октябрь</xsl:when>
								<xsl:when test="$Column = 11">Ноябрь</xsl:when>
								<xsl:when test="$Column = 12">Декабрь</xsl:when>
							</xsl:choose>
						</span>
					</xsl:when>
					<xsl:when test="concat(substring($Date, 1, 4), format-number($Column, '00')) > concat(substring($Now, 1, 4), substring($Now, 6, 2)) or concat(substring($Now, 1, 4), substring($Now, 6, 2)) - 2 > concat(substring($Date, 1, 4), format-number($Column, '00'))">
						<span class="normal s disabled pad5">
							<xsl:choose>
								<xsl:when test="$Column = 1">Январь</xsl:when>
								<xsl:when test="$Column = 2">Февраль</xsl:when>
								<xsl:when test="$Column = 3">Март</xsl:when>
								<xsl:when test="$Column = 4">Апрель</xsl:when>
								<xsl:when test="$Column = 5">Май</xsl:when>
								<xsl:when test="$Column = 6">Июнь</xsl:when>
								<xsl:when test="$Column = 7">Июль</xsl:when>
								<xsl:when test="$Column = 8">Август</xsl:when>
								<xsl:when test="$Column = 9">Сентябрь</xsl:when>
								<xsl:when test="$Column = 10">Октябрь</xsl:when>
								<xsl:when test="$Column = 11">Ноябрь</xsl:when>
								<xsl:when test="$Column = 12">Декабрь</xsl:when>
							</xsl:choose>
						</span>
					</xsl:when>
					<xsl:otherwise>
						<a class="normal s pad5">
							<xsl:attribute name="href">
								<xsl:text>?</xsl:text>
								<xsl:value-of select="concat('idn', '=', $User_ID)" />
								<xsl:text>&amp;</xsl:text>
								<xsl:value-of select="concat('date', '=', substring($Date, 1, 4), '-', format-number($Column, '00'), '-', substring($Date, 9, 2))" />
								<xsl:if test="$Q != 0">
									<xsl:text>&amp;</xsl:text>
									<xsl:value-of select="concat('q', '=', $Q)" />
								</xsl:if>
								<xsl:text>#1</xsl:text>
							</xsl:attribute>
							<xsl:choose>
								<xsl:when test="$Column = 1">Январь</xsl:when>
								<xsl:when test="$Column = 2">Февраль</xsl:when>
								<xsl:when test="$Column = 3">Март</xsl:when>
								<xsl:when test="$Column = 4">Апрель</xsl:when>
								<xsl:when test="$Column = 5">Май</xsl:when>
								<xsl:when test="$Column = 6">Июнь</xsl:when>
								<xsl:when test="$Column = 7">Июль</xsl:when>
								<xsl:when test="$Column = 8">Август</xsl:when>
								<xsl:when test="$Column = 9">Сентябрь</xsl:when>
								<xsl:when test="$Column = 10">Октябрь</xsl:when>
								<xsl:when test="$Column = 11">Ноябрь</xsl:when>
								<xsl:when test="$Column = 12">Декабрь</xsl:when>
							</xsl:choose>
						</a>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
		</div>
	</xsl:template>

	<xsl:template match="Root" mode="Year">
		<xsl:variable name="Date" select="@Date" />
		<xsl:variable name="User_ID" select="@User_ID" />
		<div class="nowrap">
			<span class="normal s bold pad5 100px">Год:</span>
			<xsl:choose>
				<xsl:when test="substring($Date, 1, 4) = 2006">
					<span class="normal s bold white pad5">2005</span>
				</xsl:when>
				<xsl:otherwise>
					<span class="normal s disabled pad5">2005</span>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:choose>
				<xsl:when test="substring($Date, 1, 4) = 2006">
					<span class="normal s bold white pad5">2006</span>
				</xsl:when>
				<xsl:otherwise>
					<span class="normal s disabled pad5">2006</span>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:choose>
				<xsl:when test="substring($Date, 1, 4) = 2007">
					<span class="normal s bold white pad5 selected">2007</span>
				</xsl:when>
				<xsl:otherwise>
					<a class="normal s">
						<xsl:attribute name="href">
							<xsl:text>?</xsl:text>
							<xsl:value-of select="concat('idn', '=', $User_ID)" />
							<xsl:text>&amp;</xsl:text>
							<xsl:value-of select="concat('date', '=', '2007-', substring($Date, 6))" />
							<xsl:text>#1</xsl:text>
						</xsl:attribute>
						<xsl:text>2007</xsl:text>
					</a>
				</xsl:otherwise>
			</xsl:choose>
		</div>
	</xsl:template>
	
	<xsl:template match="Root" mode="Period">
		<xsl:variable name="Year" select="substring(@Date, 1, 4)" />
		<xsl:variable name="Month" select="substring(@Date, 6, 2)" />
		<xsl:variable name="D" select="@D" />
		<xsl:variable name="Q" select="@Q" />
		<xsl:variable name="Date" select="@Date" />
		<xsl:variable name="User_ID" select="@User_ID" />
		<div class="nowrap">
			<span class="normal s bold 100px pad5">Период:</span>
			<span class="normal s disabled pad5">По часам</span>
			<span class="normal s bold white selected pad5">По дням</span>
			<span class="normal s disabled pad5">По неделям</span>
			<span class="normal s disabled pad5">По месяцам</span>
		</div>
	</xsl:template>
</xsl:stylesheet>