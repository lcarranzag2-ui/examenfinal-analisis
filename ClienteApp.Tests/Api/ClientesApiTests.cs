
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ClienteApp.Tests.Api;

public class ClientesApiTests
{
    [Fact]
    public async Task Registrar_ClienteValido_DebeRetornar201()
    {
        // Arrange
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var solicitud = new
        {
            Nombre = "Ana López",
            Correo = $"ana_{Guid.NewGuid():N}@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var cliente = await response.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(cliente);
        Assert.Equal(solicitud.Nombre, cliente.Nombre);
        Assert.Equal(solicitud.Correo, cliente.Correo);
    }

    [Fact]
    public async Task Registrar_NombreVacio_DebeRetornar400()
    {
        // Arrange
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var solicitud = new
        {
            Nombre = "",
            Correo = $"correo_{Guid.NewGuid():N}@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    // ---------------------------------------------------------------
    // Examen final - Pruebas de API para GET /api/Clientes/{id}
    // ---------------------------------------------------------------

    [Fact]
    public async Task ObtenerPorId_ClienteExistente_DebeRetornar200()
    {
        // Arrange: cada factory usa una base temporal nueva (vacía),
        // por eso primero se registra un cliente para que exista.
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var solicitud = new
        {
            Nombre = "Ana López",
            Correo = $"ana_{Guid.NewGuid():N}@example.com"
        };

        var respuestaRegistro = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud);

        Assert.Equal(
            HttpStatusCode.Created,
            respuestaRegistro.StatusCode);

        var clienteCreado = await respuestaRegistro.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(clienteCreado);

        // Act: consultar el cliente recién creado por su id.
        var response = await client.GetAsync(
            $"/api/Clientes/{clienteCreado.Id}");

        // Assert: la API responde 200 OK con los datos del cliente.
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var cliente = await response.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(cliente);
        Assert.Equal(clienteCreado.Id, cliente.Id);
        Assert.Equal(solicitud.Nombre, cliente.Nombre);
        Assert.Equal(solicitud.Correo, cliente.Correo);
    }

    [Fact]
    public async Task ObtenerPorId_ClienteInexistente_DebeRetornar404()
    {
        // Arrange: base temporal vacía, ningún cliente tiene este id.
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var idInexistente = 99999;

        // Act
        var response = await client.GetAsync(
            $"/api/Clientes/{idInexistente}");

        // Assert: la API responde 404 Not Found.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        // Se revisa el mensaje para confirmar que el 404 lo devolvió
        // el controlador y no que la ruta no exista.
        var contenido = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cliente no encontrado", contenido);
    }

    private sealed record ClienteRespuesta(
        int Id,
        string Nombre,
        string Correo);
}

public sealed class ClienteApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"clientes_api_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:ClientesDb"] =
                        $"Data Source={_dbPath};Pooling=False"
                });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
