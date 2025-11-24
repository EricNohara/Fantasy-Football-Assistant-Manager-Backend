using FFOracle.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Text.Json.Serialization;

namespace FFOracle.Controllers;


[ApiController]     //establishes that class will handle API requests
[Route("api/[controller]")]     //establishes this controller's base URL path
public class StripeController : Controller  //Controller class gives access to objects/methods needed to handle HTTP
{
    private readonly IConfiguration _config;    //accesses appsettings information, including keys
    private readonly SupabaseAuthService _authService;

    //constructor
    public StripeController(IConfiguration config, SupabaseAuthService authService)
    {
        _config = config;
        _authService = authService;
    }

    //endpoint for creating a payment intent
    //The [FromBody] tag tells the code that this method will receive data from a passed in JSON file.
    // The file contents are then automatically mapped to the attributes of the argument type.
    [HttpPost("create-payment-intent")]     //establishes that post requests will come in at this URL
    public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest request)
    {
        //The payment intent is an object that is carried across the entire payment process.
        //It holds information about a specific payment and tracks the status of that payment.

        //first, get user id
        // get the token from the Authorization header
        var userId = await _authService.AuthorizeUser(Request);
        if (userId == Guid.Empty)
        {
            return Unauthorized("Invalid token");
        }


        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];   //provide API key

        //Collect information on the new payment
        var options = new PaymentIntentCreateOptions
        {
            Amount = request.Amount, // amount to pay; for USD, it is in cents
            Currency = "usd",   //currency denomination
            PaymentMethodTypes = new List<string> { "card" },   //payment types to offer
            Metadata = new Dictionary<string, string>   //store additional data to be used to complete the purchase on payment
            {
                { "userId", userId.ToString() },        // the ID of the user making the purchase
                { "tokens", request.Tokens.ToString() }     // the number of tokens being purchased
            }
        };

        var service = new PaymentIntentService();   //PaymentIntentService is a class for interfacing with the API
        var paymentIntent = await service.CreateAsync(options); //request a new payment intent from the Stripe servers

        //return a success response. Do not pass the payment intent to the client, just sent the client secret.
        //The client secret is a frontend-safe identifier for a transaction/payment intent.
        return Ok(new { clientSecret = paymentIntent.ClientSecret });
    }
}

//class to contain payment quantity information. Populated by JSON sent from client and passed
// to controller as input.
public class PaymentRequest
{
    [JsonPropertyName("packageId")]
    public string PackageId { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("tokens")]
    public int Tokens { get; set; }

    [JsonPropertyName("packageName")]
    public string PackageName { get; set; }
}
