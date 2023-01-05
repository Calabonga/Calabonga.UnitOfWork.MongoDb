using Calabonga.UnitOfWork.MongoDb;
using WebApplicationWithMongo.Application.TagHelpers.PagedListHelper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddTransient<IPagedListTagHelperService, PagedListTagHelperService>();
builder.Services.AddUnitOfWork(builder.Configuration.GetSection(nameof(DatabaseSettings)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
