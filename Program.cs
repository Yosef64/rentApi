var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key.json");
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/pizza",()=> Users.GetUsersAsync());
app.MapGet("/pizza/{email}", (string email)=> Users.GetUserByEmail(email));
app.MapPost("pizza",(User user)=> Users.AddUser(user));
app.MapGet("/posts", ()=>Posts.GetPostsAsync());
app.MapPost("/posts", (Post post)=>Posts.AddPostAsync(post));
app.Run();

