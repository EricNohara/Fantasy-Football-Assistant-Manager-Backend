using Microsoft.AspNetCore.Mvc;
using Stripe;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace FFOracle.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StripeWebhookController : Controller
{

    private readonly ILogger<StripeWebhookController> _logger;  //a logger I'll use to indicate successful payments
    private readonly IConfiguration _config;    //accesses appsettings info

    //constructor to initialize the stored logger and config
    public StripeWebhookController(ILogger<StripeWebhookController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    [HttpPost("check-payment-completion")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        //Read the request body
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        //retrieve the webhook secret.
        //As of now, WE HAVE NO WEBHOOK ENDPOINT WITH STRIPE. That requires a proper public URL for our
        // backend. I have used the stripe CLI to make a temporary bridge from which we can get responses.
        var webhookSecret = _config["Stripe:WebhookSecret"];
        //verify that it's actually there
        if (string.IsNullOrEmpty(webhookSecret))
        {
            _logger.LogError("Stripe WebhookSecret is not configured.");
            return BadRequest("Webhook secret not configured.");
        }

        try
        {
            //Parse the message into an Event object and ensure it's a valid stripe message
            var stripeEvent = EventUtility.ConstructEvent(
                json,   
                Request.Headers["Stripe-Signature"],    //use this to verify that it has the stripe header
                webhookSecret
            );

            //Each case of the switch statement handles a different event type.
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    {
                        //for now, just log the success
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        _logger.LogInformation("PaymentIntent succeeded: {Id}, amount: {Amount}", paymentIntent.Id, paymentIntent.Amount);
                        //This is where DB would be updated
                        break;
                    }

                case "payment_intent.payment_failed":
                    {
                        //for now, just log the failure
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        _logger.LogWarning("PaymentIntent failed: {Id}", paymentIntent.Id);
                        //This is where a user notification might be called
                        break;
                    }
                //I can add other events here as needed, but for now we only need those two
                
                    //Log any event aside from the ones expected
                default:
                    _logger.LogInformation("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                    break;
            }

            //This response tells stripe the message was received
            return Ok();
        }
        catch (StripeException ex)
        {
            //If anything goes wrong in validating the message, log it and tell stripe a bad message was received
            _logger.LogError(ex, "Stripe webhook handling failed: {Message}", ex.Message);
            return BadRequest();
        }
        catch (System.Exception ex)
        {
            //Catch other errors
            _logger.LogError(ex, "Unexpected error handling Stripe webhook");
            return StatusCode(500);
        }
    }
}
