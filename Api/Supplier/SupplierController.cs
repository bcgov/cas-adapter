namespace Api;

[Route("api/[controller]")]
[ApiController]
public class SupplierController(ICasService casService) : Controller
{
    [HttpGet("{supplierNumber}")]
    public async Task<IActionResult> GetSupplierByNumber([FromRoute] string supplierNumber)
    {
        (var result, var statusCode) = await casService.GetSupplierByNumber(supplierNumber);

        return StatusCode((int)statusCode, new JsonResult(result).Value);
    }

    [HttpGet("{supplierNumber}/site/{supplierSiteCode}")]
    public async Task<IActionResult> GetSupplierByNumberAndSiteCode([FromRoute] string supplierNumber, [FromRoute] string supplierSiteCode)
    {
        (var result, var statusCode) = await casService.GetSupplierByNumberAndSiteCode(supplierNumber, supplierSiteCode);

        return StatusCode((int)statusCode, new JsonResult(result).Value);
    }

    [HttpGet("suppliersearch/{supplierName}")]
    public async Task<IActionResult> GetSupplierByName([FromRoute] string supplierName)
    {
        (var result, var statusCode) = await casService.FindSupplierByName(supplierName);

        return StatusCode((int)statusCode, new JsonResult(result).Value);
    }

    [HttpGet("{lastName}/lastname/{sin}/sin")]
    public async Task<IActionResult> GetSupplierByLastNameAndSin([FromRoute] string lastName, [FromRoute] string sin)
    {
        (var result, var statusCode) = await casService.GetSupplierByLastNameAndSin(lastName, sin);

        return StatusCode((int)statusCode, new JsonResult(result).Value);
    }

    [HttpGet("{businessNumber}/businessnumber")]
    public async Task<IActionResult> GetSupplierByBusinessNumber([FromRoute] string businessNumber)
    {
        (var result, var statusCode) = await casService.GetSupplierByBusinessNumber(businessNumber);

        return StatusCode((int)statusCode, new JsonResult(result).Value);
    }

    [HttpGet("supplierbyname/{supplierName}/{postalCode}")]
    public async Task<IActionResult> GetSupplierByNameAndPostalCode([FromRoute] string supplierName, [FromRoute] string postalCode)
    {
        (var result, var statusCode) = await casService.GetSupplierByNameAndPostalCode(supplierName, postalCode);

        return StatusCode((int)statusCode, new JsonResult(result).Value);
    }
}
