namespace NSD.WSLouch.Internals
{
	/// <summary>
	///     Построитель SOAP-запросов
	/// </summary>
	internal static class RequestBuilder
	{
		/// <summary>
		///     Построить заголовок SOAP-запроса
		/// </summary>
		/// <param name="model">
		///     Модель заголовка SOAP-запроса
		/// </param>
		/// <returns>
		///     Заголовок SOAP-запроса
		/// </returns>
		public static string Build(HeaderModel model)
		{
			return string.Format(
				@"<wsse:Security xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"">
	<ds:Signature xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
		<ds:SignedInfo xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
			<ds:CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/>
			<ds:SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/>
			<ds:Reference URI=""#{0}"">
				<ds:Transforms>
					<ds:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/>
				</ds:Transforms>
				<ds:DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/>
				<ds:DigestValue>{1}</ds:DigestValue>
			</ds:Reference>
		</ds:SignedInfo>
		<ds:SignatureValue></ds:SignatureValue>
	</ds:Signature>
</wsse:Security>", model.Uri, model.DigestValue);
		}

		/// <summary>
		///     Построить конверт SOAP-запроса
		/// </summary>
		/// <param name="model">
		///     Модель конверта SOAP-запроса
		/// </param>
		/// <returns>
		///     Конверт SOAP-запроса
		/// </returns>
		public static string Build(EnvelopeModel model)
		{
			return string.Format(
				@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<SOAP-ENV:Header>
	</SOAP-ENV:Header>
	<SOAP-ENV:Body wsu:Id=""{0}"" xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
		<{1} xmlns=""http://wslouch.micex.com/"">
		</{1}>
	</SOAP-ENV:Body>
</SOAP-ENV:Envelope>", model.RequestId, model.MethodName);
		}

		/// <summary>
		///     Построить SOAP-запрос с вложением
		/// </summary>
		/// <param name="model">
		///     MIME-модель
		/// </param>
		/// <returns>
		///     SOAP-запрос с вложением
		/// </returns>
		public static string Build(MimeModel model)
		{
			return string.Format(
				@"--{0}
Content-Type: text/xml; charset=utf-8
SOAPAction: """"
Content-Length: {1}

{2}


--{0}
Content-Type: application/binary
Content-Id: <{3}>
Content-Length: {4}

", model.Boundary, model.ContentLength1, model.Message, model.DataId, model.ContentLength2);
		}
	}
}
