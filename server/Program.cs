using Stonks.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddScoped<PolygonService>(provider =>
{
    var httpClient = new HttpClient();
    var config = provider.GetRequiredService<IConfiguration>();
    return new PolygonService(httpClient, config);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>{
        options.AddPolicy("AllowBlazor", policy =>{
                policy.SetIsOriginAllowed(origin =>
                new Uri(origin).Host == "localhost")
                .AllowAnyMethod()
                .AllowAnyHeader();
                });
        });

builder.Services.AddControllers();
builder.Services.AddHttpClient<PolygonService>();


var app = builder.Build();

app.UseCors("AllowBlazor");
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
