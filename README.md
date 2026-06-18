# DOZP - Projekt Digitalizácie a OCR Spracovania Publikácií UK FF

**Projekt retrokonverzie katalógov knižnice Univerzity Komenského v Bratislave**

Komplexný systém pre digitalizáciu a OCR spracovanie publikácií, obálok a obsahov kníh z knižničného fondu. Projekt bol vytvorený v roku 2015 ako súčasť iniciatívy na zachovávanie a sprístupnenie historických a knižničných zbierok.

---

## 📋 Tabuľka obsahu

- [Popis projektu](#popis-projektu)
- [Architektúra systému](#architektúra-systému)
- [Štruktúra projektu](#štruktúra-projektu)
- [Použité technológie](#použité-technológie)
- [Komponenty](#komponenty)
- [Inštalácia a nastavenie](#inštalácia-a-nastavenie)
- [Použitie](#použitie)
- [Vývojové prostredia](#vývojové-prostredia)
- [Licencia](#licencia)

---

## Popis projektu

DOZP je integrovaný systém zameraný na:

- **Digitalizáciu** knižničných zbierok a publikácií
- **OCR spracovanie** skenovaných materiálov na extrakciu textu
- **Správu metadát** a katalógizáciu digitálnych zdrojov
- **Webové rozhranie** pre prehľadávanie a prístup k digitalizovanému obsahu
- **Štatistické analýzy** procesov digitalizácie

Projekt pozostáva z modulárneho systému viacerých aplikácií a služieb pracujúcich v synergi:
- Desktopové aplikácie pre skenovanie a OCR spracovanie
- Backendové služby pre spracovanie a správu dát
- Webové rozhranie pre koncových užívateľov
- Agentné systémy pre automatizované úlohy

---

## Architektúra systému

### Celková architektúra

Systém DOZP nasleduje **vrstvovú architektúru (N-tier)** s jasným oddelením zodpovednosti:

```
┌─────────────────────────────────────────────────────┐
│         Prezentačná vrstva (Web)                    │
│  - Comdat.DOZP.Web (ASP.NET WebForms)               │
│  - Webové rozhranie, autentifikácia, štatistiky     │
└────────────────┬────────────────────────────────────┘
                 │
┌─────────────────┴────────────────────────────────────┐
│         Servisná vrstva                              │
│  - Comdat.DOZP.Service (WCF Services)                │
│  - Business logic, spracovanie dát                   │
│  - Comdat.DOZP.Process (Controllers)                 │
└────────────────┬────────────────────────────────────┘
                 │
┌─────────────────┴────────────────────────────────────┐
│         Vrstva Aplikačnej logiky                     │
│  - Comdat.DOZP.Core (Business entities & logic)      │
│  - Comdat.DOZP.OCR (OCR processing)                  │
│  - Comdat.DOZP.Scan (Scanning management)            │
│  - Comdat.DOZP.App (Desktop App - XAML)              │
│  - Comdat.DOZP.Agent (Background Processing)         │
└────────────────┬────────────────────────────────────┘
                 │
┌─────────────────┴────────────────────────────────────┐
│         Dátová vrstva                                │
│  - Comdat.DOZP.Data (Data Access, ORM)               │
│  - Mapovanie entít, business objekty                 │
│  - Prístup k databáze                                │
└──────────────────────────────────────────────────────┘
```

### Tok dát

```
Skeňovanie                OCR Spracovanie              Web Prístup
┌────────────┐           ┌──────────────┐            ┌──────────────┐
│ Skener     │──────────▶│ OCR Engine   │──────────▶ │ Web Portal   │
│ (Hardvér)  │           │ (Tesseract?) │            │ (ASP.NET)    │
└────────────┘           └──────────────┘            └──────────────┘
       │                       │                           │
       └───────────────────────┼───────────────────────────┘
                               │
                    ┌──────────▼──────────┐
                    │   Databáza         │
                    │ (SQL Server)        │
                    └─────────────────────┘
```

---

## Štruktúra projektu

### Hierarchia adresárov a súborov

```
DOZP/
├── Comdat.DOZP.Agent/              # Background Agent Service
│   ├── Program.cs                  # Entry point agenta
│   ├── App.config                  # Konfigurácia
│   ├── Comdat.DOZP.Agent.csproj   # C# Project File
│   ├── packages.config              # NuGet závisitosti
│   └── Properties/                 # Assembly informácie
│
├── Comdat.DOZP.App/                # Desktopová aplikácia (WPF/XAML)
│   ├── MainWindow.xaml             # Hlavné GUI
│   ├── MainWindow.xaml.cs          # Code-behind
│   ├── App.xaml                    # Aplikačné zdroje a štýly
│   ├── App.xaml.cs                 # Application code-behind
│   ├── Comdat.DOZP.App.csproj     # Project file
│   ├── Comdat.DOZP.Scan.ico        # Aplikačná ikona
│   ├── app.config                  # Konfigurácia (Debug/Release)
│   ├── app.Debug.config            # Debug konfigurácia
│   ├── app.Release.config          # Release konfigurácia
│   ├── Controls/                   # XAML User Controls
│   ├── Dialogs/                    # Dialógové okná
│   ├── Images/                     # Obrázky a grafika
│   ├── Themes/                     # XAML motivy a štýly
│   ├── Utils/                      # Pomocné utility triedy
│   ├── packages/                   # NuGet balíčky (cache)
│   ├── packages.config             # NuGet dependency manifest
│   └── Properties/                 # Assembly properties
│
├── Comdat.DOZP.Core/               # Jadro aplikácie (Business Logic)
│   ├── App.cs                      # Aplikačné konfigurácie
│   ├── Extensions.cs               # Rozšírenia a helper metódy
│   ├── Comdat.DOZP.Core.csproj    # Project file
│   ├── packages.config             # NuGet závislosti
│   ├── Config/                     # Konfigurčné entity
│   ├── Criteria/                   # Kritéria pre vyhľadávanie
│   ├── DataReaders/                # Čitače a parsery dát
│   ├── Entities/                   # Doménové entity a modely
│   ├── Enums/                      # Výčty a konštanty
│   ├── Images/                     # Image processing logika
│   ├── OCR/                        # OCR engine logika
│   ├── Statistics/                 # Štatistické výpočty
│   ├── Utils/                      # Utility funkcie
│   └── Properties/                 # Assembly properties
│
├── Comdat.DOZP.Data/               # Dátová vrstva (ORM, Repository)
│   ├── Extensions.cs               # LINQ extensions
│   ├── App.config                  # Database configuration
│   ├── Comdat.DOZP.Data.csproj    # Project file
│   ├── packages.config             # NuGet dependencies
│   ├── Business/                   # Business objekty
│   ├── Mapping/                    # ORM mapovanie (Entity mappings)
│   ├── Repository/                 # Repository pattern impl.
│   └── Properties/                 # Assembly properties
│
├── Comdat.DOZP.OCR/                # OCR Aplikácia (WPF/XAML)
│   ├── MainWindow.xaml             # OCR GUI
│   ├── MainWindow.xaml.cs          # OCR code-behind
│   ├── App.xaml                    # Application resources
│   ├── App.xaml.cs                 # Application startup
│   ├── Comdat.DOZP.OCR.csproj     # Project file
│   ├── Comdat.DOZP.OCR.ico        # OCR ikona
│   ├── app.config                  # App configuration
│   ├── packages.config             # NuGet dependencies
│   ├── Dialogs/                    # OCR dialógové okná
│   ├── Images/                     # OCR grafika
│   └── Properties/                 # Assembly properties
│
├── Comdat.DOZP.Process/            # Process Controllers (WCF)
│   ├── AuthController.cs           # Autentifikácia a autorizácia
│   ├── DozpController.cs           # Hlavný controller - business logika
│   ├── ExceptionMessage.cs         # Chybové správy a handling
│   ├── WcfExtensions.cs            # WCF rozšírenia
│   ├── Comdat.DOZP.Process.csproj # Project file
│   ├── app.config                  # WCF configuration
│   ├── Service References/         # WCF service references
│   └── Properties/                 # Assembly properties
│
├── Comdat.DOZP.Scan/               # Skenovacia Aplikácia (WPF/XAML)
│   ├── MainWindow.xaml             # Skenovanie GUI
│   ├── MainWindow.xaml.cs          # Skenovanie code-behind
│   ├── App.xaml                    # Application resources
│   ├── App.xaml.cs                 # Application startup
│   ├── Comdat.DOZP.Scan.csproj    # Project file
│   ├── Comdat.DOZP.Scan.ico       # Skenovacia ikona
│   ├── app.config                  # App configuration
│   ├── packages.config             # NuGet dependencies
│   ├── Controls/                   # Skenovacie kontroly
│   ├── Dialogs/                    # Dialógové okná
│   ├── Images/                     # Skenovacie obrázky
│   ├── Resources/                  # Aplikačné zdroje
│   ├── Themes/                     # Motivy a štýly
│   └── Properties/                 # Assembly properties
│
├── Comdat.DOZP.Service/            # WCF Servisné kontrakty
│   ├── DozpService.cs              # Primárna WCF servisná impl.
│   ├── UserProfile.cs              # Profil a správa užívateľa
│   ├── Comdat.DOZP.Services.csproj# Project file
│   ├── App.config                  # Service configuration
│   ├── Diagram.cd                  # Class diagram
│   ├── packages.config             # NuGet dependencies
│   ├── Contracts/                  # Service kontrakty
│   ├── ObalkyKnih/                 # Modul "Obálky kníh"
│   └── Properties/                 # Assembly properties
│
├── Comdat.DOZP.Web/                # ASP.NET WebForms Aplikácia
│   ├── Default.aspx                # Úvodná stránka
│   ├── Default.aspx.cs             # Úvodná logika
│   ├── Login.aspx                  # Prihlásenie
│   ├── Contact.aspx                # Kontakt stránka
│   ├── Site.Master                 # Master page layout
│   ├── Service.svc                 # WCF Service endpoint
│   ├── Global.asax                 # Aplikačné eventy
│   ├── Web.config                  # Konfigurácia
│   ├── Comdat.DOZP.Web.csproj     # Project file
│   ├── packages.config             # NuGet dependencies
│   ├── Account/                    # Správa účtov
│   ├── Admin/                      # Admin panel
│   ├── App_GlobalResources/        # Globálne zdroje
│   ├── App_Themes/                 # Aplikačné motivy
│   ├── Catalogues/                 # Prehľad katalógov
│   ├── Controls/                   # User Controls
│   ├── Download/                   # Stiahnutie materiálov
│   ├── Help/                       # Nápoveda
│   ├── Images/                     # Webové obrázky
│   ├── Scripts/                    # JavaScript kódy
│   ├── Statistics/                 # Štatistické reporty
│   └── Properties/                 # Assembly properties
│
├── Comdat.DOZP.sln                # Visual Studio Solution File
├── DOZP-Skenování.png             # Dokumentácia - diagram skenovania
├── DOZP-Zpracování.png            # Dokumentácia - diagram spracovania
├── README.md                       # Táto dokumentácia
├── .gitignore                      # Git ignore pravidlá
└── .gitattributes                  # Git attributes
```

---

## Použité technológie

### Programovací jazyk
- **C# (.NET Framework 4.0+)** - Primárny jazyk vývoja
- **.NET Framework** - Runtime a base class library

### Prezentačné vrstvy
- **Windows Presentation Foundation (WPF)** - Desktopové GUI aplikácie
  - XAML (eXtensible Application Markup Language)
  - Data Binding, Triggers, Converters
  - Comdat.DOZP.App, Comdat.DOZP.Scan, Comdat.DOZP.OCR

- **ASP.NET WebForms** - Webové rozhranie
  - Master Pages, User Controls
  - Comdat.DOZP.Web

### Backend & Business Logic
- **Windows Communication Foundation (WCF)** - Web služby
  - SOAP protocol
  - Service contracts, Data contracts
  - Comdat.DOZP.Service, Comdat.DOZP.Process

- **LINQ (Language Integrated Query)** - Dotazy na dáta
  - LINQ to SQL
  - Entity Framework

### Dátová vrstva
- **SQL Server** - Relačná databáza
- **ORM** - Object-Relational Mapping
  - LINQ to SQL alebo Entity Framework

### Knižnice a Frameworky
- **.NET Framework 4.0+**
- **System.ServiceModel** - WCF komponenty
- **System.Data.Linq** - LINQ to SQL
- **System.Windows** - WPF komponenty
- **System.Web** - ASP.NET komponenty

### OCR Engine
- **Tesseract OCR** (cez wrapper)
- Alebo vlastný OCR engine

### Vývojové nástroje
- **Visual Studio 2010+** - IDE
- **NuGet Package Manager** - Správa závislostí
- **Git** - Verziovaní systém
- **SQL Server Management Studio** - Správa databázy

---

## Komponenty

### 1. **Comdat.DOZP.Core** - Jadro aplikácie
Centralizovaná business logika s entitami, kritériami vyhľadávania, OCR logiku a štatistické výpočty.

### 2. **Comdat.DOZP.Data** - Dátová vrstva
Abstrakcia prístupu k databáze s Repository pattern, ORM mapovaním a business objektami.

### 3. **Comdat.DOZP.Scan** - Skenovacia aplikácia (WPF)
Desktopová aplikácia pre ovládanie skenerov, nastavenie parametrov a ukladanie skenov.

### 4. **Comdat.DOZP.OCR** - OCR spracovacia aplikácia (WPF)
Spracovanie skenovaných obrázkov a extrakcia textu s možnosťou editácie a ukladania.

### 5. **Comdat.DOZP.App** - Hlavná aplikácia (WPF)
Centralizovaná správa a koordinácia procesov digitalizácie.

### 6. **Comdat.DOZP.Service** - WCF Services
Backend servisné kontrakty s SOAP komunikáciou.

### 7. **Comdat.DOZP.Process** - Process Controllers
Riadiace jednotky pre správu procesov s autentifikáciou a spracovaním chýb.

### 8. **Comdat.DOZP.Web** - Webové rozhranie (ASP.NET)
Webový portál pre prehliadavanie katalógov, sťahovanie dokumentov a správu účtov.

### 9. **Comdat.DOZP.Agent** - Background Service
Agent pre automatizované úlohy na pozadí.

---

## Inštalácia a nastavenie

### Predpoklady
- Windows OS (XP/Vista/7/8/10+)
- .NET Framework 4.0+
- Visual Studio 2010+
- SQL Server 2008+
- Internet Information Services (IIS) 7.0+

### Kroky inštalácie

1. **Klonovanie repozitára:**
```bash
git clone https://github.com/ppucik/DOZP.git
cd DOZP
```

2. **Obnovenie NuGet balíčkov:**
```bash
nuget restore Comdat.DOZP.sln
```

3. **Konfigurácia databázy:**
   - Vytvorte SQL Server databázu: `DOZP_DB`
   - Upravte connection string v `Web.config` a `App.config`

4. **Kompilácia:**
```bash
msbuild Comdat.DOZP.sln /p:Configuration=Release
```

5. **Nasadenie webovej aplikácie do IIS**
   - Publikovanie Comdat.DOZP.Web

6. **Spustenie desktopových aplikácií:**
```bash
.\Comdat.DOZP.Scan\bin\Release\Comdat.DOZP.Scan.exe
.\Comdat.DOZP.OCR\bin\Release\Comdat.DOZP.OCR.exe
.\Comdat.DOZP.App\bin\Release\Comdat.DOZP.App.exe
```

---

## Použitie

### Skenovanie dokumentov
1. Spustite Comdat.DOZP.Scan.exe
2. Pripojte skener a nastavte parametre (DPI, farebný režim)
3. Spustite skenovanie a organizujte skeny do projektov

### OCR Spracovanie
1. Spustite Comdat.DOZP.OCR.exe
2. Načítajte obrázky a nastavte OCR parametre
3. Spustite spracovanie a upravte výsledky
4. Uložte výsledky do databázy

### Webový portál
1. Otvorte http://localhost/DOZP/ v prehliadači
2. Prihláste sa
3. Prehľadávajte katalógy a sťahujte dokumenty

---

## Vývojové prostredia

- Visual Studio 2010+
- Git repozitár: https://github.com/ppucik/DOZP
- Default branch: `master`

### Build a Release
```bash
# Debug Build
msbuild Comdat.DOZP.sln /p:Configuration=Debug

# Release Build
msbuild Comdat.DOZP.sln /p:Configuration=Release
```

---

## Konfiguračné súbory

- **Web.config** - Webová aplikácia
- **App.config** - Desktopové aplikácie
- **packages.config** - NuGet dependencies

---

## Grafické dokumentácie

- **DOZP-Skenování.png** - Diagram skenovacieho procesu
- **DOZP-Zpracování.png** - Diagram spracovania a OCR workflow

---

## Kontakt a podpora

**Vlastník projektu:** [@ppucik](https://github.com/ppucik)
**GitHub:** https://github.com/ppucik/DOZP
**Problému hlásenie:** [GitHub issues](https://github.com/ppucik/DOZP/issues)

---

## História projektu

| Dátum | Udalosť |
|-------|--------|
| 2015 | Vytvorenie projektu - začiatok digitalizácie |
| 2015+ | Vývoj a nasadenie komponentov |
| 2026 | Aktualizácia dokumentácie |

**Status:** Aktívny projekt

---

## Budúce zlepšenia

- Migrácia na .NET 6+/Core
- Cloudové nasadenie (Azure/AWS)
- Mobile aplikácia (Xamarin)
- Modernizácia webového rozhrania (React.js/Vue.js)
- AI OCR integrácia (Google Cloud Vision, AWS Textract)
- Microservices architektúra
- Docker a Kubernetes
- REST API a GraphQL
- SignalR notifikácie

---

**Posledná aktualizácia:** Jún 2026

*Táto dokumentácia poskytuje komplexný prehľad projektu DOZP, jeho architektúry, komponentov a inštrukcií na inštaláciu a použitie.*

**© UK FF Praha**
