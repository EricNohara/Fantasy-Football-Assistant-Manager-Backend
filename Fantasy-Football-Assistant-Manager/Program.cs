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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
