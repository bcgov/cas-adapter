
namespace Client;

public class CasService(ICasHttpClient _httpClient, Model.Settings.Client _settings, ILogger<CasHttpClient> _logger) : ICasService
{
    private string _invoiceBaseUrl => $"{_settings.BaseUrl}/cfs/apinvoice/";
    private string _supplierBaseUrl => $"{_settings.BaseUrl}/cfs/supplier/";
    private string _supplierSearchBaseUrl => $"{_settings.BaseUrl}/cfs/suppliersearch/";

    public async Task<Response> CreateInvoice(Invoice invoice)
    {
        if (invoice == null)
        {
            return new Response("Invoice data is required.", HttpStatusCode.BadRequest);
        }

        try
        {
            var jsonString = invoice.ToJSONString();
            return await _httpClient.Post(_invoiceBaseUrl, jsonString);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error generating invoice: {invoice.InvoiceNumber}.");
            dynamic errorObject = new JObject();
            errorObject.invoice_number = invoice.InvoiceNumber;
            errorObject.CAS_Returned_Messages = "Internal Error: " + e.Message;
            return new(JsonConvert.SerializeObject(errorObject), HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response> GetInvoice(string invoiceNumber, string supplierNumber, string supplierSiteCode)
    {
        if (string.IsNullOrEmpty(invoiceNumber) || string.IsNullOrEmpty(supplierNumber) || string.IsNullOrEmpty(supplierSiteCode))
        {
            return new Response("Invoice number, supplier number, and supplier site code are required.", HttpStatusCode.BadRequest);
        }

        try
        {
            var url = $"{_invoiceBaseUrl}{invoiceNumber}/{supplierNumber}/{supplierSiteCode}";
            return await _httpClient.Get(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error searching Invoice: {invoiceNumber}, Supplier Number: {supplierNumber}, Supplier Site Code: {supplierSiteCode}.");
            return new Response(e.Message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response> GetPayment(string paymentNumber, string payGroup)
    {
        paymentNumber.ThrowIfNullOrEmpty();
        payGroup.ThrowIfNullOrEmpty();

        try
        {
            var url = $"{_invoiceBaseUrl}payment/{paymentNumber}/{payGroup}";
            return await _httpClient.Get(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error searching for payment, Payment Number: {paymentNumber}, Pay Group: {payGroup}.");
            return new Response(e.Message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response> GetSupplierByNumber(string supplierNumber)
    {
        if (string.IsNullOrEmpty(supplierNumber))
        {
            return new Response("Supplier Number is required.", HttpStatusCode.BadRequest);
        }

        try
        {
            var url = $"{_supplierBaseUrl}{supplierNumber}";
            return await _httpClient.Get(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error searching Supplier By Number and Site Code, Supplier Number : {supplierNumber}.");
            return new("Internal Error: " + e.Message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response> GetSupplierByNumberAndSiteCode(string supplierNumber, string supplierSiteCode)
    {
        if (string.IsNullOrEmpty(supplierNumber) || string.IsNullOrEmpty(supplierSiteCode))
        {
            return new Response("Supplier Number and Supplier Site Code are required.", HttpStatusCode.BadRequest);
        }

        try
        {
            var url = $"{_supplierBaseUrl}{supplierNumber}/site/{supplierSiteCode}";
            return await _httpClient.Get(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error searching Supplier By Number and Site Code, Supplier Number : {supplierNumber}, Supplier Site Code: {supplierSiteCode}.");
            return new("Internal Error: " + e.Message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response> FindSupplierByName(string supplierName)
    {
        if (string.IsNullOrEmpty(supplierName))
        {
            return new Response("Supplier Name is required.", HttpStatusCode.BadRequest);
        }

        if (supplierName.Length < 4)
        {
            return new Response("Supplier Name must be at least 4 characters long.", HttpStatusCode.BadRequest);
        }

        try
        {

            var url = $"{_supplierSearchBaseUrl}{supplierName}";
            return await _httpClient.Get(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error searching Supplier By Name, Supplier Name: {supplierName}.");
            return new("Internal Error: " + e.Message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response> GetSupplierByLastNameAndSin(string lastName, string sin)
    {
        if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(sin))
        {
            return new Response("Last Name and SIN are required.", HttpStatusCode.BadRequest);
        }

        try
        {

            var url = $"{_supplierBaseUrl}{lastName}/lastname/{sin}/sin";
            return await _httpClient.Get(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error searching Supplier By Last Name and SIN, Last Name: {lastName}, and SIN was provided.");
            return new("Internal Error: " + e.Message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response> GetSupplierByBusinessNumber(string businessNumber)
    {
        if (string.IsNullOrEmpty(businessNumber))
        {
            return new Response("Business Number is required.", HttpStatusCode.BadRequest);
        }

        try
        {
            var url = $"{_supplierBaseUrl}{businessNumber}/businessnumber";
            return await _httpClient.Get(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error searching Supplier By Business Number, Business Number: {businessNumber}.");
            return new("Internal Error: " + e.Message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Response> GetSupplierByNameAndPostalCode(string supplierName, string postalCode)
    {
        if (string.IsNullOrEmpty(supplierName) || string.IsNullOrEmpty(postalCode))
        {
            return new Response("Supplier Name and Postal Code are required.", HttpStatusCode.BadRequest);
        }

        try
        {
            var url = $"{_settings.BaseUrl}/cfs/supplierbyname/{supplierName}/{postalCode}";
            return await _httpClient.Get(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error searching Supplier By Supplier Name and Postal Code, Supplier Name: {supplierName}, and Postal Code: {postalCode}.");
            return new("Internal Error: " + e.Message, HttpStatusCode.InternalServerError);
        }
    }
}

public record Response(string Content, HttpStatusCode StatusCode);