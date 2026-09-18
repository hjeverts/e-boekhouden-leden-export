# Crescendo_ledenlijst

Desktop-app om snel een leden- of donateurlijst uit te draaien als PDF, CSV of ODS.

- Laadt een ledenlijst-export (.xlsx) uit de ledenadministratie.
- Toont alle rijen in een tabel; boven elke kolom staat een selectievakje dat bepaalt
  of die kolom wordt meegenomen in de export.
- Dropdown om te filteren: alle personen, alleen leden (lidnummer < 10000) of alleen
  donateurs (lidnummer >= 10000).
- Export naar PDF, CSV (`;`-gescheiden) of ODS (OpenDocument Spreadsheet).
- Gebouwd met [Avalonia UI](https://avaloniaui.net/) (.NET), compileert naar een
  losstaande executable voor zowel Windows als Linux (getest op Ubuntu; werkt ook op
  Debian en CachyOS/Arch, mits de gebruikelijke grafische libraries aanwezig zijn).

## Vereisten

- [.NET SDK 10](https://dotnet.microsoft.com/download) (of nieuwer) om te bouwen.
- Op Linux: een grafische sessie (X11 of Wayland via XWayland) om de app te draaien.

## Project openen / bouwen

```bash
dotnet build
```

## Draaien tijdens ontwikkeling

```bash
dotnet run --project src/Crescendo.LedenLijst
```

## Gebruik

1. Klik op **Bestand openen…** en kies de ledenlijst-export (.xlsx).
2. Kies bij **Weergave** of je alle personen, alleen leden, of alleen donateurs wilt zien.
3. Vink boven de gewenste kolommen het selectievakje aan/uit (**Alles aan**/**Alles uit**
   zetten in één keer alle kolommen aan of uit).
4. Klik op **CSV**, **PDF** of **ODS** en kies waar het bestand opgeslagen moet worden.

De export bevat alleen de aangevinkte kolommen, voor de rijen die bij de gekozen
weergave (alle/leden/donateurs) horen.

## Uitleveren als losstaande app (self-contained)

Deze commando's bouwen een enkel uitvoerbaar bestand dat geen aparte .NET-installatie
nodig heeft op de doelmachine.

**Windows (x64):**

```bash
dotnet publish src/Crescendo.LedenLijst -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64
```

Resultaat: `publish/win-x64/CrescendoLedenlijst.exe` (plus enkele native `.dll`'s ernaast
die niet in het single-file bestand passen — de hele map meenemen).

**Linux x64 (Debian, CachyOS, Ubuntu, …):**

```bash
dotnet publish src/Crescendo.LedenLijst -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/linux-x64
```

Resultaat: `publish/linux-x64/CrescendoLedenlijst` (plus enkele native `.so`'s ernaast —
de hele map meenemen). Uitvoerbaar maken indien nodig met `chmod +x`.

**Linux ARM64 (bv. CachyOS op ARM):**

```bash
dotnet publish src/Crescendo.LedenLijst -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=true -o publish/linux-arm64
```

Op Linux zijn voor de GUI meestal wel de gebruikelijke desktop-libraries nodig
(fontconfig, X11 of Wayland/XWayland) — die staan standaard op zowel Debian als
CachyOS met een desktopomgeving.

## Projectstructuur

```
src/Crescendo.LedenLijst/
  Models/        Member-model (incl. lid/donateur-indeling op basis van lidnummer)
  Services/      Inlezen van de .xlsx-export
  Services/Export/  CSV-, PDF- en ODS-exporters
  ViewModels/    Kolomselectie, filter en gridstate (MVVM)
  Views/         Het hoofdvenster (grid + toolbar)
data/            Voorbeeld ledenlijst-export
```
