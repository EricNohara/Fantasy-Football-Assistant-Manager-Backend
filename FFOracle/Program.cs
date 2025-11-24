using FFOracle.Services;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

#region --- Supabase Configuration ---

var supabaseSection = builder.Configuration.GetRequiredSection("Supabase");
var supabaseUrl = supabaseSection["Url"];
var supabaseServiceRoleKey = supabaseSection["ServiceRoleKey"];

if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseServiceRoleKey))
    throw new InvalidOperationException("Supabase configuration is missing! Check appsettings.json or environment variables.");

builder.Services.AddSingleton<Client>(_ =>
{
    var supabaseOptions = new SupabaseOptions
    {
        AutoConnectRealtime = true
    };

    return new Client(supabaseUrl, supabaseServiceRoleKey, supabaseOptions);
});

#endregion


#region --- Service Registrations ---

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Frontend CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                  "http://localhost:3000",
                  "http://localhost:5173",  // Vite default
                  "http://localhost:3001",  // Alternative React port
                  "http://localhost:8080"   // Alternative dev server
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// External services
builder.Services.AddHttpClient<NflVerseService>();
builder.Services.AddScoped<ChatGPTService>();
builder.Services.AddScoped<EspnService>();

// App-specific services
builder.Services.AddScoped<SupabaseAuthService>();

#endregion


#region --- Stripe Configuration ---

var stripeSection = builder.Configuration.GetRequiredSection("Stripe");
var stripeSecretKey = stripeSection["SecretKey"];
var stripePublishableKey = stripeSection["PublishableKey"];

if (string.IsNullOrEmpty(stripeSecretKey))
    throw new InvalidOperationException("Stripe configuration is missing! Add your keys to appsettings.json.");

// Set global Stripe API key
Stripe.StripeConfiguration.ApiKey = stripeSecretKey;

#endregion


var app = builder.Build();

#region --- Middleware Pipeline ---

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();
