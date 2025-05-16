namespace Api;

[Route("api/[controller]")]
[ApiController]
public class InvoiceController(ICasService casService) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] Invoice invoice)
    {
        (var result, var statusCode) = await casService.CreateInvoice(invoice);
        return StatusCode((int)statusCode, new JsonResult(result).Value);
    }

    [HttpGet("{invoiceNumber}/{supplierNumber}/{supplierSiteCode}")]
    public async Task<IActionResult> Search(string invoiceNumber, string supplierNumber, string supplierSiteCode)
    {
        (var result, var statusCode) = await casService.GetInvoice(invoiceNumber, supplierNumber, supplierSiteCode);
        return StatusCode((int)statusCode, new JsonResult(result).Value);
    }

    [HttpGet("payment/{paymentNumber}/{payGroup}")]
    public async Task<IActionResult> GetPayment(string paymentNumber, string payGroup)
    {
        (var result, var statusCode) = await casService.GetPayment(paymentNumber, payGroup);
        return StatusCode((int)statusCode, new JsonResult(result).Value);
    }
}
