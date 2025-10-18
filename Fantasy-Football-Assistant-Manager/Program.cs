using Fantasy_Football_Assistant_Manager.Services;
using Supabase;
//using Supabase.Postgrest.Models;

var builder = WebApplication.CreateBuilder(args);

// Supabase settings
var supabaseSection = builder.Configuration.GetRequiredSection("Supabase");
var supabaseUrl = supabaseSection["Url"];
var supabaseServiceRoleKey = supabaseSection["ServiceRoleKey"];

if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseServiceRoleKey))
{
    throw new InvalidOperationException("Supabase configuration is missing! Check appsettings.json or environment variables.");
}

// Register Supabase clients - one for anon key and one for service role key for privledged operations
builder.Services.AddSingleton<Client>(sp =>
{
    var options = new SupabaseOptions { AutoConnectRealtime = true };
    return new Client(supabaseUrl, supabaseServiceRoleKey, options);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Added this to allow connection to local frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000") // your frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add http client and nflverseservice
builder.Services.AddHttpClient<NflVerseService>();

var app = builder.Build();

//enable CORS
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//assign variables for stripe keys. Currently, I do not need them. They're set in preparation for future use.
var stripeSection = builder.Configuration.GetRequiredSection("Stripe");
var stripeSecretKey = stripeSection["SecretKey"];
var stripePublishableKey = stripeSection["PublishableKey"];
if (string.IsNullOrEmpty(stripeSecretKey))
{
    throw new InvalidOperationException("Stripe configuration is missing! Add your keys to appsettings.json.");
}
// Set the global default variable for the stripe API key. This makes it usable anywhere in the app.
Stripe.StripeConfiguration.ApiKey = stripeSecretKey;


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();