using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BusinessTravelSystem.App.Models;
using Microsoft.JSInterop;

namespace BusinessTravelSystem.App.Services;

public sealed partial class MockDatabaseService : IMockDatabaseService
{
    private const string StorageKey = "bts.prototype.mockDatabase.v1";
    private const string DefaultAdministratorEmployeeNo = "01023712";

    private static readonly string[] SeedFiles =
    {
        "data/mock/mock-seed-reference.json",
        "data/mock/mock-seed-identity.json",
        "data/mock/mock-seed-trips.json",
        "data/mock/mock-seed-expenses.json",
        "data/mock/mock-seed-workflow.json",
        "data/mock/mock-seed-history.json"
    };

    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private JsonObject? _database;

    public MockDatabaseService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public event Action? Changed;

    public bool IsInitialized => _database is not null;

    public async Task InitializeAsync()
    {
        if (IsInitialized)
        {
            return;
        }

        await _gate.WaitAsync();
        try
        {
            if (IsInitialized)
            {
                return;
            }

            var storedJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            _database = TryParseDatabase(storedJson);

            if (_database is not null)
            {
                try
                {
                    MockDataIntegrityValidator.Validate(_database);
                }
                catch (InvalidOperationException)
                {
                    _database = null;
                }
            }

            if (_database is null)
            {
                _database = await LoadSeedAsync();
                MockDataIntegrityValidator.Validate(_database);
                await PersistAsync();
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ResetAsync()
    {
        await _gate.WaitAsync();
        try
        {
            _database = await LoadSeedAsync();
            MockDataIntegrityValidator.Validate(_database);
            await PersistAsync();
        }
        finally
        {
            _gate.Release();
        }

        Changed?.Invoke();
    }

    public async Task<JsonObject> GetSnapshotAsync()
    {
        await InitializeAsync();
        return GetDatabase().DeepClone().AsObject();
    }

    public async Task<MockCredentialRecord?> FindCredentialAsync(string employeeNoOrEmail)
    {
        await InitializeAsync();

        var identifier = employeeNoOrEmail.Trim();
        if (identifier.Length == 0)
        {
            return null;
        }

        var traveller = Rows("TRAVELLER")
            .Select(AsObject)
            .FirstOrDefault(row =>
                IsActive(row) &&
                (EqualsText(Text(row, "EMPLOYEE_NO"), identifier) ||
                 EqualsText(Text(row, "EMAIL"), identifier)));

        return traveller is null ? null : BuildCredential(traveller);
    }

    public Task<MockCredentialRecord?> GetDefaultAdministratorAsync() =>
        FindCredentialAsync(DefaultAdministratorEmployeeNo);
}
