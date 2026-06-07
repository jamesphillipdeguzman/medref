## 👥 Team 16 Members

- James Phillip De Guzman
- Nefi Zaldana
- Jeremy Aaron Herrera

### 📌 Project Details

- Project Name: Medref
- Description: A medical reference tool for ICD codes and more...
- Architecture - Blazor WASM → ASP.NET API → MedlinePlus Connect

### 🔗 Project Link (TBA)

### 🔗 Trello Board Link

https://trello.com/b/4jlbalcB/medref

### 🔗 External API Link

https://medlineplus.gov/medlineplus-connect/web-service/

### ▶ Local development

This solution is split into two apps:

- Frontend: `MedRef.Client` on `http://localhost:5265`
- Backend: `MedRef.Server` on `http://localhost:5035`

Run them from the solution root with explicit project paths, or from each project folder:

```powershell
dotnet run --project .\MedRef.Server\MedRef.Server.csproj
dotnet run --project .\MedRef.Client\MedRef.Client.csproj
```

The frontend page you want is `http://localhost:5265/icd10-search`, and it calls the backend proxy at `http://localhost:5035/api/medlineproxy?code=`.
