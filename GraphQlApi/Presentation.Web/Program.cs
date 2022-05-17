using GraphQL.DataLoader;
using GraphQL.MicrosoftDI;
using GraphQL.SystemTextJson;
using GraphQL.AspNetCore3;
using GraphQL;
using Domain.GraphQlTypes;
using Domain;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri("https://bot-secrets-dqemux.vault.azure.net/"),
        new DefaultAzureCredential());
}

builder.Services.AddCors();

builder.Services.AddDbContextFactory<CelaHackContext>(options =>
  options.UseCosmos(
            "https://cela-hack-22.documents.azure.com:443/",
            builder.Configuration["CosmosAccountKey"],
            databaseName: "CelaHack"));

builder.Services.AddGraphQL(gqlBuilder => gqlBuilder
    .AddSystemTextJson()
    .AddDataLoader()
    // .AddAuthorization() // uncomment to enable Authorization (see https://github.com/Shane32/GraphQL.AspNetCore3#authorization-configuration)
    .AddErrorInfoProvider(opt => opt.ExposeExceptionStackTrace = builder.Environment.IsDevelopment())
    .AddSchema<BotSchema>()
    .AddGraphTypes(typeof(BotSchema).Assembly)
    .AddClrTypeMappings(typeof(BotSchema).Assembly)
    .AddAutoClrMappings()
);



var app = builder.Build();

app.UseCors(builder => builder
   .AllowAnyOrigin()
   .AllowAnyMethod()
   .AllowAnyHeader());

app.UseGraphQLPlayground("/graphql/playground");
app.UseGraphQL<BotSchema>();

app.Run();
