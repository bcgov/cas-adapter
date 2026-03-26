namespace Client;

public class CasService(ICasHttpClient _httpClient, IAppSettings appSettings, ILogger<CasHttpClient> _logger) : ICasService
{
    private string _invoiceBaseUrl => $"{appSettings.Client.BaseUrl}/cfs/apinvoice/";
    private string _supplierBaseUrl => $"{appSettings.Client.BaseUrl}/cfs/supplier/";
    private string _supplierSearchBaseUrl => $"{appSettings.Client.BaseUrl}/cfs/suppliersearch/";

    public async Task<Response> CreateInvoice(Invoice invoice)
    {
        if (invoice == null)
        {
            return new Response("Invoice data is required.", HttpStatusCode.BadRequest);
        }

        try
        {
            var jsonString = invoice.ToJSONString();
            return await _httpClient.Post(_invoiceBaseUrl, jsonString, true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error generating invoice: {invoice.InvoiceNumber}.");
            dynamic errorObject = new JObject();
            errorObject.invoice_number = invoice.InvoiceNumber;
            if (!appSettings.IsProduction)
            {
            errorObject.CAS_Returned_Messages = "Internal Error: " + e.Message;
            }
            else
            {
                errorObject.CAS_Returned_Messages = "Internal Error, invoice was not generated.";
            }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching invoice.");
            if (appSettings.IsProduction)
            {
                return new Response("Internal Error, could not retrieve invoice.", HttpStatusCode.InternalServerError);
        }
            else
            {
                return new Response("Internal Error: " + ex.Message, HttpStatusCode.InternalServerError);
    }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching for payment.");
            if (appSettings.IsProduction)
            {
                return new Response("Internal Error, could not retrieve payment.", HttpStatusCode.InternalServerError);
        }
            else
            {
                return new Response("Internal Error: " + ex.Message, HttpStatusCode.InternalServerError);
    }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching Supplier By Number.");
            if (appSettings.IsProduction) 
            {
                return new Response("Internal Error, could not retrieve supplier.", HttpStatusCode.InternalServerError);
        }
            else
            {
                return new Response("Internal Error: " + ex.Message, HttpStatusCode.InternalServerError);
    }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching Supplier By Number and Site Code.");
            if (appSettings.IsProduction)
            {
                return new Response("Internal Error, could not retrieve supplier by supplier number and site code.", HttpStatusCode.InternalServerError);
        }
            else
            {
                return new Response("Internal Error: " + ex.Message, HttpStatusCode.InternalServerError);
    }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching Supplier By Name.");
            if (appSettings.IsProduction)
            {
                return new Response("Internal Error, could not retrieve supplier by supplier name.", HttpStatusCode.InternalServerError);
        }
            else
            {
                return new Response("Internal Error: " + ex.Message, HttpStatusCode.InternalServerError);
    }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching Supplier By Last Name and SIN.");
            if (appSettings.IsProduction)
            {
                return new Response("Internal Error, could not retrieve supplier by last name and SIN.", HttpStatusCode.InternalServerError);
        }
            else
            {
                return new Response("Internal Error: " + ex.Message, HttpStatusCode.InternalServerError);
    }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching Supplier By Business Number.");
            if (appSettings.IsProduction)
            {
                return new Response("Internal Error, could not retrieve supplier by business number.", HttpStatusCode.InternalServerError);
            }
            else
            {
                return new Response("Internal Error: " + ex.Message, HttpStatusCode.InternalServerError);
            }
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
            var url = $"{appSettings.Client.BaseUrl}/cfs/supplierbyname/{supplierName}/{postalCode}";
            return await _httpClient.Get(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching Supplier By Supplier Name and Postal Code.");
            if (appSettings.IsProduction)
            {
                return new Response("Internal Error, could not retrieve supplier by supplier name and postal code.", HttpStatusCode.InternalServerError);
            }
            else
            {
                return new Response("Internal Error: " + ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}

public record Response(string Content, HttpStatusCode StatusCode);
