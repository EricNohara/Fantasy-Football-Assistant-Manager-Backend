using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Fantasy_Football_Assistant_Manager.Controllers
{

    [ApiController]     //establishes that class will handle API requests
    [Route("api/[controller]")]     //establishes this controller's base URL path
    public class StripeController : Controller  //Controller class gives access to objects/methods needed to handle HTTP
    {
        private readonly IConfiguration _config;    //stores appsettings information, including keys

        //constructor
        public StripeController(IConfiguration config)
        {
            _config = config;
        }

        //endpoint for creating a payment intent
        //The [FromBody] tag tells the code that this method will receive data from a passed in JSON file.
        // The file contents are then automatically mapped to the attributes of the argument type.
        [HttpPost("create-payment-intent")]     //establishes that post requests will come in at this URL
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest request)
        {
            //The payment intent is an object that is carried across the entire payment process.
            //It holds information about a specific payment and tracks the status of that payment.

            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];   //provide API key

            //Collect information on the new payment
            var options = new PaymentIntentCreateOptions
            {
                Amount = request.Amount, // amount to pay; for USD, it is in cents
                Currency = "usd",   //currency denomination
                PaymentMethodTypes = new List<string> { "card" },   //payment types to offer
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
        public long Amount { get; set; }
    }
}
