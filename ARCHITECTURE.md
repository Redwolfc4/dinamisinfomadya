# 🏛️ Panduan Arsitektur & Analisis Mendalam: DinamisInfoMadya
> **Platform:** .NET 10 (C# 14 / modern .NET ecosystem)  
> **Arsitektur:** Decoupled Fullstack Client-Server (ASP.NET Core Web API + Blazor WebAssembly)  
> **Tipe Dokumen:** Panduan Pembelajaran & Blueprint Arsitektur (Educational & Technical Blueprint)

---

## 📑 Daftar Isi
1. [Pengantar & Visi Solusi](#1-pengantar--visi-solusi)
2. [Peta Struktur Direktori & Analisis Rinci Setiap Berkas](#2-peta-struktur-direktori--analisis-rinci-setiap-berkas)
   - 2.1 [Bedah File Tingkat Solusi (Root Workspace)](#21-bedah-file-tingkat-solusi-root-workspace)
   - 2.2 [Bedah File & Folder Proyek Backend (backend/)](#22-bedah-file--folder-proyek-backend-backend)
   - 2.3 [Bedah File & Folder Proyek Frontend (frontend/)](#23-bedah-file--folder-proyek-frontend-frontend)
   - 2.4 [Matriks Relasi & Alur Antar Berkas](#24-matriks-relasi--alur-antar-berkas)
3. [Format Solusi Modern: DinamisInfoMadya.slnx](#3-format-solusi-modern-dinamisinfomadyaslnx)
4. [Analisis Mendalam Backend (ASP.NET Core Web API)](#4-analisis-mendalam-backend-aspnet-core-web-api)
   - 4.1 File Konfigurasi Project (`backend.csproj`)
   - 4.2 Entry Point & Middleware Pipeline (`Program.cs`)
   - 4.3 Struktur Controller & Model DTO (`WeatherForecastController.cs`)
   - 4.4 OpenAPI / Swagger Generasi Baru
   - 4.5 Konfigurasi Lingkungan (`appsettings.json` & `launchSettings.json`)
5. [Analisis Mendalam Frontend (Blazor WebAssembly)](#5-analisis-mendalam-frontend-blazor-webassembly)
   - 5.1 Bagaimana Blazor WebAssembly Bekerja?
   - 5.2 File Konfigurasi Client (`frontend.csproj`)
   - 5.3 Bootstrapping Client-Side (`Program.cs`)
   - 5.4 Sistem Komponen, Routing & Layouting
   - 5.5 CSS Isolation & Asset Fingerprinting
6. [Alur Komunikasi Data (End-to-End Data Flow)](#6-alur-komunikasi-data-end-to-end-data-flow)
7. [Tantangan Nyata: CORS & Cara Menghubungkan Frontend ke Backend](#7-tantangan-nyata-cors--cara-menghubungkan-frontend-ke-backend)
8. [Panduan Operasional: Menjalankan & Debugging](#8-panduan-operasional-menjalankan--debugging)
9. [Roadmap Menuju Enterprise Clean Architecture](#9-roadmap-menuju-enterprise-clean-architecture)

---

## 1. Pengantar & Visi Solusi

Project **DinamisInfoMadya** dirancang dengan arsitektur **Decoupled Fullstack** berbasis **.NET 10**. 
Berbeda dengan pendekatan monolitik klasik (seperti ASP.NET MVC atau Razor Pages di mana server merender HTML lalu mengirimkannya ke browser), project ini memisahkan dua tanggung jawab utama secara tegas:

```text
+------------------------------------+           +------------------------------------+
|       CLIENT: WEB BROWSER          |           |       SERVER / HOST MACHINE        |
|                                    |           |                                    |
|  +------------------------------+  |           |  +------------------------------+  |
|  |   Frontend: Blazor Wasm      |  |           |  |   Backend: ASP.NET Core API  |  |
|  |   Port: 5181 / 7180          |  |           |  |   Port: 5266 / 7240          |  |
|  +------------------------------+  |           |  +------------------------------+  |
|                 │                  |           |                 │                  |
+-----------------│------------------+           +-----------------│------------------+
                  │                                                │
                  │═══════════ HTTP REST (JSON Payload) ══════════>│
                  │<══════════ Response (Data / Status) ═══════════│
                                                                   │
                                                                   ▼
                                                 +------------------------------------+
                                                 |    Database / External Services    |
                                                 +------------------------------------+
```

### Mengapa Pendekatan Ini Sangat Kuat?
1. **Single Language Ecosystem (C# End-to-End):**
   Developer tidak perlu berpindah mental (*context switching*) antara TypeScript/JavaScript di frontend dan C#/Java di backend. Logika validasi, class model data (DTO), dan fungsi utilitas dapat ditulis dalam bahasa C# yang sama.
2. **Kinerja Tinggi di Sisi Client:**
   Blazor WebAssembly dikompilasi menjadi bytecode Wasm yang dieksekusi mendekati kecepatan native (*near-native speed*) langsung di engine browser client, memanfaatkan CPU client tanpa membebani server untuk rendering antarmuka.
3. **Skalabilitas Server:**
   Backend murni bertindak sebagai penyedia data (Stateless REST API). Server tidak menyimpan session UI pengguna, sehingga mudah di-*scale* (secara horizontal melalui container/load balancer).

---

## 2. Peta Struktur Direktori & Analisis Rinci Setiap Berkas

Berikut adalah pohon direktori lengkap dari project **DinamisInfoMadya** sesuai kondisi aktual di sistem:

```text
dinamisinfomadya/
├── .git/                            # Repositori version control Git lokal
├── .gitignore                       # Aturan pengabaian file build, cache, dan rahasia
├── .vscode/                         # Konfigurasi workspace Antigravity IDE / VS Code
│   └── settings.json                # Pengaturan isolasi repository Git
├── DinamisInfoMadya.slnx            # File solusi .NET 10 (Format XML baru)
├── ARCHITECTURE.md                  # Panduan arsitektur & dokumentasi pembelajaran
│
├── backend/                         # === PROYEK BACKEND (ASP.NET Core Web API) ===
│   ├── Controllers/
│   │   └── WeatherForecastController.cs # Controller REST API untuk endpoint /WeatherForecast
│   ├── Properties/
│   │   └── launchSettings.json      # Profil peluncuran Kestrel (Port 5266 HTTP & 7240 HTTPS)
│   ├── appsettings.json             # Konfigurasi global runtime (Logging level, App settings)
│   ├── appsettings.Development.json # Konfigurasi khusus lingkungan Development
│   ├── backend.csproj               # Metadata proyek C#, TargetFramework net10.0, OpenAPI package
│   ├── backend.http                 # File scratchpad pengujian request HTTP langsung di IDE
│   ├── Program.cs                   # Titik masuk utama (Entry Point), DI container & Middleware pipeline
│   └── WeatherForecast.cs           # Class Model / Data Transfer Object (DTO)
│
└── frontend/                        # === PROYEK FRONTEND (Blazor WebAssembly - Client-Side SPA) ===
    ├── Layout/
    │   ├── MainLayout.razor         # Kerangka layout utama (Sidebar + Area konten @Body)
    │   ├── MainLayout.razor.css     # CSS Scoped terisolasi khusus MainLayout
    │   ├── NavMenu.razor            # Komponen navigasi menu di sidebar dengan NavLink
    │   └── NavMenu.razor.css        # CSS Scoped terisolasi khusus NavMenu
    ├── Pages/
    │   ├── Counter.razor            # Halaman /counter (Demonstrasi state reaktif C# lokal)
    │   ├── Home.razor               # Halaman utama / (Pesan selamat datang)
    │   ├── NotFound.razor           # Halaman fallback 404 jika URL tidak ditemukan di router
    │   └── Weather.razor            # Halaman /weather (Demonstrasi HTTP Fetching & rendering tabel)
    ├── Properties/
    │   └── launchSettings.json      # Profil peluncuran dev-server Wasm (Port 5181 HTTP & 7180 HTTPS)
    ├── wwwroot/                     # Aset statis murni yang disajikan langsung ke browser
    │   ├── css/
    │   │   └── app.css              # Styling stylesheet global aplikasi
    │   ├── lib/
    │   │   └── bootstrap/           # Library framework CSS/JS Bootstrap lokal
    │   ├── sample-data/
    │   │   └── weather.json         # Mock data JSON statis untuk demonstrasi fetching
    │   ├── favicon.png              # Ikon website (favicon)
    │   ├── icon-192.png             # Ikon aplikasi PWA (Progressive Web App)
    │   └── index.html               # File HTML induk tempat runtime WebAssembly di-mount
    ├── App.razor                    # Komponen akar Blazor, konfigurasi Router & Default Layout
    ├── _Imports.razor               # Namespace global Razor (@using bersama untuk seluruh komponen)
    ├── frontend.csproj              # Metadata proyek Blazor Wasm net10.0 & DevServer package
    └── Program.cs                   # Entry point client, WebAssemblyHostBuilder, mount #app & HttpClient
```

---

### 2.1 Bedah File Tingkat Solusi (Root Workspace)

Di tingkat terluar (root), terdapat file-file yang mengatur lingkungan kerja, version control, dan integrasi antar proyek:

| Berkas / Direktori | Tipe & Format | Peran & Penjelasan Arsitektural |
| :--- | :--- | :--- |
| **`DinamisInfoMadya.slnx`** | Solution XML | Menggabungkan proyek `backend` dan `frontend` menjadi satu kesatuan solusi .NET 10. Menggunakan format XML modern yang ringkas sehingga compiler dan IDE tahu proyek mana saja yang harus di-build secara bersamaan. |
| **`.gitignore`** | Git Rule | Mencegah file biner hasil kompilasi (`bin/`, `obj/`), cache dependensi (`.nuget/`, `node_modules/`), profil pengguna IDE (`.vs/`), dan file konfigurasi lokal rahasia agar tidak terunggah ke repositori Git. |
| **`.vscode/settings.json`** | Editor Config | Mengunci fitur *Git Auto Repository Detection* ke mode `subFolders`, memastikan Source Control di IDE fokus hanya pada project `dinamisinfomadya` dan tidak salah mendeteksi repository luar. |
| **`ARCHITECTURE.md`** | Markdown Doc | Panduan komprehensif, cetak biru arsitektur, dan referensi belajar bagi developer untuk memahami seluruh mekanisme sistem. |

---

### 2.2 Bedah File & Folder Proyek Backend (`backend/`)

Proyek backend dibangun di atas template **ASP.NET Core Web API (.NET 10)**. Tanggung jawab utamanya adalah memproses logika bisnis, menyediakan endpoint RESTful, dan mengembalikan data dalam format JSON:

#### A. Folder `backend/Controllers/`
- **`WeatherForecastController.cs`**:
  - **Peran:** Controller RESTful yang mewarisi `ControllerBase`. Dihias dengan atribut `[ApiController]` dan `[Route("[controller]")]`.
  - **Mekanisme:** Menangani request HTTP `GET` ke endpoint `/WeatherForecast`. Method `Get()` di dalamnya menghasilkan array berisi 5 data perkiraan cuaca acak dan langsung mengembalikannya sebagai JSON payload ke pemanggil.

#### B. Folder `backend/Properties/`
- **`launchSettings.json`**:
  - **Peran:** Mengatur konfigurasi server Kestrel saat dijalankan di komputer developer (Local Development).
  - **Mekanisme:** Mendefinisikan port komunikasi:
    - Profil `http`: Berjalan di `http://localhost:5266`
    - Profil `https`: Berjalan di `https://localhost:7240` (dan HTTP `http://localhost:5266`)
    - Menetapkan variabel lingkungan `ASPNETCORE_ENVIRONMENT = "Development"`.

#### C. File Konfigurasi Runtime (`appsettings.json` & `appsettings.Development.json`)
- **`appsettings.json`**:
  - Berisi konfigurasi global sistem seperti tingkat logging (`LogLevel: Information` untuk aplikasi umum, `Warning` untuk framework ASP.NET Core) dan aturan `AllowedHosts: "*"`.
- **`appsettings.Development.json`**:
  - Meng-override pengaturan di atas khusus saat aplikasi berjalan dalam mode Development (misalnya mengaktifkan log yang lebih detail saat proses debugging).

#### D. Berkas Kode & Utilitas Lainnya
- **`Program.cs`**:
  - **Titik Masuk Utama (Entry Point):** Tempat aplikasi backend dimulai (`WebApplication.CreateBuilder`).
  - **Dependency Injection (DI):** Mendaftarkan Controller (`builder.Services.AddControllers()`) dan metadata OpenAPI (`builder.Services.AddOpenApi()`).
  - **Middleware Pipeline:** Mengatur urutan eksekusi HTTP Request (`UseHttpsRedirection`, `UseAuthorization`, `MapControllers`).
- **`WeatherForecast.cs`**:
  - Berisi deklarasi class Model DTO (*Data Transfer Object*). Mendefinisikan atribut data: `Date` (tipe `DateOnly`), `TemperatureC` (integer Celcius), `TemperatureF` (computed property Fahrenheit), dan `Summary` (keterangan cuaca).
- **`backend.csproj`**:
  - File proyek MSBuild yang menentukan target framework `net10.0`, mengaktifkan *nullable checks*, *implicit usings*, serta mereferensikan package `Microsoft.AspNetCore.OpenApi` versi `10.0.15`.
- **`backend.http`**:
  - File scratchpad bawaan IDE yang memungkinkan developer mengirim request HTTP secara langsung dari editor (menguji `GET http://localhost:5266/weatherforecast/`) tanpa perlu membuka Postman atau browser.

---

### 2.3 Bedah File & Folder Proyek Frontend (`frontend/`)

Proyek frontend dibangun menggunakan **Blazor WebAssembly (.NET 10)**. Aplikasi ini adalah Single Page Application (SPA) yang berjalan 100% di browser pengguna:

#### A. Folder `frontend/Layout/` (Tata Letak Antarmuka)
- **`MainLayout.razor`**:
  - **Peran:** Rangka halaman utama yang membungkus semua halaman aplikasi. Mewarisi `LayoutComponentBase`.
  - **Struktur:** Terbagi menjadi dua area utama: `<div class="sidebar">` (memuat komponen `NavMenu`) dan `<main>` (memuat top-row link serta placeholder `@Body` tempat konten halaman aktif dirender).
- **`MainLayout.razor.css`**:
  - Menggunakan fitur **CSS Isolation (Scoped CSS)**. Styling di dalam file ini hanya berlaku khusus untuk tag di `MainLayout.razor`, sehingga selector class tidak akan bertabrakan dengan komponen lain di aplikasi.
- **`NavMenu.razor`**:
  - Komponen navigasi samping (sidebar menu). Menggunakan komponen bawaan Blazor `<NavLink>` yang otomatis menambahkan class CSS `active` saat URL browser cocok dengan atribut `href` (misal rute `/`, `/counter`, `/weather`). Memiliki state C# `collapseNavMenu` untuk membuka/menutup menu di layar ponsel.
- **`NavMenu.razor.css`**:
  - Scoped CSS terisolasi khusus untuk tata letak tombol navigasi, background gradasi sidebar, dan efek hover menu.

#### B. Folder `frontend/Pages/` (Halaman Rute / Views)
Setiap file di dalam folder ini memiliki direktif `@page "/..."` yang menandakannya sebagai halaman yang bisa dikunjungi melalui URL browser:

- **`Home.razor`** (`@page "/"`):
  - Halaman beranda yang pertama kali dilihat pengguna saat membuka alamat utama website. Menggunakan komponen `<PageTitle>` untuk mengubah judul tab browser.
- **`Counter.razor`** (`@page "/counter"`):
  - Halaman demonstrasi interaktivitas reaktif Blazor. Memiliki tombol `<button @onclick="IncrementCount">` yang memodifikasi variabel C# `currentCount`. Blazor secara otomatis mendeteksi perubahan state dan memperbarui tampilan angka di layar secara instan tanpa reload browser.
- **`Weather.razor`** (`@page "/weather"`):
  - Halaman demonstrasi pengambilan data asinkron (*Asynchronous Data Fetching*). Menggunakan `@inject HttpClient Http` untuk memanggil API di lifecycle `OnInitializedAsync()`. Menampilkan status *Loading...* saat data masih diunduh, lalu merender array cuaca ke dalam tabel Bootstrap.
- **`NotFound.razor`**:
  - Komponen fallback yang otomatis dirender oleh Router jika pengguna mengetik URL yang tidak terdaftar di sistem (Halaman 404 Not Found).

#### C. Folder `frontend/Properties/`
- **`launchSettings.json`**:
  - Konfigurasi server lokal untuk frontend Blazor WebAssembly:
    - Profil `http`: Berjalan di `http://localhost:5181`
    - Profil `https`: Berjalan di `https://localhost:7180` (dan HTTP `http://localhost:5181`)
    - Dilengkapi pengaturan `inspectUri` untuk mengaktifkan debugging C# langsung di Chrome/Edge Developer Tools melalui WebSocket proxy.

#### D. Folder `frontend/wwwroot/` (Aset Statis Murni)
Semua file di dalam folder ini disajikan apa adanya secara publik ke browser:
- **`index.html`**:
  - Pintu gerbang utama (Host page) aplikasi SPA. Memuat link CSS Bootstrap, link `css/app.css`, ikon favicon, serta script krusial:
    ```html
    <script src="_framework/blazor.webassembly#[.{fingerprint}].js"></script>
    ```
    Script inilah yang memuat runtime WebAssembly dan mengompilasi C# ke memori browser.
- **`css/app.css`**:
  - Stylesheet global untuk tipografi, animasi loading progress bar (`.loading-progress`), dan alert box kesalahan Blazor (`#blazor-error-ui`).
- **`lib/bootstrap/`**:
  - Berisi file CSS dan JavaScript lokal framework Twitter Bootstrap untuk kebutuhan grid sistem, styling tombol, form, dan tabel responsif.
- **`sample-data/weather.json`**:
  - File JSON statis berisi contoh data cuaca yang saat ini dibaca oleh `Weather.razor` sebagai mock data sebelum dihubungkan ke backend riil.
- **`favicon.png` & `icon-192.png`**:
  - Gambar logo aplikasi untuk browser tab dan icon Progressive Web Application (PWA).

#### E. Berkas Inti Blazor di Root Frontend
- **`App.razor`**:
  - Komponen induk paling atas yang dipanggil pertama kali. Berisi komponen `<Router AppAssembly="@typeof(App).Assembly">` yang bertugas mendengarkan perubahan URL browser dan merutekannya ke halaman yang sesuai di dalam folder `Pages/`.
- **`_Imports.razor`**:
  - Berkas sakti Blazor! Semua baris `@using` yang diletakkan di file ini (seperti `Microsoft.AspNetCore.Components.Web`, `System.Net.Http.Json`, dll) otomatis berlaku untuk seluruh file `.razor` di dalam project. Developer tidak perlu mengetik `@using` secara berulang-ulang di setiap halaman.
- **`Program.cs`**:
  - Titik awal eksekusi WebAssembly di browser. Menggunakan `WebAssemblyHostBuilder` untuk memetakan komponen `<App>` ke dalam elemen HTML `<div id="app">` di `index.html` dan meregistrasikan `HttpClient` ke container Dependency Injection.
- **`frontend.csproj`**:
  - File proyek MSBuild bertipe SDK `Microsoft.NET.Sdk.BlazorWebAssembly` untuk .NET 10, mereferensikan paket `Microsoft.AspNetCore.Components.WebAssembly` dan server development lokal.

---

### 2.4 Matriks Relasi & Alur Antar Berkas

Bagaimana berkas-berkas di atas saling berkomunikasi satu sama lain? Perhatikan matriks dependensi berikut:

```text
[ Browser Mengakses http://localhost:5181/ ]
                      │
                      ▼
            frontend/wwwroot/index.html
                      │
                      ▼ (Memuat Runtime WebAssembly)
            frontend/Program.cs
                      │
                      ▼ (Mount Komponen Root)
            frontend/App.razor
                      │
                      ▼ (Mencocokkan Rute URL "/")
            frontend/Layout/MainLayout.razor ── (Memuat Sidebar) ──> frontend/Layout/NavMenu.razor
                      │
                      ▼ (Merender Konten Halaman Aktif ke @Body)
            frontend/Pages/Home.razor (atau Counter.razor / Weather.razor)
                      │
                      │ (Jika di halaman /weather)
                      ▼
            Mengirim Request HTTP (HttpClient)
                      │
                      ▼
   ═════════════ JARINGAN HTTP REST ═════════════
                      │
                      ▼ (Menerima Request di Port 5266)
            backend/Program.cs (Middleware Pipeline)
                      │
                      ▼ (Mengarahkan ke Controller)
            backend/Controllers/WeatherForecastController.cs
                      │
                      ▼ (Menggunakan Model Data)
            backend/WeatherForecast.cs
                      │
                      ▼
            Mengembalikan Payload JSON ke Frontend
```

---

## 3. Format Solusi Modern: DinamisInfoMadya.slnx

Pada .NET 10, Microsoft memperkenalkan format solusi baru berbasis XML dengan ekstensi `.slnx`, menggantikan format teks kuno `.sln` yang sudah dipakai sejak Visual Studio awal tahun 2000-an.

Isi file [DinamisInfoMadya.slnx](file:///run/media/salahudin/New%20Volume/github/dinamisinfomadya/DinamisInfoMadya.slnx):
```xml
<Solution>
  <Project Path="backend/backend.csproj" />
  <Project Path="frontend/frontend.csproj" />
</Solution>
```

### Mengapa `.slnx` Jauh Lebih Baik?
- **Human-Readable & Minimalis:** Format `.sln` lama memiliki ratusan baris GUID acak (`{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}`) yang sulit dibaca manusia. Format `.slnx` bersih dan deklaratif.
- **Bebas Git Merge Conflict:** Di format lama, dua developer yang menambahkan project baru sering mengalami merge conflict hebat. Di `.slnx`, struktur XML sederhana membuat conflict hampir tidak pernah terjadi.

---

## 4. Analisis Mendalam Backend (ASP.NET Core Web API)

### 4.1 File Konfigurasi Project (`backend/backend.csproj`)
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.15" />
  </ItemGroup>
</Project>
```
- `<Project Sdk="Microsoft.NET.Sdk.Web">`: Menginstruksikan compiler untuk mengikutsertakan framework ASP.NET Core (Kestrel Web Server, routing engine, HTTP abstractions).
- `<Nullable>enable</Nullable>`: Mengaktifkan *Nullable Reference Types*. Compiler C# akan memberikan peringatan (warning) jika ada variabel yang berisiko bernilai `null` tanpa pengecekan, mencegah `NullReferenceException`.
- `<ImplicitUsings>enable</ImplicitUsings>`: Otomatis mengimpor namespace umum (seperti `System`, `System.Collections.Generic`, `System.Linq`, `System.Threading.Tasks`) sehingga file C# tidak penuh dengan baris `using` di paling atas.
- Package `Microsoft.AspNetCore.OpenApi`: Library resmi Microsoft untuk dokumentasi OpenAPI spek v3 di .NET 10.

---

### 4.2 Entry Point & Middleware Pipeline (`backend/Program.cs`)
File ini adalah jantung dari backend. Mari bedah baris demi baris:

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRASI SERVICE (Dependency Injection Container)
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 2. HTTP REQUEST PIPELINE (Middleware)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### Cara Kerja Middleware Pipeline:
Di ASP.NET Core, setiap HTTP Request yang masuk melewati rantai pipa (*pipeline*) sebelum mencapai Controller, dan response-nya kembali melewati pipa yang sama:

```text
[ CLIENT (Browser) ]
    │
    │  1. HTTP Request (GET /weatherforecast)
    ▼
┌────────────────────────────────────────────────────────┐
│               ASP.NET Core Middleware Pipeline         │
│                                                        │
│  [ UseHttpsRedirection ]                               │
│       │ (Validasi protokol HTTPS/HTTP)                 │
│       ▼                                                │
│  [ UseAuthorization ]                                  │
│       │ (Pemeriksaan token / hak akses pengguna)       │
│       ▼                                                │
│  [ MapControllers ]                                    │
│       │ (Routing engine mencocokkan route URL)         │
│       ▼                                                │
└───────┬────────────────────────────────────────────────┘
        │
        │ 2. Invoke Action Method
        ▼
┌────────────────────────────────────────────────────────┐
│             WeatherForecastController                  │
│                                                        │
│   Get() => IEnumerable<WeatherForecast>                │
│   Menghasilkan data array ramalan cuaca                │
└───────┬────────────────────────────────────────────────┘
        │
        │ 3. Return Raw Data (Objek C#)
        ▼
┌────────────────────────────────────────────────────────┐
│               Formatters & Serialization               │
│                                                        │
│   Serialisasi Objek C# menjadi JSON string             │
└───────┬────────────────────────────────────────────────┘
        │
        │ 4. HTTP 200 OK Response (application/json)
        ▼
[ CLIENT (Browser Menerima Data) ]
```

---

### 4.3 Struktur Controller & Model DTO

#### File Model (`backend/WeatherForecast.cs`):
```csharp
namespace backend;

public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556); // Computed Property
    public string? Summary { get; set; }
}
```
- `DateOnly`: Tipe data modern (diperkenalkan sejak .NET 6) untuk menyimpan tanggal tanpa komponen waktu (jam/menit/detik). Sangat akurat untuk tanggal kalender.
- `TemperatureF`: *Computed property* (ekspresi lambda `=>`). Nilainya tidak disimpan di memori melainkan dihitung otomatis setiap kali diakses berdasarkan nilai `TemperatureC`.

#### File Controller (`backend/Controllers/WeatherForecastController.cs`):
```csharp
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        // Menghasilkan data random untuk simulasi
    }
}
```
- `[ApiController]`: Atribut sakti ASP.NET Core yang otomatis mengaktifkan:
  - Validasi model otomatis (jika ada input invalid, otomatis return 400 Bad Request).
  - Inferensi sumber binding parameter (`[FromBody]`, `[FromQuery]`).
- `[Route("[controller]")]`: Menentukan URL route berdasarkan nama class sebelum kata "Controller". Karena nama class-nya `WeatherForecastController`, URL-nya menjadi `/WeatherForecast`.
- `ControllerBase`: Class dasar untuk Web API (tidak mengandung view rendering seperti MVC standar).

---

### 4.4 OpenAPI Generasi Baru di .NET 10
Pada versi .NET sebelum 9, biasanya digunakan library pihak ketiga *Swashbuckle.AspNetCore*.
Mulai .NET 9 dan .NET 10, Microsoft menyediakan package resmi `Microsoft.AspNetCore.OpenApi`:
- `builder.Services.AddOpenApi()` meregistrasi generator schema.
- `app.MapOpenApi()` mengekspos endpoint metadata di `/openapi/v1.json`.
- Dokumen JSON ini bisa dipakai oleh alat frontend (seperti generator client, Swagger UI, atau Postman) untuk mengetahui semua endpoint yang tersedia secara otomatis.

---

### 4.5 Pengujian Cepat dengan `backend/backend.http`
File ini adalah fitur bawaan IDE untuk melakukan pengetesan REST API tanpa perlu membuka software eksternal seperti Postman:
```http
@backend_HostAddress = http://localhost:5266

GET {{backend_HostAddress}}/weatherforecast/
Accept: application/json
```
Sensei bisa mengklik tombol "Send Request" langsung di editor untuk melihat respon JSON dari API!

---

## 5. Analisis Mendalam Frontend (Blazor WebAssembly)

### 5.1 Bagaimana Blazor WebAssembly Bekerja?
Blazor WebAssembly bukanlah JavaScript. Blazor mengompilasi kode C# sensei dan membungkusnya bersama runtime .NET mini (berbasis mono-wasm) berukuran kecil.
1. Saat browser membuka website, browser mengunduh `index.html` dan `blazor.webassembly.js`.
2. Script ini mengunduh runtime `.wasm` dan DLL aplikasi ke memori browser client.
3. Kode C# berjalan langsung di dalam sandbox WebAssembly browser! Tidak perlu instalasi plugin apa pun bagi pengguna akhir.

---

### 5.2 Bootstrapping Client-Side (`frontend/Program.cs`)
```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Memasang komponen App.razor ke tag HTML <div id="app"> di index.html
builder.RootComponents.Add<App>("#app");

// Memasang tag head (title, meta) secara dinamis
builder.RootComponents.Add<HeadOutlet>("head::after");

// Mendaftarkan HttpClient ke Dependency Injection container
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

await builder.Build().RunAsync();
```

> [!IMPORTANT]
> Perhatikan baris `BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)`.
> Secara default, `HttpClient` di frontend mengarah ke domain/port dirinya sendiri (`http://localhost:5181`).
> Inilah mengapa di `Weather.razor`, aplikasi mengambil data dari `"sample-data/weather.json"`, bukan langsung memanggil backend port 5266!

---

### 5.3 Sistem Komponen & Routing (`App.razor`)
```razor
<Router AppAssembly="@typeof(App).Assembly" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)"/>
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
</Router>
```
- `<Router>`: Memindai semua class di assembly yang memiliki dekorasi direktif `@page "/..."`.
- `RouteView`: Jika rute URL cocok, komponen halaman terkait akan dibungkus ke dalam `MainLayout`.
- `NotFoundPage`: Jika pengguna mengetik URL yang salah, otomatis menampilkan komponen `Pages/NotFound.razor`.

---

### 5.4 State Management Sederhana (`frontend/Pages/Counter.razor`)
```razor
@page "/counter"

<h1>Counter</h1>
<p role="status">Current count: @currentCount</p>
<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}
```
Ketika tombol diklik (`@onclick="IncrementCount"`):
1. Event handler dieksekusi di WebAssembly.
2. Variabel `currentCount` bertambah 1.
3. Blazor mendeteksi perubahan state dan otomatis memicu *re-rendering* diff DOM secara efisien tanpa reload halaman!

---

### 5.5 Asset Fingerprinting (.NET 10 Feature)
Di `frontend/wwwroot/index.html`:
```html
<script src="_framework/blazor.webassembly#[.{fingerprint}].js"></script>
```
Dan di `frontend/frontend.csproj`:
```xml
<OverrideHtmlAssetPlaceholders>true</OverrideHtmlAssetPlaceholders>
```
Ini adalah fitur baru .NET 10 untuk *cache-busting*. Compiler menyisipkan hash unik (*fingerprint*) pada nama file statis saat build, memastikan browser pengguna tidak akan memakai script cache lama saat sensei merilis pembaruan!

---

## 6. Alur Komunikasi Data (End-to-End Data Flow)

Berikut perbandingan alur data yang ada saat ini (Mock Data) versus alur produksi (Real API Call):

### Skenario 1: Saat Ini (Standalone Mock)
```text
Browser User -> Navigasi ke /weather
    └── Weather.razor (OnInitializedAsync)
            └── Memanggil Http.GetFromJsonAsync("sample-data/weather.json")
                    └── Mengambil file statis lokal di wwwroot
```

### Skenario 2: Terhubung ke Backend API (Fullstack Terintegrasi)
```text
Browser User -> Navigasi ke /weather
    └── Weather.razor (OnInitializedAsync)
            └── Memanggil Http.GetFromJsonAsync("http://localhost:5266/weatherforecast")
                    │
                    ▼ [HTTP GET Request lintas Port via Network]
    Backend Kestrel Server (Port 5266)
            └── Program.cs Pipeline (CORS Check -> Routing)
                    └── WeatherForecastController.Get()
                            └── Return List<WeatherForecast>
                                    │
                                    ▼ [JSON Serialization]
    Browser User (Blazor Wasm)
            └── Menerima JSON & Melakukan Deserialisasi ke C# Object
                    └── Mengisi tabel HTML & Merender ke Layar
```

---

## 7. Tantangan Nyata: CORS & Cara Menghubungkan Frontend ke Backend

Jika sensei nanti mengubah `Weather.razor` untuk mengambil data langsung dari `http://localhost:5266/weatherforecast`, browser akan melempar pesan galat merah:
> `Cross-Origin Request Blocked (CORS Policy)`

### Mengapa Galat CORS Terjadi?
Frontend berjalan di `http://localhost:5181` dan Backend berjalan di `http://localhost:5266`.
Karena nomor port berbeda, browser menganggapnya sebagai dua domain berbeda (*Cross-Origin*). Demi keamanan, browser melarang WebAssembly membaca data dari port lain kecuali diizinkan secara eksplisit oleh server.

### Solusi CORS di Backend:
Tambahkan CORS policy di [backend/Program.cs](file:///run/media/salahudin/New%20Volume/github/dinamisinfomadya/backend/Program.cs):

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Daftarkan Kebijakan CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClientPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5181", "https://localhost:7180")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 2. Aktifkan Middleware CORS (Wajib ditaruh sebelum MapControllers)
app.UseCors("BlazorClientPolicy");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## 8. Panduan Operasional: Menjalankan & Debugging

Karena project ini terdiri dari dua aplikasi independen, sensei dapat menjalankannya melalui terminal terpisah:

### Menjalankan Backend:
Buka terminal 1:
```bash
cd backend
dotnet run
```
> Backend akan aktif di `http://localhost:5266` (dan `https://localhost:7240`).

### Menjalankan Frontend:
Buka terminal 2:
```bash
cd frontend
dotnet run
```
> Frontend akan aktif di `http://localhost:5181`. Buka link tersebut di browser!

### Menjalankan dengan Hot Reload (Mode Development):
Gunakan flag `watch` agar setiap kali sensei mengedit file `.cs` atau `.razor`, aplikasi otomatis memperbarui tampilan tanpa perlu restart manual:
```bash
dotnet watch --project backend
dotnet watch --project frontend
```

---

## 9. Roadmap Menuju Enterprise Clean Architecture

Saat project DinamisInfoMadya ini bertumbuh besar, pola pemisahan saat ini bisa ditingkatkan menjadi **Clean Architecture** (Onion / Hexagonal Architecture):

```text
========================================================================================
                              LAYER PRESENTASI (UI & API)
========================================================================================
    [ frontend (Blazor Wasm) ]                    [ backend (ASP.NET Core API) ]
                 │                                               │
                 │                                               │ (Meneruskan Request)
                 │                                               ▼
                 │                               ┌──────────────────────────────┐
                 │                               │ DinamisInfoMadya.Application │
                 │                               │   - Interfaces & UseCases    │
                 │                               │   - Command/Query Handlers   │
                 │                               └──────────────┬───────────────┘
                 │                                              │
                 │ (Menggunakan DTO)                            │ (Mengatur Entitas)
                 ▼                                              ▼
┌──────────────────────────────┐                 ┌──────────────────────────────┐
│    DinamisInfoMadya.Shared   │                 │     DinamisInfoMadya.Domain  │
│      - DTOs / Models         │                 │       - Core Entities        │
│      - API Contracts         │                 │       - Value Objects        │
│      - Validations           │                 │       - Domain Exceptions    │
└──────────────────────────────┘                 └──────────────▲───────────────┘
                 ▲                                              │
                 │                                              │ (Implementasi Data)
                 └──────────────────────────────────────────────┤
                                                                │
                                                 ┌──────────────┴───────────────┐
                                                 │ DinamisInfoMadya.Infrastructure
                                                 │   - EF Core & DbContext      │
                                                 │   - SQL / NoSQL Repositories │
                                                 │   - External Services        │
                                                 └──────────────────────────────┘
========================================================================================
```

### Rekomendasi Langkah Selanjutnya:
1. **Buat Project `Shared` (Class Library):**
   Pindahkan class seperti `WeatherForecast.cs` ke project bersama (`DinamisInfoMadya.Shared`). Dengan begitu, baik backend maupun frontend menggunakan class definisi yang persis sama tanpa perlu duplikasi kode.
2. **Implementasikan Entity Framework Core (EF Core):**
   Gunakan EF Core dengan database (PostgreSQL, SQL Server, atau SQLite) untuk mengganti data acak (*random generator*) menjadi data riil persisten.
3. **Autentikasi & Otorisasi:**
   Gunakan ASP.NET Core Identity atau JWT (JSON Web Tokens) dengan `AuthenticationStateProvider` di Blazor agar sistem mendukung Login, Register, dan Hak Akses Role pengguna.

---
*Dokumen ini disusun khusus sebagai bahan pembelajaran dan dokumentasi teknis project DinamisInfoMadya.*

